using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebSmokeTest.Engine;

namespace WebSmokeTest.UnitTests
{
    [TestClass]
    public class AnalyseDocumentTests
    {
        [TestMethod]
        public void LoadHomePageRunAnalysys()
        {
            Shared.Classes.ThreadManager.Initialise();
            string path = Path.GetFullPath(Directory.GetCurrentDirectory() + "..\\..\\..\\..\\TestDocs\\");

            Report report = Report.LoadFromFile(Path.Combine(path, "HomePagePluginManagerWebsite.dat"));
            List<PageReport> pages = new List<PageReport>();
            pages.AddRange(report.Pages);
            report.Pages.Clear();

            pages.ForEach(p => report.PageAdd(p, null));

            while (!report.AnalysisComplete)
            {
                Thread.Sleep(50);
            }

            Assert.AreEqual(244, pages[0].NodeCount);
        }
    }
}
