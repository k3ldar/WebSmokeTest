using System;

namespace SmokeTest.Shared
{
    public enum FormStatus
    {
        /// <summary>
        /// Form has been found and added to the list ready to process
        /// </summary>
        New,

        /// <summary>
        /// Form method was not GET or POST
        /// </summary>
        UnrecognisedMethod,

        /// <summary>
        /// An unexpected error occurred whilst processing the form
        /// </summary>
        Error,

        /// <summary>
        /// The form will not be processed as corresponding values have not been configured.
        /// </summary>
        NotProcessing,
    }

    public enum PageType
    {
        WebPage = 1,

        WebImage = 2
    }

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
