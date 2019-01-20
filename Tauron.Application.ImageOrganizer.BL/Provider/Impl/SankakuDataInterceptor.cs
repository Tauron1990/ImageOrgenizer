using System;
using System.Net;
using Tauron.Application.ImageOrganizer.BL.Provider.Browser;

namespace Tauron.Application.ImageOrganizer.BL.Provider.Impl
{
    public class SankakuDataInterceptor : IDataInterceptor
    {
        private const string Domain = @"chan.sankakucomplex.com";

        //private readonly Func<bool> _intercepting;
        private readonly Action<byte[]> _data;

        public SankakuDataInterceptor(Func<bool> intercepting, Action<byte[]> data)
        {
            //_intercepting = intercepting;
            _data = data;
        }

        //public bool Active => _intercepting();

        //public bool Intercept(string url)
        //{
        //    if (!_intercepting()) return url.Contains("chan.sankakucomplex.com");
        //    return !url.Contains("preview") && (url.Contains("cs.sankakucomplex.com/data") || url.Contains("chan.sankakucomplex.com"));
        //}

        //public void Data(byte[] data, string url) => _data(data);

        //public bool LoadRequest(string url)
        //{
        //    var uri = new Uri(url);
        //    if (_intercepting())
        //        return uri.Host != "chan.sankakucomplex.com" && uri.Host == "cs.sankakucomplex.com" && uri.Host != "www.sankakucomplex.com" && url.Contains("data");
        //    return false;
        //}

        public bool CanProcess(string url) => url.Contains("chan.sankakucomplex.com") || url.Contains("cs.sankakucomplex.com");

        public void Prepare(WebRequest request)
        {
            HttpWebRequest webRequest = (HttpWebRequest)request;

            webRequest.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/71.0.3578.98 Safari/537.36";
            webRequest.AllowAutoRedirect = true;
            webRequest.CookieContainer = new CookieContainer(10);
            webRequest.CookieContainer.Add(new Cookie("locale", "en", string.Empty, domain: Domain));
            webRequest.CookieContainer.Add(new Cookie("hide-news-ticker", "1", string.Empty, Domain));
            webRequest.CookieContainer.Add(new Cookie("auto_page", "1", string.Empty, Domain));
            webRequest.CookieContainer.Add(new Cookie("hide_resized_notice", "1", string.Empty, Domain));
            webRequest.CookieContainer.Add(new Cookie("blacklisted_tags", "", string.Empty, Domain));
        }

        public void FeddData(byte[] data, string url) => _data(data);

        public void Clear()
        {
        }
    }
}