using System;
using System.IO;
using System.Linq;

namespace ImageOrganizer.Data.Container.SingleFile
{
    public class HeaderHelper
    {
        public static readonly byte[] EntryHeader =
        {
            84, 97, 117, 114,
            111, 110, 95, 72,
            101, 97, 100, 101,
            114, 95, 67, 111,
            110, 116, 101, 110,
            116
        };

        public static int HeaderLength => EntryHeader.Length;

        public static long Find(Stream stream) => FindPosition(stream, EntryHeader);

        private static long FindPosition(Stream stream, byte[] byteSequence)
        {
            if (byteSequence.Length > stream.Length)
                return -1;

            byte[] buffer = new byte[byteSequence.Length];

            BufferedStream bufStream = new BufferedStream(stream, byteSequence.Length);

            while (bufStream.Read(buffer, 0, byteSequence.Length) == byteSequence.Length)
            {
                if (byteSequence.SequenceEqual(buffer))
                    return bufStream.Position; // - byteSequence.Length;

                bufStream.Position -= byteSequence.Length - PadLeftSequence(buffer, byteSequence);
            }

            return -1;
        }

        private static int PadLeftSequence(byte[] bytes, byte[] seqBytes)
        {
            int i = 1;
            while (i < bytes.Length)
            {
                int n = bytes.Length - i;
                byte[] aux1 = new byte[n];
                byte[] aux2 = new byte[n];
                Array.Copy(bytes, i, aux1, 0, n);
                Array.Copy(seqBytes, aux2, n);
                if (aux1.SequenceEqual(aux2))
                    return i;
                i++;
            }

            return i;
        }
    }
}