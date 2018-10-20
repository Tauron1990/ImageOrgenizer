using System.Collections.Generic;
using ImageOrganizer.BL;
using Tauron.Application.Common.BaseLayer.Data;

namespace ImageOrganizer.Data.Entities
{
    public sealed class TagEntity : GenericBaseEntity<string>
    {
        public ICollection<ImageTag> ImageTags { get; set; }

        public TagTypeEntity Type { get; set; }

        public string Description { get; set; }
    }
}