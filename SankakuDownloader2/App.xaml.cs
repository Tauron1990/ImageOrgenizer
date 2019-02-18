using System.Windows;
using Syncfusion.Licensing;
using Tauron.Application;

namespace SankakuDownloader2
{
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App
    {
        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            SyncfusionLicenseProvider.RegisterLicense("NTA5NTdAMzEzNjJlMzQyZTMwbE93VS9sSW9Md3c3anNsanlRSlV4WlNPb2Zrb3k5dmdIcHlHdjZmbitQND0=");
            FastStart.Start(this);
        }
    }
}
