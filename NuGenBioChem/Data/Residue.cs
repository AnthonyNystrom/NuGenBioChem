using System;
using System.ComponentModel; 
using NuGenBioChem.Data.Transactions;

namespace NuGenBioChem.Data
{
    /// <summary>
    /// Representation of an amino acids (or a nucleo acids) residue which linked to form a protein
    /// </summary>
    public class Residue : INotifyPropertyChanged
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

        // Chain where residue is
        readonly Transactable<Chain> chain = new Transactable<Chain>(null);
        // Atoms in the residue
        readonly AtomCollection atoms = new AtomCollection();
        // Standard residue name (if known)
        readonly Transactable<string> name = new Transactable<string>("UNK");
        // Alfa-Carbon of the residue
        readonly Transactable<Atom> alfaCarbon = new Transactable<Atom>(null);
        // Residiue sequence index
        readonly Transactable<int> sequenceNumber = new Transactable<int>(0);
        // Residue material
        readonly Transactable<System.Windows.Media.Media3D.Material> material = new Transactable<System.Windows.Media.Media3D.Material>(null);   

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets chain where residue is
        /// </summary>
        public Chain Chain
        {
            get { return this.chain.Value; }
            set 
            {
                this.chain.Value = value; 

            }
        }

        /// <summary>
        /// Gets of sets residue name
        /// </summary>
        public string Name
        {
            get { return this.name.Value; }
            set
            {
                this.name.Value = value;
            }
        }

        /// <summary>
        /// Gets collection of atoms in the residue
        /// </summary>
        public AtomCollection Atoms
        {
            get { return this.atoms; }
        }

        /// <summary>
        /// Gets or sets alfa-carbon atom reference
        /// </summary>
        public Atom AlfaCarbon
        {
            get
            {
                return alfaCarbon.Value;
            }
            set
            {
                alfaCarbon.Value = value; 
            }
        }
       
        /// <summary>
        /// Gets or sets the sequence index of the residue in the molecule
        /// </summary>
        public int SequenceNumber
        {
            get { return sequenceNumber.Value; }
            set
            {
                sequenceNumber.Value = value; 
            }
        }

        /// <summary>
        /// Gets or sets material of the residue
        /// </summary>
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
        /// Default constructor
        /// </summary>
        public Residue()
        {
            chain.Changed += (s, a) => RaisePropertyChanged("Chain");
            name.Changed += (s, a) => RaisePropertyChanged("Name");
            material.Changed += (s, a) => RaisePropertyChanged("Material");
        }

        #endregion

        #region Methods

        /// <summary>
        /// Find the first entry of an element into the residue
        /// </summary>
        /// <param name="symbol">Atom of an element</param>
        /// <returns>Atom or null if the element does not contain in residue</returns>
        public Atom FindAtom(string symbol)
        {
            foreach(Atom atom in this.atoms) 
            {
                if (atom.Element.Symbol == symbol) return atom; 
            }
            return null;
        }

        /// <summary>
        /// Gets the secondary structure type of the residue
        /// </summary>
        /// <returns>Type of the secondary structure</returns>
        public SecondaryStructureType GetStructureType()
        {
            return this.Chain.GetStructureType(this); 
        }

        /// <summary>
        /// Gets the seconsdary structure of the residue or null
        /// </summary>
        /// <returns>Secondary structure or null</returns>
        public SecondaryStructure GetSecondaryStructure()
        {
            return this.Chain.FindStructure(this);  
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Creates string representation of the residue
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            return string.Format("{0}, {1} atoms, has alfa carbon = {2}",
                Name, Atoms.Count.ToString(), (AlfaCarbon != null).ToString());
        }

        #endregion
    }

}
