using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using NuGenBioChem.Data.Transactions;

namespace NuGenBioChem.Data
{
    /// <summary>
    /// Represents substance
    /// </summary>
    public class Substance : INotifyPropertyChanged
    {
        #region Events

        /// <summary>
        /// Occurs when property has been changed
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        
        #endregion

        #region Fields

        // Name of the style
        readonly Transactable<string> name = new Transactable<string>("Untitled");
        // Molecules
        readonly MoleculeCollection molecules = new MoleculeCollection();
        
        // Currently selected atoms 
        readonly ObservableCollection<Atom> selectedAtoms = new ObservableCollection<Atom>();

        // Currently selected residues
        readonly ObservableCollection<Residue> selectedResidues = new ObservableCollection<Residue>();

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets name of the style
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
        /// Gets or sets molecules
        /// </summary>
        public MoleculeCollection Molecules
        {
            get { return molecules; }
        }
        
        /// <summary>
        /// Gets selected atoms
        /// </summary>
        public ObservableCollection<Atom> SelectedAtoms
        {
            get { return selectedAtoms; }
        }

        /// <summary>
        /// Gets selected residues
        /// </summary>
        public ObservableCollection<Residue> SelectedResidues
        {
            get { return selectedResidues; }
        }

        #endregion
        
        #region Initialization

        /// <summary>
        /// Constructor
        /// </summary>
        public Substance()
        {
            molecules.Substance = this;
            name.Changed += (s, a) => RaisePropertyChanged("Name");            
        }

        #endregion
    }
}
