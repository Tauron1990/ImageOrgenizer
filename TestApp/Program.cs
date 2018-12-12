using System;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;


namespace TestApp
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            var temp = new Uri(@"https://chan.sankakucomplex.com/stylesheets/style.css?231");

            //var temp = new SankakuBaseProvider();
            //temp.Load(@"https://chan.sankakucomplex.com/post/show/7306623");

            //File.WriteAllBytes("test.jpg", temp.DownloadImage());
            
            using (TestContext con = new TestContext())
            {
                con.Database.EnsureDeleted();
                con.Database.Migrate();

                TestData testData = new TestData {TestProp2 = "HalloWelt"};
                con.TestDatas.Add(testData);

                Random ran = new Random();

                for (int i = 0; i < 25; i++)
                {
                    var testData2 = new TestData2 { TestProp = ran.Next().ToString() };
                    con.TestData2s.Add(testData2);

                    testData.Connectors.Add(new TestDataConnector { TestData = testData, TestData2 =  testData2});
                }

                con.SaveChanges();
            }

            using (TestContext con = new TestContext())
            {
                var temp2 = con.TestDatas.Include(td => td.Connectors).ThenInclude(td => td.TestData2).First();
            }

            Console.Write("Fertig");
            Console.ReadKey();
        }
    }
}
