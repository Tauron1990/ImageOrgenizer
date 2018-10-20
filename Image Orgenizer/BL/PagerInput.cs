using System.Collections.Generic;

namespace ImageOrganizer.BL
{
    public class PagerInput
    {
        public PagerInput(int next, int count, bool reverse, bool favorite, IEnumerable<string> tagFilter)
        {
            Next = next;
            Count = count;
            TagFilter = tagFilter;
            Reverse = reverse;
            Favorite = favorite;
        }

        public int Next { get; }

        public int Count { get; }

        public bool Reverse { get; }

        public bool Favorite { get; }

        public IEnumerable<string> TagFilter { get; }
    }
}