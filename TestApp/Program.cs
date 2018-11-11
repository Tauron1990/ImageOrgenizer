using System;
using System.IO;
using ImageOrganizer.BL.Provider.Impl;


namespace TestApp
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            var temp = new SankakuBaseProvider();
            temp.Load(@"https://chan.sankakucomplex.com/post/show/7306623");
            
            File.WriteAllBytes("test.jpg", temp.DownloadImage());

            Console.Write("Fertig");
            Console.ReadKey();
        }
    }
}
