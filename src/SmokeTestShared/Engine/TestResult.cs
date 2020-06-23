namespace SmokeTest.Shared.Engine
{
    public sealed class TestResult
    {
        #region Constructors

        public TestResult()
        {

        }

        #endregion Constructors

        #region Properties

        public string Name { get; set; }

        public int Position { get; set; }

        public decimal TimeTaken { get; set; }

        public int ErrorCount { get; set; }

        public int Index { get; set; }

        public bool DiscoveredTest { get; set; }

        public bool Enabled { get; set; }

        public string InternalName { get; set; }

        #endregion Properties
    }
}
