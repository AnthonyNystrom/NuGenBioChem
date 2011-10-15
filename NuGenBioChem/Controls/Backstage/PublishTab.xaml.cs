using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using NuGenBioChem.Visualization;

namespace NuGenBioChem.Controls.Backstage
{
    /// <summary>
    /// Interaction logic for PublishTab.xaml
    /// </summary>
    public partial class PublishTab : UserControl
    {
        
        /// <summary>
        /// Gets visualizer
        /// </summary>
        public Visualizer Visualizer
        {
            get { return ((Visualizer)previewPage.PageContent); }
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public PublishTab()
        {
            InitializeComponent();
        }

        // Handles save image click
        private void OnSaveImageClick(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "24-bit Bitmap (*.bmp)|*.bmp|JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|GIF (*.gif)|*.gif|PNG (*.png)|*.png";
            dlg.FilterIndex = 4;
            bool? dlgResult = dlg.ShowDialog(Window.GetWindow(this));
            if(dlgResult.HasValue && dlgResult.Value)
            {
                Visualizer visualizer = Visualizer;
                RenderTargetBitmap bitmap = new RenderTargetBitmap((int)previewPage.PageWidth, (int)previewPage.PageHeight, 96, 96, PixelFormats.Pbgra32);
                
                visualizer.RenderTo(bitmap);

                BitmapEncoder encoder = null;
                string ext = System.IO.Path.GetExtension(dlg.FileName);
                if (ext == "bmp") encoder = new BmpBitmapEncoder();
                else if ((ext == "jpg") || (ext == "jpeg")) encoder = new JpegBitmapEncoder();
                else encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmap));
                using (FileStream stream = new FileStream(dlg.FileName, FileMode.Create, FileAccess.Write))
                {
                    encoder.Save(stream);
                    stream.Flush();
                }
            }
        }
    }
}
