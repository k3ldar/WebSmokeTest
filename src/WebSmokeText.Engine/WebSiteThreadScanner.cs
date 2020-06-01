using System;

using Shared.Classes;

using SmokeTest.Shared.Engine;

namespace SmokeTest.Engine
{

    public class ThreadWebsiteScan : ThreadManager
    {
        private WebMonitor _crawler;
        private readonly SmokeTestProperties _properties;
        private Report _report;

        public ThreadWebsiteScan(in SmokeTestProperties properties, in long uniqueId)
            : base(null, new TimeSpan(0, 0, 1), null, 500, 0)
        {
            HangTimeout = 0;
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            UniqueId = uniqueId;
            ContinueIfGlobalException = true;
        }

        public override void CancelThread(int timeout = 10000, bool isUnResponsive = false)
        {
            base.CancelThread(timeout, isUnResponsive);

            if (_crawler != null)
                _crawler.Cancel();
        }

        protected override bool Run(object parameters)
        {
            _crawler = new WebMonitor(_properties, this);
            try
            {
                _crawler.Run();
            }
            finally
            {
                _report = _crawler.Report;
                _crawler.Dispose();
            }

            _properties.LastVerified = DateTime.Now;

            return false;
        }

        #region Event Wrappers

        #endregion Event Wrappers

        #region Events

        //public event ErrorEventHandler OnError;
        //public event PageParsedEventHandler OnPageParsed;
        //public event BeforePageParsed BeforeParse;

        #endregion Events

        #region Properties

        public Report Report
        {
            get
            {
                return _report;
            }
        }

        public long UniqueId { get; private set; }

        #endregion Properties
    }
}
