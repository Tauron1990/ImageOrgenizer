using System.Linq;

namespace ImageOrganizer.Data.Container.Compse
{
    public class ComposeTransaction : IContainerTransaction
    {
        private readonly IContainerTransaction[] _transactions;

        public ComposeTransaction(IContainerTransaction[] transactions) => _transactions = transactions;

        public void Dispose()
        {
            foreach (var containerTransaction in _transactions)
                containerTransaction.Dispose();
        }

        public TTechno TryCast<TTechno>()
        {
            var temp = _transactions
                .Select(t => t.TryCast<TTechno>())
                .FirstOrDefault(t => t != null);
            if (temp != null) return temp;

            if (typeof(TTechno) == typeof(ComposeTransaction))
                return (TTechno)(object)this;

            return default;
        }

        public void Commit()
        {
            foreach (var transaction in _transactions)
                transaction.Commit();
        }

        public void Rollback()
        {
            foreach (var transaction in _transactions)
                transaction.Rollback();
        }
    }
}