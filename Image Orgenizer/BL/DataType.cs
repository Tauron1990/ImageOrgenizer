using System;

namespace ImageOrganizer.BL
{
    [Flags]
    public enum DataType
    {
        ImageData = 1,
        TagData = 2,
        TagTypeData = 4
    }
}