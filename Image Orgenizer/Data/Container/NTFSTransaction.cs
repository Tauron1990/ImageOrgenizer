using Alphaleonis.Win32.Filesystem;

namespace ImageOrganizer.Data.Container
{
    public class NTFSTransaction : IContainerTransaction
    {
        private object _gate = new object();
        private bool _compled;

        public KernelTransaction KernelTransaction { get; }
        
        public NTFSTransaction()
        {
            KernelTransaction = new KernelTransaction();
        }

        public void Dispose()
        {
            lock (_gate)
                KernelTransaction.Dispose();
        }

        public TTechno TryCast<TTechno>() => KernelTransaction is TTechno techno ? techno : default;

        public void Commit()
        {
            lock (_gate)
            {
                if (_compled)
                    return;
                _compled = true;
                KernelTransaction.Commit();
            }
        }

        public void Rollback()
        {
            lock (_gate)
            {
                if(_compled)
                    return;
                _compled = true;
                KernelTransaction.Rollback();
            }
        }
    }

}