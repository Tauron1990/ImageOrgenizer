using System;
using System.Collections.Generic;
using Tauron.Application.Common.BaseLayer.Data;

namespace Tauron.Application.ImageOrganizer.Data.Entities
{
    public sealed class ImageEntity : GenericBaseEntity<int>, IEquatable<ImageEntity>
    {
        private string _name;
        private string _providerName;
        private int _randomCount;
        private int _viewCount;
        private bool _favorite;
        private DateTime _added;
        private ICollection<ImageTag> _imageTags;
        private string _author;
        private int _sortOrder;

        public string Name
        {
            get => _name;
            set => SetWithNotify(ref _name, value);
        }

        public string ProviderName
        {
            get => _providerName;
            set => SetWithNotify(ref _providerName, value);
        }

        public int RandomCount
        {
            get => _randomCount;
            set => SetWithNotify(ref _randomCount, value);
        }

        public int ViewCount
        {
            get => _viewCount;
            set => SetWithNotify(ref _viewCount, value);
        }

        public bool Favorite
        {
            get => _favorite;
            set => SetWithNotify(ref _favorite, value);
        }

        public DateTime Added
        {
            get => _added;
            set => SetWithNotify(ref _added, value);
        }

        public ICollection<ImageTag> ImageTags
        {
            get => _imageTags;
            set => SetWithNotify(ref _imageTags, value);
        }

        public string Author
        {
            get => _author;
            set => SetWithNotify(ref _author, value);
        }

        public int SortOrder
        {
            get => _sortOrder;
            set => SetWithNotify(ref _sortOrder, value);
        }

        public bool Equals(ImageEntity other)
        {
            if (other == null) return false;

            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is ImageEntity other && Equals(other);
        }

        public override int GetHashCode() => Id.GetHashCode();

        public static bool operator ==(ImageEntity left, ImageEntity right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ImageEntity left, ImageEntity right)
        {
            return !Equals(left, right);
        }
    }
}