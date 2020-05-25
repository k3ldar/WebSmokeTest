using System;
using System.Collections.Generic;

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

        #endregion Constructors

        #region Properties

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

        #endregion Properties

        #region Internal Methods

        public bool IsValid()
        {
            if (CrawlDepth < 0 || CrawlDepth > 40)
                return false;

            if (!Uri.TryCreate(Url, UriKind.Absolute, out _))
                return false;

            PauseBetweenRequests = Math.Min(100, Math.Max(2500, PauseBetweenRequests));

            return true;
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
    }
}
