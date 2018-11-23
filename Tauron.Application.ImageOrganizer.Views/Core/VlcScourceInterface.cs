using System.ComponentModel;
using System.IO;
using Tauron.Application.ImageOrganizer.UI.Video;
using Vlc.DotNet.Core;
using Vlc.DotNet.Wpf;

namespace Tauron.Application.ImageOrganizer.Views.Core
{
    public class VlcScourceInterface : IVideoSourceProvider
    {
        private class AudioInterface : IAudio
        {
            private readonly IAudioManagement _audio;

            public AudioInterface(IAudioManagement audio) => _audio = audio;

            public bool IsMute
            {
                get => _audio.IsMute;
                set => _audio.IsMute = value;
            }
        }

        private class MediaPlayerInterface : IMediaPlayer
        {
            private readonly VlcMediaPlayer _player;

            public MediaPlayerInterface(VlcMediaPlayer player) => _player = player;

            public void Stop() => _player.Stop();

            public IAudio Audio => new AudioInterface(_player.Audio);

            public void Play(Stream media, string options) => _player.Play(media, options);
        }

        private readonly VlcVideoSourceProvider _sourceProvider;

        public VlcScourceInterface(VlcVideoSourceProvider sourceProvider) => _sourceProvider = sourceProvider;

        public void Dispose() => _sourceProvider.Dispose();

        public event PropertyChangedEventHandler PropertyChanged
        {
            add => _sourceProvider.PropertyChanged += value;
            remove => _sourceProvider.PropertyChanged -= value;
        }

        public IMediaPlayer MediaPlayer => new MediaPlayerInterface(_sourceProvider.MediaPlayer);

        public object VideoSource => _sourceProvider.VideoSource;

        public void CreatePlayer(DirectoryInfo basePath) => _sourceProvider.CreatePlayer(basePath);
    }
}