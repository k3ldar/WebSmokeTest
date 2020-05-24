namespace SmokeTest.Engine
{
    public sealed class PageAnalysis
    {
        #region Constructors

        public PageAnalysis()
        {
            Header = new HeaderAnalysis();
            Body = new BodyAnalysis();
        }

        #endregion Constructors

        #region Properties

        public HeaderAnalysis Header { get; set; }

        public BodyAnalysis Body { get; set; }

        #endregion Properties

        #region Internal Methods

        internal void Clear()
        {
            Header.Clear();
            Body.Clear();
        }

        #endregion Internal Methods
    }
}
