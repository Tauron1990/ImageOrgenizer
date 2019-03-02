using System;

namespace Tauron.Application.ImageOrganizer.BL.Provider
{
    public interface IDownloadManager
    {
        event EventHandler<ProviderLockChangeEventArgs> ProviderLockChangeEvent; 
        bool IsPaused { get; }
        event EventHandler<DownloadChangedEventArgs> DownloadChangedEvent;
        void Start();
        void Pause();
        void ShutDown();
    }
}