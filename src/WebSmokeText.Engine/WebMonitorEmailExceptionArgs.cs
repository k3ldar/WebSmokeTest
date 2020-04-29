using System;

namespace WebSmokeTest.Engine
{
    public sealed class WebMonitorEmailExceptionArgs
    {
        public WebMonitorEmailExceptionArgs(Exception error) { Error = error; }
        public Exception Error { get; private set; }
    }
}
