using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using NuGenBioChem.Data;
using NuGenBioChem.Data.Importers;
using NuGenBioChem.Visualization;

namespace NuGenBioChem.Converters
{
    /// <summary>
    /// Converter. Generates preview for style
    /// </summary>
    public class StylePreviewConverter : IValueConverter
    {
        static Visualizer visualizer = new Visualizer();
        static Substance mol = new Substance();
        static Substance pdb = new Substance();
        static Dictionary<string, ImageSource> cache = new Dictionary<string,ImageSource>();

        static StylePreviewConverter()
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
            string styleName = value as string;
            if (styleName != null && cache.ContainsKey(styleName)) return cache[styleName];

            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap(70, 50, 96, 96, PixelFormats.Pbgra32);
            

            if (styleName == null) return renderTargetBitmap;
            Style style = new Style(styleName);

            if (mol.Molecules.Count == 0)
            {
                // Load mol
                string data = (new StreamReader(Application.GetResourceStream(new Uri("pack://application:,,,/NuGenBioChem;component/Resources/Molecules/Preview.mol")).Stream)).ReadToEnd();
                mol.Molecules.Add(Molefile.Parse(data));
                // Load pdb
                data = (new StreamReader(Application.GetResourceStream(new Uri("pack://application:,,,/NuGenBioChem;component/Resources/Molecules/Preview.pdb")).Stream)).ReadToEnd();
                ProteinDataBankFile pdbImporter = new ProteinDataBankFile(data.Split(new string[]{"\r\n"}, StringSplitOptions.None));
                pdb.Molecules.Add(pdbImporter.Molecules[0]);              
            }
            
            visualizer.Substance = IsProteinVisible(style) ? pdb : mol;
            visualizer.SubstanceStyle = style;
            visualizer.CreatePreview(renderTargetBitmap);

            cache.Add(styleName, renderTargetBitmap);
            return renderTargetBitmap;
        }

        static bool IsProteinVisible(Style style)
        {
            if (style.GeometryStyle.HelixWidth * style.GeometryStyle.HelixHeight <= 0.01 &&
                style.GeometryStyle.SheetWidth * style.GeometryStyle.SheetHeight <= 0.01 &&
                style.GeometryStyle.TurnWidth * style.GeometryStyle.TurnHeight <= 0.01) return false;

            return true;
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
