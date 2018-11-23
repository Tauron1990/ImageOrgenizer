using System.Collections.Generic;

namespace Tauron.Application.ImageOrganizer.BL
{
    public class PagerInput
    {
        public PagerInput(int next, int count, bool favorite, IEnumerable<string> tagFilter)
        {
            Next = next;
            Count = count;
            TagFilter = tagFilter;
            Favorite = favorite;
        }

        public int Next { get; }

        public int Count { get; }

        public bool Favorite { get; }

        public IEnumerable<string> TagFilter { get; }
    }
}