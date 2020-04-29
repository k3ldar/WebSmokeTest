using System.Net;


namespace WebSmokeTest.Engine
{
    public sealed class WebMonitorCookieArgs
    {
        public WebMonitorCookieArgs(Cookie cookie) { Cookie = cookie; }

        public Cookie Cookie { get; private set; }
    }
}
