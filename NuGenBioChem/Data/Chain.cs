using System;
using System.ComponentModel;
using NuGenBioChem.Data.Transactions; 

namespace NuGenBioChem.Data
{
    /// <summary>
    /// Representation of a covalently linked amino acid residues to form a protein molecule
    /// </summary>
    public class Chain : INotifyPropertyChanged 
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

        // Amino acid residues in the chain
        readonly ResidueCollection residues = new ResidueCollection();
        // Collection of the secondary structure elements (helices and sheets)
        readonly SecondaryStructureCollection secondaryStructures = new SecondaryStructureCollection();
        // Molecule where the chain is 
        readonly Transactable<Molecule> molecule = new Transactable<Molecule>(null);     
        // Name of the chain
        readonly Transactable<string> name = new Transactable<string>("");

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets name of the chain
        /// </summary>
        public string Name
        {
            get { return name.Value; }
            set { name.Value = value; }
        }

        /// <summary>
        /// Gets or sets the molecule where the chain is
        /// </summary>
        public Molecule Molecule
        {
            get { return this.molecule.Value; }
            set
            {
                this.molecule.Value = value;
            }
        }

        /// <summary>
        /// Gets residues in the chain
        /// </summary>
        public ResidueCollection Residues
        {
            get { return this.residues; }
        }

        /// <summary>
        /// Gets secondary structure elements in the chain
        /// </summary>
        public SecondaryStructureCollection SecondaryStructures
        {
            get { return this.secondaryStructures; }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Default constructor
        /// </summary>
        public Chain()
        {
            this.residues.Chain = this;
            this.secondaryStructures.Chain = this; 
            this.molecule.Changed += (s, a) => RaisePropertyChanged("Molecule"); 
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the type of the secondary structure which contain
        /// a specified residue
        /// </summary>
        /// <param name="resdiue">Residue</param>
        /// <returns>Secondary structure</returns>
        public SecondaryStructureType GetStructureType(Residue resdiue)
        {
            SecondaryStructure structure = this.FindStructure(resdiue);
            if (structure != null) return structure.StructureType;
            return SecondaryStructureType.NotDefined;
        }

        /// <summary>
        /// Finds structure in this chain that contain a residue with specified sequence number
        /// if there is not it - returns null
        /// </summary>
        /// <param name="resdiue">Residue</param>
        /// <returns>Secondary structure or null</returns>
        public SecondaryStructure FindStructure(Residue resdiue)
        {
            int residueSequenceNumber = resdiue.SequenceNumber;
            foreach(SecondaryStructure structure in this.secondaryStructures) 
            {
                if (residueSequenceNumber >= structure.FirstResidueSequenceNumber && residueSequenceNumber <= structure.LastResidueSequenceNumber)
                    return structure;
            }
            return null;
        }
        
        #endregion
    }
}
