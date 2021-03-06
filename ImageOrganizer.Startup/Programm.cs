﻿using System;
//using System.IO;
//using CefSharp;
//using CefSharp.OffScreen;
//using Tauron;
using Tauron.Application;

namespace ImageOrganizer.Startup
{
    public static class Programm
    {
        private static readonly string BasePath = AppDomain.CurrentDomain.BaseDirectory;

        [STAThread]
        [LoaderOptimization(LoaderOptimization.SingleDomain)]
        public static void Main()
        {
            //string cachePath = Path.Combine(BasePath, "Cache");

            //var di = new DirectoryInfo(cachePath);
            //if (di.Exists)
            //{
            //    try
            //    {
            //        if (di.CreationTime + TimeSpan.FromDays(40) < DateTime.Now)
            //        {
            //            di.Delete(true);
            //            di.CreateResult();
            //        }
            //    }
            //    catch (IOException) { }
            //}
            //else
            //    di.CreateResult();

            //string userData = Path.Combine(BasePath, "UserData");
            //userData.CreateDirectoryIfNotExis();

            //CefSettings settings = new CefSettings
            //{
            //    UserDataPath = userData,
            //    BrowserSubprocessPath = Path.Combine(BasePath, "x86", "CefSharp.BrowserSubprocess.exe"),
            //    PersistSessionCookies = true,
            //    CachePath = cachePath
            //};


            //var initialize = Cef.Initialize(settings);
            //if (!initialize)
            //    return;

            try
            {
                WpfApplication.Run<App>();
            }
            finally
            {
                //Cef.Shutdown();
            }
        }
    }
}