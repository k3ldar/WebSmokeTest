using System;
using System.Collections.Generic;

namespace SmokeTest.Scheduler.Models
{
    public sealed class TestRunViewModels
    {
        public TestRunViewModels(in List<TestRunViewModel> runModels, in string position)
        {
            RunModels = runModels ?? throw new ArgumentNullException(nameof(runModels));
            Position = position ?? String.Empty;

            foreach (TestRunViewModel model in runModels)
            {
                Queued += model.QueuePositions.Length;
                Running += model.UniqueRunIdentifiers.Length;

                if (model.LastRunResult == Shared.LastRunResult.Error)
                {
                    Errors++;
                }
                else if (model.LastRunResult == Shared.LastRunResult.Warning)
                {
                    Warnings++;
                }
                else if (model.LastRunResult == Shared.LastRunResult.Success)
                {
                    Success++;
                }
                else if (model.LastRunResult == Shared.LastRunResult.NotRun)
                {
                    NotRun++;
                }
            }
        }

        public List<TestRunViewModel> RunModels { get; private set; }

        public string OverallStatus { get; set; }

        public int Errors { get; private set; }

        public int Warnings { get; private set; }

        public int Success { get; private set; }

        public int NotRun { get; private set; }

        public int Running { get; private set; }

        public int Queued { get; private set; }

        public string Position { get; private set; }

        #region Public Methods

        public string FormatElapsedTime(TimeSpan elapsed)
        {
            System.Text.StringBuilder elapsedtime = new System.Text.StringBuilder();

            if (elapsed.Hours > 0)
                elapsedtime.AppendFormat("{0} hours", elapsed.Hours);

            if (elapsed.Minutes > 0)
            {
                if (elapsedtime.Length > 0)
                    elapsedtime.Append(", ");

                elapsedtime.AppendFormat("{0} minutes", elapsed.Minutes);
            }

            if (elapsed.Seconds > 0)
            {
                if (elapsedtime.Length > 0)
                    elapsedtime.Append(" and ");

                elapsedtime.AppendFormat("{0} seconds", elapsed.Seconds);
            }

            if (elapsedtime.Length == 0)
                elapsedtime.Append("??");

            return elapsedtime.ToString();
        }

        #endregion Public Methods
    }
}
