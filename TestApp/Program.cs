using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Media.Imaging;
using ExCSS;

namespace TestApp
{

    static class Program
    {
        public static void Main(string[] args)
        {
            string basePath = @"D:\Downloads\Shankaku";
            string file = @"7424527.gif";
            string fullPath = Path.Combine(basePath, file);

            try
            {
                var frame = BitmapFrame.Create(new Uri(fullPath));
            }
            catch (Exception e)
            {
                Type test = e.GetType();

                Console.WriteLine(e);
                throw;
            }


            Console.Write("Fertig");
            Console.ReadKey();
        }
    }
}
