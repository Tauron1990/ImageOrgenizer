using System;
using System.IO;

namespace Tauron.Application.ImageOrganizer.UI.Video
{
    public interface IMediaPlayer
    {
        void Stop();
        IAudio Audio { get; }
        IDisposable Play(Stream media, string options);
    }
}