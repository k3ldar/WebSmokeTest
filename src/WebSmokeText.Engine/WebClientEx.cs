using System;
using System.IO;
using System.Net;
using System.Text;
using System.Web;

using Shared.Classes;

namespace SmokeTest.Engine
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

        public bool AllowAutoRedirect { get; set; }

        public new WebHeaderCollection ResponseHeaders { get; set; }

        public WebClientEx()
        {
            AllowAutoRedirect = true;
            Timeout = 200;
            UserAgent = "Mozilla /4.0 (compatible; MSIE 7.0; Windows NT 5.1; .NET CLR 2.0.50727); SmokeTest/v1.0";
            CookieContainer = new CookieContainer();
            RequestTimeOut = 200;// 5000;
        }

        public string PostFormData(Uri address, NVPCodec formValues, out int responseCode)
        {
            return SubmitData("POST", address, "application/x-www-form-urlencoded", formValues.Encode(), out responseCode);
        }

        public string PostJsonData(Uri address, string jsonData, out int responseCode)
        {
            return SubmitData("POST", address, "application/json", jsonData, out responseCode);
        }

        public string PostXmlData(Uri address, string xmlData, out int responseCode)
        {
            return SubmitData("POST", address, "application/xml", xmlData, out responseCode);
        }

        public string SubmitData(string method, Uri address, string contentType, string body, out int responseCode)
        { 
            responseCode = 0;

            if (address == null)
                throw new ArgumentNullException(nameof(address));

            string Result = String.Empty;
            body = body.Trim();
            byte[] data = Encoding.ASCII.GetBytes(body);

            HttpWebRequest objRequest = (HttpWebRequest)WebRequest.Create(address);
            objRequest.Method = method;
            objRequest.Timeout = Timeout;
            objRequest.UserAgent = UserAgent;
            objRequest.ContentType = contentType;
            objRequest.ContentLength = data.Length;


            if (Headers != null)
            {
                foreach (string s in Headers)
                {
                    objRequest.Headers.Add(s, Headers[s]);
                }
            }

            objRequest.Accept = "text/html, application/xhtml+xml, image/jxr, */*";

            try
            {
                using (Stream myWriter = objRequest.GetRequestStream())
                {
                    myWriter.Write(data, 0, data.Length);
                }
            }
            catch (Exception e)
            {
                return e.Message;
            }

            try
            {
                HttpWebResponse webResponse = (HttpWebResponse)objRequest.GetResponse();

                using (StreamReader sr = new StreamReader(webResponse.GetResponseStream()))
                {
                    Result = sr.ReadToEnd();
                }

                responseCode = 200;
            }
            catch (WebException webEx)
            {
                if (webEx.Status == WebExceptionStatus.ProtocolError && webEx.Response is HttpWebResponse)
                {
                    HttpWebResponse response = (HttpWebResponse)webEx.Response;

                    responseCode = (int)response.StatusCode;
                }
            }

            return Result;
        }

        public string GetData(Uri address, out int responseCode)
        {
            if (address == null)
                throw new ArgumentNullException(nameof(address));

            ResponseHeaders = null;

            responseCode = -1;
            try
            {

                WebRequest request = GetWebRequest(address);

                using (WebResponse webResponse = request.GetResponse())
                {

                    using (Stream data = OpenRead(address.ToString()))
                    {
                        StreamReader reader = new StreamReader(data);
                        try
                        {
                            string Result = reader.ReadToEnd();
                            responseCode = 200;
                            ResponseHeaders = webResponse.Headers;

                            if (webResponse != null && webResponse.GetType() == typeof(HttpWebResponse))
                            {
                                ResponseCookies = ((HttpWebResponse)webResponse).Cookies;
                            }

                            return Result;
                        }
                        finally
                        {
                            reader.Close();
                            reader.Dispose();
                        }
                    }
                }
            }
            catch (WebException webEx)
            {
                if (webEx.Status == WebExceptionStatus.ProtocolError && webEx.Response is HttpWebResponse)
                {
                    HttpWebResponse response = (HttpWebResponse)webEx.Response;

                    responseCode = (int)response.StatusCode;
                    ResponseHeaders = response.Headers;
                }
            }

            return String.Empty;
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
                ((HttpWebRequest)request).AllowAutoRedirect = AllowAutoRedirect;
            }

            return request;
        }
    }
}
