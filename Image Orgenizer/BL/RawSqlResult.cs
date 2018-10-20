using System;

namespace ImageOrganizer.BL
{
    public class RawSqlResult
    {
        public Exception Exception { get; }

        public ImageData ImageData { get; }

        public int Position { get; }

        public bool Sucess { get; }

        public RawSqlResult(ImageData data, int position)
        {
            Sucess = data != null;
            ImageData = data;
            Position = position;
        }

        public RawSqlResult(Exception e)
        {
            Exception = e;
            Position = -1;
        }
    }
}