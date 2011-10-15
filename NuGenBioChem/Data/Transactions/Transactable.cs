using System;

namespace NuGenBioChem.Data.Transactions
{
    /// <summary>
    /// Represent container for a value 
    /// which can be in transactable context
    /// </summary>
    /// <typeparam name="T">Any type</typeparam>
    [Serializable]
    public class Transactable<T>
    {
        #region Events

        /// <summary>
        /// Occurs when value has been changed
        /// </summary>
        public event EventHandler<ValueChangedEventArgs<T>> Changed;

        #endregion

        #region Fields

        T value;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets current value
        /// </summary>
        public T Value
        {
            get
            {
                return value;
            }
            set
            {
                object boxedValue = value;
                object boxedThisValue = this.value;
                if ((boxedThisValue == null) && (boxedValue == null)) return;
                if (boxedValue != null && value.Equals(this.value)) return;

                T previous = this.value;
                T current = value;

                if (Transaction.Current != null)
                {
                    Transaction.Current.Perform(new Operation(
                        () => SetValue(current),
                        () => SetValue(previous)));
                }
                else SetValue(value);
                
            }
        }

        void SetValue(T v)
        {
            if ((value as object == null) && (v as object == null)) return;
            if (((value as object != null) && (v as object != null)) && value.Equals(v)) return;

            T previous = value;
            this.value = v;

            if (Changed != null) Changed(this, new ValueChangedEventArgs<T>(previous, v));
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Default constructor
        /// </summary>
        public Transactable()
        {
            value = default(T);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="initial">Initial value</param> 
        public Transactable(T initial)
        {
            value = initial;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns that represents the current object
        /// </summary>
        /// <returns>
        /// That represents the current object
        /// </returns>
        public override string ToString()
        {
            object boxed = value;
            if (boxed == null) return "";
            return boxed.ToString();
        }

        #endregion
    }

    /// <summary>
    /// Represents event arguments for value changed
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ValueChangedEventArgs<T> : EventArgs
    {
        /// <summary>
        /// Previous value
        /// </summary>
        public readonly T Previous;
        /// <summary>
        /// Current value
        /// </summary>
        public readonly T Current;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="previous">Previous value</param>
        /// <param name="current">Current value</param>
        public ValueChangedEventArgs(T previous, T current)
        {
            Previous = previous;
            Current = current;
        }
    }
}
