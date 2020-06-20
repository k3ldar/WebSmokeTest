using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

using PluginManager.Abstractions;

using Shared;
using Shared.Classes;

using SmokeTest.Engine;
using SmokeTest.Internal;
using SmokeTest.Shared;
using SmokeTest.Shared.Classes;
using SmokeTest.Shared.Engine;


namespace SmokeTest.Middleware
{
    public class TestRunManager : ITestRunManager
    {
        #region Constants


        #endregion Constants

        #region Private Members

        private static readonly object _lockObject = new object();
        private static readonly List<TestQueueItem> _scheduledTests = new List<TestQueueItem>();
        private static readonly List<TestRunItem> _activeTestRuns = new List<TestRunItem>();
        private readonly ILogger _logger;
        private readonly IScheduleHelper _scheduleHelper;
        private readonly ITestConfigurationProvider _testConfigurationProvider;
        private readonly IReportHelper _reportHelper;
        private readonly int _maxTestRuns;
        private readonly ISaveData _saveData;
        private readonly string _dataPath;
        private readonly IIdManager _idManager;

        #endregion Private Members

        #region Constructors

        public TestRunManager(ILogger logger, ISettingsProvider settingsProvider, ISaveData saveData,
            IScheduleHelper scheduleHelper, ITestConfigurationProvider configurationProvider,
            IReportHelper reportHelper, IIdManager idManager)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _saveData = saveData ?? throw new ArgumentNullException(nameof(saveData));
            _scheduleHelper = scheduleHelper ?? throw new ArgumentNullException(nameof(scheduleHelper));
            _testConfigurationProvider = configurationProvider ?? throw new ArgumentNullException(nameof(configurationProvider));
            _reportHelper = reportHelper ?? throw new ArgumentNullException(nameof(reportHelper));
            _idManager = idManager ?? throw new ArgumentNullException(nameof(idManager));

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

