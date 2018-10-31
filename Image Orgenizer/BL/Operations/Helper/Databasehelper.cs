using System.Collections.Generic;
using ImageOrganizer.Data.Entities;

namespace ImageOrganizer.BL.Operations.Helper
{
    public static class DatabaseHelper
    {
        public static void SetOrder(this List<ImageEntity> toSort)
        {
            var naturalSorter = new NaturalStringComparer();
            toSort.Sort((entity, imageEntity) => naturalSorter.Compare(entity.Name, imageEntity.Name));

            for (int i = 0; i < toSort.Count; i++)
            {
                toSort[i].SortOrder = i;
            }
        }
    }
}