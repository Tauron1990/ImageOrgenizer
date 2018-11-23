using System;
using System.Threading;

namespace ImageOrganizer
{
    public static class Extensions
    {
        public static void WaitOne(this WaitHandle handle, CancellationToken token)
        {
            try
            {
                while (!handle.WaitOne(5000))
                    token.ThrowIfCancellationRequested();
            }
            catch (OperationCanceledException)
            {
            }
        }
    }
}