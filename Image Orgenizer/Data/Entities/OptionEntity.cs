using Tauron.Application.Common.BaseLayer.Data;

namespace ImageOrganizer.Data.Entities
{
    public class OptionEntity : GenericBaseEntity<string>
    {
        public string Value { get; set; }
    }
}