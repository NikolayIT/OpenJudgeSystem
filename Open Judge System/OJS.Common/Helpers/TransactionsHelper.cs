namespace OJS.Common.Helpers
{
    using System.Transactions;

    using IsolationLevel = System.Transactions.IsolationLevel;

    public class TransactionsHelper
    {
        public static TransactionScope CreateTransactionScope() => new TransactionScope();

        public static TransactionScope CreateTransactionScope(IsolationLevel isolationLevel)
        {
            var transactionOptions = new TransactionOptions
            {
                IsolationLevel = isolationLevel
            };

            return new TransactionScope(TransactionScopeOption.Required, transactionOptions);
        }
    }
}