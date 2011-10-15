using System;
using System.ComponentModel;
using NuGenBioChem.Data.Transactions;

namespace NuGenBioChem.Data
{
    /// <summary>
    /// Represents secondary structure element of the protein (helix or sheet)
    /// </summary>
    public class SecondaryStructure : INotifyPropertyChanged  
    {
        #region Events

        /// <summary>
        /// Occures when property has been changed
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region Fields

        // Type of the secondary structure
        readonly Transactable<SecondaryStructureType> structureType = new Transactable<SecondaryStructureType>();
        // Chain where secondary structure is
        readonly Transactable<Chain> chain = new Transactable<Chain>(); 
        // First residue index in the chain
        readonly Transactable<int> firstResidueSequenceNumber = new Transactable<int>();
        // Last residue index in the chain
        readonly Transactable<int> lastResidueSequenceNumber = new Transactable<int>();
        
        // Material of the secondary structure
        readonly Transactable<System.Windows.Media.Media3D.Material> material = new Transactable<System.Windows.Media.Media3D.Material>();

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the type of the secondary structure
        /// </summary>
        public SecondaryStructureType StructureType
        {
            get { return structureType.Value; }
            set
            {
                structureType.Value = value; 
            }
        }

        /// <summary>
        /// Gets or sets the chain where secondary structure is
        /// </summary>
        public Chain Chain
        {
            get { return this.chain.Value; }
            set
            {
                chain.Value = value;
            }
        }

        /// <summary>
        /// Gets or sets first residue index of the structure
        /// </summary>
        public int FirstResidueSequenceNumber
        {
            get { return firstResidueSequenceNumber.Value; }
            set
            {
                firstResidueSequenceNumber.Value = value;
            }
        }

        /// <summary>
        /// Gets or sets last residue index of the structure
        /// </summary>
        public int LastResidueSequenceNumber
        {
            get { return lastResidueSequenceNumber.Value; }
            set
            {
                lastResidueSequenceNumber.Value = value;
            }
        }

        public System.Windows.Media.Media3D.Material Material
        {
            get { return this.material.Value; }
            set
            {
                this.material.Value = value;
            }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Default Constructor
        /// </summary>
        public SecondaryStructure()
        {
            structureType.Changed += (s, a) => RaisePropertyChanged("StructureType");
            chain.Changed += (s, a) => RaisePropertyChanged("Chain");
            firstResidueSequenceNumber.Changed += (s, a) => RaisePropertyChanged("FirstResidueIndex");
            lastResidueSequenceNumber.Changed += (s, a) => RaisePropertyChanged("LastResidueIndex");
            this.material.Changed += (s, a) => RaisePropertyChanged("Material");
        }

        #endregion
    }
}
