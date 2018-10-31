using System;
using System.Collections.Generic;
using Tauron.Application.Common.BaseLayer.Data;

namespace ImageOrganizer.Data.Entities
{
    public sealed class ImageEntity : GenericBaseEntity<int>, IEquatable<ImageEntity>
    {
        public string Name { get; set; }

        public string ProviderName { get; set; }

        public int RandomCount { get; set; }

        public int ViewCount { get; set; }

        public bool Favorite { get; set; }

        public DateTime Added { get; set; }

        public ICollection<ImageTag> ImageTags { get; set; }

        public string Author { get; set; }

        public int SortOrder { get; set; }

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