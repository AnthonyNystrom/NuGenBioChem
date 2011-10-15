using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;

namespace NuGenBioChem.Data.Transactions
{
    /// <summary>
    /// Represents transactable collection
    /// </summary>
    [Serializable]
    [SuppressMessage("Microsoft.Usage", "CA0001")]
    public class TransactableCollection<T> : ObservableCollection<T>
    {
        /// <summary>
        /// Occurs when an item is added, removed, changed, moved, or the entire list is refreshed.
        /// </summary>
        [field: NonSerialized]
        public override event NotifyCollectionChangedEventHandler CollectionChanged;


        /// <summary>
        /// Raises the CollectionChanged event with the provided arguments.
        /// </summary>
        /// <param name="e">Arguments of the event being raised.</param>
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnCollectionChanged(e);
            if (CollectionChanged != null) CollectionChanged(this, e);
        }

        // When it is seted, that means we musnt 
        // perform transactable operations
        bool transactionEnable = true;

        /// <summary>
        /// Inserts an item into the collection at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="item"/> should be inserted.</param><param name="item">The object to insert.</param>
        protected override void InsertItem(int index, T item)
        {
            if (Transaction.Current != null && transactionEnable)
            {
                transactionEnable = false;
                Transaction.Current.Perform(new Operation(
                    () => base.InsertItem(index, item),
                    () => base.RemoveItem(index)));
                transactionEnable = true;
            }
            else base.InsertItem(index, item);
        }

        /// <summary>
        /// Removes all items from the collection.
        /// </summary>
        protected override void ClearItems()
        {
            if (Transaction.Current != null && transactionEnable)
            {
                transactionEnable = false;
                
                T[] clearedItems = new T[Count];
                CopyTo(clearedItems, 0);

                Transaction.Current.Perform(new Operation(
                    () => base.ClearItems(),
                    () => AddRange(clearedItems)));
                transactionEnable = true;
            }
            else base.ClearItems();
        }

        /// <summary>
        /// Moves the item at the specified index to a new location in the collection.
        /// </summary>
        /// <param name="oldIndex">The zero-based index specifying the location of the item to be moved.</param><param name="newIndex">The zero-based index specifying the new location of the item.</param>
        protected override void MoveItem(int oldIndex, int newIndex)
        {
            if (Transaction.Current != null && transactionEnable)
            {
                transactionEnable = false;
                Transaction.Current.Perform(new Operation(
                    () => base.MoveItem(oldIndex, newIndex),
                    () => base.MoveItem(newIndex, oldIndex)));
                transactionEnable = true;
            }
            else base.MoveItem(oldIndex, newIndex);
        }

        /// <summary>
        /// Removes the item at the specified index of the collection.
        /// </summary>
        /// <param name="index">The zero-based index of the element to remove.</param>
        protected override void RemoveItem(int index)
        {
            if (Transaction.Current != null && transactionEnable)
            {
                transactionEnable = false;
                T removedItem = this[index];
                Transaction.Current.Perform(new Operation(
                    () => base.RemoveItem(index),
                    () => base.InsertItem(index, removedItem)));
                transactionEnable = true;
            }
            else base.RemoveItem(index); 
        }


        /// <summary>
        /// Replaces the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to replace.</param><param name="item">The new value for the element at the specified index.</param>
        protected override void SetItem(int index, T item)
        {
            if (Transaction.Current != null && transactionEnable)
            {
                transactionEnable = false;
                T previousItem = this[index];
                Transaction.Current.Perform(new Operation(
                    () => base.SetItem(index, item),
                    () => base.SetItem(index, previousItem)));
                transactionEnable = true;
            }
            else base.SetItem(index, item); 
        }

        /// <summary>
        /// Adds range of items
        /// </summary>
        /// <param name="items">Items</param>
        public void AddRange(IEnumerable<T> items)
        {
            foreach (T item in items)
            {
                Add(item);
            }
        }

    }
}
