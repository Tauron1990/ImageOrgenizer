using System;

namespace ImageOrganizer.BL.Provider
{
    public class DownloadChangedEventArgs : EventArgs
    {
        public DownloadAction DownloadAction { get; }

        public DownloadItem DownloadItem { get; }

        public DownloadChangedEventArgs(DownloadAction downloadAction, DownloadItem downloadItem)
        {
            DownloadAction = downloadAction;
            DownloadItem = downloadItem;
        }
    }
}