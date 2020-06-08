using System;

using SmokeTest.Shared;

namespace SmokeTest.Scheduler.Models
{
    public sealed class TestRunViewModel
    {
        #region Constructors

        public TestRunViewModel(in string testName,
            in string configurationName,
            in long uniqueId,
            in LastRunResult lastRunResult,
            in DateTime nextRun,
            in DateTime? lastRun,
            in TestItem[] queuePositions,
            in TestItem[] uniqueRunIdentifiers,
            in ReportSummary[] reportSummaries)
        {
            if (String.IsNullOrEmpty(testName))
                throw new ArgumentNullException(nameof(testName));

            if (String.IsNullOrEmpty(configurationName))
                throw new ArgumentNullException(nameof(configurationName));

            TestName = testName;
            ConfigurationName = configurationName;
            NextRun = nextRun;
            LastRun = lastRun;
            UniqueId = uniqueId;
            LastRunResult = lastRunResult;
            QueuePositions = queuePositions;
            UniqueRunIdentifiers = uniqueRunIdentifiers;
            ReportSummaries = reportSummaries;
        }

        #endregion Constructors

        #region Properties

        public string TestName { get; private set; }

        public string ConfigurationName { get; private set; }

        public DateTime NextRun { get; private set; }

        public DateTime? LastRun { get; private set; }

        public LastRunResult LastRunResult { get; private set; }

        public TestItem[] UniqueRunIdentifiers { get; private set; }

        public TestItem[] QueuePositions { get; private set; }

        public long UniqueId { get; private set; }

        public ReportSummary[] ReportSummaries { get; private set; }

        #endregion Properties
    }
}
