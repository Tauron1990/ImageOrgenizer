using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CefSharp;
using CefSharp.Handler;
using TestApp;

namespace TestCefApp
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private class MemoryStreamResponseFilter : IResponseFilter
        {
            private MemoryStream _memoryStream;

            bool IResponseFilter.InitFilter()
            {
                //NOTE: We could initialize this earlier, just one possible use of InitFilter
                _memoryStream = new MemoryStream();

                return true;
            }

            FilterStatus IResponseFilter.Filter(Stream dataIn, out long dataInRead, Stream dataOut, out long dataOutWritten)
            {
                if (dataIn == null)
                {
                    dataInRead = 0;
                    dataOutWritten = 0;

                    return FilterStatus.Done;
                }

                dataInRead = dataIn.Length;
                dataOutWritten = Math.Min(dataInRead, dataOut.Length);

                //Important we copy dataIn to dataOut
                dataIn.CopyTo(dataOut);

                //Copy data to stream
                dataIn.Position = 0;
                dataIn.CopyTo(_memoryStream);

                return FilterStatus.Done;
            }

            void IDisposable.Dispose()
            {
                _memoryStream.Dispose();
                _memoryStream = null;
            }

            public byte[] Data => _memoryStream.ToArray();
        }

        private class RequestHandler : IRequestHandler
        {
            public bool OnBeforeBrowse(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, bool userGesture, bool isRedirect) => false;

            public bool OnOpenUrlFromTab(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, string targetUrl, WindowOpenDisposition targetDisposition, bool userGesture) => false;

            public bool OnCertificateError(IWebBrowser chromiumWebBrowser, IBrowser browser, CefErrorCode errorCode, string requestUrl, ISslInfo sslInfo, IRequestCallback callback)
            {
                callback.Dispose();
                return false;
            }

            public void OnPluginCrashed(IWebBrowser chromiumWebBrowser, IBrowser browser, string pluginPath)
            {
            }

            public CefReturnValue OnBeforeResourceLoad(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IRequestCallback callback)
            {
                Uri uri = new Uri(request.Url);
                if (uri.Host != "chan.sankakucomplex.com" && uri.Host != "cs.sankakucomplex.com" && uri.Host != "www.sankakucomplex.com")
                {
                    callback.Dispose();
                    return CefReturnValue.Cancel;
                }
                callback.Dispose();
                return CefReturnValue.Continue;
            }

            public bool GetAuthCredentials(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, bool isProxy, string host, int port, string realm, string scheme, IAuthCallback callback)
            {
                callback.Dispose();
                return false;
            }

            public bool OnSelectClientCertificate(IWebBrowser chromiumWebBrowser, IBrowser browser, bool isProxy, string host, int port, X509Certificate2Collection certificates, ISelectClientCertificateCallback callback)
            {
                callback.Dispose();
                return false;
            }

            public void OnRenderProcessTerminated(IWebBrowser chromiumWebBrowser, IBrowser browser, CefTerminationStatus status)
            {
                
            }

            public bool CanGetCookies(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request) => true;

            public bool CanSetCookie(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, Cookie cookie) => true;

            public bool OnQuotaRequest(IWebBrowser chromiumWebBrowser, IBrowser browser, string originUrl, long newSize, IRequestCallback callback)
            {
                callback.Dispose();
                return false;
            }

            public void OnResourceRedirect(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IResponse response, ref string newUrl)
            {
            }

            public bool OnProtocolExecution(IWebBrowser chromiumWebBrowser, IBrowser browser, string url) => true;

            public void OnRenderViewReady(IWebBrowser chromiumWebBrowser, IBrowser browser)
            {
            }

            public bool OnResourceResponse(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IResponse response) => false;

            private readonly Dictionary<ulong, MemoryStreamResponseFilter> _responseDictionary = new Dictionary<ulong, MemoryStreamResponseFilter>();

            public IResponseFilter GetResourceResponseFilter(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, IResponse response)
            {
                if (request.Url.Contains("preview") || !request.Url.Contains("cs.sankakucomplex.com/data"))
                    return null;

                var url = request.Url;
                var dataFilter = new MemoryStreamResponseFilter();
                _responseDictionary.Add(request.Identifier, dataFilter);
                return dataFilter;
            }

            public void OnResourceLoadComplete(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IResponse response, UrlRequestStatus status, long receivedContentLength)
            {
                if (_responseDictionary.TryGetValue(request.Identifier, out var filter))
                {
                    var data = filter.Data; //This returns a byte[]
                    File.WriteAllBytes("test.jpg", data);
                }
            }
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            WebBrowser.RequestHandler = new RequestHandler();
            WebBrowser.DownloadHandler = new DownloadHandler();
        }
    }
}
