using System;

namespace SmokeTest.Shared.Classes
{
    public sealed class TestSchedule
    {
        #region Constructors

        public TestSchedule()
        {
            LastRunResult = LastRunResult.NotRun;
        }

        public TestSchedule(in long uniqidId, in string name, in string testId,
            in DateTime startTime)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            if (uniqidId < 1)
                throw new ArgumentOutOfRangeException(nameof(uniqidId));

            if (String.IsNullOrEmpty(testId))
                throw new ArgumentOutOfRangeException(nameof(testId));

            UniqueId = uniqidId;
            Name = name;
            TestId = testId;
            ScheduleType = ScheduleType.Once;
            StartTime = startTime;
            LastRunResult = LastRunResult.NotRun;
        }

        public TestSchedule(in long uniqidId, in string name, in string testId,
            in DateTime startTime, in DateTime? expires, in int frequency)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            if (uniqidId < 1)
                throw new ArgumentOutOfRangeException(nameof(uniqidId));

            if (String.IsNullOrEmpty(testId))
                throw new ArgumentOutOfRangeException(nameof(testId));

            if (frequency < 1)
                throw new ArgumentOutOfRangeException(nameof(frequency));

            UniqueId = uniqidId;
            Name = name;
            TestId = testId;
            ScheduleType = ScheduleType.Daily;
            StartTime = startTime;
            Frequency = frequency;
            Expires = expires;
            LastRunResult = LastRunResult.NotRun;
        }

        public TestSchedule(in long uniqidId, in string name, in string testId,
            in DateTime startTime, in DateTime? expires,
            in int frequency, in ScheduleDay scheduleDay)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            if (uniqidId < 1)
                throw new ArgumentOutOfRangeException(nameof(uniqidId));

            if (String.IsNullOrEmpty(testId))
                throw new ArgumentOutOfRangeException(nameof(testId));

            if (frequency < 1)
                throw new ArgumentOutOfRangeException(nameof(frequency));

            if ((int)scheduleDay < 1 || (scheduleDay | ScheduleDay.NotSet) == ScheduleDay.NotSet)
                throw new ArgumentException(nameof(scheduleDay));

            UniqueId = uniqidId;
            Name = name;
            TestId = testId;
            ScheduleType = ScheduleType.Weekly;
            StartTime = startTime;
            ScheduleDays = scheduleDay;
            Frequency = frequency;
            Expires = expires;
            LastRunResult = LastRunResult.NotRun;
        }

        #endregion Constructors

        #region Properties

        public long UniqueId { get; set; }

        public string Name { get; set; }

        public string TestId { get; set; }

        public ScheduleType ScheduleType { get; set; }

        public DateTime StartTime { get; set; }

        public int Frequency { get; set; }

        public ScheduleDay ScheduleDays { get; set; }

        public DateTime? LastRun { get; set; }

        public LastRunResult LastRunResult { get; set; }

        public DateTime? Expires { get; set; }

        public bool Enabled
        {
            get
            {
                if (Expires.HasValue)
                {
                    if (DateTime.Now > Expires.Value)
                        return false;
                }

                if (ScheduleType == ScheduleType.Once && LastRun.HasValue)
                {
                    return false;
                }

                return true;
            }
        }

        #endregion Properties

        #region Public Methods

        public TimeSpan NextRun()
        {
            if (!LastRun.HasValue)
            {
                return DateTime.Now - StartTime;
            }

            switch (ScheduleType)
            {
                case ScheduleType.Once:
                    return DateTime.Now - StartTime;

                case ScheduleType.Daily:
                    return CalculateNextRunDaily();

                case ScheduleType.Weekly:
                    return CalculateNextWeeklyRun();

                    //case ScheduleType.Monthly:
                    //    return CalculateNextMonthlyRun();
            }

            throw new InvalidOperationException();
        }

        #endregion Public Methods

        #region Private Methods

        private TimeSpan CalculateNextRunDaily()
        {
            DateTime startTime = LastRun.Value;
            startTime = new DateTime(startTime.Year, startTime.Month, startTime.Day + Frequency, StartTime.Hour, StartTime.Minute, StartTime.Second);

            return startTime - DateTime.Now;
        }

        private TimeSpan CalculateNextWeeklyRun()
        {
            DateTime startTime = LastRun.Value.AddDays(1);
            startTime = new DateTime(startTime.Year, startTime.Month, startTime.Day, StartTime.Hour, StartTime.Minute, StartTime.Second);

            int loopCount = 0;

            while (true)
            {
                if (startTime.DayOfWeek == DayOfWeek.Monday && ScheduleDays.HasFlag(ScheduleDay.Monday))
                {
                    return DateTime.Now - startTime;
                }

                if (startTime.DayOfWeek == DayOfWeek.Tuesday && ScheduleDays.HasFlag(ScheduleDay.Tuesday))
                {
                    return DateTime.Now - startTime;
                }

                if (startTime.DayOfWeek == DayOfWeek.Wednesday && ScheduleDays.HasFlag(ScheduleDay.Wednesday))
                {
                    return DateTime.Now - startTime;
                }

                if (startTime.DayOfWeek == DayOfWeek.Thursday && ScheduleDays.HasFlag(ScheduleDay.Thursday))
                {
                    return DateTime.Now - startTime;
                }

                if (startTime.DayOfWeek == DayOfWeek.Friday && ScheduleDays.HasFlag(ScheduleDay.Friday))
                {
                    return DateTime.Now - startTime;
                }

                if (startTime.DayOfWeek == DayOfWeek.Saturday && ScheduleDays.HasFlag(ScheduleDay.Saturday))
                {
                    return DateTime.Now - startTime;
                }

                if (startTime.DayOfWeek == DayOfWeek.Sunday && ScheduleDays.HasFlag(ScheduleDay.Sunday))
                {
                    return DateTime.Now - startTime;
                }

                if (loopCount > 5000)
                    break;

                startTime = startTime.AddDays(1);
            }

            return new TimeSpan();
        }

        //private TimeSpan CalculateNextMonthlyRun()
        //{
        //    DateTime startTime = LastRun.Value;
        //    startTime = new DateTime(startTime.Year, startTime.Month + Frequency, startTime.Day, StartTime.Hour, StartTime.Minute, StartTime.Second);

        //    return startTime - DateTime.Now;
        //}

        #endregion Private Methods
    }
}
