using System;
using System.ComponentModel;
using NuGenBioChem.Data.Transactions;

namespace NuGenBioChem.Data
{
    /// <summary>
    /// Repesentation of a residue chain
    /// </summary>
    public class ResidueCollection : TransactableCollection<Residue>
    {
        #region Events

        void RaisePropertyChanged(string propertyName)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));  
        }

        #endregion

        #region Fields

        // Chain where residues are
        readonly Transactable<Chain> chain = new Transactable<Chain>(null); 

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets chain where residues are
        /// </summary>
        public Chain Chain
        {
            get { return chain.Value; }
            set
            {
                chain.Value = value;
                foreach (Residue residue in this) residue.Chain = value;
            }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Default constructor
        /// </summary>
        public ResidueCollection()
        {
            this.chain.Changed += (s, a) => RaisePropertyChanged("Chain");  
        }

        #endregion

        #region Overrides

        protected override void SetItem(int index, Residue item)
        {
            base.SetItem(index, item);
            item.Chain = Chain;
        }

        protected override void InsertItem(int index, Residue item)
        {
            base.InsertItem(index, item);
            item.Chain = Chain;
        }

        #endregion
    }
}
