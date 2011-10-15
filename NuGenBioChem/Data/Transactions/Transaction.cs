using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NuGenBioChem.Data.Commands;

namespace NuGenBioChem.Data.Transactions
{
    /// <summary>
    /// Represents a named group of actions with data
    /// </summary>
    public class Transaction : IDisposable
    {
        #region Static

        /// <summary>
        /// Current transaction
        /// </summary>
        public static Transaction Current
        { 
            get;
            private set;
        }

        // If suspendCount == 0 than the transaction
        // mechanism is not suspended
        static int suspendCount = 0;

        /// <summary>
        /// Gets the value indicating whether 
        /// the transaction mechanism is suspended
        /// </summary>
        public static bool IsSuspended
        {
            get { return suspendCount != 0; }
        }

        /// <summary>
        /// Suspends the transaction mechanism
        /// </summary>
        public static void Suspend()
        {
            suspendCount++;
        }

        /// <summary>
        /// Resumes the transaction mechanism
        /// </summary>
        public static void Resume()
        {
            suspendCount--;
        }

        #endregion

        #region Fields

        // List with operations
        readonly List<Operation> operations = new List<Operation>();

        // Status of the transaction
        enum Status
        {
            Processing = 0,
            Commited,
            Canceled
        }
        Status status;

        // Is the transaction rollbacked
        bool rollbacked;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets name of the transaction
        /// </summary>
        public string Name
        {
            get; private set;
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name of the transaction</param>
        public Transaction(string name)
        {
            Name = name;
            if (Current == null) Current = this;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Performs the given operation
        /// </summary>
        /// <param name="operation">Operation</param>
        public void Perform(Operation operation)
        {
            if (status != Status.Processing) throw new Exception("Unable to perform any new operation in this state");
            operation.Perform();
            operations.Add(operation);
            if (Current != this) Current.operations.Add(operation);
        }

        /// <summary>
        /// Commits the given operations
        /// </summary>
        public void Commit()
        {
            if (status != Status.Processing) throw new Exception("Unable to commit a transaction in this state");
            status = Status.Commited;
            if (Current == this)
            {
                Current = null;

                TransactionContext transactionContext = TransactionContext.Current;
                if (!IsSuspended && transactionContext != null && operations.Count != 0)
                {
                    if (transactionContext.PerformedTransactions.Count >= transactionContext.MaxHistoryLength)
                        transactionContext.PerformedTransactions.RemoveAt(transactionContext.PerformedTransactions.Count - 1);
                    transactionContext.PerformedTransactions.Insert(0, this);
                    transactionContext.RollbackedTransactions.Clear();
                }
            }
        }

        /// <summary>
        /// Cancels the given operations
        /// </summary>
        public void Cancel()
        {
            if (status != Status.Processing) throw new Exception("Unable to cancel a transaction in this state");
            status = Status.Canceled;

            for (int i = operations.Count - 1; i >= 0; i--) operations[i].Rollback();
            operations.Clear();
            if (Current == this) Current = null;
        }

        /// <summary>
        /// Performs all inner operations
        /// </summary>
        public void Perform()
        {
            if (status != Status.Commited) throw new Exception("Unable to perform a transaction in this state");
            if (!rollbacked) throw new Exception("This transaction has already performed");
            foreach (Operation operation in operations) operation.Perform();
            rollbacked = false;
        }

        /// <summary>
        /// Rollback all inner operations
        /// </summary>
        public void Rollback()
        {
            if (status != Status.Commited) throw new Exception("Unable to rollback a transaction in this state");
            if (rollbacked) throw new Exception("This transaction has already rollbacked");
            for (int i = operations.Count - 1; i >= 0; i--) operations[i].Rollback();
            rollbacked = true;
        }

        /// <summary>
        /// Cancels the transaction
        /// </summary>
        public void Dispose()
        {
            if (status == Status.Commited) return;
            if (!rollbacked) for (int i = operations.Count - 1; i >= 0; i--) operations[i].Rollback();
            operations.Clear();
            status = Status.Canceled;
            if (Current == this) Current = null;
        }

        /// <summary>
        /// Converts the transaction to string
        /// </summary>
        /// <returns>string</returns>
        public override string ToString()
        {
            return Name;
        }

        #endregion
    }

    /// <summary>
    /// Represents atomic operation
    /// </summary>
    public class Operation
    {
        /// <summary>
        /// Action to perform the operation
        /// </summary>
        public readonly Action Perform;

        /// <summary>
        /// Action to undo the operation
        /// </summary>
        public readonly Action Rollback;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="perform">Action to perform the operation</param>
        /// <param name="rollback">Action to undo the operation</param>
        public Operation(Action perform, Action rollback)
        {
            Perform = perform;
            Rollback = rollback;
        }
    }
}
