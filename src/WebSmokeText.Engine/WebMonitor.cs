using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

using Shared.Classes;

using SharedPluginFeatures;

namespace WebSmokeTest.Engine
{
    public sealed class WebMonitor : IDisposable
    {
        #region Private Members

        private static readonly object _lockObject = new object();
        private readonly SmokeTestProperties _properties;
        private readonly MailAddressCollection _recipients;
        private WebClientEx _client;
        private bool _cancelScan = false;
        private readonly Timings _pageLoadTimings;

        private readonly Report _report;
        private readonly ThreadManager _parentThread;

        #endregion Private Members

        #region Constructors / Destructors

        public WebMonitor(in SmokeTestProperties properties)
            : this(properties, null)
        {

        }

        public WebMonitor(in SmokeTestProperties properties, in ThreadManager parentThread)
        {
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _parentThread = parentThread;

            _pageLoadTimings = new Timings();
            _report = new Report();

            if (!_properties.IsValid())
                throw new ArgumentException(nameof(properties));

            _recipients = new MailAddressCollection();
            _client = new WebClientEx();
            _client.UserAgent = properties.UserAgent;

            SessionCookieAdded = false;

            foreach (KeyValuePair<string, string> headers in properties.Headers)
            {
                _client.Headers.Add(headers.Key, headers.Value);
            }

            if (properties.SendEmails && !String.IsNullOrEmpty(properties.EmailName) &&
                !String.IsNullOrEmpty(properties.EmailAddress))
            {
                _recipients.Add(new MailAddress(properties.EmailAddress, properties.EmailName));
            }

            if (!String.IsNullOrEmpty(properties.CookieName) && !String.IsNullOrEmpty(properties.CookieValue) &&
                !String.IsNullOrEmpty(properties.CookiePath) && !String.IsNullOrEmpty(properties.CookieDomain))
            {
                _client.Cookies(new Uri(properties.Url)).Add(new Cookie(properties.CookieName, properties.CookieValue, properties.CookiePath,
                    properties.CookieDomain));
            }
        }

        #endregion Constructors / Destructors

        #region Properties

        private bool SessionCookieAdded { get; set; }

        public Report Report
        {
            get
            {
                return _report;
            }
        }

        /// <summary>
        /// Retrieves page load timing data
        /// </summary>
        /// <value>Timings</value>
        public Timings PageLoadTimings
        {
            get
            {
                return _pageLoadTimings;
            }
        }

        #endregion Properties

        #region Public Methods

        /// <summary>
        /// Initializes a web crawl
        /// </summary>
        /// <returns>bool, true if web crawl successful otherwise false</returns>
        public bool Run()
        {
            _report.Clear();

            List<string> linksParsed = new List<string>();
            try
            {
                if (!ParsePage(linksParsed, _properties.Url, new Uri(_properties.Url), 0, new Uri(_properties.Url)))
                    return false;
            }
            catch (Exception err)
            {
                RaiseError(err, null, null, null);
            }
            finally
            {
                linksParsed.Clear();
            }

            //foreach (Cookie cookie in _client.CookieContainer.GetCookies(new Uri(_properties.Url)))
            //{
            //    _client.CookieContainer.Add(cookie);
            //}


            if (_properties.ClearHtmlDataAfterAnalysis)
            {
                foreach (PageReport page in _report.Pages)
                {
                    page.Content = GenerateCheckSum(page.Content);

                    if (_properties.ClearImageDataAfterAnalysis)
                    {
                        foreach (ImageReport img in page.Images)
                        {
                            img.Bytes = GenerateCheckSum(img.Bytes);
                        }
                    }
                }
            }


            string errorDetail = GetErrorInformation();


            if (_report.Errors.Count > 0)
            {
                if (_report.Errors.Count > 0)
                {
                    SendEmail(String.Format("{0}\r\n{1}\r\n\r\n{2}", _properties.Url, "Website Checked, errors found",
                        errorDetail));
                }
                else
                {
                    if (!_properties.SendEmailsErrorOnly)
                        SendEmail(String.Format("{0}\r\n{1}", _properties.Url, "Website Checked, in order"));
                }
            }

            _report.SetTimings(_pageLoadTimings);

            DateTime timeoutStart = DateTime.Now;

            while (!_report.AnalysisComplete)
            {
                Thread.Sleep(100);

                TimeSpan span = DateTime.Now - timeoutStart;

                if (span.TotalSeconds > 30)
                    break;
            }

            // finally force a garbage collection
            GC.Collect(2, GCCollectionMode.Forced);


            return _report.Errors.Count == 0;
        }

