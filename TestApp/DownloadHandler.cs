﻿using System.IO;
using CefSharp;
using Tauron;

namespace TestApp {
    class DownloadHandler : IDownloadHandler
    {
        public static readonly string FilePath = Path.Combine(Program.BasePath, "Files");

        public void OnBeforeDownload(IWebBrowser chromiumWebBrowser, IBrowser browser, DownloadItem downloadItem, IBeforeDownloadCallback callback)
        {
            if(!downloadItem.IsValid ||  callback.IsDisposed) return;

            FilePath.CreateDirectoryIfNotExis();

            callback.Continue(Path.Combine(FilePath, downloadItem.SuggestedFileName), false);
        }

        public void OnDownloadUpdated(IWebBrowser chromiumWebBrowser, IBrowser browser, DownloadItem downloadItem, IDownloadItemCallback callback)
        {

        }
    }
}