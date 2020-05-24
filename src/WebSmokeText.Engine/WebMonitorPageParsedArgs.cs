using System;

namespace SmokeTest.Engine
{
    public sealed class WebMonitorPageParsedArgs
    {
        public WebMonitorPageParsedArgs(Uri uri, double timeTaken) { Page = uri; TimeTaken = timeTaken; }

        public Uri Page { get; private set; }

        public double TimeTaken { get; private set; }
    }
}
