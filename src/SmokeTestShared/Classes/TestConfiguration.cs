using System;
using System.Collections.Generic;

using Shared.Classes;

namespace SmokeTest.Shared
{
    public sealed class TestConfiguration
    {
        #region Constructors

        public TestConfiguration()
        {
            QueueData = new List<TestItem>();
        }

        public TestConfiguration(in string name, in string url, in int crawlDepth, in int maxPages,
            in int millisecondsBetweenRequest, in string userAgent, in string uniqueId, in bool checkImages,
            in bool clearHtmlData, in bool clearImageData, in List<string> additionalUrls, in NVPCodec headers)
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

        public List<TestItem> QueueData { get; set; }

        #endregion Properties

        #region Public Methods

        public void AddQueueData(in TestItem queueItem)
        {
            if (queueItem == null)
                throw new ArgumentNullException(nameof(queueItem));

            QueueData.Add(queueItem);
        }

        #endregion Public Methods
    }
}
