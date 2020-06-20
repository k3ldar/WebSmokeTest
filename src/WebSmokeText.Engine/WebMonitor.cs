using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Web;

using Newtonsoft.Json;

using Shared;
using Shared.Classes;

using SharedPluginFeatures;

using SmokeTest.Shared;
using SmokeTest.Shared.Engine;

namespace SmokeTest.Engine
{
    public sealed class WebMonitor : IDisposable
    {
        #region Private Members

        private static readonly object _lockObject = new object();
        private readonly Queue<Uri> _urlProcessList;
        private readonly SmokeTestProperties _properties;
        private readonly MailAddressCollection _recipients;
        private WebClientEx _client;
        private bool _cancelScan = false;
        private readonly Timings _pageLoadTimings;
        private readonly ITestRunLogger _testRunLogger;
        private readonly List<IPEndPoint> _endPoints;

        private readonly Report _report;
        private readonly ThreadManager _parentThread;
        private DateTime _tcpConnectionChecked = DateTime.UtcNow.AddHours(-1);
        private bool _lastTcpConnectionResult = false;
        private const int MillisecondsBetweenTcpChecks = 2000;
        private int MaximumOpenEndpoints { get; set; }

        #endregion Private Members

        #region Constructors / Destructors

        public WebMonitor(in SmokeTestProperties properties)
            : this(properties, null, new TestRunLogger(DateTime.UtcNow.Ticks))
        {

        }

