using System;

using Shared.Classes;

namespace WebSmokeTest.Engine
{

    public class ThreadWebsiteScan : ThreadManager
    {
        private WebMonitor _crawler;
        private readonly SmokeTestProperties _properties;

        public ThreadWebsiteScan(in SmokeTestProperties properties)
            : base(null, new TimeSpan(0, 0, 1), null, 500, 0)
        {
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
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
    }
}
