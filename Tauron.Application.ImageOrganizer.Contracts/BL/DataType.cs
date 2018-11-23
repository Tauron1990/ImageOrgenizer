using System;

namespace Tauron.Application.ImageOrganizer.BL
{
    [Flags]
    public enum DataType
    {
        ImageData = 1,
        TagData = 2,
        TagTypeData = 4
    }
}