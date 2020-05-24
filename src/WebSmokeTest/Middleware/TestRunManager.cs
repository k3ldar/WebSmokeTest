using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using PluginManager.Abstractions;

using Shared;
using Shared.Classes;

using SmokeTest.Classes;
using SmokeTest.Engine;
using SmokeTest.Shared;
using SmokeTest.Shared.Classes;

namespace SmokeTest.Middleware
{
    public class TestRunManager : ITestRunManager
    {
        #region Constants


        #endregion Constants

        #region Private Members

        private static readonly object _lockObject = new object();
        private static readonly Queue<TestQueueItem> _scheduledTests = new Queue<TestQueueItem>();
        private static readonly List<TestRunItem> _activeTestRuns = new List<TestRunItem>();
        private readonly ILogger _logger;
        private readonly IScheduleHelper _scheduleHelper;
        private readonly ITestConfigurationProvider _testConfigurationProvider;
        private readonly int _maxTestRuns;
        private readonly ISaveData _saveData;
        private readonly string _dataPath;

        #endregion Private Members

        #region Constructors

        public TestRunManager(ILogger logger, ISettingsProvider settingsProvider, ISaveData saveData,
            IScheduleHelper scheduleHelper, ITestConfigurationProvider configurationProvider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _saveData = saveData ?? throw new ArgumentNullException(nameof(saveData));
            _scheduleHelper = scheduleHelper ?? throw new ArgumentNullException(nameof(scheduleHelper));
            _testConfigurationProvider = configurationProvider ?? throw new ArgumentNullException(nameof(configurationProvider));

            if (settingsProvider == null)
                throw new ArgumentNullException(nameof(settingsProvider));


            TestRunManagerSettings settings = settingsProvider.GetSettings<TestRunManagerSettings>(nameof(TestRunManager));
            _maxTestRuns = Utilities.CheckMinMax(settings.MaximumTestRuns, 2, 10);
            _dataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "SmokeTest", "Reports");

        }

        #endregion Constructors

        #region ITestRunManager Methods

        public void ProcessTests()
        {
            using (TimedLock timedLock = TimedLock.Lock(_lockObject))
            {
                PrepareAutomaticTestsForRunning();

                if (_activeTestRuns.Count < _maxTestRuns)
                {
                    if (_scheduledTests.TryDequeue(out TestQueueItem testQueueItem))
                    {
                        testQueueItem.End = DateTime.Now.Ticks;
                        ThreadWebsiteScan websiteScan = new ThreadWebsiteScan(testQueueItem.SmokeTestProperties, DateTime.Now.Ticks);
                        websiteScan.ThreadFinishing += WebsiteScan_ThreadFinishing;
                        TestRunItem testRunItem = new TestRunItem(websiteScan, testQueueItem.Test);

                        _activeTestRuns.Add(testRunItem);

                        ThreadManager.ThreadStart(websiteScan, $"Smoke Test: {testQueueItem.SmokeTestProperties.Url}", ThreadPriority.Lowest);

                        //TestSchedule configuration = _scheduleHelper.Schedules
                        //    .Where(tc => tc.UniqueId.Equals(testQueueItem.TestId)).FirstOrDefault();

                        //if (configuration != null)
                        //{
                        //    //configuration.AddQueueData(testQueueItem);
                        //    //_testConfigurationProvider.SaveConfiguration(configuration);
                        //}

                    }
                }
            }
        }

        public void CancelAll()
        {

        }

        public int[] QueuePositions(in long testId)
        {
            using (TimedLock timedLock = TimedLock.Lock(_lockObject))
            {
                List<int> Result = new List<int>();
                int i = 0;

                foreach (TestItem item in _scheduledTests)
                {
                    if (item.TestId.Equals(testId))
                    {
                        Result.Add(i);
                    }

                    i++;
                }

                return Result.ToArray();
            }
        }

        public TestItem[] ActiveTests(in long testId)
        {
            using (TimedLock timedLock = TimedLock.Lock(_lockObject))
            {
                List<TestItem> Result = new List<TestItem>();
                long id = testId;

                foreach (TestRunItem item in _activeTestRuns.Where(at => at.TestId.Equals(id)).ToList()) 
                {
                    Result.Add(new TestItem(item.UniqueId, item.TestId, item.Start, 0));
                }

                return Result.ToArray();
            }
        }

        public void RunTest(in TestSchedule testSchedule)
        {
            if (testSchedule == null)
            {
                throw new ArgumentNullException(nameof(testSchedule));
            }

            AddTestScheduleToQueue(testSchedule);
        }

        #endregion ITestRunManager Methods


        #region ITestRunManager Properties

        public TestItem[] RunningTests
        {
            get
            {
                using (TimedLock timedLock = TimedLock.Lock(_lockObject))
                {
                    List<TestItem> Result = new List<TestItem>();

                    foreach (TestRunItem item in _activeTestRuns)
                    {
                        Result.Add(new TestItem(item.UniqueId, item.TestId, item.Start, 0));
                    }

                    return Result.ToArray();
                }
            }
        }

