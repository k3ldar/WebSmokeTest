namespace SmokeTest.Engine
{
    public class WebMonitorBeforePageParseArgs
    {
        public WebMonitorBeforePageParseArgs(string uri, PageType pageType, string masterPage) { Page = uri; PageType = pageType; MasterPage = masterPage; }
        public string Page { get; private set; }
        public PageType PageType { get; private set; }
        public string MasterPage { get; private set; }
    }
}
