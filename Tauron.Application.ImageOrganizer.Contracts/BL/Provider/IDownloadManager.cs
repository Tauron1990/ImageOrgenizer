using System;

namespace Tauron.Application.ImageOrganizer.BL.Provider
{
    public interface IDownloadManager
    {
        bool IsPaused { get; }
        event EventHandler<DownloadChangedEventArgs> DowloandChangedEvent;
        void Start();
        void Pause();
        void ShutDown();
    }
}