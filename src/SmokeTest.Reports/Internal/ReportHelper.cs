﻿using System;
using System.IO;
using SmokeTest.Engine;
using SmokeTest.Shared;
using SmokeTest.Shared.Interfaces;
using Newtonsoft.Json;
using System.Collections.Generic;
using SmokeTest.Shared.Engine;
using System.Linq;
using su = Shared.Utilities;

namespace SmokeTest.Reports.Internal
{
    public class ReportHelper : IReportHelper
    {
        #region Private Members

        private readonly IScheduleHelper _scheduleHelper;
        private readonly ILoadData _loadData;
        private readonly ISaveData _saveData;
        private readonly string _dataPath;
        private readonly List<ReportSummary> _reportSummary;

        #endregion Private Members

        #region Constructors

        public ReportHelper(IScheduleHelper scheduleHelper, ILoadData loadData, ISaveData saveData)
        {
            _scheduleHelper = scheduleHelper ?? throw new ArgumentNullException(nameof(scheduleHelper));
            _loadData = loadData ?? throw new ArgumentNullException(nameof(loadData));
            _saveData = saveData ?? throw new ArgumentNullException(nameof(saveData));

            _reportSummary = new List<ReportSummary>();
            _dataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "SmokeTest");

            InitialLoadTestData();
        }

        #endregion Constructors

        #region IReportHelper Methods

        public void AddReport(in Report report)
        {
            if (report == null)
                throw new ArgumentNullException(nameof(report));

            LoadReportSummary(report);
        }

        public ReportSummary[] ReportSummary(long testScheduleId, int count)
        {
            List<ReportSummary> Result = _reportSummary.Where(rs => rs.TestId.Equals(testScheduleId))
                .OrderByDescending(o => o.EndTime)
                .Take(count)
                .ToList();

            if (Result.Count > 0)
            {
                double longestTime = Result.Max(rs => rs.TotalTime);

                foreach (ReportSummary summary in Result)
                {
                    summary.TimePercentage = su.Percentage(longestTime, summary.TotalTime);
                }
            }

            return Result.ToArray();
        }

        #endregion IReportHelper Methods

        #region Private Methods

        private void InitialLoadTestData()
        {
            string[] reportfiles = Directory.GetFiles(_dataPath, "*.rpt", SearchOption.AllDirectories);

            foreach (string file in reportfiles)
            {
                Report report = Report.LoadFromFile(file);
                LoadReportSummary(report);
            }
        }

        private void LoadReportSummary(in Report report)
        {
            ReportSummary summary = new ReportSummary(report.TestSchedule, report.StartTime, report.EndTime, 
                report.RunResult, report.TotalRequests);
            _reportSummary.Add(summary);
        }

        #endregion Private Methods
    }
}
