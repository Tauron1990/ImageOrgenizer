using System;
using System.Threading;
using CefSharp;
using CefSharp.OffScreen;
using Tauron.Application.ImageOrganizer.BL.Provider.Browser;

namespace ImageOrganizer.Startup.BrowserImpl
{
    public class BrowserImpl : IBrowserHelper, IDisposable
    {
        private readonly ChromiumWebBrowser _webBrowser;
        private readonly InterceptingRequestHandler _handler;
        private readonly AutoResetEvent _loadHelper = new AutoResetEvent(false);
        private string _source;

        public BrowserImpl()
        {
            _handler = new InterceptingRequestHandler();
            _webBrowser = new ChromiumWebBrowser{ RequestHandler = _handler };
            _webBrowser.FrameLoadEnd += (sender, args) =>
            {
                if (args.Frame.IsMain)
                    _loadHelper.Set();
            };

            while (!_webBrowser.IsBrowserInitialized) { }
        }

        public string GetSource() => _source ?? (_source =_webBrowser.GetSourceAsync().Result);

        public void Load(string url)
        {
            _source = null;
            if (_handler.IsActive)
                _handler.ManualResetEvent.Reset();
            _webBrowser.Load(url);
            _loadHelper.WaitOne();
            _handler.ManualResetEvent.WaitOne();
        }

        public void RegisterInterceptor(string name, Func<IDataInterceptor> interceptor) => _handler.RegisterInterceptor(name, interceptor);

        public void Dispose()
        {
            _webBrowser?.Dispose();
            _loadHelper?.Dispose();
            _handler?.Dispose();
        }
    }
}