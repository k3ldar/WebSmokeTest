using System;

using SmokeTest.Scheduler.Controllers;
using SmokeTest.Shared;

namespace SmokeTest.Scheduler.Classes
{
    public class HomeCardRunSchedules : HomeCard
    {
        public override string Title => "Scheduled Tests";

        public override string Image => String.Empty;

        public override string Description => String.Empty;

        public override string Url => $"/{ScheduleController.Name}/{nameof(ScheduleController.RunSchedules)}/";

        public override bool PartialView => true;

        public override int SortOrder => 100;
    }
}
