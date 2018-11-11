using System;
using System.Reflection;
using System.Windows;

namespace ImageOrganizer
{
    public static class AppConststands
    {
        public const string MainWindowName = "AppMainWindow";

        public const string FullScreenModelName = "FullScreenModel";

        public const string ImageFullScreen = "ImageFullScreen";

        public const string ImageViewer = "ImageViewer";

        public const string OptrationManagerModel = "OperationManagerModel";

        public const string FileImporter = "FileImporter";

        public const string DownloadManager = "DownloadManager";

        public const string DownloadManagerModel = "DownloadManagerModel";

        public const string ImageManagerModel = "ImageManagerModel";

        public const string ProfileManagerName = "profileManager";

        public const string QueryEditorName = "QueryEditor";

        public const string PreviewWindowName = "ImagePreviewWindow";

        public const string CustomQueryControl = "CustomQueryControl";

        public const string ImageEditorName = "ImageEditor";

        public const string ContainerManager = "ContainerManager";

        public static void NotImplemented()
        {
            var method = typeof(Environment).GetMethod("GetResourceString", BindingFlags.NonPublic | BindingFlags.Static, null, new []{typeof(string)}, new []{new ParameterModifier(1), });

            string msg = (string) method.Invoke(null, new object[] {"Arg_NotImplementedException"});
            MessageBox.Show(msg);
        }
    }
}