using System.ComponentModel;
using System.Windows.Media.Media3D;
using Media3D = System.Windows.Media.Media3D;
using NuGenBioChem.Data.Transactions;

namespace NuGenBioChem.Data
{
    /// <summary>
    /// Represents atom
    /// </summary>
    public class Atom : INotifyPropertyChanged
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

        // Position of the atom
        readonly Transactable<Point3D> position = new Transactable<Point3D>();
        // Chemical element
        readonly Transactable<Element> element = new Transactable<Element>(Element.Undefined);
        // Molecule where the atom is
        readonly Transactable<Molecule> molecule = new Transactable<Molecule>(null);
        
        // Actual radius
        readonly Transactable<double> radius = new Transactable<double>(0.5);

        // Atom material
        readonly Transactable<Media3D.Material> material = new Transactable<Media3D.Material>();

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets position of the atom
        /// </summary>
        public Point3D Position
        {
            get { return position.Value; }
            set
            {
                // TODO: Add transactio here?
                position.Value = value;
            }
        }

        /// <summary>
        /// Gets or sets representing element
        /// </summary>
        public Element Element
        {
            get { return element.Value; }
            set
            {
                // TODO: Add transaction here?
                element.Value = value;
            }
        }

        /// <summary>
        /// Gets or sets molecule where the atom is
        /// </summary>
        public Molecule Molecule
        {
            get { return molecule.Value; }
            set
            {
                // TODO: maybe add here transaction
                molecule.Value = value;
            }
        }
        
        #endregion

        #region Initialization

        /// <summary>
        /// Default constructor
        /// </summary>
        public Atom()
        {
            position.Changed += (s, a) => RaisePropertyChanged("Position");
            element.Changed += (s, a) => RaisePropertyChanged("Element");
            molecule.Changed += (s, a) => RaisePropertyChanged("Molecule");
            radius.Changed += (s, a) => RaisePropertyChanged("Radius");
            material.Changed += (s, a) => RaisePropertyChanged("Material");
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
            return string.Format("[{0}] {1}: Position = ({2})", element.Value.Symbol, element.Value.Name, Position);
        }
        
        #endregion
    }
}
