using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
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

            private readonly Func<Action<ImageSource>, VlcMediaPlayer> _player;
            private readonly Action<ImageSource> _source;
            private readonly Action<ImageSource> _cleanUp;
            private readonly Dispatcher _dispatcher;

            private IAudio _audio;

            public MediaPlayerInterface(Func<Action<ImageSource>, VlcMediaPlayer> player, Action<ImageSource> source, Action<ImageSource> cleanUp, Dispatcher dispatcher)
            {
                _player = player;
                _source = source;
                _cleanUp = cleanUp;
                _dispatcher = dispatcher;
            }

            public void Stop() => _player(_source)?.Stop();

            public IAudio Audio => _audio ?? (_audio = new AudioInterface(() => _player(_source)?.Audio));

            public IDisposable Play(Stream media)
            {
                if (!_dispatcher.CheckAccess())
                    return _dispatcher.Invoke(() => Play(media));
                try
                {
                    
                    var source = BitmapFrame.Create(media);
                    _source(source);

                    return new ImageDispose(source, _cleanUp);
                }
                catch (Exception e)
                    when (e is NotSupportedException || e is ArgumentException)
                { }

                var tempPlayer = _player(_source);
                var vlcmedia = tempPlayer.SetMedia(media, "input-repeat=65535");
                tempPlayer.Play();

                return new MediaDispose(vlcmedia, tempPlayer);
            }
        }

        private readonly Action<ImageSource> _sourceAction;
        private readonly Action<ImageSource> _cleanAction;
        private readonly Dispatcher _dispatcher;
        private readonly Func<bool, VlcVideoSourceProvider> _sourceProvider;
        private DirectoryInfo _directoryInfo;

        private IMediaPlayer _mediaPlayer;

        public VlcScourceInterface(Func<bool, VlcVideoSourceProvider> sourceProvider, Action<ImageSource> sourceAction, Action<ImageSource> cleanAction, Dispatcher dispatcher)
        {
            _sourceProvider = sourceProvider;
            _sourceAction = sourceAction;
            _cleanAction = cleanAction;
            _dispatcher = dispatcher;
        }

        public void Dispose() => _sourceProvider(false)?.Dispose();
        
        public event Action<object> SourceChangedEvent;

        public bool ExistPlayer => _directoryInfo != null;

        public IMediaPlayer MediaPlayer 
            => _mediaPlayer ?? (_mediaPlayer = new MediaPlayerInterface(InternalCreatePlayer, NewSource, _cleanAction, _dispatcher));

        public void CreatePlayer(DirectoryInfo basePath) => _directoryInfo = basePath;

        private VlcMediaPlayer InternalCreatePlayer(Action<ImageSource> source)
        {
            var sourceProvider = _sourceProvider(true);
            if (sourceProvider.MediaPlayer != null) return sourceProvider.MediaPlayer;

            sourceProvider.PropertyChanged += (sender, args) => source(sourceProvider.VideoSource);
            sourceProvider.CreatePlayer(_directoryInfo, "--repeat");
            sourceProvider.MediaPlayer.EndReached += (s, e) => Task.Run(() => sourceProvider.MediaPlayer.Play());
            sourceProvider.MediaPlayer.VideoOutChanged += (sender, args) => Task.Run(() => ((VlcMediaPlayer) sender).Audio.IsMute = true);
            
            return sourceProvider.MediaPlayer;
        }

        private void OnSourceChangedEvent(object obj) => SourceChangedEvent?.Invoke(obj);

        private void NewSource(ImageSource source)
        {
            _sourceAction(source);
            OnSourceChangedEvent(source);
        }
    }
}