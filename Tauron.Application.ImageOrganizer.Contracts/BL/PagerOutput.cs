using System.Collections.Generic;

namespace Tauron.Application.ImageOrganizer.BL
{
    public class PagerOutput
    {
        public PagerOutput(IList<ImageData> imageData, int start, bool setNull)
        {
            ImageData = imageData;
            Start = start;
            SetNull = setNull;
        }

        public IList<ImageData> ImageData { get; }

        public int Start { get; }

        public bool SetNull { get; }
    }
}