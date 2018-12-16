using System.Collections.Generic;
using Tauron.Application.Common.BaseLayer.Data;

namespace Tauron.Application.ImageOrganizer.Data.Entities
{
    public sealed class TagEntity : GenericBaseEntity<int>
    {
        private TagTypeEntity _type;
        private ICollection<ImageTag> _images;
        private string _description;
        private string _name;

        public string Name
        {
            get => _name;
            set => SetWithNotify(ref _name, value);
        }

        public ICollection<ImageTag> Images
        {
            get => _images;
            set => SetWithNotify(ref _images, value);
        }

        public TagTypeEntity Type
        {
            get => _type;
            set => SetWithNotify(ref _type, value);
        }

        public string Description
        {
            get => _description;
            set => SetWithNotify(ref _description, value);
        }
    }
}