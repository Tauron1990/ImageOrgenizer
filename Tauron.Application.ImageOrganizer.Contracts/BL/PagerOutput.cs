using System.Collections.Generic;

namespace Tauron.Application.ImageOrganizer.BL
{
    public class PagerOutput
    {
        public PagerOutput(int next, IList<ImageData> imageData, int start)
        {
            Next = next;
            ImageData = imageData;
            Start = start;
        }

        public int Next { get; }

        public IList<ImageData> ImageData { get; }

        public int Start { get; }
    }
}