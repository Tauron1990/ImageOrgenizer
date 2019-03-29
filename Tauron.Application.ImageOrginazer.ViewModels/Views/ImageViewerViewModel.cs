using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Tauron.Application.Commands;
using Tauron.Application.ImageOrganizer;
using Tauron.Application.ImageOrganizer.BL;
using Tauron.Application.ImageOrganizer.BL.Services;
using Tauron.Application.ImageOrganizer.UI;
using Tauron.Application.ImageOrginazer.ViewModels.Core;
using Tauron.Application.ImageOrginazer.ViewModels.Resources;
using Tauron.Application.ImageOrginazer.ViewModels.Views.Models;
using Tauron.Application.Ioc;
using Tauron.Application.Models;

using IVideoSourceProvider = Tauron.Application.ImageOrganizer.UI.IVideoSourceProvider;
using Timer = System.Timers.Timer;
using CanClick = System.Func<object, bool>;
using Click = System.Action<object>;

namespace Tauron.Application.ImageOrginazer.ViewModels.Views
{
    [ExportViewModel(AppConststands.ImageViewer)]
    [Shared]
    public class ImageViewerViewModel : MainViewControllerBase, IDisposable, IVideoSourceProvider
    {
        private class TagOperator : IDisposable
        {
            private UIObservableCollection<TagElement> _collection;
            private readonly BlockingCollection<Action> _actions = new BlockingCollection<Action>();

            public TagOperator(UIObservableCollection<TagElement> collection) => _collection = collection;

            public void Start()
            {
                Task.Factory.StartNew(Runner, TaskCreationOptions.LongRunning);
            }

            private void Runner()
            {
                foreach (var action in _actions.GetConsumingEnumerable())
                {
                    if(_actions.Count < 0) continue;

                    action();
                }
            }

            public void Add(ImageData data, CanClick canClick, Click click)
            {
                _collection?.Clear();

                _actions.Add(() =>
                {
                    var coll = _collection;
                    if(coll == null) return;

                    using (coll.BlockChangedMessages())
                    {
                        foreach (var dataTag in data.Tags
                            .Select(td => new TagElement(td))
                            .GroupBy(te => te.Type?.Color)
                            .OrderBy(g => g.Key).SelectMany(g => g))
                        {
                            dataTag.Click = new SimpleCommand(canClick, click, dataTag);
                            _collection?.Add(dataTag);
                        }
                    }
                });
            }

            public void Dispose()
            {
                Interlocked.Exchange(ref _collection, null);
                _actions.CompleteAdding();
                Thread.Sleep(500);
                _actions.Dispose();
            }
        }

        private const string RepeatOption = "--repeat";
        private readonly Timer _saveTimer;
        private TagOperator _tagOperator;

        private string _currentProfileName;
        private string _errorMessage;

        private string _navigatorText = string.Empty;
        private string _oldNavigatorText;

        private ImageOrganizer.UI.Video.IVideoSourceProvider _sourceProvider;
        private bool _viewError;
        private string _programmTitle;
        private bool _imageMenuEnabeld;
        private VideoManager _videoManager;
        //private bool _queueShow;
        private double _lockScreenOpacity;

        public ImageViewerViewModel()
        {
            _tagCanClickDel = CanTagClick;
            _tagClickDel = TagClick;

            _saveTimer = new Timer();
            _saveTimer.Elapsed += SaveCallback;
            _saveTimer.AutoReset = false;
            _saveTimer.Interval = TimeSpan.FromMinutes(1).TotalMilliseconds;
            _videoManager = new VideoManager();

            ParseFilterString();
        }

        public ImageOrganizer.UI.Video.IVideoSourceProvider VideoSource
        {
            get => _sourceProvider;
            set
            {
                if (_sourceProvider != null)
                {
                    _sourceProvider.SourceChangedEvent -= SourceProviderOnSourceChanged;
                    _sourceProvider.Dispose();
                }
                _sourceProvider = value;
                if (_sourceProvider != null)
                    _sourceProvider.SourceChangedEvent += SourceProviderOnSourceChanged;

                //if(_queueShow)
                //    ShowImage();
            }
        }

        [InjectModel(AppConststands.FullScreenModelName)]
        public FullScreenModel FullScreenModel { get; set; }

        [InjectModel(AppConststands.OptrationManagerModel)]
        public OperationManagerModel OperationManagerModel { get; set; }

        [InjectModel(AppConststands.LockScreenModel)]
        public LockScreenManagerModel LockScreen { get; set; }

        [Inject]
        public IImageService Operator { get; set; }

        [Inject]
        public IDBSettings Settings { get; set; }

        [InjectModel(AppConststands.ImageManagerModel)]
        public ImageViewerModel ViewerModel { get; set; }

