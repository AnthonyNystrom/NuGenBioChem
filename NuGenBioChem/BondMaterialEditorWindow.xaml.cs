using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using NuGenBioChem.Data;

namespace NuGenBioChem
{
    /// <summary>
    /// Bond Material Editor Window
    /// </summary>
    public partial class BondMaterialEditorWindow
    {
        #region Bond Material

        /// <summary>
        /// Gets or sets color style
        /// </summary>
        public Material BondMaterial
        {
            get { return (Material)DataContext; }
            set { DataContext = value; }
        }

        #endregion

        /// <summary>
        /// Default constructor
        /// </summary>
        public BondMaterialEditorWindow()
        {
            InitializeComponent();
        }
    }
}
