using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

using Newtonsoft.Json;

using Shared.Classes;

using SharedPluginFeatures;
using SmokeTest.Shared;

namespace SmokeTest.Shared.Engine
{
    public sealed class Report
    {
        #region Private Members

        private readonly List<string> _linksParsed;
        private readonly List<string> _imagesParsed;
        private readonly object _lockObject = new object();

        #endregion Private Members

        #region Constructors

        public Report()
        {
            Pages = new List<PageReport>();
            Images = new List<ImageReport>();
            Errors = new List<ErrorData>();
            Cookies = new List<Cookie>();
            Forms = new List<FormReport>();
            _linksParsed = new List<string>();
            _imagesParsed = new List<string>();
        }

        #endregion Constructors

        #region Properties

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public LastRunResult RunResult { get; set; }

        public long TestSchedule { get; set; }

        public List<PageReport> Pages { get; set; }

        public List<ImageReport> Images { get; set; }

        public List<ErrorData> Errors { get; set; }

        public List<Cookie> Cookies { get; set; }

        public List<FormReport> Forms { get; set; }

        public Dictionary<string, string> Headers { get; set; }

        public uint TotalRequests { get; set; }

        public decimal TotalSlowest { get; set; }

        public decimal TotalFastest { get; set; }

        public decimal TotalAverage { get; set; }

        public decimal TrimmedAverage { get; set; }

        public decimal TotalTime { get; set; }

        public bool AnalysisComplete
        {
            get
            {
                return Pages.Where(p => !p.AnalysisComplete).Count() == 0;
            }
        }

        #endregion Properties

        #region Public Methods

        public void Clear()
        {
            Pages.Clear();
            Images.Clear();
            Errors.Clear();
            Cookies.Clear();
            Forms.Clear();
            TotalRequests = 0;
            TotalSlowest = 0;
            TotalFastest = 0;
            TotalAverage = 0;
            TrimmedAverage = 0;
            TotalTime = 0;
        }

        public void FormReportAdd(FormReport form)
        {
            if (form == null)
                throw new ArgumentNullException(nameof(form));

            if (Forms
                .Where(f => f.Action.Equals(form.Action, StringComparison.OrdinalIgnoreCase) &&
                        f.Method.Equals(form.Method, StringComparison.OrdinalIgnoreCase))
                .Any())
            {
                return;
            }

            Forms.Add(form);
        }

        public void PageAdd(in PageReport page, in ThreadManager parent, in SmokeTestProperties properties)
        {
            if (page == null)
                throw new ArgumentNullException(nameof(page));

            Pages.Add(page);

            PageAnalyser analyser = new PageAnalyser(this, page, parent,
                properties.ClearHtmlDataAfterAnalysis, properties.ClearImageDataAfterAnalysis);
            ThreadManager.ThreadStart(analyser, $"Page Analyser: {page.Url}", System.Threading.ThreadPriority.Lowest);
        }

        public void SetTimings(in Timings timings)
        {
            if (timings == null)
                throw new ArgumentNullException(nameof(timings));

            TotalRequests = timings.Requests;
            TotalSlowest = timings.Slowest;
            TotalFastest = timings.Fastest;
            TotalAverage = timings.Average;
            TrimmedAverage = timings.TrimmedAverage;
            TotalTime = timings.Total;
        }

        public void AddError(in ErrorData error)
        {
            if (error == null)
                throw new ArgumentNullException(nameof(error));

            error.Index = Errors.Count;
            Errors.Add(error);
        }

        public void AddCookie(Cookie cookie)
        {
            if (cookie == null)
                throw new ArgumentNullException(nameof(cookie));

            if (Cookies.Where(c => c.Name.Equals(cookie.Name) && c.Value.Equals(cookie.Value) && c.Expires.Equals(cookie.Expires)).Any())
                return;

            Cookies.Add(cookie);
        }

        public void SaveToFile(string file, bool deleteExisting)
        {
            if (String.IsNullOrEmpty(file))
                throw new ArgumentNullException(nameof(file));

            if (!Directory.Exists(Path.GetDirectoryName(file)))
                throw new ArgumentException(nameof(file));

            if (File.Exists(file) && deleteExisting)
                File.Delete(file);

            File.WriteAllText(file, JsonConvert.SerializeObject(this));
        }

        #endregion Public Methods

        #region Internal Methods

        public void ImageAdd(string url)
        {
            using (TimedLock timedLock = TimedLock.Lock(_lockObject))
            {
                url = url.ToLower();

                if (_imagesParsed.Contains(url))
                    return;

                _imagesParsed.Add(url);
            }
        }

        public bool ImageParsed(string url)
        {
            using (TimedLock timedLock = TimedLock.Lock(_lockObject))
            {
                url = url.ToLower();

                return _imagesParsed.Contains(url);
            }
        }

        public void LinkAdd(string url)
        {
            using (TimedLock timedLock = TimedLock.Lock(_lockObject))
            {
                url = url.ToLower();

                if (_linksParsed.Contains(url))
                    return;

                _linksParsed.Add(url);
            }
        }

        public void LinkAdd(Uri url)
        {
            LinkAdd(url.ToString());
        }

        public void LinkRemove(in Uri url)
        {
            using (TimedLock timedLock = TimedLock.Lock(_lockObject))
            {
                string loweredUrl = url.ToString().ToLower();

                if (_linksParsed.Contains(loweredUrl))
                {
                    _linksParsed.Remove(loweredUrl);
                }
            }
        }

        public bool LinkParsed(string url)
        {
            using (TimedLock timedLock = TimedLock.Lock(_lockObject))
            {
                url = url.ToLower();

                return _linksParsed.Contains(url);
            }
        }

        public bool LinkParsed(Uri url)
        {
            return LinkParsed(url.ToString());
        }

        public void ClearParsedLinks()
        {
            using (TimedLock timedLock = TimedLock.Lock(_lockObject))
            {
                _linksParsed.Clear();
                _imagesParsed.Clear();
            }
        }

        #endregion Internal Methods

        #region Public Static Methods

        public static Report LoadFromFile(in string file)
        {
            if (String.IsNullOrEmpty(file))
                throw new ArgumentNullException(nameof(file));

            if (!Directory.Exists(Path.GetDirectoryName(file)))
                throw new ArgumentException(nameof(file));

            if (!File.Exists(file))
                throw new ArgumentException(nameof(file));

            return JsonConvert.DeserializeObject<Report>(File.ReadAllText(file));
        }

        #endregion Public Static Methods
    }
}
