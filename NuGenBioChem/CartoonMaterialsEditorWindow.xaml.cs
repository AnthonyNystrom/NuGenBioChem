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
    /// Cartoon Materials Editor Window
    /// </summary>
    public partial class CartoonMaterialsEditorWindow
    {
        #region Color Style

        /// <summary>
        /// Gets or sets color style
        /// </summary>
        public ColorStyle ColorStyle
        {
            get { return (ColorStyle)DataContext; }
            set { DataContext = value; }
        }

        #endregion

        /// <summary>
        /// Default constructor
        /// </summary>
        public CartoonMaterialsEditorWindow()
        {
            InitializeComponent();
        }
    }
}
