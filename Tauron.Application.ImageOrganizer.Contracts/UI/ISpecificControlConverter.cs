using System;

namespace Tauron.Application.ImageOrganizer.UI
{
    public interface ISpecificControlConverter
    {
        Type ControlType { get; }

        object Convert(object input);
    }
}