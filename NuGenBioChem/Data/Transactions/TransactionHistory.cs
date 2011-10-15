using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace NuGenBioChem.Data.Transactions
{
    /// <summary>
    /// Represents history of commited transactions (order from new to all)
    /// </summary>
    public class TransactionHistory : ObservableCollection<Transaction>
    {
        readonly Stack<Transaction> redos = new Stack<Transaction>();

        /// <summary>
        /// Undos previous transaction
        /// </summary>
        public void Undo()
        {
            if (Count == 0) return;
            this[0].Rollback();
            redos.Push(this[0]);
            RemoveAt(0);
        }

        /// <summary>
        /// Redos the latest undoed operation
        /// </summary>
        public void Redo()
        {
            if (redos.Count == 0) return;
            Transaction transaction = redos.Pop();
            transaction.Perform();
            Insert(0, transaction);
        }
    }
}
