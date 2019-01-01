using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using CefSharp;
using Tauron.Application.ImageOrganizer.BL.Provider.Browser;

namespace ImageOrganizer.Startup.BrowserImpl
{
    internal class InterceptingRequestHandler : IRequestHandler, IDisposable
    {
        public ManualResetEvent ManualResetEvent { get; } = new ManualResetEvent(true);

        bool IRequestHandler.OnBeforeBrowse(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, bool userGesture, bool isRedirect) => false;

        bool IRequestHandler.OnOpenUrlFromTab(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, string targetUrl, WindowOpenDisposition targetDisposition, bool userGesture) => false;

        bool IRequestHandler.OnCertificateError(IWebBrowser chromiumWebBrowser, IBrowser browser, CefErrorCode errorCode, string requestUrl, ISslInfo sslInfo, IRequestCallback callback)
        {
            callback.Dispose();
            return false;
        }

        void IRequestHandler.OnPluginCrashed(IWebBrowser chromiumWebBrowser, IBrowser browser, string pluginPath) { }

        public bool IsActive => _dataInterceptors.Any(di => di.Value.Active);

        CefReturnValue IRequestHandler.OnBeforeResourceLoad(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IRequestCallback callback) => _dataInterceptors.Select(e => e.Value).Any(i => i.Intercept(request.Url)) ? CefReturnValue.Continue : CefReturnValue.Cancel;

        bool IRequestHandler.GetAuthCredentials(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, bool isProxy, string host, int port, string realm, string scheme, IAuthCallback callback)
        {
            callback.Dispose();
            return false;
        }

        bool IRequestHandler.OnSelectClientCertificate(IWebBrowser chromiumWebBrowser, IBrowser browser, bool isProxy, string host, int port, X509Certificate2Collection certificates,
            ISelectClientCertificateCallback callback)
        {
            callback.Dispose();
            return false;
        }

        void IRequestHandler.OnRenderProcessTerminated(IWebBrowser chromiumWebBrowser, IBrowser browser, CefTerminationStatus status) { }

        bool IRequestHandler.CanGetCookies(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request) => true;

        bool IRequestHandler.CanSetCookie(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, Cookie cookie) => true;

        bool IRequestHandler.OnQuotaRequest(IWebBrowser chromiumWebBrowser, IBrowser browser, string originUrl, long newSize, IRequestCallback callback)
        {
            callback.Dispose();
            return false;
        }

        void IRequestHandler.OnResourceRedirect(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IResponse response, ref string newUrl) { }

        bool IRequestHandler.OnProtocolExecution(IWebBrowser chromiumWebBrowser, IBrowser browser, string url) => true;

        void IRequestHandler.OnRenderViewReady(IWebBrowser chromiumWebBrowser, IBrowser browser) { }

        bool IRequestHandler.OnResourceResponse(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IResponse response) => false;

        private readonly Dictionary<ulong, MemoryStreamResponseFilter> _responseDictionary = new Dictionary<ulong, MemoryStreamResponseFilter>();
        private readonly Dictionary<string, IDataInterceptor> _dataInterceptors = new Dictionary<string, IDataInterceptor>();

        IResponseFilter IRequestHandler.GetResourceResponseFilter(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, IResponse response)
        {
            List<(string, IDataInterceptor)> list = new List<(string, IDataInterceptor)>();

            foreach (var interceptor in _dataInterceptors.Where(di => di.Value.LoadRequest(request.Url)).Select(di => di.Value)) list.Add((request.Url, interceptor));

            if (list.Count == 0) return null;

            var dataFilter = new MemoryStreamResponseFilter(list);
            _responseDictionary.Add(request.Identifier, dataFilter);
            return dataFilter;
        }

        void IRequestHandler.OnResourceLoadComplete(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IResponse response, UrlRequestStatus status, long receivedContentLength)
        {
            if (!_responseDictionary.TryGetValue(request.Identifier, out var filter)) return;

            filter.FeedData();
            _responseDictionary.Remove(request.Identifier);
            if (_responseDictionary.Count == 0) ManualResetEvent.Set();
        }

        public void RegisterInterceptor(string name, Func<IDataInterceptor> interceptorFunc)
        {
            if (_dataInterceptors.ContainsKey(name)) return;

            _dataInterceptors[name] = interceptorFunc();
        }

        public void Dispose() => ManualResetEvent?.Dispose();
    }
}