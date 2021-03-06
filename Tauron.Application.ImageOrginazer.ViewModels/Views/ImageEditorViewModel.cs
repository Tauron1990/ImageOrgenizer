﻿using System;
using System.Collections.Generic;
using System.Linq;
using Tauron.Application.ImageOrganizer;
using Tauron.Application.ImageOrganizer.BL;
using Tauron.Application.ImageOrganizer.BL.Provider;
using Tauron.Application.ImageOrganizer.BL.Services;
using Tauron.Application.ImageOrganizer.UI;
using Tauron.Application.ImageOrginazer.ViewModels.Resources;
using Tauron.Application.ImageOrginazer.ViewModels.Views.ImageEditorHelper;
using Tauron.Application.ImageOrginazer.ViewModels.Views.Models;
using Tauron.Application.Ioc;
using Tauron.Application.Models;

namespace Tauron.Application.ImageOrginazer.ViewModels.Views
{
    [ExportViewModel(AppConststands.ImageEditorName), Shared]
    public class ImageEditorViewModel : MainViewControllerBase
    {
        private class ValidationHelper<TDataItem> : IDisposable
        {
            private readonly IDataGrid _sender;
            private readonly IRowValidatingArgs _e;
            private bool _isOk = true;

            public TDataItem Data { get; private set; }

            public ValidationHelper(IDataGrid sender, IRowValidatingArgs e)
            {
                _sender = sender;
                _e = e;
            }

            public bool NeedValidate()
            {
                if (!_sender.IsAddNewIndex(_e.RowIndex)) return false;
                if (!(_e.RowData is TDataItem data)) return false;

                Data = data;
                return true;
            }

            public void Assert(Func<TDataItem, (string Name, bool IsOk)> assertFunc, Func<string> message)
            {
                var result = assertFunc(Data);

                if (result.IsOk) return;

                _isOk = false;
                _e.ErrorMessages[result.Name] = message();
            }

            public void Dispose() => _e.IsValid = _isOk;
        }

        private int _selectedTab;

        [Inject]
        public IEditorService Operator { get; set; }

        [InjectModel(AppConststands.OptrationManagerModel)]
        public OperationManagerModel OperationManager { get; set; }

        [Inject]
        public IProviderManager ProviderManager { get; set; }

        ////public Func<ICollectionViewAdv> ViewFunc { get; set; }

        ////public ICollectionViewAdv View => ViewFunc();

        public override string ProgrammTitle { get; } = UIResources.ImageEditor_ProgrammTitle;
        public override bool IsSidebarEnabled { get; }
        public override bool IsNavigatorEnabled { get; }
        public override bool IsMainControlEnabled { get; }

        public override string ControlButtonLabel { get; } = UIResources.Common_Label_Back;

        public override void OnClick() => MainWindowViewModel.ShowImagesAction();

        public ImageCollection ImageDatas { get; private set; }
        public TagTypeCollection TagTypeCollection { get; private set; }
        public TagCollection TagCollection { get; private set; }

        public int SelectedTab
        {
            get => _selectedTab;
            set => SetProperty(ref _selectedTab, value);
        }

        public override void EnterView()
        {
            using (OperationManager.EnterOperation())
            {
                SelectedTab = 0;
                PrepareTagType();
                PrepareTag();
                PrepareImages();
            }

        }

        public override void ExitView()
        {
            ClearTagType();
            ClearTag();
            ClearImages();
        }

        public override void BuildCompled()
        {
            ImageDatas = new ImageCollection(Operator);
            TagTypeCollection = new TagTypeCollection(Operator);
            TagCollection = new TagCollection(Operator);
            TagCollection.CheckInsertEvent += OnTagInsert;

            Providers = ProviderManager.Ids;
        }

        #region TagType
        
        private void PrepareTagType()
        {
            DataTrigger.TagTypeChangedEvent += OnTagTypeChangedEvent;
            TagTypeCollection.AddRange(Operator.GetAllData(DataType.TagTypeData).TagTypeDatas);
        }

