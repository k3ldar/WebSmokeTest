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
    }
}
