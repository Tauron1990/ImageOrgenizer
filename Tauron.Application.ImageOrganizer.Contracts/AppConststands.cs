using System;
using System.Reflection;

namespace Tauron.Application.ImageOrganizer
{
    public static class AppConststands
    {
        public const string ApplicationName = "Image Organizer";

        public const string MainWindowName = "AppMainWindow";

        public const string FullScreenModelName = "FullScreenModel";

        public const string ImageFullScreen = "ImageFullScreen";

        public const string ImageViewer = "ImageViewer";

        public const string OptrationManagerModel = "OperationManagerModel";

        public const string FileImporter = "FileImporter";

        public const string DownloadManager = "DownloadManager";

        public const string DownloadManagerModel = "DownloadManagerModel";

        public const string ImageManagerModel = "ImageManagerModel";

        public const string ProfileManagerViewModelName = "ProfileManagerViewModel";

        public const string QueryEditorName = "QueryEditor";

        public const string PreviewWindowName = "ImagePreviewWindow";

        public const string CustomQueryControl = "CustomQueryControl";

        public const string ImageEditorName = "ImageEditor";

        public const string ContainerManager = "ContainerManager";

        public const string OrderedPager = "ImageViewerModel_PagerName_Ordered";
        
        public const string ProviderNon = "ProviderNon";

        public const string LockScreenModel = "LockScreenModel";

        public const string ProfileManagerModelName = "ProfileManagerModel";

        public static void NotImplemented()
        {
            var method = typeof(Environment).GetMethod("GetResourceString", BindingFlags.NonPublic | BindingFlags.Static, null, new []{typeof(string)}, new []{new ParameterModifier(1), });

            string msg = (string) method?.Invoke(null, new object[] {"Arg_NotImplementedException"});
            CommonApplication.Current.Container.Resolve<IDialogFactory>().ShowMessageBox(null, msg, msg, MsgBoxButton.Ok, MsgBoxImage.Warning);
        }
    }
}