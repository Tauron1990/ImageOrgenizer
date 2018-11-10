using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using ImageOrganizer.BL;
using ImageOrganizer.BL.Provider.Impl;
using Tauron.Application;
using Tauron.Application.Commands;

namespace ImageOrganizer.Views.ImageEditorHelper
{
    public class ImageDataItem : EditorItemBase, IEditorItem<ImageData>
    {
        private readonly ImageData _data;
        private readonly Operator _operator;

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
            _data = data;
            _operator = @operator;
            Id = data.Id;
            Tags.CollectionChanged += (sender, args) => OnChangedEvent(this);
            CreateCommand();
            Update(data);
        }

        private void CreateCommand() => UpdateImage = new SimpleCommand(o => _operator != null, o => _operator.SpecialUpdateImage(Create()));

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