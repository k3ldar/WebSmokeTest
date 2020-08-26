using System;
using System.Security.Cryptography.Xml;

using Shared.Classes;

using SmokeTest.Shared;
using SmokeTest.Shared.Engine;

namespace SmokeTest.Engine
{

    public class ThreadWebsiteScan : ThreadManager
    {
        private WebMonitor _crawler;
        private readonly SmokeTestProperties _properties;
        private readonly ITestRunLogger _testRunLogger;
        private readonly ILicenseFactory _licenseFactory;
        private Report _report;

        public ThreadWebsiteScan(in ILicenseFactory licenseFactory, in SmokeTestProperties properties, 
            in long uniqueId, ITestRunLogger testRunLogger)
            : base(null, new TimeSpan(0, 0, 1), null, 500, 0)
        {
            HangTimeout = 0;

            _licenseFactory = licenseFactory ?? throw new ArgumentNullException(nameof(licenseFactory));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _testRunLogger = testRunLogger ?? throw new ArgumentNullException(nameof(testRunLogger));
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
            _crawler = new WebMonitor(_licenseFactory, _properties, this, _testRunLogger);
            try
            {
                _crawler.Run();
            }
            finally
            {
                _report = _crawler.Report;
                _crawler.Dispose();
            }

            _properties.LastVerified = DateTime.UtcNow;

            _testRunLogger.Log("Finished, closing, going home!");

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
