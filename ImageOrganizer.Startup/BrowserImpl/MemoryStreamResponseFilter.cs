using System;
using System.Collections.Generic;
using System.IO;
using CefSharp;
using Tauron.Application.ImageOrganizer.BL.Provider.Browser;

namespace ImageOrganizer.Startup.BrowserImpl {
    internal class MemoryStreamResponseFilter : IResponseFilter
    {
        private readonly IEnumerable<(string, IDataInterceptor)> _interceptors;
        private MemoryStream _memoryStream;

        public MemoryStreamResponseFilter(IEnumerable<(string, IDataInterceptor)> interceptors) => _interceptors = interceptors;

        bool IResponseFilter.InitFilter()
        {
            _memoryStream = new MemoryStream();
            return true;
        }

        FilterStatus IResponseFilter.Filter(Stream dataIn, out long dataInRead, Stream dataOut, out long dataOutWritten)
        {
            if (dataIn == null)
            {
                dataInRead = 0;
                dataOutWritten = 0;

                return FilterStatus.Done;
            }

            var dataOutLenght = dataOut.Length;
            var dataInLenght = dataIn.Length;
            var readCount = (int)Math.Min(dataOutLenght, dataInLenght);

            dataInRead = readCount;
            dataOutWritten = readCount;

            var buffer = new byte[readCount];

            var readCountReal = dataIn.Read(buffer, 0, readCount);
            if (readCountReal != readCount)
                return FilterStatus.Error;

            _memoryStream.Write(buffer, 0, buffer.Length);
            dataOut.Write(buffer, 0, buffer.Length);

            return FilterStatus.Done;
        }

        void IDisposable.Dispose()
        {
            _memoryStream = null;
        }

        public void FeedData()
        {
            byte[] data = _memoryStream.ToArray();
            _memoryStream.Dispose();

            foreach (var interceptor in _interceptors) interceptor.Item2.Data(data, interceptor.Item1);
        }
    }
}