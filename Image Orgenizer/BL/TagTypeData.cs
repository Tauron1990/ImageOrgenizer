using System;
using ImageOrganizer.Data.Entities;

namespace ImageOrganizer.BL
{
    public class TagTypeData : IComparable<TagTypeData>, IEquatable<TagTypeData>
    {
        public TagTypeData(string name, string color)
        {
            Name = name;
            Color = color;
        }

        public TagTypeData(TagTypeEntity entity)
            : this(entity.Id, entity.Color)
        {
        }

        public string Name { get; }

        public string Color { get; set; }

        public TagTypeEntity ToEntity() => new TagTypeEntity {Color = Color, Id = Name};

        public int CompareTo(TagTypeData other)
        {
            if (ReferenceEquals(this, other)) return 0;
            return ReferenceEquals(null, other) ? 1 : string.Compare(Name, other.Name, StringComparison.Ordinal);
        }

        public bool Equals(TagTypeData other)
        {
            if (ReferenceEquals(null, other)) return false;
            return ReferenceEquals(this, other) || string.Equals(Name, other.Name);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((TagTypeData) obj);
        }

        public override int GetHashCode() => (Name != null ? Name.GetHashCode() : 0);

        public static bool operator ==(TagTypeData left, TagTypeData right) => Equals(left, right);

        public static bool operator !=(TagTypeData left, TagTypeData right) => !Equals(left, right);

        public override string ToString() => Name;
    }
}