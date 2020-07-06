using System;
using System.Collections.Generic;
using System.Net;

using SharedPluginFeatures;

using SmokeTest.Shared;
using SmokeTest.Shared.Engine;

namespace SmokeTest.Reports.Models
{
    public class TestSummaryModel : BaseModel
    {
        public TestSummaryModel(in BaseModelData modelData, in long reportId, in long testSchedule,
            in string configurationId, in bool isUserLoggedIn,
            in DateTime startTime, in DateTime endTime, in LastRunResult runResult,
            in uint totalRequests, in decimal slowest, in decimal fastest,
            in decimal average, in decimal trimmedAverage, in decimal totalTime,
            in Dictionary<string, string> headers, in List<ErrorDataModel> errors,
            in List<Cookie> cookies, in List<FormAnalysis> forms,
            in List<ImageReport> images, in List<PageReport> pages,
            in int minimumLoadTime, in bool siteScan,
            in List<TestResult> testResults, in HashSet<string> disabledTests)
            : base(modelData)
        {
            if (String.IsNullOrEmpty(configurationId))
                throw new ArgumentNullException(nameof(configurationId));

            ReportId = reportId;
            ConfigurationId = configurationId;
            IsUserLoggedIn = isUserLoggedIn;
            TestSchedule = testSchedule;
            StartTime = startTime;
            EndTime = endTime;
            RunResult = runResult;
            TotalRequests = totalRequests;
            TotalSlowest = slowest;
            TotalFastest = fastest;
            TotalAverage = average;
            TrimmedAverage = trimmedAverage;
            TotalTime = totalTime;
            Cookies = cookies ?? throw new ArgumentNullException(nameof(cookies));
            Errors = errors ?? throw new ArgumentNullException(nameof(errors));
            Forms = forms ?? throw new ArgumentNullException(nameof(forms));
            Headers = headers ?? new Dictionary<string, string>();
            Images = images ?? throw new ArgumentNullException(nameof(images));
            Pages = pages ?? throw new ArgumentNullException(nameof(pages));
            DisabledTests = disabledTests ?? throw new ArgumentNullException(nameof(disabledTests));
            TestResults = testResults;
            MinimumLoadTime = minimumLoadTime;
            SiteScan = siteScan;
        }

        #region Properties

        public long ReportId { get; private set; }

        public string ConfigurationId { get; private set; }

        public DateTime StartTime { get; private set; }

        public DateTime EndTime { get; private set; }

        public LastRunResult RunResult { get; private set; }

        public long TestSchedule { get; private set; }

        public uint TotalRequests { get; private set; }

        public decimal TotalSlowest { get; private set; }

        public decimal TotalFastest { get; private set; }

        public decimal TotalAverage { get; private set; }

        public decimal TrimmedAverage { get; private set; }

        public decimal TotalTime { get; private set; }

        public int MinimumLoadTime { get; private set; }

        public bool SiteScan { get; private set; }

        public List<Cookie> Cookies { get; set; }

        public List<ErrorDataModel> Errors { get; private set; }

        public List<TestResult> TestResults { get; private set; }

        public List<FormAnalysis> Forms { get; set; }

        public Dictionary<string, string> Headers;

        public List<ImageReport> Images { get; set; }

        public List<PageReport> Pages { get; set; }

        public HashSet<string> DisabledTests { get; private set; }

        public bool IsUserLoggedIn { get; private set; }

        #endregion Properties
    }
}
