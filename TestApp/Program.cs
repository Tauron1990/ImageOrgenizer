using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ImageOrganizer.BL;

namespace TestApp
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            const string test = "Tauron_Header_Content";
            byte[] bytes = Encoding.ASCII.GetBytes(test);
            
            StringBuilder builder = new StringBuilder();
            builder.Append("= { ");

            foreach (var chunk in bytes.Split(4))
            {
                foreach (var b in chunk)
                    builder.Append(b).Append(", ");
                builder.AppendLine();
            }

            builder.Remove(builder.Length - 4, 4);
            builder.Append(" };");

            Clipboard.SetText(builder.ToString());
            Console.Write(builder);

            Console.ReadKey();
        }

        public static IEnumerable<IEnumerable<T>> Split<T>(this IEnumerable<T> fullBatch, int chunkSize)
        {
            if (chunkSize <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(chunkSize),
                    chunkSize,
                    "Chunk size cannot be less than or equal to zero.");
            }

            if (fullBatch == null)
            {
                throw new ArgumentNullException(nameof(fullBatch), "Input to be split cannot be null.");
            }

            var cellCounter = 0;
            var chunk = new List<T>(chunkSize);

            foreach (var element in fullBatch)
            {
                if (cellCounter++ == chunkSize)
                {
                    yield return chunk;
                    chunk = new List<T>(chunkSize);
                    cellCounter = 1;
                }

                chunk.Add(element);
            }

            yield return chunk;
        }
    }
}
