using System.ComponentModel;
using NuGenBioChem.Data.Transactions;

namespace NuGenBioChem.Data
{
    /// <summary>
    /// Represents collection of molecules
    /// </summary>
    public class AtomCollection : TransactableCollection<Atom>
    {
        #region Events
        
        void RaisePropertyChanged(string propertyName)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region Fields

        // Parent object
        readonly Transactable<Molecule> molecule = new Transactable<Molecule>();

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets molecule where atoms are
        /// </summary>
        public Molecule Molecule
        {
            get { return molecule.Value; }
            set
            {
                molecule.Value = value;
                foreach (Atom atom in this) atom.Molecule = value;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Inserts an item into the collection at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which item should be inserted.</param>
        /// <param name="item">The object to insert.</param>
        protected override void InsertItem(int index, Atom item)
        {
            base.InsertItem(index, item);
            item.Molecule = molecule.Value;
        }

        /// <summary>
        /// Replaces the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to replace.</param>
        /// <param name="item">The new value for the element at the specified index.</param>
        protected override void SetItem(int index, Atom item)
        {
            base.SetItem(index, item);
            item.Molecule = molecule.Value;
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Constructor
        /// </summary>
        public AtomCollection()
        {
            molecule.Changed += (s, a) => RaisePropertyChanged("Molecule");
        }

        #endregion
    }
}
