using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Syncfusion.Data.Extensions;
using Tauron.Application.ImageOrganizer.BL.Provider.Browser;

namespace ImageOrganizer.Startup.BrowserImpl
{
    public class BrowserHelper : IBrowserHelper
    {
        private readonly Dictionary<string, IDataInterceptor> _interceptors = new Dictionary<string, IDataInterceptor>();
        private string _source;
        private MemoryStream _cache;
        private ExtendedWebClient _extendedWebClient;

        public string GetSource()
        {
            if (!string.IsNullOrWhiteSpace(_source))
                return _source;

            _cache.Position = 0;
            using (StreamReader reader = new StreamReader(_cache, Encoding.UTF8, true, 1024, true))
                _source = reader.ReadToEnd();

            return _source;
        }

        public bool Load(string url)
        {
            var interceptors = _interceptors.Where(di => di.Value.CanProcess(url)).Select(di => di.Value).ToArray();

            try
            {
                if (_extendedWebClient == null)
                    _extendedWebClient = new ExtendedWebClient(true, 0, new CookieContainer());

                _cache = new MemoryStream(_extendedWebClient.DownloadData(url));
                
                var ok = _cache.Length != 0;

                interceptors.ForEach(di => di.FeddData(_cache.ToArray(), url));

                return ok;
            }
            catch
            {
                return false;
            }
        }

        public void RegisterInterceptor(string name, Func<IDataInterceptor> interceptor)
        {
            if(_interceptors.ContainsKey(name)) return;
            _interceptors[name] = interceptor();
        }

        public void Clear()
        {
            _interceptors.ForEach(di => di.Value.Clear());
            _cache?.Dispose();
            _cache = null;
            _source = null;
        }
    }
}