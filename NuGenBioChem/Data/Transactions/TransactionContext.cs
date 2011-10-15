using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using NuGenBioChem.Data.Commands;

namespace NuGenBioChem.Data.Transactions
{
    /// <summary>
    /// Represents context of transactions
    /// </summary>
    public class TransactionContext
    {
        #region Static Properties

        /// <summary>
        /// Gets current transaction context
        /// </summary>
        public static TransactionContext Current { get; set; }

        #endregion

        #region Static Methods



        #endregion

        #region Fields

        // Performed transactions
        ObservableCollection<Transaction> performedTransactions = new ObservableCollection<Transaction>();
        // Rollbacked transactions
        ObservableCollection<Transaction> rollbackedTransactions = new ObservableCollection<Transaction>();
        // Maximum number of rollbacked & performed transactions
        int maxHistoryLength = 50;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets maximum number of rollbacked & performed transactions
        /// </summary>
        public int MaxHistoryLength
        {
            get { return maxHistoryLength; }
            set 
            { 
                maxHistoryLength = value;
                for(int i = performedTransactions.Count - 1; i <= maxHistoryLength; i--)
                    performedTransactions.RemoveAt(i);
                for (int i = rollbackedTransactions.Count - 1; i <= maxHistoryLength; i--)
                    rollbackedTransactions.RemoveAt(i);
            }
        }

        /// <summary>
        /// Gets a value indicating whether can undo
        /// </summary>
        public bool CanUndo
        {
            get { return PerformedTransactions.Count > 0; }
        }

        /// <summary>
        /// Gets a value indicating whether can redo
        /// </summary>
        public bool CanRedo
        {
            get { return RollbackedTransactions.Count > 0; }
        }

        /// <summary>
        /// Gets performed transactions
        /// </summary>
        public ObservableCollection<Transaction> PerformedTransactions
        {
            get { return performedTransactions; }
        }
        
        /// <summary>
        /// Gets rollbacked transactions
        /// </summary>
        public ObservableCollection<Transaction> RollbackedTransactions
        {
            get { return rollbackedTransactions; }
        }

        static TransactionContext()
        {
            Current = null;
        }

        #endregion

        #region Commands

        #region Undo Command

        // Command
        RelayCommand undoCommand;

        /// <summary>
        /// Gets Undo Command
        /// </summary>
        public RelayCommand UndoCommand
        {
            get
            {
                if (undoCommand == null)
                {
                    undoCommand = new RelayCommand(x => OnUndo((Transaction)x), x=>CanUndo, false);
                    PerformedTransactions.CollectionChanged += (s, e) => undoCommand.RaiseCanExecuteChanged();
                }
                return undoCommand;
            }
        }

        void OnUndo(Transaction transaction)
        {
            if (transaction==null) Undo();
            else Undo(transaction);
        }        

        #endregion

        #region Redo Command

        // Command
        RelayCommand redoCommand;

        /// <summary>
        /// Gets Redo Command
        /// </summary>
        public RelayCommand RedoCommand
        {
            get
            {
                if (redoCommand == null)
                {
                    redoCommand = new RelayCommand(x => OnRedo((Transaction)x), x => CanRedo, false);
                    RollbackedTransactions.CollectionChanged += (s,e) => redoCommand.RaiseCanExecuteChanged();
                }
                return redoCommand;
            }
        }

        void OnRedo(Transaction transaction)
        {
            if (transaction == null) Redo();
            else Redo(transaction);
        }

        #endregion

        #endregion

        #region Methods

        /// <summary>
        /// Undos transactions up to the given
        /// </summary>
        /// <param name="transaction"></param>
        public void Undo(Transaction transaction)
        {
            while (PerformedTransactions.Count > 0)
            {
                Transaction item = PerformedTransactions[0];
                item.Rollback();
                PerformedTransactions.RemoveAt(0);
                if (RollbackedTransactions.Count >= MaxHistoryLength) 
                    RollbackedTransactions.RemoveAt(RollbackedTransactions.Count - 1);
                RollbackedTransactions.Insert(0, item);
                if (item == transaction) return;
            }
        }

        /// <summary>
        /// Redo transactions up to the given
        /// </summary>
        /// <param name="transaction"></param>
        public void Redo(Transaction transaction)
        {
            while (RollbackedTransactions.Count > 0)
            {
                Transaction item = RollbackedTransactions[0];
                item.Perform();
                RollbackedTransactions.RemoveAt(0);
                if (PerformedTransactions.Count >= MaxHistoryLength)
                    PerformedTransactions.RemoveAt(PerformedTransactions.Count - 1);
                PerformedTransactions.Insert(0, item);
                if (item == transaction) return;
            }
        }

        /// <summary>
        /// Undos the latest transaction
        /// </summary>
        public void Undo()
        {
            if (PerformedTransactions.Count == 0) return;
            Undo(PerformedTransactions[0]);
        }

        /// <summary>
        /// Redo the latest undoed transaction
        /// </summary>
        public void Redo()
        {
            if (RollbackedTransactions.Count == 0) return;
            Redo(RollbackedTransactions[0]);
        }

        /// <summary>
        /// Clears all history of performed and rollbacked commands
        /// </summary>
        public void Reset()
        {
            PerformedTransactions.Clear();
            RollbackedTransactions.Clear();
        }

        #endregion
    }
}
