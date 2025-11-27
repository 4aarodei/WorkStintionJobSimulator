using System;
using System.Reflection;

namespace WorkstationJobSimulator.Events
{
    /// <summary>
    /// Дискретний генератор подій:
    /// 1 виклик Generate() = 1 симульована година.
    /// EventChance(λ) інтерпретується як середня кількість подій цього типу на добу.
    /// </summary>
    public class SimulationEventGenerator
    {
        private readonly Random _random = new();

        private readonly double _airProbPerHour;
        private readonly double _outageProbPerHour;

        /// <summary>
        /// Поточна година симуляції (рахуємо скільки годин уже пройшло).
        /// </summary>
        public int CurrentSimHour { get; private set; } = 0;

        /// <summary>
        /// Завжди 1 година, бо 1 тік = 1 година.
        /// Залишив для сумісності, якщо десь ще використовується.
        /// </summary>
        public TimeSpan LastSimulatedInterval => TimeSpan.FromHours(1);

        public SimulationEventGenerator()
        {
            // Витягуємо λ із атрибутів: λ (подій/день)
            double lambdaAir = GetLambdaFromAttribute(typeof(AirAlarm));
            double lambdaOutage = GetLambdaFromAttribute(typeof(TurningOffTheLights));

            // Ймовірність хоча б однієї події за 1 годину для пуассонівського процесу:
            // p = 1 - exp(-λ/24)
            _airProbPerHour = 1 - Math.Exp(-lambdaAir / 24.0);
            _outageProbPerHour = 1 - Math.Exp(-lambdaOutage / 24.0);
        }

        private static double GetLambdaFromAttribute(Type t)
        {
            var attr = t.GetCustomAttribute<EventChanceAttribute>();
            if (attr is null || attr.Chance <= 0)
            {
                throw new InvalidOperationException(
                    $"Тип {t.Name} не має коректного EventChanceAttribute.");
            }

            return attr.Chance; // λ (подій/день)
        }

        /// <summary>
        /// 1 тік симуляції (1 година).
        /// Повертає:
        ///   - TurningOffTheLights, якщо в цю годину сталося відключення
        ///   - AirAlarm, якщо відключення не було, але сталася тривога
        ///   - null, якщо в цю годину подій немає
        /// </summary>
        public SimulationEvent? Generate()
        {
            CurrentSimHour++;

            // Спочатку перевіряємо відключення світла
            bool outageOccurs = _random.NextDouble() < _outageProbPerHour;
            if (outageOccurs)
            {
                return new TurningOffTheLights();
            }

            // Якщо відключення немає, пробуємо повітряну тривогу
            bool airAlarmOccurs = _random.NextDouble() < _airProbPerHour;
            if (airAlarmOccurs)
            {
                return new AirAlarm();
            }

            // Жодної події в цю годину
            return null;
        }

        /// <summary>
        /// Конвертує 1 симульовану годину у реальну затримку для Thread.Sleep.
        /// Наприклад: realSecondsPerSimHour = 2 => 1 сим-година = 2 секунди реального часу.
        /// </summary>
        public TimeSpan GetRealDelay(double realSecondsPerSimHour = 1.0)
        {
            if (realSecondsPerSimHour <= 0)
                return TimeSpan.Zero;

            return TimeSpan.FromSeconds(realSecondsPerSimHour);
        }
    }
}
