using System.ComponentModel;
using NuGenBioChem.Data.Transactions;

namespace NuGenBioChem.Data
{
    /// <summary>
    /// Represents collection of molecules
    /// </summary>
    public class MoleculeCollection : TransactableCollection<Molecule>
    {
        #region Events
        
        void RaisePropertyChanged(string propertyName)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region Fields

        // Parent object
        readonly Transactable<Substance> substance = new Transactable<Substance>();

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets substance
        /// </summary>
        public Substance Substance
        {
            get { return substance.Value; }
            set
            {
                substance.Value = value;
                foreach (Molecule molecule in this) molecule.Substance = value;
            }
        }
        
        #endregion

        #region Methods

        /// <summary>
        /// Inserts an item into the collection at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which item should be inserted.</param>
        /// <param name="item">The object to insert.</param>
        protected override void InsertItem(int index, Molecule item)
        {
            base.InsertItem(index, item);
            item.Substance = substance.Value;
        }

        /// <summary>
        /// Replaces the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to replace.</param>
        /// <param name="item">The new value for the element at the specified index.</param>
        protected override void SetItem(int index, Molecule item)
        {
            base.SetItem(index, item);
            item.Substance = substance.Value;
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Constructor
        /// </summary>
        public MoleculeCollection()
        {
            substance.Changed += (s, a) => RaisePropertyChanged("Substance");
        }

        #endregion
    }
}
