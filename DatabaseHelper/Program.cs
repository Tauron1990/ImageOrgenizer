using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Tauron.Application.ImageOrganizer.Data;
using Tauron.Application.ImageOrganizer.Data.Entities;

namespace DatabaseHelper
{
    class Program
    {
        //private class DownloadComparer : IEqualityComparer<DownloadEntity>, IComparer<DownloadEntity>
        //{
        //    public int Compare(DownloadEntity x, DownloadEntity y)
        //    {
        //        int result = string.Compare(x?.Provider, y?.Provider, StringComparison.Ordinal);
        //        if (result == 0)
        //            result = string.Compare(x?.Image, y?.Image, StringComparison.Ordinal);
        //        if (result == 0)
        //            result = string.Compare(x?.Metadata, y?.Metadata, StringComparison.Ordinal);
        //        return result;
        //    }

        //    public bool Equals(DownloadEntity x, DownloadEntity y) => Compare(x, y) == 0;

        //    public int GetHashCode(DownloadEntity obj)
        //    {
        //        unchecked // Overflow is fine, just wrap
        //        {
        //            int hash = (int)2166136261;
        //            // Suitable nullity checks etc, of course :)
        //            if(!string.IsNullOrWhiteSpace(obj.Provider))
        //                hash = (hash * 16777619) ^ obj.Provider.GetHashCode();
        //            if (!string.IsNullOrWhiteSpace(obj.Image))
        //                hash = (hash * 16777619) ^ obj.Image.GetHashCode();
        //            if (!string.IsNullOrWhiteSpace(obj.Metadata))
        //                hash = (hash * 16777619) ^ obj.Metadata.GetHashCode();
        //            return hash;
        //        }
        //    }
        //}

        static void Main(string[] args)
        {
            const string path = @"I:\Sankaku Safe Test\Main.imgdb";

            using (var db = new DatabaseImpl(path))
            {
                //int count = db.Downloads.Count(de => de.DownloadStade == DownloadStade.Queued);
                //int counrReal = db.Downloads.Where(de => de.DownloadStade == DownloadStade.Queued).AsEnumerable().Distinct(new DownloadComparer()).Count();
                Stopwatch watch = new Stopwatch();
                watch.Start();
                var img = db.Images.Include(ie => ie.Tags).ThenInclude(t => t.TagEntity).ThenInclude(tt => tt.Type).OrderBy(ie => ie.SortOrder).Skip(75_0000).Take(1).ToArray();
                var time = watch.Elapsed;
                Console.WriteLine(time);

                Console.WriteLine("Compled: {0}", db.Downloads.Count(dl => dl.DownloadStade == DownloadStade.Compled));
                Console.WriteLine("Error: {0}", db.Downloads.Count(dl => dl.DownloadStade == DownloadStade.Failed));
            }

            Console.ReadKey();
        }
    }
}
