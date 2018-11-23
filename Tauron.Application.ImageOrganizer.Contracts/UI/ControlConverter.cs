using System.Linq;
using Tauron.Application.Views;

namespace Tauron.Application.ImageOrganizer.UI
{
    public class ControlConverter : ISimpleConverter
    {
        private static ISpecificControlConverter[] _controlConverters;
        private static ISpecificControlConverter[] ControlConverters => _controlConverters ?? (_controlConverters = CommonApplication.Current.Container.ResolveAll<ISpecificControlConverter>(null).ToArray());

        public object Convert(object input)
        {
            var type = input.GetType();

            var converter = ControlConverters.FirstOrDefault(cc => cc.ControlType == type);

            return converter?.Convert(input) ?? input;
        }
    }
}