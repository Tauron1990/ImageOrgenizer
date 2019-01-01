using System;
using System.Collections.Generic;
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
        static void Main(string[] args)
        {
            const string path = @"I:\Sankaku Safe Test\Main.imgdb";

            using (var db = new DatabaseImpl(path))
            {
                var time = DateTime.Now;
                var temp = db.Downloads.AsNoTracking()
                    .OrderBy(e => e.Schedule)
                    .Where(de => de.DownloadStade == DownloadStade.Queued)
                    .Where(de => de.Schedule < time)
                    .GroupBy(de => de.DownloadType)
                    .ToArray();

                var filter = new IGrouping<DownloadType, DownloadEntity>[6];

                foreach (var grouping in temp)
                {
                    switch (grouping.Key)
                    {
                        case DownloadType.UpdateTags:
                            filter[2] = grouping;
                            break;
                        case DownloadType.DownloadTags:
                            filter[3] = grouping;
                            break;
                        case DownloadType.DownloadImage:
                            filter[0] = grouping;
                            break;
                        case DownloadType.ReDownload:
                            filter[1] = grouping;
                            break;
                        case DownloadType.UpdateColor:
                            filter[4] = grouping;
                            break;
                        case DownloadType.UpdateDescription:
                            filter[5] = grouping;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                var downloads = filter.SelectMany(g => g ?? Enumerable.Empty<DownloadEntity>()).Take(10).ToArray();

                //var arr = db.Downloads.Where(d => d.DownloadType == DownloadType.DownloadImage).ToArray();
                //db.Downloads.RemoveRange(arr);
                //db.SaveChanges();
            }
        }
    }
}
