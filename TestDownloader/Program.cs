using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SharpBits.Base;

namespace TestDownloader
{
    class Program
    {
        static void Main(string[] args)
        {
            using (BitsManager man = new BitsManager())
            {
                var job = man.CreateJob("TestDownload", JobType.Download);
                job.AddFile(
                    @"https://cs.sankakucomplex.com/data/82/04/8204b48dcccf815dee27999ebfbb9164.jpg?e=1548351377&m=FMC7mCAkIji_HQW1nMvz5A",
                    @"D:\Githan\test.jpg");
                job.Resume();

                while (job.State != JobState.Transferred)
                {
                    Thread.Sleep(10000);
                }
            }
            
        }
    }
}
