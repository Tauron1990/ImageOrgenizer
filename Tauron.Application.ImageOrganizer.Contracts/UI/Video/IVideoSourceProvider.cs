using System;
using System.ComponentModel;
using System.IO;

namespace Tauron.Application.ImageOrganizer.UI.Video
{
    public interface IVideoSourceProvider : IDisposable, INotifyPropertyChanged
    {
        IMediaPlayer MediaPlayer { get; }
        object VideoSource { get; }
        void CreatePlayer(DirectoryInfo basePath);
    }
}