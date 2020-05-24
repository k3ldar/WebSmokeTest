using SmokeTest.Shared;

namespace SmokeTest.Settings.Classes
{
    public sealed class HomeCardConfiguration : HomeCard
    {
        public override string Title => "Test Configurations";

        public override string Image => "/img/TestConfiguration.png";

        public override string Description => "View Test Configurations";

        public override string Url => "/Configuration/";

        public override bool PartialView => false;

        public override int SortOrder => -100;
    }
}
