﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using ImageOrganizer.Resources;
using JetBrains.Annotations;
using NLog;
using NLog.Config;
using Syncfusion.Licensing;
using Syncfusion.Themes.VisualStudio2015.WPF;
using Tauron;
using Tauron.Application;
using Tauron.Application.Common.BaseLayer.BusinessLayer;
using Tauron.Application.Implement;
using Tauron.Application.Implementation;
using Tauron.Application.Ioc;
using Tauron.Application.Views;

namespace ImageOrganizer
{
    internal class App : WpfApplication, ISingleInstanceApp
    {
        public App()
            : base(true)
        {
            SyncfusionLicenseProvider.RegisterLicense("MjcwMTFAMzEzNjJlMzMyZTMwUy9NTnVGSU9scHNKNW0rSU51VG1ON3ZBQ2ozWGpqZXUwKzJ2RitJM1pzTT0=");
        }

        public bool SignalExternalCommandLineArgs(IList<string> args)
        {
            MainWindow?.Focus();
            return true;
        }

        //public static void Setup([NotNull] Mutex mutex, [NotNull] string channelName)
        //{
        //    if (mutex == null) throw new ArgumentNullException(nameof(mutex));
        //    if (channelName == null) throw new ArgumentNullException(nameof(channelName));

        //    Run<App>(app => SingleInstance<App>.InitializeAsFirstInstance(mutex, channelName, app), CultureInfo.InstalledUICulture);
        //}

        protected override void ConfigSplash()
        {
            var dic = new PackUriHelper().Load<ResourceDictionary>("StartResources.xaml");

            CurrentWpfApplication.Resources = dic;

            var control = new ContentControl
            {
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                Height = 236,
                Width = 414,
                Content = dic["MainLabel"]
            };

            SplashMessageListener.CurrentListner.SplashContent = control;
            SplashMessageListener.CurrentListner.MainLabelForeground = "Black";
            SplashMessageListener.CurrentListner.MainLabelBackground = dic["MainLabelbackground"];
        }

        protected override IWindow DoStartup(CommandLineProcessor prcessor)
        {
            var temp = ViewManager.Manager.CreateWindow(AppConststands.MainWindowName);
            MainWindow = temp;

            CurrentWpfApplication.Dispatcher.Invoke(() =>
            {
                Current.MainWindow = temp;
                CurrentWpfApplication.MainWindow = (Window) temp.TranslateForTechnology();
            });
            return temp;
        }

        protected override void LoadCommands()
        {
            base.LoadCommands();
            CommandBinder.AutoRegister = true;
        }

        protected override void LoadResources()
        {
            SimpleLocalize.Register(UIResources.ResourceManager, typeof(App).Assembly);

            List<string> target = new VisualStudio2015SkinHelper().GetDictionaries("MSControls", string.Empty);

            foreach (var theme in target)
            {
                var app = Application.Current;
                app.Resources.MergedDictionaries.Add((ResourceDictionary)Application.LoadComponent(new Uri(theme, UriKind.Relative)));
            }

            //Application.Current.Resources.MergedDictionaries.Add(
            //                                                                    (ResourceDictionary)
            //                                                                    Application.LoadComponent(new PackUriHelper().GetUri("Theme.xaml", typeof(App).Assembly.FullName,
            //                                                                                                                                        false)));
        }

        public override string GetdefaultFileLocation()
        {
            return GetDicPath();
        }

        protected override void MainWindowClosed(object sender, EventArgs e)
        {
        }

        private static string GetDicPath()
        {
            return
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
                    .CombinePath("Tauron\\Image Orgenizer");
        }

        protected override void Fill(IContainer container)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));

            var resolver = new ExportResolver();

            resolver.AddPath(AppDomain.CurrentDomain.BaseDirectory, "*.dll", SearchOption.AllDirectories, false);

            container.Register(resolver);
        }

        protected override void ConfigurateLagging(LoggingConfiguration config)
        {
            base.ConfigurateLagging(config);

#if DEBUG
            LogManager.ThrowExceptions = true;
            LogManager.GlobalThreshold = LogLevel.Trace;
#endif
        }
    }
}