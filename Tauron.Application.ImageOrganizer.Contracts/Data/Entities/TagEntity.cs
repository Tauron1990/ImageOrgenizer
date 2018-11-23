using System.Collections.Generic;
using Tauron.Application.Common.BaseLayer.Data;

namespace Tauron.Application.ImageOrganizer.Data.Entities
{
    public sealed class TagEntity : GenericBaseEntity<string>
    {
        private TagTypeEntity _type;
        private ICollection<ImageTag> _imageTags;
        private string _description;


        public ICollection<ImageTag> ImageTags
        {
            get => _imageTags;
            set => SetWithNotify(ref _imageTags, value);
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