using System;
using System.IO;
using System.Linq;
using System.Diagnostics;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shared.Classes;
using SharedPluginFeatures;

using WebSmokeTest.Engine;

namespace WebSmokeTest.UnitTests
{
    [TestClass]
    public class ReportTests
    {
        [TestInitialize]
        public void TestInitialise()
        {
            ThreadManager.Initialise();
            ThreadManager.AllowThreadPool = true;
            ThreadManager.MaximumPoolSize = 20000;

            System.Net.ServicePointManager.DefaultConnectionLimit = 100;
            System.Net.ServicePointManager.ReusePort = true;
            System.Net.ServicePointManager.ServerCertificateValidationCallback = (message, cert, chain, sslPolicyErrors) => {
                return true;
            };
            System.Net.ServicePointManager.MaxServicePoints = 5;
            AppDomain.CurrentDomain.FirstChanceException += (sender, eventArgs) =>
            {
                Debug.WriteLine(eventArgs.Exception.ToString());
            };
        }

        [TestMethod]
        public void OpenPluginManagerWebsiteHomePageValidateImageCount()
        {
            SmokeTestProperties properties = new SmokeTestProperties()
            {
                Url = "http://www.pluginmanager.website",
                CrawlDepth = 1,
                MaximumPages = 1,
                SessionCookieName = "demo_website_session",
            };

            using (WebMonitor webMonitor = new WebMonitor(properties))
            {
                webMonitor.Run();

                Timings timings = webMonitor.PageLoadTimings;
                Report report = webMonitor.Report;

                Assert.AreEqual(2, report.Pages[0].Images.Count);
                Assert.AreEqual(1u, timings.Requests);
            }
        }

        [TestMethod]

        public void ReportSaveToDiskAndLoadFromDisk1Page()
        {
            Report report;

            SmokeTestProperties properties = new SmokeTestProperties()
            {
                Url = "http://localhost:5000",
                CrawlDepth = 1,
                MaximumPages = 1,
                SessionCookieName = "demo_website_session",
            };

            using (WebMonitor webMonitor = new WebMonitor(properties))
            {
                webMonitor.Run();

                Timings timings = webMonitor.PageLoadTimings;
                report = webMonitor.Report;

                Assert.AreEqual(2, report.Pages[0].Images.Count);
                Assert.AreEqual(1u, timings.Requests);
            }

            string tempPath = Path.Combine(Path.GetTempPath(), "SaveReport.dat");

            report.SaveToFile(tempPath, true);

            Report reportCopy = Report.LoadFromFile(tempPath);

            Assert.AreEqual(report.TotalFastest, reportCopy.TotalFastest);
            Assert.AreEqual(report.TotalAverage, reportCopy.TotalAverage);
            Assert.AreEqual(report.TotalRequests, reportCopy.TotalRequests);
            Assert.AreEqual(report.TotalSlowest, reportCopy.TotalSlowest);
            Assert.AreEqual(report.TotalTime, reportCopy.TotalTime);
            Assert.AreEqual(report.Pages.Count, reportCopy.Pages.Count);
            Assert.IsTrue(report.Pages.Count > 0);
            Assert.IsTrue(report.Pages[0].Content.Equals(reportCopy.Pages[0].Content));
            Assert.IsTrue(report.Images.Count > 0);
            Assert.AreEqual(report.Images[0].Url, reportCopy.Images[0].Url);
            Assert.IsTrue(report.Pages[0].Images[0].Bytes.SequenceEqual(reportCopy.Pages[0].Images[0].Bytes));
        }

        [TestMethod]

        public void ReportSaveToDiskAndLoadFromDisk50Pages()
        {
            Report report;
            Timings timings;

            SmokeTestProperties properties = new SmokeTestProperties()
            {
                Url = "http://localhost:5000/",
                CrawlDepth = 25,
                MaximumPages = 50,
                PauseBetweenRequests = 50,
                SessionCookieName = "demo_website_session",
            };

            using (WebMonitor webMonitor = new WebMonitor(properties))
            {
                webMonitor.Run();

                timings = webMonitor.PageLoadTimings;
                report = webMonitor.Report;

                Assert.AreEqual(2, report.Pages[0].Images.Count);
            }

            string tempPath = Path.Combine(Path.GetTempPath(), "SaveReport.dat");

            report.SaveToFile(tempPath, true);

            Report reportCopy = Report.LoadFromFile(tempPath);

            Assert.AreEqual(report.TotalFastest, reportCopy.TotalFastest);
            Assert.AreEqual(report.TotalAverage, reportCopy.TotalAverage);
            Assert.AreEqual(report.TotalRequests, reportCopy.TotalRequests);
            Assert.AreEqual(report.TotalSlowest, reportCopy.TotalSlowest);
            Assert.AreEqual(report.TotalTime, reportCopy.TotalTime);
            Assert.AreEqual(report.Pages.Count, reportCopy.Pages.Count);
            Assert.IsTrue(report.Pages.Count > 0);
            Assert.IsTrue(report.Pages[0].Content.Equals(reportCopy.Pages[0].Content));
            Assert.IsTrue(report.Images.Count > 0);
            Assert.AreEqual(report.Images[0].Url, reportCopy.Images[0].Url);
            Assert.IsTrue(report.Pages[0].Images[0].Bytes.SequenceEqual(reportCopy.Pages[0].Images[0].Bytes));
            Assert.AreEqual(50u, timings.Requests);
        }

