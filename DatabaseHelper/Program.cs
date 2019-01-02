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
                foreach (var tagType in db.TagTypes) tagType.Color = @"https://chan.sankakucomplex.com/stylesheets/style.css?231";
                foreach (var downloadEntity in db.Downloads.Where(de => de.DownloadType == DownloadType.UpdateColor))
                {
                    downloadEntity.FailedCount = 0;
                    downloadEntity.DownloadStade = DownloadStade.Queued;
                }
                db.SaveChanges();
            }
        }
    }
}
