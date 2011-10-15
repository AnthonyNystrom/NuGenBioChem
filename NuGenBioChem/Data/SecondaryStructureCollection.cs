using System;
using System.ComponentModel;
using NuGenBioChem.Data.Transactions; 

namespace NuGenBioChem.Data
{
    /// <summary>
    /// Collection of the secondary structures
    /// </summary>
    public class SecondaryStructureCollection : TransactableCollection<SecondaryStructure>
    {
        #region Events

        void RaisePropertyChanged(string propertyName)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName)); 
        }

        #endregion

        #region Fields

        // Chain where the secondary structure elements are;
        readonly Transactable<Chain> chain = new Transactable<Chain>(null);

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the chain where the secondary structure elements are
        /// </summary>
        public Chain Chain
        {
            get { return this.chain.Value; }
            set
            {
                this.chain.Value = value;
                foreach (SecondaryStructure structure in this) structure.Chain = value;
            }
        }

        #endregion

        #region Initialization

        public SecondaryStructureCollection()
        {
            chain.Changed += (s, a) => RaisePropertyChanged("Chain");
        }

        #endregion
    }
}
