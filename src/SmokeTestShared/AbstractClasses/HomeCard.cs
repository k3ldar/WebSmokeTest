namespace SmokeTest.Shared
{
    public abstract class HomeCard
    {
        public abstract string Title { get; }

        public abstract string Image { get; }

        public abstract string Description { get; }

        public abstract string Url { get; }

        public abstract bool PartialView { get; }

        public abstract int SortOrder { get; }
    }
}
