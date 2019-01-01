using System;
using Tauron.Application.ImageOrganizer.BL.Provider.Browser;

namespace Tauron.Application.ImageOrganizer.BL.Provider.Impl
{
    public class SankakuDataInterceptor : IDataInterceptor
    {
        private readonly Func<bool> _intercepting;
        private readonly Action<byte[]> _data;

        public SankakuDataInterceptor(Func<bool> intercepting, Action<byte[]> data)
        {
            _intercepting = intercepting;
            _data = data;
        }

        public bool Active => _intercepting();

        public bool Intercept(string url)
        {
            if (!_intercepting()) return url.Contains("chan.sankakucomplex.com");
            return !url.Contains("preview") && (url.Contains("cs.sankakucomplex.com/data") || url.Contains("chan.sankakucomplex.com"));
        }

        public void Data(byte[] data, string url) => _data(data);

        public bool LoadRequest(string url)
        {
            var uri = new Uri(url);
            if(_intercepting())
                return uri.Host != "chan.sankakucomplex.com" && uri.Host == "cs.sankakucomplex.com" && uri.Host != "www.sankakucomplex.com" && url.Contains("data");
            return false;
        }
    }
}