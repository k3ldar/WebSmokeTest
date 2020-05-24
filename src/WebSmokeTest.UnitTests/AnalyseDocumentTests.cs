﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Shared.Classes;

using SmokeTest.Engine;

namespace SmokeTest.UnitTests
{
    [TestClass]
    public class AnalyseDocumentTests
    {
        [TestMethod]
        [Ignore]
        public void LoadHomePageRunAnalysys()
        {
            ThreadManager.Initialise();
            string path = Path.GetFullPath(Directory.GetCurrentDirectory() + "..\\..\\..\\..\\TestDocs\\");

            Report report = Report.LoadFromFile(Path.Combine(path, "HomePagePluginManagerWebsite.dat"));
            List<PageReport> pages = new List<PageReport>();
            pages.AddRange(report.Pages);
            report.Pages.Clear();

            SmokeTestProperties properties = new SmokeTestProperties()
            {
                ClearHtmlDataAfterAnalysis = true,
                ClearImageDataAfterAnalysis = true,
            };

            pages.ForEach(p => report.PageAdd(p, null, properties));

            DateTime startTime = DateTime.Now;

            while (!report.AnalysisComplete)
            {
                Thread.Sleep(50);

                TimeSpan span = DateTime.Now - startTime;

                Assert.IsTrue(span.TotalSeconds < 10);
            }

            Assert.AreEqual(244, pages[0].NodeCount);
        }
    }
}
