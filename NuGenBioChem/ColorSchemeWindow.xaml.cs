using System;
using System.Windows;
using System.Windows.Media;
using NuGenBioChem.Data;

namespace NuGenBioChem
{
    /// <summary>
    /// Represents dialog for color scheme editing
    /// </summary>
    public partial class ColorSchemeWindow : System.Windows.Window
    {
        #region Fields


        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets color scheme
        /// </summary>
        public ColorScheme ColorScheme
        {
            get { return periodicalTable.ColorScheme;}
            set
            {
                periodicalTable.ColorScheme = value;
            }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Default constructor
        /// </summary>
        public ColorSchemeWindow()
        {
            InitializeComponent();
        }

        #endregion

        #region Event Handlers
        
        void OnMaterialChanged(object sender, EventArgs e)
        {
            
        }

        #endregion
    }
}