        public WebMonitor(in SmokeTestProperties properties, in ThreadManager parentThread, ITestRunLogger testRunLogger)
        {
            MaximumOpenEndpoints = 100;
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _testRunLogger = testRunLogger ?? throw new ArgumentNullException(nameof(testRunLogger));
            _parentThread = parentThread;
            _urlProcessList = new Queue<Uri>();
            _endPoints = new List<IPEndPoint>();

            _pageLoadTimings = new Timings();
            _report = new Report();
            _report.MinimumLoadTime = _properties.MinimumLoadTime;
            _report.SiteScan = _properties.SiteScan;

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
            _testRunLogger.Log("Starting Test Run");
            try
            {
                _report.StartTime = DateTime.UtcNow;

                Uri homePage = new Uri(_properties.Url);

                if (ValidateTestsCanBeRunAgainstUrl(homePage))
                {
                    RetrieveIpAddresses(_properties.Url);

                    if (_report.SiteScan)
                    {
                        _urlProcessList.Enqueue(homePage);
                    }

                    DiscoverOnSiteTests(homePage, 0);

                    RunDiscoveredTests(homePage);

                    while (true)
                    {
                        if (_report.Pages.Count >= _properties.MaximumPages)
                        {
                            _testRunLogger.Log($"Maximum pages exceeded {_properties.MaximumPages}");
                            break;
                        }

                        Uri urlToProcess = null;

                        using (TimedLock timedLock = TimedLock.Lock(_lockObject))
                        {
                            if (!_urlProcessList.TryDequeue(out urlToProcess))
                                break;
                        }

                        if (!ParsePage(urlToProcess.ToString(), urlToProcess, 0, urlToProcess))
                            return false;
                    }
                }
                else
                {
                    _testRunLogger.Log("Site is not enabled for WebSmokeTest");
                }
            }
            catch (Exception err)
            {
                _testRunLogger.Log(err);
                RaiseError(err, null, null, null);
            }
            finally
            {
                _report.ClearParsedLinks();
                _report.EndTime = DateTime.UtcNow;
            }

            //foreach (Cookie cookie in _client.CookieContainer.GetCookies(new Uri(_properties.Url)))
            //{
            //    _client.CookieContainer.Add(cookie);
            //}

            string errorDetail = GetErrorInformation();


            if (_properties.SendEmails && _report.Errors.Count > 0)
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

            DateTime timeoutStart = DateTime.UtcNow;

            while (!_report.AnalysisComplete)
            {
                Thread.Sleep(100);

                TimeSpan span = DateTime.UtcNow - timeoutStart;

                if (span.TotalSeconds > 30)
                    break;
            }

            _testRunLogger.Log("Running Garbage Collection");

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

        /// <summary>
        /// This test first does a get of the url, then proceeds to submit data using post etc
        /// </summary>
        /// <param name="test"></param>
        /// <param name="testClient"></param>
        /// <param name="rootUrl"></param>
        private string SubmitTestForm(in WebSmokeTestItem test, in WebClientEx testClient, in TestResult testResult,
            in string rootUrl, out int responseCode)
        {
            responseCode = -1;
            string route = BuildRouteParameters(rootUrl + test.Route, test.Parameters);
            Uri uriRoute = new Uri(route);
            string getData = testClient.GetData(uriRoute, out _);
            string formId = test.FormId;
            NVPCodec inputCodec = new NVPCodec();
            inputCodec.Decode(test.InputData);
            string cookies = String.Empty;
            string referer = route;

            foreach (string check in test.ResponseData)
            {
                if (!getData.Contains(check))
                {
                    _report.AddError(new ErrorData(new Exception($"Response {check} not found in {test.Name}"), uriRoute));
                    testResult.ErrorCount++;
                }
            }

            if (referer.EndsWith("/"))
                referer = referer.Substring(0, referer.Length - 1);

            if (testClient.ResponseCookies != null)
            {
                foreach (Cookie cookie in testClient.ResponseCookies)
                {
                    if (cookies.Length == 0)
                        cookies = $"{cookie.Name}={cookie.Value}";
                    else
                        cookies += $";{cookie.Name}={cookie.Value}";
                }
            }

            if (!String.IsNullOrEmpty(_properties.CookieName) && !String.IsNullOrEmpty(_properties.CookieValue) &&
                !String.IsNullOrEmpty(_properties.CookiePath) && !String.IsNullOrEmpty(_properties.CookieDomain))
            {
                if (cookies.Length == 0)
                    cookies = $"{_properties.CookieName}={_properties.CookieValue}";
                else
                    cookies += $";{_properties.CookieName}={_properties.CookieValue}";
            }

            testClient.Headers.Add(HttpRequestHeader.Cookie, cookies);
            PageReport pageReport = new PageReport(route, 0, getData);
            ProcessForms(pageReport, new Uri(route), new Uri(route));
            pageReport.ProcessingComplete = true;

            PageAnalyser pageAnalyser = new PageAnalyser(_report, pageReport, null, true, true);

            if (pageAnalyser.Analyse(pageReport))
            {
                FormAnalysis form = pageReport.Analysis.Body.Forms.Where(f => f.Id == formId).FirstOrDefault();

                if (form == null)
                {
                    _report.AddError(new ErrorData(new Exception($"Form {formId} was not found.  Invalid WebSmokeTest"), uriRoute));
                    return String.Empty;
                }

                NVPCodec postValues = new NVPCodec();

                foreach (FormInput item in form.Inputs)
                {
                    if (String.IsNullOrEmpty(item.Name))
                        continue;

                    if (inputCodec.AllKeys.Contains(item.Name))
                    {
                        if (postValues.AllKeys.Where(k => k.Equals(item.Name)).Any())
                            postValues[item.Name] = inputCodec[item.Name];
                        else
                            postValues.Add(item.Name, inputCodec[item.Name]);
                    }
                    else
                    {
                        if (postValues.AllKeys.Where(k => k.Equals(item.Name)).Any())
                            postValues[item.Name] = item.Contents ?? String.Empty;
                        else
                            postValues.Add(item.Name, item.Contents ?? String.Empty);
                    }
                }

                string action = form.Action.StartsWith("/") ? form.Action.Substring(1) : form.Action;
                testClient.Headers.Add("Referer", referer);
                testClient.Encoding = Encoding.UTF8;
                testClient.Headers.Add("AcceptEncoding", "gzip, deflate");
                testClient.Headers.Add("Cache-Control", "max-age=0");
                try
                {
                    return testClient.PostFormData(new Uri(rootUrl + action), postValues, out responseCode);
                }
                catch (Exception response)
                {
                    _report.AddError(new ErrorData(new Exception($"Test: {test.Name}; Error: {response.Message}"), uriRoute));
                }
            }

            return String.Empty;
        }

        /// <summary>
        /// This test submits data as part of a body request using post etc
        /// </summary>
        /// <param name="test"></param>
        /// <param name="testClient"></param>
        /// <param name="rootUrl"></param>
        private string SubmitTestData(in WebSmokeTestItem test, in WebClientEx testClient, in TestResult testResult,
            in string rootUrl, out int responseCode)
        {
            string data = test.InputData.Trim();
            responseCode = -1;
            string route = rootUrl + test.Route;
            Uri uriRoute = new Uri(route);
            string getData = testClient.GetData(uriRoute, out _);
            string formId = test.FormId;
            NVPCodec inputCodec = new NVPCodec();
            inputCodec.Decode(test.InputData);
            string cookies = String.Empty;
            string referer = route;

            foreach (string check in test.ResponseData)
            {
                if (!getData.Contains(check))
                {
                    _report.AddError(new ErrorData(new Exception($"Response {check} not found in {test.Name}"), uriRoute));
                    testResult.ErrorCount++;
                }
            }

            if (referer.EndsWith("/"))
                referer = referer.Substring(0, referer.Length - 1);

            foreach (Cookie cookie in testClient.ResponseCookies)
            {
                if (cookies.Length == 0)
                    cookies = $"{cookie.Name}={cookie.Value}";
                else
                    cookies += $";{cookie.Name}={cookie.Value}";
            }

            if (!String.IsNullOrEmpty(_properties.CookieName) && !String.IsNullOrEmpty(_properties.CookieValue) &&
                !String.IsNullOrEmpty(_properties.CookiePath) && !String.IsNullOrEmpty(_properties.CookieDomain))
            {
                if (cookies.Length == 0)
                    cookies = $"{_properties.CookieName}={_properties.CookieValue}";
                else
                    cookies += $";{_properties.CookieName}={_properties.CookieValue}";
            }

            testClient.Headers.Add(HttpRequestHeader.Cookie, cookies);

            testClient.Headers.Add("Referer", referer);
            testClient.Encoding = Encoding.UTF8;
            testClient.Headers.Add("AcceptEncoding", "gzip, deflate");
            testClient.Headers.Add("Cache-Control", "max-age=0");
            try
            {
                switch (data[0])
                {
                    case '{':
                    case '[':
                        return testClient.PostJsonData(new Uri(rootUrl + test.Route), test.InputData, out responseCode);

                    case '<':
                        return testClient.PostXmlData(new Uri(rootUrl + test.Route), test.InputData, out responseCode);
                    default:
                        throw new InvalidOperationException("Unknown data type, could not determine xml or json");
                }
            }
            catch (Exception response)
            {
                _report.AddError(new ErrorData(new Exception($"Test: {test.Name}; Error: {response.Message}"), uriRoute));
            }

            return String.Empty;
        }

        /// <summary>
        /// This test submits data as part of a body request using post etc
        /// </summary>
        /// <param name="test"></param>
        /// <param name="testClient"></param>
        /// <param name="rootUrl"></param>
        private string SubmitGetRequest(in WebSmokeTestItem test, in WebClientEx testClient,
            in string rootUrl, out int responseCode)
        {
            responseCode = -1;
            string route = rootUrl + test.Route;
            NVPCodec inputCodec = new NVPCodec();
            inputCodec.Decode(test.Parameters);

            if (!String.IsNullOrEmpty(_properties.CookieName) && !String.IsNullOrEmpty(_properties.CookieValue) &&
                !String.IsNullOrEmpty(_properties.CookiePath) && !String.IsNullOrEmpty(_properties.CookieDomain))
            {
                string cookies = $"{_properties.CookieName}={_properties.CookieValue}";

                testClient.Headers.Add(HttpRequestHeader.Cookie, cookies);
            }

            Uri uriRoute = new Uri(BuildRouteParameters(route, test.Parameters));
            return testClient.GetData(uriRoute, out responseCode);
        }

        private string BuildRouteParameters(in string route, in string parameters)
        {
            string Result = String.Empty;
            bool startFound = false;
            string token = String.Empty;

            foreach (char c in route)
            {
                if (!startFound && c == '{')
                {
                    startFound = true;
                    token += c;
                }
                else if (startFound && c == '}')
                {
                    token += c;

                    Result += GetTokenValue(token, parameters);
                    token = String.Empty;
                    startFound = false;
                }
                else if (startFound)
                {
                    token += c;
                }
                else if (!startFound)
                {
                    Result += c;
                }
            }

            return Result;
        }

        private string GetTokenValue(in string token, in string parameters)
        {
            string Result = String.Empty;

            if (!String.IsNullOrEmpty(parameters))
            {
                NVPCodec codec = new NVPCodec();
                codec.Decode(parameters);

                foreach (string key in codec.AllKeys)
                {
                    if (key.Equals(token.Substring(1, token.Length - 2)))
                    {
                        Result = HttpUtility.UrlEncode(codec[key]);
                        break;
                    }
                }

            }

            return Result;
        }

        /// <summary>
        /// Runs any tests discovered on a website
        /// </summary>
        /// <param name="homePage"></param>
        private void RunDiscoveredTests(in Uri homePage)
        {
            List<WebSmokeTestItem> discoveredTests = _report.DiscoveredTests.OrderBy(dt => dt.Position).ToList();
            string rootSite = $"{homePage.Scheme}://{homePage.Authority}/";

            foreach (WebSmokeTestItem test in discoveredTests)
            {
                try
                {
                    using (WebClientEx testClient = new WebClientEx())
                    {
                        TestResult testResult = new TestResult()
                        {
                            Name = test.Name,
                            Position = test.Position,
                        };
                        testClient.UserAgent = _properties.UserAgent;
                        testClient.Timeout = 60000;
                        testClient.AllowAutoRedirect = false;

                        Uri uriRoute = new Uri(rootSite + test.Route);

                        foreach (KeyValuePair<string, string> headers in _properties.Headers)
                        {
                            testClient.Headers.Add(headers.Key, headers.Value);
                        }

                        string response = null;
                        int responseCode = 0;
                        bool searchSubmitResponseData = false;
                        Timings testTimings = new Timings();

                        using (StopWatchTimer stopwatchTimer = StopWatchTimer.Initialise(_pageLoadTimings))
                        {
                            using (StopWatchTimer testTimer = StopWatchTimer.Initialise(testTimings))
                            {
                                if (String.IsNullOrEmpty(test.FormId) && test.Method.Equals("GET", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    response = SubmitGetRequest(test, testClient, rootSite, out responseCode);
                                    searchSubmitResponseData = false;
                                }
                                else if (String.IsNullOrEmpty(test.FormId))
                                {
                                    response = SubmitTestData(test, testClient, testResult, rootSite, out responseCode);
                                    searchSubmitResponseData = true;
                                }
                                else
                                {
                                    response = SubmitTestForm(test, testClient, testResult, rootSite, out responseCode);
                                    searchSubmitResponseData = true;
                                }

                                if (!responseCode.Equals(test.Response))
                                {
                                    _report.AddError(new ErrorData(new Exception($"Response code {test.Response} expected; {responseCode} received"), uriRoute));
                                    testResult.ErrorCount++;
                                }

                                switch (responseCode)
                                {
                                    case 301:
                                    case 302:
                                        ValidateRedirectUrl(test, testClient, uriRoute);
                                        break;
                                }

                                if (searchSubmitResponseData)
                                {
                                    foreach (string check in test.SubmitResponseData)
                                    {
                                        if (!response.Contains(check))
                                        {
                                            _report.AddError(new ErrorData(new Exception($"Response {check} not found in {test.Name}"), uriRoute));
                                            testResult.ErrorCount++;
                                        }
                                    }
                                }
                                else
                                {
                                    foreach (string check in test.ResponseData)
                                    {
                                        if (!response.Contains(check))
                                        {
                                            _report.AddError(new ErrorData(new Exception($"Response {check} not found in {test.Name}"), uriRoute));
                                            testResult.ErrorCount++;
                                        }
                                    }
                                }
                            }
                        }

                        testResult.TimeTaken = testTimings.Slowest;

                        _report.TestResults.Add(testResult);
                    }
                }
                catch (Exception exception)
                {
                    LogError(exception, homePage, homePage, homePage);
                }

                ValidatePortUseage();
            }
        }

        private void ValidateRedirectUrl(in WebSmokeTestItem test, in WebClientEx testClient, in Uri route)
        {
            string redirectUrl = String.Empty;

            if (testClient.ResponseHeaders.AllKeys.Where(k => k.Equals("Location", StringComparison.InvariantCultureIgnoreCase)).Any())
            {
                redirectUrl = testClient.ResponseHeaders["Location"];
            }

            if (!test.ResponseUrl.Equals(redirectUrl, StringComparison.InvariantCultureIgnoreCase))
                _report.AddError(new ErrorData(new Exception($"Test {test.Name} recieved incorrect Redirect Url, expection {test.ResponseUrl}; received: {redirectUrl}"), route));
        }

        /// <summary>
        /// Discovers tests on website
        /// </summary>
        private void DiscoverOnSiteTests(in Uri homePage, in int attempt)
        {
            _testRunLogger.Log($"Discovering Site Tests: Attempt: {attempt + 1}");

            int tests = GetSiteTestCount(homePage);

            if (tests == -1 && attempt < 5)
            {
                DiscoverOnSiteTests(homePage, attempt + 1);
                return;
            }

            int counter = 0;
            while (counter < tests)
            {
                WebSmokeTestItem discoveredTest = GetSiteTestItem(homePage, counter);

                if (discoveredTest != null)
                {
                    _testRunLogger.Log($"Discovered Test: {discoveredTest.Name}");
                    _report.AddDiscoveredTest(discoveredTest);
                }

                counter++;

                ValidatePortUseage();
            }
        }

        /// <summary>
        /// Validates that the website is setup for WebSmokeTest
        /// </summary>
        /// <returns></returns>
        private bool ValidateTestsCanBeRunAgainstUrl(in Uri homepage)
        {
            try
            {
                if (String.IsNullOrEmpty(_properties.EncryptionKey))
                    throw new InvalidOperationException("Encryption key is invalid");

                SmokeTestWebRequest(homepage, "/smoketest/siteid/", out string data);

                data = Utilities.Decrypt(data, _properties.EncryptionKey);

                if (!data.Equals(_properties.SiteId))
                    throw new InvalidOperationException("Incorrect Site Id returned");

                return true;
            }
            catch (WebException err)
            {
                _testRunLogger.Log(err);
                if (err.Message.Contains("(404)"))
                {
                    LogError(new Exception($"{homepage.ToString()} is not configured for Smoke Testing"), homepage, homepage, homepage);
                }
                else
                {
                    LogError(err, homepage, null, homepage);
                }
            }
            catch (FormatException)
            {
                Exception encryptErr = new Exception($"Unable to decrypt data from {homepage.ToString()}");
                _testRunLogger.Log(encryptErr);
                LogError(encryptErr, homepage, null, homepage);
            }
            catch (Exception err)
            {
                _testRunLogger.Log(err);
                LogError(err, homepage, null, homepage);
            }

            return false;
        }

        private int GetSiteTestCount(in Uri homepage)
        {
            SmokeTestWebRequest(homepage, "/smoketest/count/", out string data);

            data = Utilities.Decrypt(data, _properties.EncryptionKey);

            if (Int32.TryParse(data, out int Result))
                return Result;

            return -1;
        }

        private WebSmokeTestItem GetSiteTestItem(in Uri homepage, in int index)
        {
            SmokeTestWebRequest(homepage, $"/smoketest/test/{index}", out string data);

            data = Utilities.Decrypt(data, _properties.EncryptionKey);

            return JsonConvert.DeserializeObject<WebSmokeTestItem>(data);
        }

        private void SmokeTestWebRequest(Uri homepage, string url, out string data)
        {
            using (StopWatchTimer stopwatchTimer = StopWatchTimer.Initialise(_pageLoadTimings))
            {
                Uri modifiedUri = new Uri($"{homepage.Scheme}://{homepage.IdnHost}:{homepage.Port}{url}");
                data = _client.GetData(modifiedUri, out _);
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
                    i,
                    item.Uri == null ? String.Empty : item.Uri.ToString(),
                    item.MissingLink == null ? String.Empty : item.MissingLink.ToString(),
                    item.Error == null ? String.Empty : item.Error.Message,
                    item.OriginatingLink == null ? String.Empty : item.OriginatingLink.ToString());

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
        private bool ParsePage(in string startsWith,
            in Uri url, int depth, in Uri originatingLink)
        {
            if (_report.Pages.Count >= _properties.MaximumPages)
            {
                _testRunLogger.Log($"Maximum page count exceeded: {_properties.MaximumPages}");
                return true;
            }

            if (depth > _properties.CrawlDepth)
            {
                using (TimedLock timedLock = TimedLock.Lock(_lockObject))
                {
                    if (!_report.LinkParsed(url.ToString()))
                    {
                        _urlProcessList.Enqueue(url);
                    }
                }

                return true;
            }

            using (TimedLock.Lock(_lockObject))
            {
                if (_report.LinkParsed(url))
                    return true;
            }

            _testRunLogger.Log($"Processing Url: {url.ToString()}");

            if (_cancelScan)
            {
                _testRunLogger.Log("Test run cancelled");
                return false;
            }

            ValidatePortUseage();

            Uri modifiedUri = ModifyUrl(url.ToString());

            string webData = null;
            Timings pageLoad = new Timings();

            using (StopWatchTimer pageLoadTimer = StopWatchTimer.Initialise(pageLoad))
            {
                int responseCode = -1;
                try
                {
                    using (StopWatchTimer stopwatchTimer = StopWatchTimer.Initialise(_pageLoadTimings))
                    {
                        webData = _client.GetData(modifiedUri, out responseCode);
                    }
                }
                catch (WebException err)
                {
                    _testRunLogger.Log(err);
                    if (responseCode == 404 || err.Message.Contains("(404)"))
                    {
                        LogError(err, modifiedUri, modifiedUri, originatingLink);
                    }
                    else if (err.Message.StartsWith("The operation has timed out") && _client.Timeout < 5000)
                    {
                        _client.Timeout = _client.Timeout + 50;

                        using (TimedLock timedLock = TimedLock.Lock(_lockObject))
                        {
                            _urlProcessList.Enqueue(url);
                            return true;
                        }

                    }
                    else
                        LogError(err, modifiedUri, null, originatingLink);
                }
                catch (Exception err)
                {
                    _testRunLogger.Log(err);
                    LogError(err, modifiedUri, null, originatingLink);
                }
            }

            if (String.IsNullOrEmpty(webData))
                return true;

            List<string> links = ParseHtml(url.ToString(), webData);
            try
            {
                PageReport pageReport = new PageReport(modifiedUri.ToString(), pageLoad.Total, webData);
                try
                {
                    _report.PageAdd(pageReport, _parentThread, _properties);

                    using (TimedLock.Lock(_lockObject))
                    {
                        _report.LinkAdd(url);
                    }

                    if (!SessionCookieAdded &&
                        _client.ResponseCookies != null &&
                        _client.ResponseCookies[_properties.SessionCookieName] != null)
                    {
                        _client.CookieContainer.Add(_client.ResponseCookies[_properties.SessionCookieName]);
                        SessionCookieAdded = true;
                    }

                    if (_client.ResponseCookies != null)
                    {
                        foreach (Cookie cookie in _client.ResponseCookies)
                        {
                            _report.AddCookie(cookie);
                        }
                    }

                    if (_client.ResponseHeaders != null)
                        pageReport.AddHeaders(_client.ResponseHeaders);

                    if (webData == null)
                        return true;

                    pageReport.Content = webData ?? String.Empty;

                    ProcessImages(pageReport, originatingLink, modifiedUri);

                    ProcessForms(pageReport, originatingLink, modifiedUri);
                }
                finally
                {
                    pageReport.ProcessingComplete = true;
                }

                foreach (string link in links)
                {

                    if (link.ToLower().StartsWith(startsWith.ToLower()))
                    {
                        pageReport.AddPageLink(link);

                        if (!ParsePage(startsWith, new Uri(link), depth + 1, url))
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

        private void ValidatePortUseage()
        {
            bool tcpConnectionLimitExceeded = TcpConnectionLimitExceeded();

            if (tcpConnectionLimitExceeded)
            {
                _testRunLogger.Log($"Tcp connection limit exceeded, pausing...");
            }

            while (tcpConnectionLimitExceeded)
            {
                Thread.Sleep(100);

                tcpConnectionLimitExceeded = TcpConnectionLimitExceeded();

                if (!tcpConnectionLimitExceeded)
                {
                    _testRunLogger.Log($"Tcp connection limit normalised, resuming...");
                    break;
                }
            }

            Thread.Sleep(Math.Max(50, _properties.PauseBetweenRequests));
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

        private void ProcessImages(in PageReport pageReport,
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

                        if (imageLink != "")
                        {
                            if (_report.ImageParsed(imageLink))
                            {
                                continue;
                            }

                            _testRunLogger.Log($"Retrieving image data: {imageLink}");

                            _report.ImageAdd(imageLink);
                            try
                            {
                                ImageReport imageReport = new ImageReport(imageLink);
                                pageReport.AddPageImage(imageReport);
                                DateTime startTimeImage = DateTime.UtcNow;

                                _report.Images.Add(imageReport);
                                imageReport.Bytes = _client.DownloadData(imageLink);

                                TimeSpan span = DateTime.UtcNow.Subtract(startTimeImage);
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
                finally
                {
                    links.Clear();
                }
            }
        }

        private void ProcessForms(in PageReport pageReport,
            in Uri originatingLink, in Uri url)
        {
            //Look at any forms, can we process those too
            List<FormReport> forms = ParseForms(url, pageReport.Content);
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

        private void LogError(in Exception error, in Uri url, in Uri link, in Uri originatingLink)
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

        private List<string> ParseHtml(in string url, in string text)
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

        private List<FormReport> ParseForms(in Uri url, in string text)
        {
            List<FormReport> Result = new List<FormReport>();

            // Scan links on this page
            HtmlParser parse = new HtmlParser(text);

            while (parse.ParseNext("form", out HtmlTag tag))
            {
                // See if this anchor links to us
                if (tag.Attributes.TryGetValue("action", out string _))
                {
                    FormReport form = new FormReport();
                    form.Url = url;
                    form.Action = tag.Attributes["action"];
                    form.Id = tag.Attributes.ContainsKey("id") ? tag.Attributes["id"] : String.Empty;
                    form.Method = tag.Attributes.ContainsKey("method") ? tag.Attributes["method"] : String.Empty;
                    Result.Add(form);
                }
            }

            return Result;
        }

        private bool SendEmail(in string Message)
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

        private void RetrieveIpAddresses(in string url)
        {
            if (Uri.TryCreate(url, UriKind.Absolute, out Uri host))
            {
                foreach (IPAddress ipAddress in Dns.GetHostAddresses(host.Host))
                {
                    _endPoints.Add(new IPEndPoint(ipAddress, host.Port));
                }
            }
        }

        private bool ContainsEndPoint(in IPEndPoint endpoint)
        {
            foreach (IPEndPoint item in _endPoints)
            {
                if (item.Equals(endpoint))
                    return true;
            }

            return false;
        }

        private bool TcpConnectionLimitExceeded()
        {
            TimeSpan span = DateTime.UtcNow - _tcpConnectionChecked;

            if (span.TotalMilliseconds > MillisecondsBetweenTcpChecks)
            {
                IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();

                int endpoints = properties.GetActiveTcpConnections().Where(c => ContainsEndPoint(c.RemoteEndPoint)).Count();

                _tcpConnectionChecked = DateTime.UtcNow;
                _lastTcpConnectionResult = endpoints > MaximumOpenEndpoints;

                return _lastTcpConnectionResult;
            }
            else
            {
                return _lastTcpConnectionResult;
            }
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
