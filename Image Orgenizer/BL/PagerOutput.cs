using System.Collections.Generic;

namespace ImageOrganizer.BL
{
    public class PagerOutput
    {
        public PagerOutput(int next, IList<ImageData> imageData)
        {
            Next = next;
            ImageData = imageData;
        }

        public int Next { get; }

        public IList<ImageData> ImageData { get; }
    }
}