        public void Cancel()
        {
            _cancelScan = true;
        }

        #endregion Public Methods

        #region Wrappers

        private void RaiseError(Exception error, Uri uri, Uri missingLink, Uri originatingLink)
        {
            ErrorData errorData = new ErrorData(error, uri, missingLink, originatingLink);

            _report.Errors.Add(errorData);
        }

        #endregion Wrappers

        #region Private Methods

        private string GenerateCheckSum(in string data)
        {
            return Convert.ToBase64String(GenerateCheckSum(Encoding.UTF8.GetBytes(data)));
        }

        private byte[] GenerateCheckSum(in byte[] data)
        {
            using (SHA256Managed sha = new SHA256Managed())
            {
                return sha.ComputeHash(data);
            }
        }

        /// <summary>
        /// Retrieves information on errors
        /// </summary>
        /// <returns>string of error information</returns>
        private string GetErrorInformation()
        {
            string Result = String.Empty;

            for (int i = 0; i < _report.Errors.Count; i++)
            {
                ErrorData item = _report.Errors[i];

                Result += String.Format("Error: {0}\r\nURI: {1}\r\nMissing Link: {2}\r\nError: {3}\r\nOriginating URI: {4}\r\n\r\n",
                    i, item.Uri.ToString(), item.MissingLink.ToString(), item.Error.Message, item.OriginatingLink.ToString());

                i++;
            }

            return Result;
        }

        /// <summary>
        /// Recursively parses all pages
        /// </summary>
        /// <param name="linksParsed"></param>
        /// <param name="client"></param>
        /// <param name="startsWith"></param>
        /// <param name="url"></param>
        /// <param name="depth"></param>
        /// <param name="originatingLink"></param>
        /// <returns>true if parsed, false if parse failed or cancelled</returns>
        private bool ParsePage(in List<string> linksParsed, string startsWith,
            Uri url, int depth, Uri originatingLink)
        {
            using (TimedLock.Lock(_lockObject))
            {
                if (linksParsed.Contains(url.ToString().ToLower()))
                    return true;

                if (!linksParsed.Contains(url.ToString().ToLower()))
                    linksParsed.Add(url.ToString().ToLower());
            }
            if (_report.Pages.Count >= _properties.MaximumPages)
                return true;

            if (_cancelScan)
                return false;

            if (depth > _properties.CrawlDepth)
                return true;

            Thread.Sleep(_properties.PauseBetweenRequests);

            Uri modifiedUri = ModifyUrl(url.ToString());

            string webData = null;
            Timings pageLoad = new Timings();
            using (StopWatchTimer pageLoadTimer = StopWatchTimer.Initialise(pageLoad))
            {
                try
                {
                    using (StopWatchTimer stopwatchTimer = StopWatchTimer.Initialise(_pageLoadTimings))
                    {
                        webData = _client.GetData(modifiedUri);
                    }
                }
                catch (WebException err)
                {
                    if (err.Message.Contains("(404)"))
                        LogError(err, modifiedUri, modifiedUri, originatingLink);
                    else
                        LogError(err, modifiedUri, null, originatingLink);
                }
                catch (Exception err)
                {
                    LogError(err, modifiedUri, null, originatingLink);
                }
            }

            PageReport pageReport = new PageReport(modifiedUri.ToString(), pageLoad.Total, webData);
            _report.PageAdd(pageReport, _parentThread);

            if (!SessionCookieAdded &&
                _client.ResponseCookies != null &&
                _client.ResponseCookies[_properties.SessionCookieName] != null)
            {
                _client.CookieContainer.Add(_client.ResponseCookies[_properties.SessionCookieName]);
                SessionCookieAdded = true;
            }

            foreach (Cookie cookie in _client.ResponseCookies)
            {
                _report.AddCookie(cookie);
            }

            pageReport.AddHeaders(_client.ResponseHeaders);

            if (webData == null)
                return true;

            pageReport.Content = webData ?? String.Empty;

            ProcessImages(linksParsed, pageReport, originatingLink, modifiedUri);

            ProcessForms(linksParsed, pageReport, originatingLink, modifiedUri);

            List<string> links = ParseHtml(url.ToString(), pageReport.Content);
            try
            {
                foreach (string link in links)
                {

                    if (link.ToLower().StartsWith(startsWith.ToLower()))
                    {
                        pageReport.AddPageLink(link);

                        if (!ParsePage(linksParsed, startsWith, new Uri(link), depth + 1, url))
                            return false;
                    }
                    else
                    {
                        pageReport.AddExternalLink(link);
                    }
                }
            }
            finally
            {
                links.Clear();
            }

            return true;
        }

