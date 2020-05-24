using System;

namespace SmokeTest.Shared
{
    public enum ScheduleType
    {
        Once = 1,
        Daily = 2,
        Weekly = 3,
        //Monthly = 4,
        PressureTest = 5,
    }

    [Flags]
    public enum ScheduleDay
    {
        NotSet = 0,
        Monday = 1,
        Tuesday = 2,
        Wednesday = 4,
        Thursday = 8,
        Friday = 16,
        Saturday = 32,
        Sunday = 64
    }

    public enum LastRunResult
    {
        NotRun,

        Success,

        Warning,

        Error
    }
}
