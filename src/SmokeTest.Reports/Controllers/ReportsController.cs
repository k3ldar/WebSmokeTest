﻿using System;
using System.Collections.Generic;
using System.IO;

using Microsoft.AspNetCore.Mvc;

using SharedPluginFeatures;

using SmokeTest.Reports.Models;
using SmokeTest.Shared;
using SmokeTest.Shared.Engine;
using SmokeTest.Shared.Interfaces;

namespace SmokeTest.Reports.Controllers
{
    public class ReportsController : BaseController
    {
        #region Constants

        public const string Name = "Reports";

        #endregion Constants

        #region Private Members

        private readonly string _dataPath;
        private readonly ILoadData _loadData;
        private readonly ITestRunManager _testRunManager;

        #endregion Private Members

        #region Constructors

        public ReportsController(ITestRunManager testRunManager)
        {
            _testRunManager = testRunManager ?? throw new ArgumentNullException(nameof(testRunManager));
            _dataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "SmokeTest");
        }

        #endregion Constructors

        #region Public Action Methods

        [HttpGet]
        [Route("[controller]/[action]/{reportId}/{testId}")]
        [Breadcrumb(nameof(TestSummary))]
        public IActionResult TestSummary(string reportId, string testId)
        {
            string reportFile = Path.Combine(_dataPath, "Reports", testId, reportId + ".rpt");

            if (!System.IO.File.Exists(reportFile))
            {
                HttpContext.Response.StatusCode = 404;
                return new EmptyResult();
            }

            return View(ConvertReportToReportModel(Report.LoadFromFile(reportFile)));
        }

        [HttpGet]
        [Route("[controller]/[action]/{uniqueRunId}")]
        public IActionResult TestRunProgress(long uniqueRunId)
        {
            TestRunProgressModel model = new TestRunProgressModel(GetModelData(), uniqueRunId);

            return View(model);
        }

        [HttpGet]
        [Route("[controller]/[action]/{uniqueRunId}/{position}")]
        public IActionResult Running(long uniqueRunId, long position)
        {
            ITestRunLogger testRunLogger = new TestRunLogger(uniqueRunId);
            string Result = testRunLogger.RetrieveData(position);
            position += Result.Length;
            
            Result = Result.Replace("\r\n", "<br />").Replace("'", "&apos;");
            bool isRunning = _testRunManager.TestRunning(uniqueRunId);

            return Json(new { content = Result, running = isRunning, position });
        }

        #endregion Public Action Methods

        #region Private Methods

        private TestSummaryModel ConvertReportToReportModel(in Report report)
        {
            List<ErrorDataModel> errorData = new List<ErrorDataModel>();

            foreach (ErrorData item in report.Errors)
            {
                errorData.Add(new ErrorDataModel(item.Error.Message, item.Uri, item.MissingLink, item.OriginatingLink));
            }

            return new TestSummaryModel(
                GetModelData(),
                report.UniqueId,
                report.TestSchedule,
                report.StartTime,
                report.EndTime,
                report.RunResult,
                report.TotalRequests,
                report.TotalSlowest,
                report.TotalFastest,
                report.TotalAverage,
                report.TrimmedAverage,
                report.TotalTime,
                report.Headers,
                errorData,
                report.Cookies,
                report.Forms,
                report.Images,
                report.Pages
                );
        }

        #endregion Private Methods
    }
}
