using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using ImageOrganizer.Data.Entities;
using JetBrains.Annotations;
using Tauron.Application;

namespace ImageOrganizer.BL
{
    public class ImageData : INotifyPropertyChanged, IEquatable<ImageData>, IComparable<ImageData>
    {
        private bool _favorite;

        public ImageData(string name, string providerName, int randomCount, int viewCount, DateTime added, IEnumerable<TagData> tags, int id, string author, bool favorite)
        {
            Name = name;
            ProviderName = providerName;
            RandomCount = randomCount;
            ViewCount = viewCount;
            Added = added;
            Tags = new List<TagData>(tags);
            Id = id;
            Author = author;
            Favorite = favorite;
        }

        public ImageData(ImageEntity entity)
            : this(entity.Name, entity.ProviderName, entity.RandomCount, entity.ViewCount, entity.Added, Enumerable.Empty<TagData>(), entity.Id, entity.Author, entity.Favorite)
        {
            if (entity.ImageTags == null) return;

            foreach (var tagEntity in entity.ImageTags
                .Where(sel => sel.TagEntity != null)
                .Select(sel => sel.TagEntity))
                Tags.Add(new TagData(tagEntity));
        }

        public ImageData(string name, string providerName)
        {
            Name = name;
            ProviderName = providerName;
            New = true;
        }

        public bool New { get; }

        public bool Favorite
        {
            get => _favorite;
            set
            {
                _favorite = value;
                OnPropertyChanged();
            }
        }
        
        public int Id { get; }

        public string Author { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string Name { get; set; }

        [Required(AllowEmptyStrings = false), UsedImplicitly]
        public string ProviderName { get; set; }

        public int RandomCount { get; }

        public int ViewCount { get; }

        public ICollection<TagData> Tags { get; }

        public DateTime Added { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        
        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public override string ToString() => Id.ToString();

        public bool Equals(ImageData other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((ImageData) obj);
        }

        public override int GetHashCode() => Id;

        public static bool operator ==(ImageData left, ImageData right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ImageData left, ImageData right)
        {
            return !Equals(left, right);
        }

        public int CompareTo(ImageData other)
        {
            if (ReferenceEquals(this, other)) return 0;
            return ReferenceEquals(null, other) ? 1 : Id.CompareTo(other.Id);
        }
    }
}