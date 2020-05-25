using System;

namespace SmokeTest.Shared
{
    public sealed class ReportSummary
    {
        #region Constructors

        public ReportSummary(in long testId, in DateTime startTime, DateTime endTime,
            LastRunResult runResult, uint totalRequests)
        {
            TestId = testId;
            StartTime = startTime;
            EndTime = endTime;
            RunResult = runResult;
            TotalRequests = totalRequests;
            TimeSpan totalTime = EndTime - startTime;
            TotalTime = totalTime.TotalSeconds;
        }

        #endregion Constructors

        #region Properties

        public long TestId { get; private set; }

        public DateTime StartTime { get; private set; }

        public DateTime EndTime { get; private set; }

        public LastRunResult RunResult { get; private set; }

        public uint TotalRequests { get; private set; }

        public double TotalTime { get; private set; }

        public double TimePercentage { get; set; }

        #endregion Properties
    }
}
