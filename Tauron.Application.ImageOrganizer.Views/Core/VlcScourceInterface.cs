using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Tauron.Application.ImageOrganizer.UI.Video;
using Vlc.DotNet.Core;
using Vlc.DotNet.Wpf;

namespace Tauron.Application.ImageOrganizer.Views.Core
{
    public class VlcScourceInterface : IVideoSourceProvider
    {
        private class AudioInterface : IAudio
        {
            private readonly Func<IAudioManagement> _audio;

            public AudioInterface(Func<IAudioManagement> audio) => _audio = audio;

            public bool IsMute
            {
                get => _audio()?.IsMute ?? true;
                set
                {
                    var audio = _audio();
                    if (audio == null) return;
                    audio.IsMute = value;
                }
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

            private class ImageDispose : IDisposable
            {
                private readonly ImageSource _source;
                private readonly Action<ImageSource> _cleanUp;

                public ImageDispose(ImageSource source, Action<ImageSource> cleanUp)
                {
                    _source = source;
                    _cleanUp = cleanUp;
                }

                public void Dispose() => _cleanUp(_source);
            }

            private readonly Func<VlcMediaPlayer> _player;
            private readonly Action<ImageSource> _source;
            private readonly Action<ImageSource> _cleanUp;
            private readonly Func<ImageSource> _playerSource;

            public MediaPlayerInterface(Func<VlcMediaPlayer> player, Action<ImageSource> source, Action<ImageSource> cleanUp, Func<ImageSource> playerSource)
            {
                _player = player;
                _source = source;
                _cleanUp = cleanUp;
                _playerSource = playerSource;
            }

            public void Stop() => _player()?.Stop();

            public IAudio Audio => new AudioInterface(() => _player()?.Audio);

            public IDisposable Play(Stream media)
            {
                try
                {
                    var source = BitmapFrame.Create(media);
                    _source(source);

                    return new ImageDispose(source, _cleanUp);
                }
                catch (Exception e)
                    when (e is NotSupportedException || e is ArgumentException)
                { }

                var tempPlayer = _player();
                var vlcmedia = tempPlayer.SetMedia(media, "input-repeat=65535");
                tempPlayer.Play();
                _source(_playerSource());

                return new MediaDispose(vlcmedia, tempPlayer);
            }
        }

        private readonly Action<ImageSource> _sourceAction;
        private readonly Action<ImageSource> _cleanAction;

        private readonly Func<VlcVideoSourceProvider> _sourceProvider;

        public VlcScourceInterface(Func<VlcVideoSourceProvider> sourceProvider, Action<ImageSource> sourceAction, Action<ImageSource> cleanAction)
        {
            _sourceProvider = sourceProvider;
            _sourceAction = sourceAction;
            _cleanAction = cleanAction;
        }

        public void Dispose() => _sourceProvider()?.Dispose();
        
        public event Action<object> SourceChangedEvent;

        public bool ExistPlayer => _sourceProvider()?.MediaPlayer != null;

        public IMediaPlayer MediaPlayer 
            => _sourceProvider()?.MediaPlayer == null ? 
                null : new MediaPlayerInterface(() => _sourceProvider().MediaPlayer, NewSource, _cleanAction, () => _sourceProvider().VideoSource);

        public void CreatePlayer(DirectoryInfo basePath)
        {
            var sourceProvider = _sourceProvider();
            sourceProvider.CreatePlayer(basePath, "--repeat");
            sourceProvider.MediaPlayer.EndReached += (s, e) => Task.Run(() => sourceProvider.MediaPlayer.Play());
            sourceProvider.MediaPlayer.VideoOutChanged += (sender, args) => Task.Run(() => ((VlcMediaPlayer) sender).Audio.IsMute = true);
        }

        private void OnSourceChangedEvent(object obj) => SourceChangedEvent?.Invoke(obj);

        private void NewSource(ImageSource source)
        {
            _sourceAction(source);
            OnSourceChangedEvent(source);
        }
    }
}