        public TestItem[] QueuedTests
        {
            get
            {
                using (TimedLock timedLock = TimedLock.Lock(_lockObject))
                {
                    List<TestItem> Result = new List<TestItem>();

                    foreach (TestItem item in _scheduledTests)
                        Result.Add(new TestItem(item.TestId, item.Start, item.End));

                    return Result.ToArray();
                }
            }
        }


        #endregion ITestRunManager Properties

        #region Private Properties


        #endregion Private Properties

        #region Private Methods

        private void AddTestScheduleToQueue(TestSchedule testSchedule)
        {
            TestConfiguration configuration = _testConfigurationProvider.Configurations
                .Where(tc => tc.UniqueId.Equals(testSchedule.TestId)).FirstOrDefault();

            if (configuration == null)
            {
                return;
            }

            SmokeTestProperties smokeTestProperties = new SmokeTestProperties()
            {
                CheckImages = configuration.CheckImages,
                ClearHtmlDataAfterAnalysis = configuration.ClearHtmlData,
                ClearImageDataAfterAnalysis = configuration.ClearImageData,
                CrawlDepth = configuration.CrawlDepth,
                MaximumPages = configuration.MaximumPages,
                PauseBetweenRequests = configuration.MillisecondsBetweenRequests,
                UserAgent = configuration.UserAgent,
                Url = configuration.Url,
            };

            NVPCodec headers = new NVPCodec();
            headers.Decode(configuration.Headers);

            foreach (string header in headers.AllKeys)
            {
                smokeTestProperties.Headers.Add(header, headers[header]);
            }

            testSchedule.LastRun = new DateTime(DateTime.Now.Ticks - testSchedule.NextRun().Ticks);
            _scheduleHelper.Update(testSchedule);
            _scheduledTests.Enqueue(new TestQueueItem(smokeTestProperties, testSchedule));
        }
        private void PrepareAutomaticTestsForRunning()
        {
            List<TestSchedule> dueSchedules = _scheduleHelper.Schedules.Where(s => s.Enabled && s.NextRun().TotalSeconds > 0).ToList();

            foreach (TestSchedule testSchedule in dueSchedules)
            {
                AddTestScheduleToQueue(testSchedule);
            }
        }

        private void WebsiteScan_ThreadFinishing(object sender, ThreadManagerEventArgs e)
        {
            using (TimedLock timedLock = TimedLock.Lock(_lockObject))
            {
                ThreadWebsiteScan threadWebsiteScan = e.Thread as ThreadWebsiteScan;

                if (threadWebsiteScan == null)
                    return;

                TestRunItem currentTestRun = _activeTestRuns.Where(tr => tr.WebsiteScan.UniqueId == threadWebsiteScan.UniqueId).FirstOrDefault();

                if (currentTestRun == null)
                    return;

                LastRunResult result = LastRunResult.NotRun;

                if (threadWebsiteScan.Report.Pages.Count == 0 && threadWebsiteScan.Report.Errors.Count > 0)
                    result = LastRunResult.Error;
                else if (threadWebsiteScan.Report.Errors.Count > 0)
                    result = LastRunResult.Warning;
                else
                    result = LastRunResult.Success;

                currentTestRun.Test.LastRunResult = result;
                _scheduleHelper.Update(currentTestRun.Test);
                _saveData.Save<Report>(threadWebsiteScan.Report, 
                    Path.Combine(_dataPath, currentTestRun.TestId.ToString("X")), 
                    $"{threadWebsiteScan.UniqueId.ToString("X")}.rpt");

                _activeTestRuns.Remove(currentTestRun);
            }
        }


        #endregion Private Methods
    }


    internal class TestQueueItem : TestItem
    {
        internal TestQueueItem(in SmokeTestProperties smokeTestProperties, in TestSchedule testSchedule)
            : base()
        {
            SmokeTestProperties = smokeTestProperties ?? throw new ArgumentNullException(nameof(smokeTestProperties));
            Test = testSchedule ?? throw new ArgumentNullException(nameof(testSchedule));
            TestId = testSchedule.UniqueId;
            Start = DateTime.Now.Ticks;
        }

        internal SmokeTestProperties SmokeTestProperties { get; private set; }

        internal TestSchedule Test { get; private set; }
    }

    internal class TestRunItem : TestItem
    {
        internal TestRunItem(in ThreadWebsiteScan websiteScan, in TestSchedule testSchedule)
            : base()
        {
            WebsiteScan = websiteScan ?? throw new ArgumentNullException(nameof(websiteScan));
            Test = testSchedule ?? throw new ArgumentNullException(nameof(testSchedule));
            TestId = testSchedule.UniqueId;
            Start = DateTime.Now.Ticks;
        }

        internal ThreadWebsiteScan WebsiteScan { get; private set; }

        internal TestSchedule Test { get; private set; }
    }
}
