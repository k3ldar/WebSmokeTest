using System;
using System.Collections.Generic;

using SharedPluginFeatures;

namespace SmokeTest.Shared.Engine
{
    public sealed class SmokeTestProperties
    {
        #region Private Members

        private string _emailTitle;
        private string _userAgent;
        private string _cookiePath;
        private string _url;

        #endregion Private Members

        #region Constructors

        public SmokeTestProperties()
        {
            Headers = new Dictionary<string, string>();
            CrawlDepth = 10;
            EmailTitle = "WebMonitor Results";
            UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1; .NET CLR 2.0.50727) SmokeTest/v1.0";
            SessionCookieName = "SessionCookie";
            PauseBetweenRequests = 0;

            CookiePath = "/";
            MaximumPages = 10;

            SendEmailsErrorOnly = true;
            SendEmails = false;

            CheckImages = true;
            ClearImageDataAfterAnalysis = true;
            ClearHtmlDataAfterAnalysis = true;
        }

        public SmokeTestProperties(TestConfiguration testConfiguration, string testScheduleId)
            : this()
        {
            TestConfiguration = testConfiguration ?? throw new ArgumentNullException();
            CheckImages = testConfiguration.CheckImages;
            ClearHtmlDataAfterAnalysis = testConfiguration.ClearHtmlData;
            ClearImageDataAfterAnalysis = testConfiguration.ClearImageData;
            CrawlDepth = testConfiguration.CrawlDepth;
            MaximumPages = testConfiguration.MaximumPages;
            PauseBetweenRequests = testConfiguration.MillisecondsBetweenRequests;
            UserAgent = testConfiguration.UserAgent;
            Url = testConfiguration.Url;
            TestScheduleId = testScheduleId;
            EncryptionKey = testConfiguration.EncryptionKey;
            MinimumLoadTime = testConfiguration.MinimumLoadTime;
            SiteScan = testConfiguration.SiteScan;
            DisabledTests = testConfiguration.DisabledTests;
        }

        #endregion Constructors

        #region Properties

        public TestConfiguration TestConfiguration { get; set; }

        /// <summary>
        /// Depth of checks into the website
        /// </summary>
        public int CrawlDepth { get; set; }

        /// <summary>
        /// URL of website to check, i.e. http://www.sieradelta.com
        /// </summary>
        /// <value>string</value>
        public string Url
        {
            get
            {
                return _url;
            }

            set
            {
                if (Uri.TryCreate(value, UriKind.Absolute, out Uri result))
                {
                    _url = result.ToString();
                }
            }
        }

        /// <summary>
        /// Determines wether to send emails to confirm results
        /// </summary>
        public bool SendEmails { get; set; }

        /// <summary>
        /// Specifies wether emails are sent only when an error is found
        /// </summary>
        public bool SendEmailsErrorOnly { get; set; }

        /// <summary>
        /// Determines wether to check website images for errors
        /// </summary>
        public bool CheckImages { get; set; }

        /// <summary>
        /// DateTime the website was last checked for errors
        /// </summary>
        public DateTime LastVerified { get; set; }

        /// <summary>
        /// Email Address where email will be sent to
        /// </summary>
        /// <value>string</value>
        public string EmailAddress { get; set; }

        /// <summary>
        /// Name of recipient for email
        /// </summary>
        /// <value>string</value>
        public string EmailName { get; set; }

        /// <summary>
        /// Title of Email
        /// </summary>
        /// <value>string</value>
        public string EmailTitle
        {
            get
            {
                return _emailTitle;
            }

            set
            {
                if (!String.IsNullOrEmpty(value))
                    _emailTitle = value;
            }
        }

        public string SessionCookieName { get; set; }

        /// <summary>
        /// Additional Cookie Name to be sent to website
        /// </summary>
        /// <value>string</value>
        public string CookieName { get; set; }

        /// <summary>
        /// Additional Cookie Value to be sent to website
        /// </summary>
        /// <value>string</value>
        public string CookieValue { get; set; }

        /// <summary>
        /// Additional Cookie Path
        /// </summary>
        /// <value>string</value>
        public string CookiePath
        {
            get
            {
                return _cookiePath;
            }

            set
            {
                if (!String.IsNullOrEmpty(value))
                    _cookiePath = value;
            }
        }

        /// <summary>
        /// Additional Cookie Domain
        /// </summary>
        /// <value>string</value>
        public string CookieDomain { get; set; }

        /// <summary>
        /// Useragent to be sent to website
        /// </summary>
        /// <value>string</value>
        public string UserAgent
        {
            get
            {
                return _userAgent;
            }

            set
            {
                if (!String.IsNullOrEmpty(value))
                    _userAgent = value;
            }
        }

        /// <summary>
        /// Number of seconds to pause between each request sent to a website
        /// </summary>
        /// <value>int</value>
        public int PauseBetweenRequests { get; set; }

        /// <summary>
        /// Additional parameter to be added to each web request
        /// </summary>
        /// <value>string</value>
        public string Parameter { get; set; }

        /// <summary>
        /// Maximum number of pages to scan
        /// </summary>
        /// <value>int</value>
        public int MaximumPages { get; set; }

        /// <summary>
        /// Additional header values that will be sent with each request
        /// </summary>
        /// <value>Dictinary&lt;string, string&gt;</value>
        public Dictionary<string, string> Headers { get; set; }

        /// <summary>
        /// If true, clears the image data and replaces it with a checksum that can be used in comparison
        /// </summary>
        /// <value>bool</value>
        public bool ClearImageDataAfterAnalysis { get; set; }

        /// <summary>
        /// If true, clears the html data and replaces it with a checksum that can be used in comparison
        /// </summary>
        /// <value>bool</value>
        public bool ClearHtmlDataAfterAnalysis { get; set; }

        /// <summary>
        /// Unique Id of the site being checked
        /// </summary>
        public string TestScheduleId { get; set; }

        /// <summary>
        /// Key used to decrypt data when retrieved using test discovery
        /// </summary>
        public string EncryptionKey { get; set; }

        /// <summary>
        /// Minimum load time for a page, if this time is exceeded then a warning is shown
        /// </summary>
        public int MinimumLoadTime { get; set; }

        /// <summary>
        /// Determines whether a site scan will be completed or not
        /// </summary>
        public bool SiteScan { get; set; }


        public HashSet<string> DisabledTests { get; set; }

        #endregion Properties

        #region Internal Methods

        public bool IsValid()
        {
            if (CrawlDepth < 0 || CrawlDepth > 40)
                return false;

            if (!Uri.TryCreate(Url, UriKind.Absolute, out _))
                return false;

            if (PauseBetweenRequests < 100)
                PauseBetweenRequests = 100;
            else if (PauseBetweenRequests > 2500)
                PauseBetweenRequests = 2500;

            return true;
        }

        public bool IsTestEnabled(in WebSmokeTestItem test)
        {
            if (test == null)
                throw new ArgumentNullException(nameof(test));

            string testName = Report.GenerateTestHash(test);

            return !DisabledTests.Contains(testName);
        }

        public bool ContainsFormReport(FormReport form)
        {
            if (form == null)
            {
                throw new ArgumentNullException(nameof(form));
            }

            return false;
        }

        public FormReport GetFormReport(FormReport form)
        {
            if (form == null)
            {
                throw new ArgumentNullException(nameof(form));
            }

            return null;
        }


        #endregion Internal Methods

        private string TestHashCode(in string s)
        {
            return s.GetHashCode().ToString();
        }
    }
}
