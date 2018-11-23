using System.Collections.Generic;

namespace Tauron.Application.ImageOrganizer.BL.Operations.Helper
{
    public sealed class NaturalStringComparer : IComparer<string>
    {
        public int Compare(string a, string b) => a.CompareNumeric(b);
    }
}