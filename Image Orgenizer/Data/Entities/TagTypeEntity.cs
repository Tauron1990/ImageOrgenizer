using System.Collections.Generic;
using Tauron.Application.Common.BaseLayer.Data;

namespace ImageOrganizer.Data.Entities
{
    public sealed class TagTypeEntity : GenericBaseEntity<string>
    {
        public ICollection<TagEntity> Tags { get; set; }

        public string Color { get; set; }
    }
}