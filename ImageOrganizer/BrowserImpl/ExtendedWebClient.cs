using System;
using System.Net;

namespace ImageOrganizer.BrowserImpl
{
    public class ExtendedWebClient : WebClient
    {
        private bool _enableCookie;

        private string _acceptLanguage;

        private static CookieContainer _cookieJar;

        private int _timeout;

        private string _referer;

        #region ctor

        public ExtendedWebClient(bool enableCompression, int timeout = -1, CookieContainer cookieJar = null)
        {
            EnableCompression = enableCompression;
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            Timeout = timeout > 0 ? timeout : 600000;

            if (cookieJar != null)
            {
                // replace old cookie jar
                CookieJar = cookieJar;
                EnableCookie = true;
            }
        }

        #endregion ctor

        public int Timeout
        {
            get => _timeout;
            set
            {
                if (value < 0) value = 0;
                _timeout = value;
            }
        }

        public bool EnableCookie
        {
            get => _enableCookie;
            set
            {
                if (value && _cookieJar == null) _cookieJar = new CookieContainer();
                _enableCookie = value;
            }
        }

        public bool EnableCompression { get; }

        public string AcceptLanguage
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_acceptLanguage)) _acceptLanguage = "en-GB,en-US;q=0.8,en;q=0.6";
                return _acceptLanguage;
            }
            set => _acceptLanguage = value;
        }

        public CookieContainer CookieJar
        {
            get => _cookieJar ?? (_cookieJar = new CookieContainer());
            private set => _cookieJar = value;
        }

        public string Referer
        {
            get => _referer;
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    _referer = value;
                    Headers.Add("Referer", _referer);
                }
                else
                    Headers.Remove("Referer");
            }
        }

        public string UserAgent => "Mozilla / 5.0(Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/67.0.3396.99 Safari/537.36";

        protected override WebRequest GetWebRequest(Uri address)
        {
            Headers.Add("user-agent", UserAgent);

            var req = base.GetWebRequest(address);

            if (req is HttpWebRequest httpReq)
            {
                httpReq.AllowAutoRedirect = true;
                if (_enableCookie) httpReq.CookieContainer = _cookieJar;
                if (EnableCompression)
                {
                    httpReq.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate");
                    httpReq.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                }

                httpReq.Headers.Add(HttpRequestHeader.AcceptLanguage, AcceptLanguage);
            }

            if (req != null)
                req.Timeout = _timeout;

            return req;
        }

        protected override WebResponse GetWebResponse(WebRequest request, IAsyncResult result)
        {
            var response = base.GetWebResponse(request, result);
            ReadCookies(response);
            return response;
        }

        protected override WebResponse GetWebResponse(WebRequest request)
        {
            var response = base.GetWebResponse(request);
            ReadCookies(response);
            return response;
        }

        private void ReadCookies(WebResponse r)
        {
            if (!(r is HttpWebResponse response) || _cookieJar == null) return;
            var cookies = response.Cookies;
            _cookieJar.Add(cookies);
        }
    }
}