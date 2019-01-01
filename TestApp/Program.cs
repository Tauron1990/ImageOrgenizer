

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using WindowsFormsApplication1;
using CefSharp;
using CefSharp.OffScreen;
using Microsoft.EntityFrameworkCore;
using Tauron;
using Tauron.Application.ImageOrganizer.BL.Provider.Impl;
using Tauron.Application.ImageOrganizer.Data;
using Tauron.Application.ImageOrganizer.Data.Entities;

namespace TestApp
{

    class MemoryStreamResponseFilter : IResponseFilter
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

            var dataOutLenght = dataOut.Length;
            var dataInLenght = dataIn.Length;
            var readCount = (int)Math.Min(dataOutLenght, dataInLenght);

            dataInRead = readCount;
            dataOutWritten = readCount;

            var buffer = new byte[readCount];

            var readCountReal = dataIn.Read(buffer, 0, readCount);
            if (readCountReal != readCount)
                return FilterStatus.Error;

            _memoryStream.Write(buffer, 0, buffer.Length);
            dataOut.Write(buffer, 0, buffer.Length);

            return FilterStatus.Done;
            //dataInRead = dataIn.Length;
            //dataOutWritten = Math.Min(dataInRead, dataOut.Length);

            ////Important we copy dataIn to dataOut
            //dataIn.CopyTo(dataOut);

            ////Copy data to stream
            //dataIn.Position = 0;
            //dataIn.CopyTo(_memoryStream);

            //return FilterStatus.Done;
        }

        void IDisposable.Dispose()
        {
            _memoryStream.Dispose();
            _memoryStream = null;
        }

        public byte[] Data => _memoryStream.ToArray();
    }
    class RequestHandler : IRequestHandler
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
                File.WriteAllBytes("test.mp4", data);
            }
        }
    }
    static class Program
    {
        public static readonly string BasePath = AppDomain.CurrentDomain.BaseDirectory;

        [STAThread]
        static void Main(string[] args)
        {
            HResultDeciphering desc = new HResultDeciphering(0x800401D0);

            DownloadHandler.FilePath.DeleteDirectory(true);
            string cachePath = Path.Combine(BasePath, "Cache");
            cachePath.CreateDirectoryIfNotExis();

            CefSettings settings = new CefSettings
            {
                BrowserSubprocessPath = Path.Combine(BasePath, "x86", "CefSharp.BrowserSubprocess.exe"),
                PersistSessionCookies = true,
                CachePath = cachePath
            };

            Cef.Initialize(settings);
            
            var browser = new ChromiumWebBrowser
            {
                Size = new System.Drawing.Size(1920, 1080),
                DownloadHandler = new DownloadHandler(),
                RequestHandler = new RequestHandler()
            };

            
            Thread.Sleep(2000);
            while (!browser.IsBrowserInitialized) { }

            browser.Load(@"https://chan.sankakucomplex.com/post/show/7427239");

            

            //browser.ScreenshotAsync(true).Result.Save("website.png");

            //var frame = browser.GetMainFrame();
            //var source = frame.GetSourceAsync().Result;

            //var prow = new SankakuBaseProvider();
            //prow.Load(source);
            //if (prow.CanRead())
            //{
            //    var test = prow.GetDownloadUrl();
            //    browser.Load(test);
            //    Thread.Sleep(10000);
            //    while (browser.IsLoading) { }

            //    var frame2 = browser.GetMainFrame();
            //    var source2 = frame.GetSourceAsync().Result;
            //}



            //string path = Path.Combine(@"I:\Sankaku Safe Test", "Main.imgdb");

            //using (var db = new DatabaseImpl(path))
            //{
            //    Console.WriteLine(db.Downloads.Count(d => d.DownloadStade == DownloadStade.Failed));
            //}
            //using (var db = new DatabaseImpl(path))
            //{
            //    StringBuilder commandBuilder = new StringBuilder();
            //    List<string> ids = new List<string>();

            //    var temp = db.Images.OrderBy(i => i.Name).ToArray();

            //    foreach (var suElements in Split(temp, 7000).Select(sel => sel.ToArray()))
            //    {
            //        ids.Clear();
            //        commandBuilder.Clear();

            //        commandBuilder//.AppendLine("BEGIN TRANSACTION;")
            //            .AppendLine("Update Images")
            //            .AppendLine("SET SortOrder = Case Id");

            //        for (var index = 0; index < suElements.Length; index++)
            //        {
            //            var entity = suElements[index];
            //            ids.Add(entity.Id.ToString());
            //            commandBuilder.AppendLine($"    WHEN {entity.Id} THEN '{index}'");
            //        }

            //        commandBuilder.AppendLine("END").Append("WHERE Id IN(");

            //        commandBuilder.Append(string.Join(", ", ids));

            //        commandBuilder.AppendLine(")");

            //        //commandBuilder.AppendLine("COMMIT;");

            //        using (var trans = db.Database.BeginTransaction())
            //        {
            //            var command = commandBuilder.ToString();
            //            var count = db.Database.ExecuteSqlCommand(new RawSqlString(command));
            //            trans.Commit();
            //        }
            //    }


            //}

            //UPDATE cityd
            //SET time_zone = CASE locId
            //    WHEN 173567 THEN '-7.000000'
            //WHEN 173568 THEN '-8.000000'
            //WHEN 173569 THEN '-6.000000'
            //WHEN 173570 THEN '-5.000000'
            //WHEN 173571 THEN '-6.000000'
            //END
            //    WHERE   locId IN(173567, 173568, 173569, 173570, 173571)

            Console.Write("Fertig");
            Console.ReadKey();
            browser.Dispose();
            Cef.Shutdown();
        }


        public static IEnumerable<IEnumerable<T>> Split<T>(T[] array, int size)
        {
            for (var i = 0; i < (float) array.Length / size; i++)
            {
                yield return array.Skip(i * size).Take(size);
            }
        }
    }
}
