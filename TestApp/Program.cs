

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WindowsFormsApplication1;
using Tauron.Application.ImageOrganizer.Data;
using Tauron.Application.ImageOrganizer.Data.Entities;

namespace TestApp
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            //HResultDeciphering test = new HResultDeciphering(0x80071A2D);

            //                     https://cs.sankakucomplex.com/data/db/e3/dbe3961c045ae380ca0c215d48d8b8b9.jpg?e=1545021241&m=IHrIiExJd5eI6qXr4xU2jw
            //var temp = new Uri(@"https://cs.sankakucomplex.com/data/db/e3/dbe3961c045ae380ca0c215d48d8b8b9.jpg?e=1545019896&m=SCTbaEWrzeqDnBkyzX-3kg");

            //var request = (HttpWebRequest)WebRequest.Create(temp);

            //request.Method = "GET";
            //request.Host = "cs.sankakucomplex.com";
            //request.UserAgent = "Mozilla / 5.0(Windows NT 10.0; Win64; x64; rv: 64.0) Gecko / 20100101 Firefox / 64.0";
            //request.Accept = "text / html,application / xhtml + xml,application / xml; q = 0.9,*/*;q=0.8";
            //request.Headers.Add("Accept-Language", "de,en-US;q=0.7,en;q=0.3");
            //request.Headers.Add("Accept-Encoding", "gzip, deflate, br");
            //request.Headers.Add("DNT", "1");
            //request.Headers.Add("Upgrade-Insecure-Requests", "1");

            //using (var stream = request.GetResponse().GetResponseStream())
            //{
            //    using (var mem = new MemoryStream())
            //    {
            //        stream.CopyTo(mem);
            //        mem.Position = 0;
            //        File.WriteAllBytes(@"I:\Sankaku Safe Test\test.jpg", mem.ToArray());
            //    }
            //}

            //DateTime noew = DateTime.Now;

            //List<DateTime> _dateTimes = new List<DateTime> { noew -TimeSpan.FromDays(1), noew, noew + TimeSpan.FromDays(1) };

            //foreach (var dateTime in _dateTimes.Where(d => d < noew))
            //{
            //    Console.Write(dateTime);
            //}

            //var temp = new SankakuBaseProvider();
            //temp.Load(@"https://chan.sankakucomplex.com/post/show/7306623");

            //File.WriteAllBytes("test.jpg", temp.DownloadImage());

            string path = Path.Combine(@"I:\Sankaku Safe Test", "Main.imgdb");

            ////using (DatabaseImpl con = new DatabaseImpl(path))
            ////{
            ////    con.Database.EnsureDeleted();
            ////    con.Database.Migrate();

            ////    ImageEntity testData = new ImageEntity { Name = "HalloWelt", ProviderName = "Test", Added = DateTime.Now, Author = "Max", Tags =   new ObservableCollection<ImageTag>()};
            ////    con.Images.Add(testData);
            ////    TagTypeEntity tt = new TagTypeEntity { Id = "testColor", Color = "Black" };
            ////    con.TagTypes.Add(tt);

            ////    Random ran = new Random();

            ////    for (int i = 0; i < 25; i++)
            ////    {
            ////        var testData2 = new TagEntity { Name = ran.Next().ToString(), Description = "Unknowen", Type = tt};
            ////        con.Tags.Add(testData2);

            ////        testData.Tags.Add(new ImageTag { ImageEntity = testData, TagEntity =  testData2});
            ////    }

            ////    con.SaveChanges();
            ////}

            //using (DatabaseImpl con = new DatabaseImpl(path))
            //{
            //    var temp2 = con.Set<ImageEntity>().AsNoTracking().Include(td => td.Tags).ThenInclude(td => td.TagEntity).ThenInclude(t => t.Type).First(e => e.Name == "175328.jpg");
            //}

            using (var db = new DatabaseImpl(path))
            {
                List<DownloadEntity> failed = db.Downloads.Where(de => de.FailedCount > 0).OrderBy(de => de.FailedCount).ToList();

                var one = failed.First();
                var two = failed.Last();
            }

            Console.Write("Fertig");
            Console.ReadKey();
        }
    }
}
