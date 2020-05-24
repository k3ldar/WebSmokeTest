using Microsoft.VisualStudio.TestTools.UnitTesting;

using SharedPluginFeatures;

using SmokeTest.Engine;

namespace SmokeTest.UnitTests
{
    [TestClass]
    public class OpenWebPages
    {
        [TestMethod]
        [Ignore]
        public void OpenPluginManagerWebsiteHomePage()
        {
            SmokeTestProperties properties = new SmokeTestProperties()
            {
                Url = "http://www.pluginmanager.website",
                CrawlDepth = 1,
                MaximumPages = 1
            };

            using (WebMonitor webMonitor = new WebMonitor(properties))
            {
                webMonitor.Run();

                Timings timings = webMonitor.PageLoadTimings;

                Assert.AreEqual(1u, timings.Requests);
                Assert.IsTrue(webMonitor.Report.Pages[0].AnalysisComplete);
            }
        }

        [TestMethod]
        [Ignore]
        public void OpenGoogleWebsiteHomePage()
        {
            SmokeTestProperties properties = new SmokeTestProperties()
            {
                Url = "https://www.google.com/",
                CrawlDepth = 1,
                MaximumPages = 1,
            };

            using (WebMonitor webMonitor = new WebMonitor(properties))
            {
                webMonitor.Run();

                Timings timings = webMonitor.PageLoadTimings;

                Assert.AreEqual(1u, timings.Requests);
            }
        }

        [TestMethod]
        [Ignore]
        public void OpenMicrosoftWebsiteHomePage()
        {
            SmokeTestProperties properties = new SmokeTestProperties()
            {
                Url = "https://www.microsoft.com/",
                CrawlDepth = 1,
                MaximumPages = 1,
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/79.0.3945.130 Safari/537.36",
            };

            using (WebMonitor webMonitor = new WebMonitor(properties))
            {
                webMonitor.Run();

                Timings timings = webMonitor.PageLoadTimings;

                Assert.AreEqual(1u, timings.Requests);
            }
        }
    }
}
