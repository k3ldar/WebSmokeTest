using System;
using System.IO;
using System.Net;

namespace WebSmokeTest.Engine
{
    internal sealed class WebClientEx : WebClient
    {
        internal CookieContainer CookieContainer { get; set; }

        internal CookieCollection Cookies(Uri uri)
        {
            return CookieContainer.GetCookies(uri);
        }

        internal CookieCollection ResponseCookies { get; private set; }

        internal string UserAgent { get; set; }

        public int Timeout { get; set; }

        public int RequestTimeOut { get; set; }

        public WebClientEx()
        {
            Timeout = 200;
            UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1; .NET CLR 2.0.50727); WebSmokeTest/v1.0";
            CookieContainer = new CookieContainer();
            RequestTimeOut = 200;// 5000;
        }

        public string GetData(Uri address)
        {
            if (address == null)
                throw new ArgumentNullException(nameof(address));

            WebRequest request = GetWebRequest(address);

            using (WebResponse webResponse = request.GetResponse())
            {
                if (webResponse != null && webResponse.GetType() == typeof(HttpWebResponse))
                {
                    ResponseCookies = ((HttpWebResponse)webResponse).Cookies;
                }

                using (Stream data = OpenRead(address.ToString()))
                {
                    StreamReader reader = new StreamReader(data);
                    try
                    {
                        return reader.ReadToEnd();
                    }
                    finally
                    {
                        reader.Close();
                        reader.Dispose();
                    }
                }
            }
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest request = base.GetWebRequest(address);

            if (request.GetType() == typeof(HttpWebRequest))
            {
                ((HttpWebRequest)request).CookieContainer = CookieContainer;
                ((HttpWebRequest)request).UserAgent = UserAgent;
                ((HttpWebRequest)request).Timeout = RequestTimeOut;
                ((HttpWebRequest)request).KeepAlive = false;
                ((HttpWebRequest)request).UnsafeAuthenticatedConnectionSharing = true;
            }

            return request;
        }
    }
}
