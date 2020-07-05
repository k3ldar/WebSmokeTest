using System;

using SmokeTest.Shared.Engine;

namespace SmokeTest.Shared
{
    public interface IReportHelper
    {
        void AddReport(in Report report);

        ReportSummary[] ReportSummary(long testScheduleId, int count);

        double EstimatedRuntime(long testScheduleId);

        Report MostRecentReport(long testScheduleId);
    }
}
