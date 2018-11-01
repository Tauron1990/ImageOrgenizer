using System.Collections.Generic;
using Tauron.Application.Common.BaseLayer.Data;

namespace ImageOrganizer.Data.Entities
{
    public sealed class TagTypeEntity : GenericBaseEntity<string>
    {
        private ICollection<TagEntity> _tags;
        private string _color;

        public ICollection<TagEntity> Tags
        {
            get => _tags;
            set => SetWithNotify(ref _tags, value);
        }

        public string Color
        {
            get => _color;
            set => SetWithNotify(ref _color, value);
        }
    }
}