        private Uri ModifyUrl(in string url)
        {
            string modifiedURL = url;

            if (!String.IsNullOrEmpty(_properties.Parameter))
            {
                if (url.Contains("?"))
                    modifiedURL += String.Format("&{0}", _properties.Parameter);
                else
                    modifiedURL += String.Format("?{0}", _properties.Parameter);
            }

            return new Uri(modifiedURL);
        }

        private void ProcessImages(in List<string> linksParsed, in PageReport pageReport,
            in Uri originatingLink, in Uri url)
        {

            if (_properties.CheckImages)
            {
                //Look at Images to see if they exist
                List<string> links = ParseHTMLImages(url.ToString(), pageReport.Content);
                try
                {
                    foreach (string imageLink in links)
                    {
                        ImageReport imageReport = new ImageReport(imageLink);
                        pageReport.AddPageImage(imageReport);

                        if (imageLink != "")
                        {
                            if (linksParsed.IndexOf(imageLink) == -1)
                            {
                                try
                                {
                                    DateTime startTimeImage = DateTime.Now;

                                    using (TimedLock.Lock(_lockObject))
                                    {
                                        if (!linksParsed.Contains(imageLink.ToLower()))
                                            linksParsed.Add(imageLink.ToLower());
                                    }

                                    _report.Images.Add(imageReport);
                                    imageReport.Bytes = _client.DownloadData(imageLink);

                                    TimeSpan span = DateTime.Now.Subtract(startTimeImage);
                                    imageReport.LoadTime = span.TotalMilliseconds / TimeSpan.TicksPerSecond;
                                }
                                catch (WebException err)
                                {
                                    if (err.Message.Contains("(404)"))
                                        LogError(err, url, new Uri(imageLink), originatingLink);
                                    else
                                        LogError(err, url, null, originatingLink);
                                }
                                catch (Exception err)
                                {
                                    LogError(err, url, new Uri(imageLink), originatingLink);
                                }
                            }
                        }
                    }
                }
                finally
                {
                    links.Clear();
                }
            }
        }

        private void ProcessForms(in List<string> linksParsed, in PageReport pageReport,
            in Uri originatingLink, in Uri url)
        {
            //Look at any forms, can we process those too
            List<FormReport> forms = ParseForms(url.ToString(), pageReport.Content);
            try
            {
                foreach (FormReport form in forms)
                {
                    if (_properties.ContainsFormReport(form))
                    {
                        FormReport formReport = _properties.GetFormReport(form);
                        try
                        {
                            if (formReport == null || formReport.Status != FormStatus.New)
                                continue;

                            if (formReport.Method.Equals("GET", StringComparison.InvariantCultureIgnoreCase))
                            {

                            }
                            else if (formReport.Method.Equals("POST", StringComparison.InvariantCultureIgnoreCase))
                            {

                            }
                            else
                            {
                                formReport.Status = FormStatus.UnrecognisedMethod;
                            }
                        }
                        catch (Exception err)
                        {
                            _report.AddError(new ErrorData(err, url, null, originatingLink));
                            formReport.Status = FormStatus.Error;
                        }
                        finally
                        {
                            _report.FormReportAdd(formReport);
                        }
                    }
                    else
                    {
                        form.Status = FormStatus.NotProcessing;
                        _report.FormReportAdd(form);
                    }
                }
            }
            finally
            {
                forms.Clear();
            }
        }

