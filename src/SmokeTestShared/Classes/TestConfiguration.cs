using System;
using System.Collections.Generic;

using Newtonsoft.Json;

using Shared.Classes;

using SharedPluginFeatures;
using SmokeTest.Shared.Engine;

namespace SmokeTest.Shared
{
    public sealed class TestConfiguration
    {
        #region Constructors

        public TestConfiguration()
        {
            DisabledTests = new HashSet<string>();
            Tests = new List<WebSmokeTestItem>();
            DiscoveredTests = new List<WebSmokeTestItem>();
            MinimumLoadTime = 500;
            SiteScan = true;
        }

        public TestConfiguration(in string name, in string url, in int crawlDepth, in int maxPages,
            in int millisecondsBetweenRequest, in string userAgent, in string uniqueId, in bool checkImages,
            in bool clearHtmlData, in bool clearImageData, in int minimumLoadTime, in bool scanSite,
            in string encryptionKey,
            in List<string> additionalUrls, in NVPCodec headers)
            : this()
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            if (String.IsNullOrEmpty(url))
                throw new ArgumentNullException(nameof(url));

            if (maxPages < -1 || maxPages == 0)
                throw new ArgumentOutOfRangeException(nameof(maxPages));

            if (millisecondsBetweenRequest < 50 || millisecondsBetweenRequest > 2500)
                throw new ArgumentOutOfRangeException(nameof(millisecondsBetweenRequest));

            if (String.IsNullOrEmpty(userAgent))
                throw new ArgumentNullException(nameof(userAgent));

            if (userAgent.Length < 10 || userAgent.Length > 150)
                throw new ArgumentOutOfRangeException(nameof(userAgent));

            if (headers == null)
                throw new ArgumentNullException(nameof(headers));

            Name = name;
            Url = url;
            CrawlDepth = crawlDepth;
            MaximumPages = maxPages;
            MillisecondsBetweenRequests = millisecondsBetweenRequest;
            UserAgent = userAgent;
            UniqueId = uniqueId;
            CheckImages = checkImages;
            ClearHtmlData = clearHtmlData;
            ClearImageData = clearImageData;
            AdditionalUrls = String.Join(";", additionalUrls.ToArray());
            Headers = headers.Encode();
            MinimumLoadTime = minimumLoadTime;
            SiteScan = scanSite;
            EncryptionKey = encryptionKey;
        }

        #endregion Constructors

        #region Properties

        public string Name { get; set; }

        public string Url { get; set; }

        public int CrawlDepth { get; set; }

        public bool CheckImages { get; set; }

        public string UserAgent { get; set; }

        public int MillisecondsBetweenRequests { get; set; }

        public int MaximumPages { get; set; }

        public string Headers { get; set; }

        public string AdditionalUrls { get; set; }

        public bool ClearImageData { get; set; }

        public bool ClearHtmlData { get; set; }

        public string UniqueId { get; set; }

        public int MinimumLoadTime { get; set; }

        public bool SiteScan { get; set; }

        public string EncryptionKey { get; set; }

        public List<WebSmokeTestItem> Tests { get; set; }

        public List<WebSmokeTestItem> DiscoveredTests { get; set; }

        public HashSet<string> DisabledTests { get; set; }

        #endregion Properties

        #region Public Methods

        public void EnableTest(in WebSmokeTestItem test)
        {
            if (test == null)
                throw new ArgumentNullException(nameof(test));

            string testName = Report.GenerateTestHash(test);

            if (DisabledTests.Contains(testName))
            {
                DisabledTests.Remove(testName);
            }
        }

        public void DisableTest(in WebSmokeTestItem test)
        {
            if (test == null)
                throw new ArgumentNullException(nameof(test));

            string testName = Report.GenerateTestHash(test);

            if (!DisabledTests.Contains(testName))
            {
                DisabledTests.Add(testName);
            }
        }

        #endregion Public Methods
    }
}
