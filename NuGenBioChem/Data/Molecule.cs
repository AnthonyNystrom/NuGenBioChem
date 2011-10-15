using System;
using System.ComponentModel;
using NuGenBioChem.Data.Transactions;

namespace NuGenBioChem.Data
{
    /// <summary>
    /// Represents molecule
    /// </summary>
    public class Molecule : INotifyPropertyChanged
    {
        #region Constants

        /// <summary>
        /// Heuristic length of the covalent bond in angstrom units
        /// </summary>
        const double CovalentBondLength = 0.0; // sqrt(3.6) ?

        #endregion

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

        // Name of the molecule
        readonly Transactable<string> name = new Transactable<string>("Untitled");
        // Atoms of the molecule
        readonly AtomCollection atoms = new AtomCollection();
        // Bonds of the molecule
        readonly BondCollection bonds = new BondCollection();

        // Substance where the molecule is
        readonly Transactable<Substance> substance = new Transactable<Substance>(null);

        // Chains of the molecule (for protein only)
        readonly ChainCollection chains = new ChainCollection();

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets name of the molecule
        /// </summary>
        public string Name
        {
            get { return name.Value; }
            set
            {
                // TODO: maybe add here transaction
                name.Value = value;
            }
        }

        /// <summary>
        /// Gets or sets substance where the molecule is
        /// </summary>
        public Substance Substance
        {
            get { return substance.Value; }
            set
            {
                // TODO: maybe add here transaction
                substance.Value = value;
            }
        }
        
        /// <summary>
        /// Gets atoms of the molecule
        /// </summary>
        public AtomCollection Atoms
        {
            get { return atoms; }
        }

        /// <summary>
        /// Gets bonds of the molecule
        /// </summary>
        public BondCollection Bonds
        {
            get { return bonds; }
        }

        /// <summary>
        /// Gets chains of the molecule (for protein molecule only)
        /// </summary>
        public ChainCollection Chains
        {
            get { return chains; }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Default constructor
        /// </summary>
        public Molecule()
        {
            atoms.Molecule = this;
            chains.Molecule = this;
            name.Changed += (s, a) => RaisePropertyChanged("Name");
            substance.Changed += (s, a) => RaisePropertyChanged("Substance");
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
            return string.Format("{0}, {1} atoms, {2} bonds, {3} chains", 
                Name, Atoms.Count.ToString(), Bonds.Count.ToString(), Chains.Count.ToString());
        }

        /// <summary>
        /// Calculates all bonds of the atoms in the molecule
        /// using heuristic covalent bond distance
        /// </summary>
        public void CalculateBonds()
        {
            bonds.Clear();
            for (int i = 0; i < atoms.Count; i++)
            {
                for (int j = i + 1; j < atoms.Count; j++)
                {
                    // The sum of the two covalent radii should equal the covalent 
                    // bond length between two atoms with some epsilon
                    const double epsilon = 0.25;
                    Atom a = atoms[i];
                    Atom b = atoms[j];
                    double distance = (a.Position - b.Position).Length;
                    double requiredDistance = a.Element.CovalentRadius + b.Element.CovalentRadius;
                    if (Math.Abs(distance - requiredDistance) <= epsilon)
                    {
                        this.bonds.Add(
                            new Bond() { Begin = a, End = b }
                        );
                    }
                }
            }
        }       

        #endregion
    }
}
