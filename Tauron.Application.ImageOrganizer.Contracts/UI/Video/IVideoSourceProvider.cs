using System;
using System.ComponentModel;
using System.IO;

namespace Tauron.Application.ImageOrganizer.UI.Video
{
    public interface IVideoSourceProvider : IDisposable
    {
        event Action<object> SourceChangedEvent;
        bool ExistPlayer { get; }
        IMediaPlayer MediaPlayer { get; }
        void CreatePlayer(DirectoryInfo basePath);
    }
}