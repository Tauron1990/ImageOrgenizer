using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using ImageOrganizer.BL;
using ImageOrganizer.BL.Operations;
using ImageOrganizer.BL.Provider.Impl;
using ImageOrganizer.Resources;
using Tauron;
using Tauron.Application;
using Tauron.Application.Commands;

namespace ImageOrganizer.Views.ImageEditorHelper
{
    public class ImageDataItem : EditorItemBase, IEditorItem<ImageData>
    {
        private readonly ImageData _data;
        private readonly Operator _operator;
        private string _updateLabel;
        private bool _updated;

        public ImageDataItem()
        {
            IsNew = true;
            Id = -1;
            ProviderName = NonProvider.ProviderNon;
            Tags.CollectionChanged += (sender, args) => OnChangedEvent(this);
            CreateCommand();
        }

        public ImageDataItem(ImageData data, Operator @operator)
        {
            UpdateLabel = UIResources.ImageEditor_ImageRowHeader_UpdateTags;
            _data = data;
            _operator = @operator;
            Id = data.Id;
            Tags.CollectionChanged += (sender, args) => OnChangedEvent(this);
            CreateCommand();
            Update(data);
        }

        private void CreateCommand() => UpdateImage = new SimpleCommand(o => _operator != null && _updated == false, Execute);

        private void Execute(object o)
        {
            //_updated = true;
            //_operator.SpecialUpdateImage(Create());

            var path = CommonApplication.Current.Container.Resolve<IDialogFactory>()
                .ShowOpenFileDialog(CommonApplication.Current.MainWindow, true, string.Empty, true, string.Empty, false, "File", true, true, out var ok).FirstOrDefault();

            if(ok != true) return;
            if(string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return;

            using (var trans = FileContainerManager.GetContainerTransaction())
            {
                if(FileContainerManager.Contains(_data.Name))
                    FileContainerManager.Remove(_data.Name, trans);
                FileContainerManager.AddFile(File.ReadAllBytes(path), _data.Name);
            }

            UpdateLabel = UIResources.ImageEditor_SpecialUpdate_Compled;
        }

        public string UpdateLabel
        {
            get => _updateLabel;
            set
            {
                _updateLabel = value;
                OnPropertyChanged();
            }
        }

        public bool Favorite
        {
            get => GetValue<bool>();
            set => SetValue(value);
        }

        public int Id { get; }

        public string Author
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        public string Name
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        public string ProviderName
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        public UIObservableCollection<TagData> Tags { get; } = new UIObservableCollection<TagData>();

        public DateTime Added
        {
            get => GetValue<DateTime>();
            set => SetValue(value);
        }

        public ICommand UpdateImage { get; set; }

        public bool Equals(IEditorItem<ImageData> other) => Id == (other as ImageDataItem)?.Id;
        public bool Equals(ImageData other) => other != null && other.Id == Id;

        public ImageData Create()
        {
            if (IsNew)
            {
                var newImage = new ImageData(Name, ProviderName)
                {
                    Added = Added,
                    Favorite = Favorite,
                    Author = Author
                };


                return newImage;
            }

            _data.Added = Added;
            _data.Favorite = Favorite;
            _data.Author = Author;
            _data.Tags.Clear();
            foreach (var tagData in Tags)
                _data.Tags.Add(tagData);

            return _data;
        }

        public void Update(Task<ImageData> data) => data.ContinueWith(t => Update(t.Result));

        private void Update(ImageData data)
        {
            if (Id != data.Id)
                throw new InvalidOperationException("Differnt Id on Editor Image");

            using (EnterUpdateMode())
            {
                Favorite = data.Favorite;
                Author = data.Author;
                Name = data.Name;
                ProviderName = data.ProviderName;
                Tags.Clear();
                Tags.AddRange(data.Tags);
                Added = data.Added;
            }
        }
    }
}