        private void OnTagTypeChangedEvent(object sender, EntityUpdate<TagTypeData> e)
        {
            switch (e.TriggerType)
            {
                case TriggerType.Update:
                    TagTypeCollection.ForceUpdateAsync(e.Data);
                    break;
                case TriggerType.Delete:
                    TagTypeCollection.Remove(e.Data);
                    break;
                case TriggerType.Insert:
                    if(!TagTypeCollection.Contains(e.Data))
                        TagTypeCollection.Add(e.Data);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ClearTagType()
        {
            DataTrigger.TagTypeChangedEvent -= OnTagTypeChangedEvent;
            TagTypeCollection.Clear();
        }

        [EventTarget(Synchronize = true, Converter = typeof(ControlConverter))]
        private void TagTypeValidating(IDataGrid sender, IRowValidatingArgs e)
        {
            
            using (var valid = new ValidationHelper<TagTypeDataItem>(sender, e))
            {
                if (!valid.NeedValidate()) return;

                valid.Assert(d => (nameof(d.Name), !string.IsNullOrWhiteSpace(d.Name)), () => UIResources.ImageEditor_ImageValidate_Name_Null);
                valid.Assert(d => (nameof(d.Name), !TagTypeCollection.Contains(d.Create())), () => UIResources.ImageEditor_TagTypeValidate_Name);
                valid.Assert(d => (nameof(d.Color), d.Color.ToLower() != "transparent"), () => UIResources.ImageEditor_TagTypeValidate_Color);
            }
        }

        [EventTarget(Synchronize = true, Converter = typeof(ControlConverter))]
        private void TagTypeRowInitiating(IDataGrid sender, IAddRowInitiatingArgs e) => e.NewObject = new TagTypeDataItem();

        #endregion

        #region Tags

        private List<TagData> _original = new List<TagData>();
        private bool _specificEditMode;
        private ImageDataItem _editElement;
        private string _statusText = UIResources.ImageEditor_TagGridStatus_All;

        public string StatusText
        {
            get => _statusText;
            set => SetProperty(ref _statusText, value);
        }

        private void PrepareTag()
        {
            DataTrigger.TagChangedEvent += OnTagChangedEvent;
            var result = Operator.GetAllData(DataType.TagData);
            _original.AddRange(result.TagDatas);
            TagCollection.AddRange(_original);
        }

        private void OnTagChangedEvent(object sender, EntityUpdate<TagData> e)
        {
            switch (e.TriggerType)
            {
                case TriggerType.Update:
                    if (_specificEditMode)
                    {
                        var index = _original.IndexOf(e.Data);
                        if (index != -1)
                            _original[index] = e.Data;
                    }

                    TagCollection.ForceUpdateAsync(e.Data);
                    break;
                case TriggerType.Delete:
                    _original.Remove(e.Data);
                    TagCollection.Remove(e.Data);
                    break;
                case TriggerType.Insert:
                    if(_original.Contains(e.Data)) return;
                    _original.Add(e.Data);
                    if (!_specificEditMode)
                        TagCollection.Add(e.Data);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ClearTag()
        {
            DataTrigger.TagChangedEvent -= OnTagChangedEvent;
            TagCollection.Clear();
            _original.Clear();
            _specificEditMode = false;
            _editElement = null;
        }

        [EventTarget(Synchronize = true, Converter = typeof(ControlConverter))]
        private void TagValidating(IDataGrid sender, IRowValidatingArgs e)
        {
            using (var valid = new ValidationHelper<TagDataItem>(sender, e))
            {
                if (!valid.NeedValidate()) return;
                if(_specificEditMode && _original.Any(c => c.Name == valid.Data.Name)) return;

                valid.Assert(td => (nameof(td.Name), !string.IsNullOrWhiteSpace(td.Name)), () => UIResources.ImageEditor_ImageValidate_Name_Null);
                valid.Assert(td => (nameof(td.Name), !_original.Contains(td.Create())), () => UIResources.ImageEditor_TagValidate_Name);
            }
        }

        [EventTarget(Synchronize = true, Converter = typeof(ControlConverter))]
        private void TagAddNew(IDataGrid grid, IAddRowInitiatingArgs e)
        {
            if (_specificEditMode)
                ((TagDataItem) e.NewObject).Tags = _original.Select(t => t.Name).ToList();
        }

        [CommandTarget]
        public void SaveTags() => ExitSpecialMode();

        [CommandTarget]
        public bool CanSaveTags() => _specificEditMode;

        private void OnTagInsert(object sender, InsertCheckEventArgs<TagDataItem> e)
        {
            if(!_specificEditMode) return;

            e.OverrideAdd = _original.Any(o => o.Name == e.EditorItem.Name);
        }

        private void EnterSpecialMode(ImageDataItem data)
        {
            if (_specificEditMode) return;
            using (OperationManager.EnterOperation())
            {
                TagCollection.Clear();
                _editElement = data;
                TagCollection.AddRange(data.Tags);
                _specificEditMode = true;
                EnableImageGrid = false;

                StatusText = _editElement.Name;
                SelectedTab = 1;
            }
        }

        private void ExitSpecialMode()
        {
            if(!_specificEditMode) return;
            using (OperationManager.EnterOperation())
            {
                using (_editElement.Tags.BlockChangedMessages())
                {
                    _editElement.Tags.Clear();
                    _editElement.Tags.AddRange(TagCollection.Select(t => t.Create()));
                }

                Operator.UpdateImage(new []{ _editElement.Create() });

                TagCollection.Clear();
                _editElement = null;
                _specificEditMode = false;
                EnableImageGrid = true;

                StatusText = UIResources.ImageEditor_TagGridStatus_All;
                TagCollection.AddRange(_original);
                SelectedTab = 0;
            }
        }

        #endregion

        #region Images

        private bool _enableImageGrid;
        private ImageDataItem _selectedImageItem;

        public bool EnableImageGrid
        {
            get => _enableImageGrid;
            set => SetProperty(ref _enableImageGrid, value);
        }

        public IEnumerable<string> Providers { get; set; }

        public ImageDataItem SelectedImageItem
        {
            get => _selectedImageItem;
            set => SetProperty(ref _selectedImageItem, value);
        }

        private void PrepareImages()
        {
            EnableImageGrid = true;
            DataTrigger.ImageChangedEvent += OnImageChangedEvent;
            var result = Operator.GetAllData(DataType.ImageData);
            ImageDatas.AddRange(result.ImageDatas);
        }

        private void OnImageChangedEvent(object sender, EntityUpdate<ImageData> e)
        {
            switch (e.TriggerType)
            {
                case TriggerType.Update:
                    ImageDatas.ForceUpdateAsync(e.Data);
                    break;
                case TriggerType.Delete:
                    ImageDatas.Remove(e.Data);
                    break;
                case TriggerType.Insert:
                    ImageDatas.Add(e.Data);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        [EventTarget(Synchronize = true, Converter = typeof(ControlConverter))]
        public void ImageValidating(IDataGrid sender, IRowValidatingArgs e)
        {
            using (var valid = new ValidationHelper<ImageDataItem>(sender, e))
            {
                if(!valid.NeedValidate()) return;

                valid.Assert(d => (nameof(d.Name), !string.IsNullOrWhiteSpace(d.Name)), () => UIResources.ImageEditor_ImageValidate_Name_Null);
                valid.Assert(d => (nameof(d.Name), ImageDatas.DataCollection.All(id => id.Name != d.Name)), () => UIResources.ImageEditor_ImageValidate_Name_Duplicate);
            }
        }

        private void ClearImages() => ImageDatas.Clear();

        [EventTarget(Synchronize = true, Converter = typeof(ControlConverter))]
        public void ImageCellDoubleTapped(IDataGrid data, ICellDoubleTappedArgs args)
        {
            if(args.MappingName == "Tags.Count" && args.Record is ImageDataItem item && !item.IsNew)
                EnterSpecialMode(item);
        }

        #endregion
    }
}