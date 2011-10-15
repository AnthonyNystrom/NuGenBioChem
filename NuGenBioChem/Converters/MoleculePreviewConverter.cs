using System;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using NuGenBioChem.Data;
using NuGenBioChem.Visualization;
using Molecule = NuGenBioChem.Data.Molecule;

namespace NuGenBioChem.Converters
{
    /// <summary>
    /// Converter. Generates preview for molecule
    /// </summary>
    public class MoleculePreviewConverter : IValueConverter
    {
        static Visualizer visualizer = new Visualizer();
        static Substance substance = new Substance();

        static MoleculePreviewConverter()
        {
            
        }

        /// <summary>
        /// Converts a value. 
        /// </summary>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        /// <param name="value">The value produced by the binding source.</param><param name="targetType">The type of the binding target property.</param><param name="parameter">The converter parameter to use.</param><param name="culture">The culture to use in the converter.</param>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap(48, 48, 96, 96, PixelFormats.Pbgra32);
            Data.Molecule molecule = value as Data.Molecule;
            if (molecule == null) return renderTargetBitmap;

            Dispatcher.CurrentDispatcher.BeginInvoke((Action) delegate
                                                                  {
                                                                      Style style = new Style();
                                                                      if (molecule.Chains.Count > 0)
                                                                          style.LoadCartoonDeafult();
                                                                      else style.LoadBallAndStickDeafult();

                                                                      substance.Molecules.Clear();
                                                                      substance.Molecules.Add(molecule);
                                                                      visualizer.SubstanceStyle = style;
                                                                      visualizer.Substance = substance;
                                                                      visualizer.CreatePreview(renderTargetBitmap);
                                                                  },DispatcherPriority.ApplicationIdle);

            return renderTargetBitmap;
        }

        /// <summary>
        /// Converts a value. 
        /// </summary>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        /// <param name="value">The value that is produced by the binding target.</param><param name="targetType">The type to convert to.</param><param name="parameter">The converter parameter to use.</param><param name="culture">The culture to use in the converter.</param>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
