using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Documents.Serialization;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Xps;
using NuGenBioChem.Controls.Backstage;

namespace NuGenBioChem.Controls
{
    /// <summary>
    /// Interaction logic for PrintingStatusBarContent.xaml
    /// </summary>
    public partial class PrintingStatusBarContent : UserControl
    {
        // Writer for printing
        private XpsDocumentWriter writer;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="writer">Writer for printing</param>
        public PrintingStatusBarContent(XpsDocumentWriter writer)
        {
            InitializeComponent();
            this.writer = writer;
            writer.WritingProgressChanged += OnWritingProgressChanged;
        }

        // Handles writer progress changes
        private void OnWritingProgressChanged(object sender, WritingProgressChangedEventArgs e)
        {
            // Update progress bar
            progressBar.Value = e.ProgressPercentage;
        }

        // Handles cancel click
        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            writer.CancelAsync();
        }
    }
}
