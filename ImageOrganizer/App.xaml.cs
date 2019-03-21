using System;
using System.Collections.Generic;
using System.Windows;
using ImageOrganizer.BrowserImpl;
using NLog;
using Syncfusion.Licensing;
using Syncfusion.Themes.VisualStudio2015.WPF;
using Tauron.Application;
using Tauron.Application.Common.Windows;
using Tauron.Application.Common.Wpf.SplashScreen;
using Tauron.Application.ImageOrganizer;
using Tauron.Application.ImageOrganizer.BL;
using Tauron.Application.ImageOrganizer.Container;
using Tauron.Application.ImageOrganizer.Data;
using Tauron.Application.ImageOrganizer.Views;
using Tauron.Application.ImageOrginazer.ViewModels;
using Tauron.Application.ImageOrginazer.ViewModels.Resources;
using Tauron.Application.IOInterface;

namespace ImageOrganizer
{
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App
    {
        private void App_OnStartup(object sender, StartupEventArgs e)
        {
#if DEBUG
            LogManager.ThrowExceptions = true;
            LogManager.GlobalThreshold = LogLevel.Trace;
#endif

            SyncfusionLicenseProvider.RegisterLicense("NzAyODJAMzEzNjJlMzQyZTMwT3BtK2VKWXQ5RjFYVEVxWXdvMlg4WkhnaTJseWtpMzJINmwrbVBzTTNDOD0=");

            FastStart.ApplicationName = AppConststands.ApplicationName;
            FastStart.MainWindowName = AppConststands.MainWindowName;
            FastStart.Start(this, container =>
            {
                container
                    .AddWindows()
                    //.AddWpf()
                    .AddBase()
                    .AddBussinesLayer()
                    .AddContainer()
                    .AddData()
                    .AddViews()
                    .AddViewModels()
                    .AddIO();
            }, null, action =>
            {
                ResourceManagerProvider.Register(UIResources.ResourceManager, typeof(MainWindow).Assembly);

                List<string> target = new VisualStudio2015SkinHelper().GetDictionaries("MSControls", string.Empty);

                foreach (var theme in target)
                {
                    Resources.MergedDictionaries.Add((ResourceDictionary)LoadComponent(new Uri(theme, UriKind.Relative)));
                }
            }, new RichSplashCore(new AppTitle("Image ", 'O', "rganizer")),
                config =>
                {
                    config.AddRuleForAllLevels(new UILayout());
                });
        }
    }
}
