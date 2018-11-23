using System;
using Tauron.Application.ImageOrganizer.Data.Entities;

namespace Tauron.Application.ImageOrganizer.BL
{
    public class TagData : IComparable<TagData>, IEquatable<TagData>
    {
        public TagData(TagTypeData type, string description, string name)
        {
            Type = type;
            Description = description;
            Name = name;
        }

        public TagData(TagEntity entity)
            : this(entity.Type != null ? new TagTypeData(entity.Type) : null, entity.Description, entity.Id)
        {
        }

        public TagTypeData Type { get; set; }

        public string Description { get; set; }

        public string Name { get; }

        public TagEntity ToEntity()
        {
            var entity = new TagEntity
            {
                Id = Name,
                Description = Description
            };

            if (Type != null)
                entity.Type = Type.ToEntity();

            return entity;
        }

        public override string ToString() => Name;

        public int CompareTo(TagData other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            return string.Compare(Name, other.Name, StringComparison.Ordinal);
        }

        public bool Equals(TagData other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Name, other.Name);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TagData) obj);
        }

        public override int GetHashCode()
        {
            return (Name != null ? Name.GetHashCode() : 0);
        }

        public static bool operator ==(TagData left, TagData right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(TagData left, TagData right)
        {
            return !Equals(left, right);
        }
    }
}