using Tauron.Application.Common.BaseLayer.Data;

namespace Tauron.Application.ImageOrganizer.Data.Entities
{
    public class OptionEntity : GenericBaseEntity<string>
    {
        private string _value;

        public string Value
        {
            get => _value;
            set => SetWithNotify(ref _value, value);
        }
    }
}