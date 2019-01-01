using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
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
            private class MediaDispose : IDisposable
            {
                private readonly VlcMedia _media;
                private readonly VlcMediaPlayer _player;

                public MediaDispose(VlcMedia media, VlcMediaPlayer player)
                {
                    _media = media;
                    _player = player;
                }

                public void Dispose()
                {
                    _player.Stop();
                    _media?.Dispose();
                }
            }

            private readonly VlcMediaPlayer _player;

            public MediaPlayerInterface(VlcMediaPlayer player) => _player = player;

            public void Stop() => _player.Stop();

            public IAudio Audio => new AudioInterface(_player.Audio);

            public IDisposable Play(Stream media)
            {
                var vlcmedia = _player.SetMedia(media, "input-repeat=65535");
                _player.Play();

                return new MediaDispose(vlcmedia, _player);
            }
        }

        private readonly VlcVideoSourceProvider _sourceProvider;

        public VlcScourceInterface(VlcVideoSourceProvider sourceProvider) => _sourceProvider = sourceProvider;

        public void Dispose() => _sourceProvider.Dispose();

        public event PropertyChangedEventHandler PropertyChanged
        {
            add => _sourceProvider.PropertyChanged += value;
            remove => _sourceProvider.PropertyChanged -= value;
        }

        public IMediaPlayer MediaPlayer => _sourceProvider.MediaPlayer == null ? null : new MediaPlayerInterface(_sourceProvider.MediaPlayer);

        public object VideoSource => _sourceProvider.VideoSource;

        public void CreatePlayer(DirectoryInfo basePath)
        {
            _sourceProvider.CreatePlayer(basePath, "--repeat");
            _sourceProvider.MediaPlayer.EndReached += (s, e) => Task.Run(() => _sourceProvider.MediaPlayer.Play());
            _sourceProvider.MediaPlayer.VideoOutChanged += (sender, args) => Task.Run(() => ((VlcMediaPlayer) sender).Audio.IsMute = true);
        }
    }
}