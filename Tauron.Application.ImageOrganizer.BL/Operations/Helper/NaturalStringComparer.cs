using System.Collections.Generic;
using Tauron.Application.ImageOrganizer.Data.Entities;

namespace Tauron.Application.ImageOrganizer.BL.Operations.Helper
{

    public sealed class ImageNaturalStringComparer : IComparer<ImageEntity>
    {
        public static readonly ImageNaturalStringComparer Comparer = new ImageNaturalStringComparer();

        private ImageNaturalStringComparer()
        {

        }

        public int Compare(ImageEntity a, ImageEntity b) => (a?.Name).CompareNumeric(b?.Name);
    }

    public sealed class NaturalStringComparer : IComparer<string>
    {
        public static readonly NaturalStringComparer Comparer = new NaturalStringComparer();

        private NaturalStringComparer()
        {
            
        }

        public int Compare(string a, string b) => a.CompareNumeric(b);
    }
}