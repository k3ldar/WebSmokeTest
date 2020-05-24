using SmokeTest.Shared;

namespace SmokeTest.Scheduler.Classes
{
    public class HomeCardSchedules : HomeCard
    {
        public override string Title => "Schedule Test";

        public override string Image => "/img/ScheduleTest.png";

        public override string Description => "Schedule Test Runs";

        public override string Url => "/Schedule/";

        public override bool PartialView => false;

        public override int SortOrder => -50;
    }
}
