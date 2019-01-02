﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Tauron.Application.Commands;
using Tauron.Application.ImageOrganizer;
using Tauron.Application.ImageOrganizer.BL;
using Tauron.Application.ImageOrganizer.UI;
using Tauron.Application.ImageOrganizer.UI.Video;
using Tauron.Application.ImageOrginazer.ViewModels.Core;
using Tauron.Application.ImageOrginazer.ViewModels.Resources;
using Tauron.Application.ImageOrginazer.ViewModels.Views.Models;
using Tauron.Application.Ioc;
using Tauron.Application.Models;
using IVideoSourceProvider = Tauron.Application.ImageOrganizer.UI.IVideoSourceProvider;

namespace Tauron.Application.ImageOrginazer.ViewModels.Views
{
    [ExportViewModel(AppConststands.ImageViewer)]
    [Shared]
    public class ImageViewerViewModel : MainViewControllerBase, IDisposable, IVideoSourceProvider
    {
        private const string RepeatOption = "--repeat";
        private readonly Timer _saveTimer;

        private string _currentProfileName;
        private string _errorMessage;

        private string _navigatorText = string.Empty;
        private string _oldNavigatorText;

        private ImageOrganizer.UI.Video.IVideoSourceProvider _sourceProvider;
        private bool _viewError;
        private string _programmTitle;
        private bool _imageMenuEnabeld;
        private VideoManager _videoManager;
        private bool _queueShow;
        private double _lockScreenOpacity;

        public ImageViewerViewModel()
        {
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

                if(_queueShow)
                    ShowImage();
            }
        }

        [InjectModel(AppConststands.FullScreenModelName)]
        public FullScreenModel FullScreenModel { get; set; }

        [InjectModel(AppConststands.OptrationManagerModel)]
        public OperationManagerModel OperationManagerModel { get; set; }

        [InjectModel(AppConststands.LockScreenModel)]
        public LockScreenManagerModel LockScreen { get; set; }

        [Inject]
        public IOperator Operator { get; set; }

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

        public override void RefreshNavigatorText() => _oldNavigatorText = NavigatorText;

        public override void RefreshNavigatorItems()
        {
            if (_oldNavigatorText == NavigatorText) return;

            RefreshAll(ViewerModel.CreateProfileData(), _currentProfileName, true);
        }

        public override void RefreshAll(ProfileData state, string profileName, bool valid)
        {
            SetControl(valid);

            if (!valid)
            {
                string error = UIResources.ImageViewerModel_InvalidDatabase;
                SetError(error);
                SetTitle(error);
                return;
            }

            if(state == null)
                state = new ProfileData(0, 0, null, 0, null, false);

            NavigatorText = state.FilterString;

            ViewerModel.SetFilter(GetTagFilter);

            _currentProfileName = profileName;
            ParseFilterString();
            ResetView(state);
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
            ViewerModel.Shutdowm();
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

                    foreach (var element in NavigatorText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(it => it.Replace('_', ' '))
                                            .Select(Operator.GetTagFilterElement)
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
                ViewError = true;
                ErrorMessage = message;
                ImageMenuEnabeld = false;
            }
        }

        public override bool CanCreateProfile() => _videoManager.ImageData != null;

        public override void EnterView() => _queueShow = true;

        private void ShowImage()
        {
            if (ViewerModel.CurrentImage == null) return;

            ShowImage(() => ViewerModel.CurrentImage);
        }

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
                Tags.Clear();

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

                    using (Tags.BlockChangedMessages())
                    {
                        foreach (var dataTag in data.Tags
                            .Select(td => new TagElement(td))
                            .GroupBy(te => te.Type?.Color)
                            .OrderBy(g => g.Key).SelectMany(g => g))
                        {
                            dataTag.Click = new SimpleCommand(CanTagClick, TagClick, dataTag);
                            Tags.Add(dataTag);
                        }
                    }

                    _saveTimer.Stop();
                    _saveTimer.Start();
                    ImageMenuEnabeld = true;
                }
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
            _saveTimer?.Dispose();
            _sourceProvider?.Dispose();
        }

        [CommandTarget]
        public void Lockscreen()
        {
            using (OperationManagerModel.EnterOperation())
            {
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
            LockScreenOpacity = 1;
            LockScreen.LockEvent += OnScreenOnLockEvent;

            ViewerModel.ResetEvent += (sender, args) =>
            {
                QueueWorkitem(() =>
                {
                    using (OperationManagerModel.EnterOperation())
                        RefreshAll(ViewerModel.CreateProfileData(true), null, false);
                });
            };
        }

        private void OnScreenOnLockEvent()
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if(LockScreenOpacity == 1) return;

            using (OperationManagerModel.EnterOperation())
            {
                SetControl(false);
                LockScreenOpacity = 1;
                _videoManager.LockDispose();
                OnLockEvent();
            }
        }

        private void OnUnlockEvent() => UiSynchronize.Synchronize.Invoke(() => UnlockEvent?.Invoke(this));

        private void OnLockEvent() => UiSynchronize.Synchronize.Invoke(() => LockEvent?.Invoke());

        public override void PrepareDeleteImage() => _videoManager.StreamDispose();
    }
}