        private void LogError(Exception error, Uri url, Uri link, Uri originatingLink)
        {
            RaiseError(error, url, link, originatingLink);
        }

        private List<string> ParseHTMLImages(in string url, in string text)
        {
            List<string> Result = new List<string>();

            // Scan links on this page
            HtmlParser parse = new HtmlParser(text);

            while (parse.ParseNext("img", out HtmlTag tag))
            {
                // See if this anchor links to us

                if (tag.Attributes.TryGetValue("src", out string value))
                {
                    if (value.StartsWith("/"))
                    {
                        string sURL = url;

                        while (!sURL.EndsWith("/"))
                            sURL = sURL.Substring(0, sURL.Length - 1);

                        sURL = sURL.Substring(0, sURL.Length - 1);

                        while (sURL.IndexOf("/", 10) > 1)
                            sURL = sURL.Substring(0, sURL.Length - 1);

                        value = sURL + value;
                    }

                    Result.Add(value);
                }
            }

            return Result;
        }

        private List<string> ParseHtml(string url, string text)
        {
            List<string> Result = new List<string>();

            // Scan links on this page
            HtmlParser parse = new HtmlParser(text);

            while (parse.ParseNext("a", out HtmlTag tag))
            {
                // See if this anchor links to us

                if (tag.Attributes.TryGetValue("href", out string value))
                {
                    if (value.StartsWith("/"))
                    {
                        string sURL = url;

                        while (!sURL.EndsWith("/"))
                            sURL = sURL.Substring(0, sURL.Length - 1);
                        sURL = sURL.Substring(0, sURL.Length - 1);

                        while (sURL.IndexOf("/", 10) > 1)
                            sURL = sURL.Substring(0, sURL.Length - 1);

                        value = sURL + value;
                    }

                    Result.Add(value);
                }
            }

            return Result;
        }

        private List<FormReport> ParseForms(string url, string text)
        {
            List<FormReport> Result = new List<FormReport>();

            // Scan links on this page
            HtmlParser parse = new HtmlParser(text);

            while (parse.ParseNext("form", out HtmlTag tag))
            {
                // See if this anchor links to us
                if (tag.Attributes.TryGetValue("action", out string value))
                {
                    FormReport form = new FormReport();
                    form.Action = tag.Attributes["action"];
                    form.Id = tag.Attributes.ContainsKey("id") ? tag.Attributes["id"] : String.Empty;
                    form.Method = tag.Attributes.ContainsKey("method") ? tag.Attributes["method"] : String.Empty;
                }
            }

            return Result;
        }

        private bool SendEmail(string Message)
        {
            bool Result = false;
            try
            {
                MailMessage msg = new MailMessage();
                try
                {
                    msg.Subject = _properties.EmailTitle;

                    msg.Body = String.Format(Message);

                    msg.ReplyToList.Add(new MailAddress("web.monitor.win@gmail.com"));

                    msg.From = new MailAddress("");

                    foreach (MailAddress address in _recipients)
                        msg.To.Add(address);

                    SmtpClient Smtp = new SmtpClient();
                    try
                    {
                        Smtp.Host = "";

                        Smtp.EnableSsl = true;
                        Smtp.Port = 8000;
                        Smtp.Credentials = new NetworkCredential(
                            "UserName",
                            "Password");

                        Smtp.Send(msg);
                        Result = true;
                    }
                    finally
                    {
                        Smtp.Dispose();
                        Smtp = null;
                    }
                }
                finally
                {
                    msg.Dispose();
                    msg = null;
                }
            }
            catch (Exception err)
            {
                _report.Errors.Add(new ErrorData(err));
            }

            return Result;
        }

        #endregion Private Methods

        #region IDisposable Methods

        public void Dispose()
        {
            System.GC.SuppressFinalize(this);

            _client.Dispose();
            _client = null;
        }

        #endregion IDisposable Methods
    }
}
