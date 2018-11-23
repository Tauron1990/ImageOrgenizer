using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Tauron.Application.Commands;
using Tauron.Application.ImageOrganizer;
using Tauron.Application.ImageOrganizer.BL;
using Tauron.Application.ImageOrginazer.ViewModels.Resources;

namespace Tauron.Application.ImageOrginazer.ViewModels.Views.ImageEditorHelper
{
    public class ImageDataItem : EditorItemBase, IEditorItem<ImageData>
    {
        private readonly ImageData _data;
        private readonly IOperator _operator;
        private string _updateLabel;
        private bool _updated;

        public ImageDataItem()
        {
            IsNew = true;
            Id = -1;
            ProviderName = AppConststands.ProviderNon;
            Tags.CollectionChanged += (sender, args) => OnChangedEvent(this);
            CreateCommand();
        }

        public ImageDataItem(ImageData data, IOperator @operator)
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

            UpdateLabel = _operator.ReplaceImage(new ReplaceImageInput(File.ReadAllBytes(path), _data.Name)) 
                ? UIResources.ImageEditor_SpecialUpdate_Compled : UIResources.ImageEditor_SpecialUpdate_Failed;
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