        public override void ExitView()
        {
            OnScreenOnLockEvent();
            LockEvent = null;
            UnlockEvent = null;
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public bool ViewError
        {
            get => _viewError;
            set => SetProperty(ref _viewError, value);
        }

        public override UIObservableCollection<TagElement> Tags { get; } = new UIObservableCollection<TagElement>();
        public override UIObservableCollection<TagFilterElement> NavigatorItems { get; } = new UIObservableCollection<TagFilterElement>();

        public override string NavigatorText
        {
            get => _navigatorText;
            set => SetProperty(ref _navigatorText, value);
        }

        public override string ProgrammTitle => _programmTitle;

        public override bool ImageMenuEnabeld
        {
            get => _imageMenuEnabeld;
            set => SetProperty(ref _imageMenuEnabeld, value);
        }

        public double LockScreenOpacity
        {
            get => _lockScreenOpacity;
            set => SetProperty(ref _lockScreenOpacity, value);
        }

        private bool _isSidebarEnabled;
        private bool _isNavigatorEnabled;
        private bool _isMainControlEnabled;

        public override bool IsSidebarEnabled => _isSidebarEnabled;
        public override bool IsNavigatorEnabled => _isNavigatorEnabled;
        public override bool IsMainControlEnabled => _isMainControlEnabled;

        public override string ControlButtonLabel { get; } = UIResources.ImageViewer_Button_Reset;

        public override void OnClick()
        {
            NavigatorText = string.Empty;
            ParseFilterString();
            ResetView(ViewerModel.CreateProfileData());
        }

        public override void Next() => ShowImage(ViewerModel.Next);

        public override void Back() => ShowImage(ViewerModel.Previous);

        public override bool CanBack() => ViewerModel.Index > 1 && ViewerModel.CurrentPager != ImageViewerModel.RandomPager;

        public override void RefreshNavigatorText() => _oldNavigatorText = NavigatorText;

        public override void RefreshNavigatorItems()
        {
            if (_oldNavigatorText == NavigatorText) return;

            RefreshAll(ViewerModel.CreateProfileData(), _currentProfileName, true);
        }

        public override void RefreshAll(ProfileData state, string profileName, bool valid)
        {
            Log.Info($"Refress Image to {profileName}");

            SetControl(valid);

            if (!valid)
            {
                string error = UIResources.ImageViewerModel_InvalidDatabase;
                SetError(error);
                SetTitle(error);
                return;
            }

            if(state == null)
                state = new ProfileData(20, 0, null, 0, null, false);

            NavigatorText = state.FilterString;

            ViewerModel.SetFilter(GetTagFilter);

            _currentProfileName = profileName;
            ParseFilterString();
            ResetView(state);

            SetTitle(ViewerModel.CurrentImage?.Name);
        }

        private void SetControl(bool ok)
        {
            _isMainControlEnabled = ok;
            _isNavigatorEnabled = ok;
            _isSidebarEnabled = ok;

            OnPropertyChanged(nameof(IsMainControlEnabled));
            OnPropertyChanged(nameof(IsNavigatorEnabled));
            OnPropertyChanged(nameof(IsSidebarEnabled));
        }

        public override void Closing()
        {
            _saveTimer.Enabled = false;
            SaveProfile();
            //ViewerModel.Shutdowm();
        }

        public override string GetCurrentImageName() => ViewerModel.CurrentImage?.Name;

        private void ParseFilterString()
        {
            using (NavigatorItems.BlockChangedMessages())
            {
                try
                {
                    NavigatorItems.Clear();

                    if (string.IsNullOrEmpty(NavigatorText))
                    {
                        var emptyLabel = UIResources.ImageViewer__Label_Empty;
                        NavigatorItems.Add(new TagFilterElement(new TagElement(emptyLabel, new TagTypeData(emptyLabel, "DarkBlue"), emptyLabel), null));
                        return;
                    }

                    Log.Info($"Parsing Navigator Text: {NavigatorText}");
                    foreach (var element in NavigatorText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(it => it.Replace('_', ' '))
                                            .Select(name => Operator.GetTagFilterElement(name))
                                            .Where(ta => ta != null))
                    {
                        if(NavigatorItems.Count != 0)
                            NavigatorItems.Add(new PlusSeperator());
                        NavigatorItems.Add(new TagFilterElement(element, NavigatorItemsClick));
                    }
                }
                catch (Exception e)
                {
                    if (e.IsCriticalApplicationException())
                        throw;

                    var emptyLabel = UIResources.ImageViewer__Label_Empty;
                    NavigatorItems.Add(new TagFilterElement(new TagElement(emptyLabel, new TagTypeData(emptyLabel, "DarkBlue"), emptyLabel), null));
                }
            }
        }

        private void NavigatorItemsClick(TagElement obj)
        {
            NavigatorText = string.Empty;
            NavigatorText = obj.Name;

            using (OperationManagerModel.EnterOperation())
                RefreshNavigatorItems();
        }

        private void ResetView(ProfileData data)
        {
            ViewerModel.Initialize(data, () => NavigatorText);
            ShowImage(() => ViewerModel.CurrentImage);
        }

        private IEnumerable<string> GetTagFilter()
        {
            if (string.IsNullOrEmpty(NavigatorText)) yield break;

            foreach (var tagFilterElement in NavigatorItems)
            {
                if(tagFilterElement is PlusSeperator) continue;
                yield return tagFilterElement.Tag.Name;
            }
        }

        public void SetError(string message)
        {
            if (message == null)
            {
                ViewError = false;
                ImageMenuEnabeld = true;
            }
            else
            {
                Log.Info($"Image Load Error: {message}");

                ViewError = true;
                ErrorMessage = message;
                ImageMenuEnabeld = false;
            }
        }

        public override bool CanCreateProfile() => _videoManager.ImageData != null;

        private void ShowImage()
        {
            if (ViewerModel.CurrentImage == null) return;

            ShowImage(() => ViewerModel.CurrentImage);
        }

        private readonly CanClick _tagCanClickDel;
        private readonly Click _tagClickDel;
        private void ShowImage(Func<ImageData> dataFunc)
        {
            using (OperationManagerModel.EnterOperation())
            {
                if (CanLockscreen())
                {
                    SetControl(false);
                    return;
                }

                _videoManager.ShowImage(dataFunc, VideoSource, Operator);
                
                if (_videoManager.ViewError)
                {
                    ImageMenuEnabeld = _videoManager.ImageData != null;
                    ViewError = true;
                    ErrorMessage = _videoManager.ErrorMessage;
                    SetTitle(ErrorMessage);
                    ImageMenuEnabeld = false;
                }
                else
                {
                    ViewError = false;
                    ErrorMessage = string.Empty;

                    var data = _videoManager.ImageData;
                    SetTitle(data.Name);

                    _tagOperator.Add(data, _tagCanClickDel, _tagClickDel);

                    _saveTimer.Stop();
                    _saveTimer.Start();
                    ImageMenuEnabeld = true;
                    ViewerModel.IncreaseViewCount();
                }

                SaveProfile();
            }
        }

        private void TagClick(object obj)
        {
            if (!(obj is TagElement tag)) return;

            using (OperationManagerModel.EnterOperation())
            {
                RefreshNavigatorText();
                NavigatorText = NavigatorText + ' ' + tag.Name.Replace(' ', '_');
                RefreshNavigatorItems();
            }
        }

        private bool CanTagClick(object arg) => arg is TagElement tag && NavigatorItems.All(i => i.Tag != null && i.Tag.Name != tag.Name);

        private void SaveCallback(object state, ElapsedEventArgs elapsedEventArgs) => SaveProfile();

        private void SaveProfile()
        {
            if(!IsMainControlEnabled) return; 

            if (string.IsNullOrWhiteSpace(_currentProfileName))
            {
                _currentProfileName = "Current";
                Settings.LastProfile = "Current";
            }

            var data = ViewerModel.CreateProfileData();
            Settings.ProfileDatas[_currentProfileName] = data;
        }

        private void SourceProviderOnSourceChanged(object e) => FullScreenModel.Image = e;

        private void SetTitle(string title)
        {
            _programmTitle = title;
            OnPropertyChangedExplicit(nameof(ProgrammTitle));
        }

        public void Dispose()
        {
            _tagOperator.Dispose();
            _saveTimer.Dispose();
            _sourceProvider?.Dispose();
        }

        [CommandTarget]
        public void Lockscreen()
        {
            using (OperationManagerModel.EnterOperation())
            {
                Log.Info("Unlock Screen");

                SetControl(true);
                LockScreenOpacity = 0;
                OnUnlockEvent();
                LockScreen.OnLockscreenReset();
                ShowImage();
            }
        }

        [CommandTarget]
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        public bool CanLockscreen() => LockScreenOpacity == 1;

        public event Action LockEvent;

        public event Action<IVideoSourceProvider> UnlockEvent;
        
        public override void BuildCompled()
        {
            _tagOperator = new TagOperator(Tags);
            _tagOperator.Start();

            LockScreenOpacity = 1;
            LockScreen.LockEvent += OnScreenOnLockEvent;

            ViewerModel.ResetEvent += (sender, args) =>
            {
                QueueWorkitem(() =>
                {
                    using (OperationManagerModel.EnterOperation())
                        RefreshAll(ViewerModel.CreateProfileData(true), null, true);
                });
            };
        }

        private void OnScreenOnLockEvent()
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if(LockScreenOpacity == 1) return;

            using (OperationManagerModel.EnterOperation())
            {
                Log.Info("Log Screen");
                SetControl(false);
                LockScreenOpacity = 1;
                _videoManager.MediaDispose();
                OnLockEvent();
            }

            InvalidateRequerySuggested();
        }

        private void OnUnlockEvent() => UiSynchronize.Synchronize.Invoke(() => UnlockEvent?.Invoke(this));

        private void OnLockEvent() => UiSynchronize.Synchronize.Invoke(() => LockEvent?.Invoke());

        public override void PrepareDeleteImage() => _videoManager.StreamDispose();
    }
}