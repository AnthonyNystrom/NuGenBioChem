using System;
using System.ComponentModel;
using NuGenBioChem.Data.Transactions;

namespace NuGenBioChem.Data
{
    /// <summary>
    /// Represents collection of the chains
    /// </summary>
    public class ChainCollection : TransactableCollection<Chain>
    {
        #region Events

        void RaisePropertyChanged(string propertyName)
        {
           OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region Fields

        // Molecule where the chains are
        readonly Transactable<Molecule> molecule = new Transactable<Molecule>(null);

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets molecule where chains are
        /// </summary>
        public Molecule Molecule
        {
            get { return molecule.Value; }
            set
            {
                molecule.Value = value;
                foreach (Chain chain in this) chain.Molecule = value; 
            }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Default constructor
        /// </summary>
        public ChainCollection()
        {
            molecule.Changed += (s, a) => RaisePropertyChanged("Molecule"); 
        }

        #endregion

        #region Overrides

        protected override void InsertItem(int index, Chain item)
        {
            base.InsertItem(index, item);
            item.Molecule = Molecule;
        }

        protected override void SetItem(int index, Chain item)
        {
            base.SetItem(index, item);
            item.Molecule = Molecule;
        } 

        #endregion
    }
}