        [TestMethod]

        public void ReportSaveToDiskAndLoadFromDisk500Pages()
        {
            Report report;
            Timings timings;

            SmokeTestProperties properties = new SmokeTestProperties()
            {
                Url = "https://localhost:5001/",
                CrawlDepth = 25,
                MaximumPages = 500,
                PauseBetweenRequests = 50,
                SessionCookieName = "demo_website_session",
            };

            using (WebMonitor webMonitor = new WebMonitor(properties))
            {
                webMonitor.Run();

                timings = webMonitor.PageLoadTimings;
                report = webMonitor.Report;

                Assert.AreEqual(2, report.Pages[0].Images.Count);
            }

            string tempPath = Path.Combine(Path.GetTempPath(), "SaveReport.dat");

            report.SaveToFile(tempPath, true);

            Report reportCopy = Report.LoadFromFile(tempPath);

            Assert.AreEqual(report.TotalFastest, reportCopy.TotalFastest);
            Assert.AreEqual(report.TotalAverage, reportCopy.TotalAverage);
            Assert.AreEqual(report.TotalRequests, reportCopy.TotalRequests);
            Assert.AreEqual(report.TotalSlowest, reportCopy.TotalSlowest);
            Assert.AreEqual(report.TotalTime, reportCopy.TotalTime);
            Assert.AreEqual(report.Pages.Count, reportCopy.Pages.Count);
            Assert.IsTrue(report.Pages.Count > 0);
            Assert.IsTrue(report.Pages[0].Content.Equals(reportCopy.Pages[0].Content));
            Assert.IsTrue(report.Images.Count > 0);
            Assert.AreEqual(report.Images[0].Url, reportCopy.Images[0].Url);
            Assert.IsTrue(report.Pages[0].Images[0].Bytes.SequenceEqual(reportCopy.Pages[0].Images[0].Bytes));
            Assert.AreEqual(500u, timings.Requests);
        }

        [TestMethod]

        public void ReportSaveToDiskAndLoadFromDiskAllPages()
        {
            Report report;
            Timings timings;

            SmokeTestProperties properties = new SmokeTestProperties()
            {
                Url = "http://localhost:5000/",
                CrawlDepth = 10,
                MaximumPages = 10000,
                PauseBetweenRequests = 0,
                SessionCookieName = "demo_website_session",
            };

            using (WebMonitor webMonitor = new WebMonitor(properties))
            {
                webMonitor.Run();

                timings = webMonitor.PageLoadTimings;
                report = webMonitor.Report;

                Assert.AreEqual(2, report.Pages[0].Images.Count);
            }

            string tempPath = Path.Combine(Path.GetTempPath(), "SaveReport.dat");

            report.SaveToFile(tempPath, true);

            Report reportCopy = Report.LoadFromFile(tempPath);

            Assert.AreEqual(report.TotalFastest, reportCopy.TotalFastest);
            Assert.AreEqual(report.TotalAverage, reportCopy.TotalAverage);
            Assert.AreEqual(report.TotalRequests, reportCopy.TotalRequests);
            Assert.AreEqual(report.TotalSlowest, reportCopy.TotalSlowest);
            Assert.AreEqual(report.TotalTime, reportCopy.TotalTime);
            Assert.AreEqual(report.Pages.Count, reportCopy.Pages.Count);
            Assert.IsTrue(report.Pages.Count > 0);
            Assert.IsTrue(report.Pages[0].Content.Equals(reportCopy.Pages[0].Content));
            Assert.IsTrue(report.Images.Count > 0);
            Assert.AreEqual(report.Images[0].Url, reportCopy.Images[0].Url);
            Assert.IsTrue(report.Pages[0].Images[0].Bytes.SequenceEqual(reportCopy.Pages[0].Images[0].Bytes));
            Assert.AreEqual(50u, timings.Requests);
        }
    }
}
