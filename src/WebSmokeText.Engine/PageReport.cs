using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

using Newtonsoft.Json;

using Shared.Classes;

namespace SmokeTest.Engine
{
    public sealed class PageReport
    {
        #region Private Members

        #endregion Private Members

        #region Constructors

        public PageReport()
        {
            Links = new List<string>();
            ExternalLinks = new List<string>();
            Images = new List<ImageReport>();
            Headers = new NVPCodec();
            AnalysisComplete = false;
            Analysis = new PageAnalysis();
            ProcessingComplete = false;
        }

        public PageReport(in string url, in decimal loadTime)
            : this()
        {
            if (String.IsNullOrEmpty(url))
                throw new ArgumentNullException(nameof(url));

            Url = url;
            LoadTime = loadTime;
        }

        public PageReport(in string url, in decimal loadTime, in string content)
            : this(url, loadTime)
        {
            Content = content ?? String.Empty;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Url of page being loaded
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Milliseconds to open the page 
        /// </summary>
        public decimal LoadTime { get; set; }

        /// <summary>
        /// Page content
        /// </summary>
        public string Content { get; set; }

        public List<string> Links { get; set; }

        public List<string> ExternalLinks { get; set; }

        public List<ImageReport> Images { get; set; }

        [JsonIgnore]
        public NVPCodec Headers { get; set; }

        public string NVPHeaders
        {
            get
            {
                return Headers.Encode();
            }

            set
            {
                Headers.Decode(value);
            }
        }

        #region Analysis

        public int Depth { get; set; }

        public int NodeCount { get; set; }

        public bool AnalysisComplete { get; set; }

        public PageAnalysis Analysis { get; set; }

        public bool DocumentTypeFound { get; set; }

        public bool HtmlNodeFound { get; set; }

        public bool HeadFound { get; set; }

        public bool BodyFound { get; set; }

        internal bool ProcessingComplete { get; set; }

        #endregion Analysis

        #endregion Properties

        #region Public Methods

        public void AddExternalLink(string link)
        {
            if (String.IsNullOrEmpty(link))
                throw new ArgumentNullException(nameof(link));

            if (ExternalLinks.Where(el => el.Equals(link, StringComparison.InvariantCultureIgnoreCase)).Any())
                return;

            ExternalLinks.Add(link);
        }

        public void AddPageLink(in string link)
        {
            if (String.IsNullOrEmpty(link))
                throw new ArgumentNullException(nameof(link));

            Links.Add(link);
        }

        public void AddPageImage(in ImageReport image)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));

            Images.Add(image);
        }

        public void AddHeaders(in WebHeaderCollection headers)
        {
            if (headers == null)
                throw new ArgumentNullException(nameof(headers));

            foreach (string item in headers)
            {
                Headers.Add(item, headers[item]);
            }
        }

        #endregion Public Methods

        #region Internal Methods

        internal void ClearAnalysisData()
        {
            Depth = 0;
            NodeCount = 0;
            AnalysisComplete = false;
            DocumentTypeFound = false;
            HtmlNodeFound = false;
            HeadFound = false;
            BodyFound = false;

            Analysis.Clear();
        }

        #endregion Internal Methods
    }
}
