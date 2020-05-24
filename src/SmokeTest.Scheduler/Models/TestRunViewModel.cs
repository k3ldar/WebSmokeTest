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
            in DateTime? lastRun, 
            in int[] queuePositions, 
            in TestItem[] uniqueRunIdentifiers)
        {
            if (String.IsNullOrEmpty(testName))
                throw new ArgumentNullException(nameof(testName));

            if (String.IsNullOrEmpty(configurationName))
                throw new ArgumentNullException(nameof(configurationName));

            TestName = testName;
            ConfigurationName = configurationName;
            LastRun = lastRun;
            UniqueId = uniqueId;
            LastRunResult = lastRunResult;
            QueuePositions = queuePositions;
            UniqueRunIdentifiers = uniqueRunIdentifiers;
        }

        #endregion Constructors

        #region Properties

        public string TestName { get; private set; }

        public string ConfigurationName { get; private set; }

        public DateTime? LastRun { get; private set; }

        public LastRunResult LastRunResult { get; private set; }

        public TestItem[] UniqueRunIdentifiers { get; private set; }

        public int[] QueuePositions { get; private set; }

        public long UniqueId { get; private set; }

        #endregion Properties
    }
}
