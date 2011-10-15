using System.ComponentModel;
using Media3D = System.Windows.Media.Media3D;
using NuGenBioChem.Data.Transactions;

namespace NuGenBioChem.Data
{
    /// <summary>
    /// Represents bond between two atoms
    /// </summary>
    public class Bond : INotifyPropertyChanged
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

        // Begin & end of the bond
        readonly Transactable<Atom> begin = new Transactable<Atom>();
        readonly Transactable<Atom> end = new Transactable<Atom>();

        // Radius
        readonly Transactable<double> radius = new Transactable<double>(0.1);
        // First material
        readonly Transactable<Media3D.Material> beginMaterial = new Transactable<Media3D.Material>();
        // Second material
        readonly Transactable<Media3D.Material> endMaterial = new Transactable<Media3D.Material>();

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets begin of the bond
        /// </summary>
        public Atom Begin
        {
            get { return begin.Value; }
            set
            {
                // TODO: Add transactio here?
                begin.Value = value;
            }
        }

        /// <summary>
        /// Gets or sets end of the bond
        /// </summary>
        public Atom End
        {
            get { return end.Value; }
            set
            {
                // TODO: Add transactio here?
                end.Value = value;
            }
        }
        
        #endregion
        
        #region Initialization

        /// <summary>
        /// Default constructor
        /// </summary>
        public Bond()
        {
            begin.Changed += (s, a) => RaisePropertyChanged("Begin");
            end.Changed += (s, a) => RaisePropertyChanged("End");
            radius.Changed += (s, a) => RaisePropertyChanged("Size");
            endMaterial.Changed += (s, a) => RaisePropertyChanged("EndMaterial");
            beginMaterial.Changed += (s, a) => RaisePropertyChanged("BeginMaterial");
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
            return string.Format("[{0}]<->[{1}]", begin, end);
        }

        #endregion
    }
}
