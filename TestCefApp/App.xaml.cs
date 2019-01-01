using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CefSharp;
using CefSharp.Wpf;
using Tauron;
using TestApp;

namespace TestCefApp
{
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application
    {
        public static readonly string BasePath = AppDomain.CurrentDomain.BaseDirectory;

        private void App_OnExit(object sender, ExitEventArgs e)
        {
            Cef.Shutdown();
        }

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            DownloadHandler.FilePath.DeleteDirectory(true);
            string cachePath = Path.Combine(BasePath, "Cache");
            cachePath.CreateDirectoryIfNotExis();

            CefSettings settings = new CefSettings
            {
                BrowserSubprocessPath = Path.Combine(BasePath, "x86", "CefSharp.BrowserSubprocess.exe"),
                PersistSessionCookies = true,
                CachePath = cachePath
            };

            var temp = Cef.Initialize(settings);

            if(!temp)
                throw new Exception("Cef Not Initilized");
        }
    }
}