                if (_activeTestRuns.Count < _maxTestRuns && _scheduledTests.Count > 0)
                {
                    TestQueueItem testQueueItem = _scheduledTests[0];
                    _scheduledTests.RemoveAt(0);

                    for (int i = 0; i < _scheduledTests.Count; i++)
                        _scheduledTests[i].Position = i + 1;

                    testQueueItem.End = DateTime.UtcNow.Ticks;
                    long uniqueId = _idManager.GenerateId();
                    ITestRunLogger testRunLogger = new TestRunLogger(uniqueId);
                    ThreadWebsiteScan websiteScan = new ThreadWebsiteScan(testQueueItem.SmokeTestProperties,
                        uniqueId, testRunLogger);
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

        public void CancelAll()
        {

        }

        public TestQueueItem[] QueuePositions(in long testId)
        {
            using (TimedLock timedLock = TimedLock.Lock(_lockObject))
            {
                List<TestQueueItem> Result = new List<TestQueueItem>();

                foreach (TestQueueItem item in _scheduledTests)
                {
                    if (item.TestId.Equals(testId))
                    {
                        Result.Add(item);
                    }
                }

                return Result.ToArray();
            }
        }

        public bool CancelQueuedItem(in long queueId)
        {
            using (TimedLock timedLock = TimedLock.Lock(_lockObject))
            {
                foreach (TestQueueItem item in _scheduledTests)
                {
                    if (item.QueueId.Equals(queueId))
                    {
                        _scheduledTests.Remove(item);

                        for (int i = 0; i < _scheduledTests.Count; i++)
                            _scheduledTests[i].Position = i + 1;

                        return true;
                    }
                }

                return false;
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
            using (TimedLock timedLock = TimedLock.Lock(_lockObject))
            {
                if (testSchedule == null)
                {
                    throw new ArgumentNullException(nameof(testSchedule));
                }

                AddTestScheduleToQueue(testSchedule, false);
            }
        }

        public bool TestRunning(in long testId)
        {
            foreach (TestRunItem testRunItem in _activeTestRuns)
            {
                if (testRunItem.UniqueId.Equals(testId))
                    return true;
            }

            return false;
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

        public TestQueueItem[] QueuedTests
        {
            get
            {
                using (TimedLock timedLock = TimedLock.Lock(_lockObject))
                {
                    List<TestQueueItem> Result = new List<TestQueueItem>();

                    foreach (TestQueueItem item in _scheduledTests)
                        Result.Add(new TestQueueItem(item.QueueId, item.SmokeTestProperties, item.Test));

                    return Result.ToArray();
                }
            }
        }


        #endregion ITestRunManager Properties

        #region Private Properties


        #endregion Private Properties

        #region Private Methods

        private void AddTestScheduleToQueue(TestSchedule testSchedule, bool updateLastRun)
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
                SiteId = testSchedule.TestId,
                EncryptionKey = testSchedule.EncryptionKey,
                MinimumLoadTime = configuration.MinimumLoadTime,
            };

            NVPCodec headers = new NVPCodec();
            headers.Decode(configuration.Headers);

            foreach (string header in headers.AllKeys)
            {
                smokeTestProperties.Headers.Add(header, headers[header]);
            }

            if (updateLastRun)
            {
                testSchedule.LastRun = DateTime.UtcNow;
                testSchedule.CalculateNextRun();
                _scheduleHelper.Update(testSchedule);
            }

            TestQueueItem newQueueItem = new TestQueueItem(_idManager.GenerateId(), smokeTestProperties, testSchedule);
            _scheduledTests.Add(newQueueItem);
            newQueueItem.Position = _scheduledTests.Count;
        }

        private void PrepareAutomaticTestsForRunning()
        {
            List<TestSchedule> dueSchedules = _scheduleHelper.Schedules.Where(s => s.Enabled && DateTime.UtcNow >= s.NextRun).ToList();

            foreach (TestSchedule testSchedule in dueSchedules)
            {
                AddTestScheduleToQueue(testSchedule, true);
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

                if (threadWebsiteScan.Report.Pages.Count == 0 &&
                    threadWebsiteScan.Report.Errors.Count == 1 &&
                    threadWebsiteScan.Report.Errors[0].Error.Message.Contains("timed out"))
                {
                    result = LastRunResult.Warning;
                }
                else if (threadWebsiteScan.Report.Pages.Count == 0 && threadWebsiteScan.Report.Errors.Count > 0)
                {
                    result = LastRunResult.Error;
                }
                else if (threadWebsiteScan.Report.Errors.Count > 0)
                {
                    int timeOutErrors = threadWebsiteScan.Report.Errors.Where(e => e.Error.Message.Equals("The operation has timed out.")).Count();

                    if (threadWebsiteScan.Report.Errors.Count == timeOutErrors)
                    {
                        result = LastRunResult.Warning;
                    }
                    else
                    {
                        result = LastRunResult.Error;
                    }
                }
                else
                {
                    if (threadWebsiteScan.Report.Pages.Where(p => p.LoadTime > threadWebsiteScan.Report.MinimumLoadTime).Any() ||
                        threadWebsiteScan.Report.TestResults.Where(tr => tr.TimeTaken > threadWebsiteScan.Report.MinimumLoadTime).Any())
                    {
                        result = LastRunResult.Warning;
                    }
                    else
                    {
                        result = LastRunResult.Success;
                    }
                }

                threadWebsiteScan.Report.RunResult = result;
                threadWebsiteScan.Report.TestSchedule = currentTestRun.TestId;

                currentTestRun.Test.LastRunResult = result;
                _scheduleHelper.Update(currentTestRun.Test);
                threadWebsiteScan.Report.UniqueId = threadWebsiteScan.UniqueId;
                _reportHelper.AddReport(threadWebsiteScan.Report);
                _saveData.Save<Report>(threadWebsiteScan.Report,
                    Path.Combine(_dataPath, currentTestRun.TestId.ToString("X")),
                    $"{threadWebsiteScan.UniqueId.ToString("X")}.rpt");

                _activeTestRuns.Remove(currentTestRun);
            }
        }


        #endregion Private Methods
    }
}
