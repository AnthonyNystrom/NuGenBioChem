using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Printing;
using System.Printing.Interop;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Documents.Serialization;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Fluent;
using NuGenBioChem.Visualization;
using ComboBox = System.Windows.Controls.ComboBox;

namespace NuGenBioChem.Controls.Backstage
{
    #region Data

    /// <summary>
    /// Printer type enum
    /// </summary>
    public enum PrinterType
    {
        /// <summary>
        /// Represents local printer
        /// </summary>
        Local = 0,
        /// <summary>
        /// Represents network printer
        /// </summary>
        Network,
        /// Represents fax
        Fax
    }

    /// <summary>
    /// represents printer information
    /// </summary>
    public class PrinterInfo
    {
        /// <summary>
        /// Gets or sets print queue
        /// </summary>
        public PrintQueue Queue { get; set; }
        /// <summary>
        /// Gets or sets a value indication whether printer is default
        /// </summary>
        public bool IsDefault { get; set; }
        /// <summary>
        /// Get or sets printer type
        /// </summary>
        public PrinterType Type { get; set; }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="queue">Print queue</param>
        /// <param name="isDefault">A value indication whether printer is default</param>
        /// <param name="type">Printer type</param>
        public PrinterInfo(PrintQueue queue, bool isDefault, PrinterType type)
        {
            Queue = queue;
            IsDefault = isDefault;
            Type = type;
        }
    }

    /// <summary>
    /// Represents printers page size
    /// </summary>
    public class PageSize : INotifyPropertyChanged
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

        #region Constants

        // String format for size
        private const string SizeString = "{0:0.##} cm x {1:0.##} cm";

        /// <summary>
        /// Converts pixels to cm`s
        /// </summary>
        public const double PixelToCm = 96.0 / 2.54;

        #endregion

        #region Fields

        private double width;
        private double height;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets name of page media szie
        /// </summary>
        public PageMediaSizeName? Name { get; set; }

        /// <summary>
        /// Gets or sets width of page in pixels
        /// </summary>
        public double Width
        {
            get { return width; }
            set
            {
                width = value;
                RaisePropertyChanged("Width");
                RaisePropertyChanged("Size");
            }
        }

        /// <summary>
        /// Gets or sets height of page in pixels
        /// </summary>
        public double Height
        {
            get { return height; }
            set
            {
                height = value;
                RaisePropertyChanged("Height");
                RaisePropertyChanged("Size");
            }
        }

        /// <summary>
        /// Gets or sets formated string with sizes of page
        /// </summary>
        public string Size
        {
            get { return string.Format(SizeString, Width / PixelToCm, Height / PixelToCm); }
        }

        /// <summary>
        /// Gets or sets page size image
        /// </summary>
        public string Image { get; set; }

        /// <summary>
        /// Gets or sets page media size
        /// </summary>
        public PageMediaSize PageMediaSize { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Page media size name</param>
        /// <param name="width">Page width</param>
        /// <param name="height">Page height</param>
        /// <param name="image">Page image</param>
        /// <param name="pageMediaSize">Page media size</param>
        public PageSize(PageMediaSizeName? name, double width, double height, string image, PageMediaSize pageMediaSize)
        {
            Name = name;
            Width = width;
            Height = height;
            Image = image;
            PageMediaSize = pageMediaSize;
        }

        #endregion

        #region Override

        ///<summary>
        ///</summary>
        ///<param name="obj"></param>
        ///<returns></returns>
        public override bool Equals(object obj)
        {
            PageSize pageSize = (obj as PageSize);
            if (pageSize == null) return false;
            return Name == pageSize.Name;
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #endregion
    }

    /// <summary>
    /// Represents printer page margin
    /// </summary>
    public class PageMargin : INotifyPropertyChanged
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

        private double left;
        private double top;
        private double right;
        private double bottom;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets left
        /// </summary>
        public double Left
        {
            get { return left; }
            set
            {
                left = value;
                RaisePropertyChanged("Left");
            }
        }

        /// <summary>
        /// Gets or sets top
        /// </summary>
        public double Top
        {
            get { return top; }
            set
            {
                top = value;
                RaisePropertyChanged("Top");
            }
        }

        /// <summary>
        /// Gets or sets right
        /// </summary>
        public double Right
        {
            get { return right; }
            set
            {
                right = value;
                RaisePropertyChanged("Right");
            }
        }

        /// <summary>
        /// Gets or sets bottom
        /// </summary>
        public double Bottom
        {
            get { return bottom; }
            set
            {
                bottom = value;
                RaisePropertyChanged("Bottom");
            }
        }

        /// <summary>
        /// Gets or sets name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Gets or sets image
        /// </summary>
        public string Image { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="top">Top</param>
        /// <param name="right">Right</param>
        /// <param name="bottom">Bottom</param>
        /// <param name="name">Name</param>
        /// <param name="image">Image</param>
        public PageMargin(double left, double top, double right, double bottom, string name, string image)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
            Name = name;
            Image = image;
        }

        #endregion
    }

    #endregion

    #region Converters

    /// <summary>
    /// Converts print status to string
    /// </summary>
    public class PrintStatusConverter : IValueConverter
    {
        /// <summary>
        /// Converts a value. 
        /// </summary>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        /// <param name="value">The value produced by the binding source.</param><param name="targetType">The type of the binding target property.</param><param name="parameter">The converter parameter to use.</param><param name="culture">The culture to use in the converter.</param>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            PrintQueueStatus status = (PrintQueueStatus)value;
            return SpotTroubleUsingQueueAttributes(status);
        }

        /// <summary>
        /// Converts a value. 
        /// </summary>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        /// <param name="value">The value that is produced by the binding target.</param><param name="targetType">The type to convert to.</param><param name="parameter">The converter parameter to use.</param><param name="culture">The culture to use in the converter.</param>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Converts print queue statuc to string
        /// </summary>
        /// <param name="status">Print queue status</param>
        /// <returns>String represents status</returns>
        internal static string SpotTroubleUsingQueueAttributes(PrintQueueStatus status)
        {
            String statusReport = string.Empty;
            if ((status & PrintQueueStatus.PaperProblem) == PrintQueueStatus.PaperProblem)
            {
                statusReport = statusReport + "Has a paper problem. ";
            }
            if ((status & PrintQueueStatus.NoToner) == PrintQueueStatus.NoToner)
            {
                statusReport = statusReport + "Is out of toner. ";
            }
            if ((status & PrintQueueStatus.DoorOpen) == PrintQueueStatus.DoorOpen)
            {
                statusReport = statusReport + "Has an open door. ";
            }
            if ((status & PrintQueueStatus.Error) == PrintQueueStatus.Error)
            {
                statusReport = statusReport + "Is in an error state. ";
            }
            if ((status & PrintQueueStatus.NotAvailable) == PrintQueueStatus.NotAvailable)
            {
                statusReport = statusReport + "Is not available. ";
            }
            if ((status & PrintQueueStatus.Offline) == PrintQueueStatus.Offline)
            {
                statusReport = statusReport + "Is off line. ";
            }
            if ((status & PrintQueueStatus.OutOfMemory) == PrintQueueStatus.OutOfMemory)
            {
                statusReport = statusReport + "Is out of memory. ";
            }
            if ((status & PrintQueueStatus.PaperOut) == PrintQueueStatus.PaperOut)
            {
                statusReport = statusReport + "Is out of paper. ";
            }
            if ((status & PrintQueueStatus.OutputBinFull) == PrintQueueStatus.OutputBinFull)
            {
                statusReport = statusReport + "Has a full output bin. ";
            }
            if ((status & PrintQueueStatus.PaperJam) == PrintQueueStatus.PaperJam)
            {
                statusReport = statusReport + "Has a paper jam. ";
            }
            if ((status & PrintQueueStatus.Paused) == PrintQueueStatus.Paused)
            {
                statusReport = statusReport + "Is paused. ";
            }
            if ((status & PrintQueueStatus.TonerLow) == PrintQueueStatus.TonerLow)
            {
                statusReport = statusReport + "Is low on toner. ";
            }
            if ((status & PrintQueueStatus.UserIntervention) == PrintQueueStatus.UserIntervention)
            {
                statusReport = statusReport + "Needs user intervention. ";
            }

            if (string.IsNullOrEmpty(statusReport)) statusReport = "Ready";
            return statusReport;
        }
    }

    /// <summary>
    /// Converts page size to string
    /// </summary>
    public class PageSizeNameConverter : IValueConverter
    {
        // Dictionary with names
        readonly static Dictionary<PageMediaSizeName, string> SizesDescription = new Dictionary<PageMediaSizeName, string>()
        {
            {PageMediaSizeName.Unknown , "Unknown paper size"},
            {PageMediaSizeName.ISOA0 , "A0"},
            {PageMediaSizeName.ISOA1 , "A1"},
            {PageMediaSizeName.ISOA10 , "A10"},
            {PageMediaSizeName.ISOA2 , "A2"},
            {PageMediaSizeName.ISOA3 , "A3"},
            {PageMediaSizeName.ISOA3Rotated , "A3 Rotated"},
            {PageMediaSizeName.ISOA3Extra , "A3 Extra"},
            {PageMediaSizeName.ISOA4 , "A4"},
            {PageMediaSizeName.ISOA4Rotated , "A4 Rotated"},
            {PageMediaSizeName.ISOA4Extra , "A4 Extra"},
            {PageMediaSizeName.ISOA5 , "A5"},
            {PageMediaSizeName.ISOA5Rotated , "A5 Rotated"},
            {PageMediaSizeName.ISOA5Extra , "A5 Extra"},
            {PageMediaSizeName.ISOA6 , "A6"},
            {PageMediaSizeName.ISOA6Rotated , "A6 Rotated"},
            {PageMediaSizeName.ISOA7 , "A7"},
            {PageMediaSizeName.ISOA8 , "A8"},
            {PageMediaSizeName.ISOA9 , "A9"},
            {PageMediaSizeName.ISOB0 , "B0"},
            {PageMediaSizeName.ISOB1 , "B1"},
            {PageMediaSizeName.ISOB10 , "B10"},
            {PageMediaSizeName.ISOB2 , "B2"},
            {PageMediaSizeName.ISOB3 , "B3"},
            {PageMediaSizeName.ISOB4 , "B4"},
            {PageMediaSizeName.ISOB4Envelope , "B4 Envelope"},
            {PageMediaSizeName.ISOB5Envelope , "B5 Envelope"},
            {PageMediaSizeName.ISOB5Extra , "B5 Extra"},
            {PageMediaSizeName.ISOB7 , "B7"},
            {PageMediaSizeName.ISOB8 , "B8"},
            {PageMediaSizeName.ISOB9 , "B9"},
            {PageMediaSizeName.ISOC0 , "C0"},
            {PageMediaSizeName.ISOC1 , "C1"},
            {PageMediaSizeName.ISOC10 , "C10"},
            {PageMediaSizeName.ISOC2 , "C2"},
            {PageMediaSizeName.ISOC3 , "C3"},
            {PageMediaSizeName.ISOC3Envelope , "C3 Envelope"},
            {PageMediaSizeName.ISOC4 , "C4"},
            {PageMediaSizeName.ISOC4Envelope , "C4 Envelope"},
            {PageMediaSizeName.ISOC5 , "C5"},
            {PageMediaSizeName.ISOC5Envelope , "C5 Envelope"},
            {PageMediaSizeName.ISOC6 , "C6"},
            {PageMediaSizeName.ISOC6Envelope , "C6 Envelope"},
            {PageMediaSizeName.ISOC6C5Envelope , "C6C5 Envelope"},
            {PageMediaSizeName.ISOC7 , "C7"},
            {PageMediaSizeName.ISOC8 , "C8"},
            {PageMediaSizeName.ISOC9 , "C9"},
            {PageMediaSizeName.ISODLEnvelope , "DL Envelope"},
            {PageMediaSizeName.ISODLEnvelopeRotated , "DL Envelope Rotated"},
            {PageMediaSizeName.ISOSRA3 , "SRA 3"},
            {PageMediaSizeName.JapanQuadrupleHagakiPostcard , "Quadruple Hagaki Postcard"},
            {PageMediaSizeName.JISB0 , "Japanese Industrial Standard B0"},
            {PageMediaSizeName.JISB1 , "Japanese Industrial Standard B1"},
            {PageMediaSizeName.JISB10 , "Japanese Industrial Standard B10"},
            {PageMediaSizeName.JISB2 , "Japanese Industrial Standard B2"},
            {PageMediaSizeName.JISB3 , "Japanese Industrial Standard B3"},
            {PageMediaSizeName.JISB4 , "Japanese Industrial Standard B4"},
            {PageMediaSizeName.JISB4Rotated , "Japanese Industrial Standard B4 Rotated"},
            {PageMediaSizeName.JISB5 , "Japanese Industrial Standard B5"},
            {PageMediaSizeName.JISB5Rotated , "Japanese Industrial Standard B5 Rotated"},
            {PageMediaSizeName.JISB6 , "Japanese Industrial Standard B6"},
            {PageMediaSizeName.JISB6Rotated , "Japanese Industrial Standard B6 Rotated"},
            {PageMediaSizeName.JISB7 , "Japanese Industrial Standard B7"},
            {PageMediaSizeName.JISB8 , "Japanese Industrial Standard B8"},
            {PageMediaSizeName.JISB9 , "Japanese Industrial Standard B9"},
            {PageMediaSizeName.JapanChou3Envelope , "Chou 3 Envelope"},
            {PageMediaSizeName.JapanChou3EnvelopeRotated , "Chou 3 Envelope Rotated"},
            {PageMediaSizeName.JapanChou4Envelope , "Chou 4 Envelope"},
            {PageMediaSizeName.JapanChou4EnvelopeRotated , "Chou 4 Envelope Rotated"},
            {PageMediaSizeName.JapanHagakiPostcard , "Hagaki Postcard"},
            {PageMediaSizeName.JapanHagakiPostcardRotated , "Hagaki Postcard Rotated"},
            {PageMediaSizeName.JapanKaku2Envelope , "Kaku 2 Envelope"},
            {PageMediaSizeName.JapanKaku2EnvelopeRotated , "Kaku 2 Envelope Rotated"},
            {PageMediaSizeName.JapanKaku3Envelope , "Kaku 3 Envelope"},
            {PageMediaSizeName.JapanKaku3EnvelopeRotated , "Kaku 3 Envelope Rotated"},
            {PageMediaSizeName.JapanYou4Envelope , "You 4 Envelope"},
            {PageMediaSizeName.NorthAmerica10x11 , "10 x 11"},
            {PageMediaSizeName.NorthAmerica10x14 , "10 x 14"},
            {PageMediaSizeName.NorthAmerica11x17 , "11 x 17"},
            {PageMediaSizeName.NorthAmerica9x11 , "9 x 11"},
            {PageMediaSizeName.NorthAmericaArchitectureASheet , "Architecture A Sheet"},
            {PageMediaSizeName.NorthAmericaArchitectureBSheet , "Architecture B Sheet"},
            {PageMediaSizeName.NorthAmericaArchitectureCSheet , "Architecture C Sheet"},
            {PageMediaSizeName.NorthAmericaArchitectureDSheet , "Architecture D Sheet"},
            {PageMediaSizeName.NorthAmericaArchitectureESheet , "Architecture E Sheet"},
            {PageMediaSizeName.NorthAmericaCSheet , "C Sheet"},
            {PageMediaSizeName.NorthAmericaDSheet , "D Sheet"},
            {PageMediaSizeName.NorthAmericaESheet , "E Sheet"},
            {PageMediaSizeName.NorthAmericaExecutive , "Executive"},
            {PageMediaSizeName.NorthAmericaGermanLegalFanfold , "German Legal Fanfold"},
            {PageMediaSizeName.NorthAmericaGermanStandardFanfold , "German Standard Fanfold"},
            {PageMediaSizeName.NorthAmericaLegal , "Legal"},
            {PageMediaSizeName.NorthAmericaLegalExtra , "Legal Extra"},
            {PageMediaSizeName.NorthAmericaLetter , "Letter"},
            {PageMediaSizeName.NorthAmericaLetterRotated , "Letter Rotated"},
            {PageMediaSizeName.NorthAmericaLetterExtra , "Letter Extra"},
            {PageMediaSizeName.NorthAmericaLetterPlus , "Letter Plus"},
            {PageMediaSizeName.NorthAmericaMonarchEnvelope , "Monarch Envelope"},
            {PageMediaSizeName.NorthAmericaNote , "Note"},
            {PageMediaSizeName.NorthAmericaNumber10Envelope , "#10 Envelope"},
            {PageMediaSizeName.NorthAmericaNumber10EnvelopeRotated , "#10 Envelope Rotated"},
            {PageMediaSizeName.NorthAmericaNumber9Envelope , "#9 Envelope"},
            {PageMediaSizeName.NorthAmericaNumber11Envelope , "#11 Envelope"},
            {PageMediaSizeName.NorthAmericaNumber12Envelope , "#12 Envelope"},
            {PageMediaSizeName.NorthAmericaNumber14Envelope , "#14 Envelope"},
            {PageMediaSizeName.NorthAmericaPersonalEnvelope , "Personal Envelope"},
            {PageMediaSizeName.NorthAmericaQuarto , "Quarto"},
            {PageMediaSizeName.NorthAmericaStatement , "Statement"},
            {PageMediaSizeName.NorthAmericaSuperA , "Super A"},
            {PageMediaSizeName.NorthAmericaSuperB , "Super B"},
            {PageMediaSizeName.NorthAmericaTabloid , "Tabloid"},
            {PageMediaSizeName.NorthAmericaTabloidExtra , "Tabloid Extra"},
            {PageMediaSizeName.OtherMetricA4Plus , "A4 Plus"},
            {PageMediaSizeName.OtherMetricA3Plus , "A3 Plus"},
            {PageMediaSizeName.OtherMetricFolio , "Folio"},
            {PageMediaSizeName.OtherMetricInviteEnvelope , "Invite Envelope"},
            {PageMediaSizeName.OtherMetricItalianEnvelope , "Italian Envelope"},
            {PageMediaSizeName.PRC1Envelope , "People's Republic of China #1 Envelope"},
            {PageMediaSizeName.PRC1EnvelopeRotated , "People's Republic of China #1 Envelope Rotated"},
            {PageMediaSizeName.PRC10Envelope , "People's Republic of China #10 Envelope"},
            {PageMediaSizeName.PRC10EnvelopeRotated , "People's Republic of China #10 Envelope Rotated"},
            {PageMediaSizeName.PRC16K , "People's Republic of China 16K"},
            {PageMediaSizeName.PRC16KRotated , "People's Republic of China 16K Rotated"},
            {PageMediaSizeName.PRC2Envelope , "People's Republic of China #2 Envelope"},
            {PageMediaSizeName.PRC2EnvelopeRotated , "People's Republic of China #2 Envelope Rotated"},
            {PageMediaSizeName.PRC32K , "People's Republic of China 32K"},
            {PageMediaSizeName.PRC32KRotated , "People's Republic of China 32K Rotated"},
            {PageMediaSizeName.PRC32KBig , "People's Republic of China 32K Big"},
            {PageMediaSizeName.PRC3Envelope , "People's Republic of China #3 Envelope"},
            {PageMediaSizeName.PRC3EnvelopeRotated ,"People's Republic of China #3 Envelope Rotated"},
            {PageMediaSizeName.PRC4Envelope , "People's Republic of China #4 Envelope"},
            {PageMediaSizeName.PRC4EnvelopeRotated ,"People's Republic of China #4 Envelope Rotated"},
            {PageMediaSizeName.PRC5Envelope , "People's Republic of China #5 Envelope"},
            {PageMediaSizeName.PRC5EnvelopeRotated , "People's Republic of China #5 Envelope Rotated"},
            {PageMediaSizeName.PRC6Envelope , "People's Republic of China #6 Envelope"},
            {PageMediaSizeName.PRC6EnvelopeRotated ,"People's Republic of China #6 Envelope Rotated"},
            {PageMediaSizeName.PRC7Envelope , "People's Republic of China #7 Envelope"},
            {PageMediaSizeName.PRC7EnvelopeRotated ,"People's Republic of China #7 Envelope Rotated"}, 
            {PageMediaSizeName.PRC8Envelope , "People's Republic of China #8 Envelope"},
            {PageMediaSizeName.PRC8EnvelopeRotated ,"People's Republic of China #8 Envelope Rotated"},
            {PageMediaSizeName.PRC9Envelope , "People's Republic of China #9 Envelope"},
            {PageMediaSizeName.PRC9EnvelopeRotated , "People's Republic of China #9 Envelope Rotated"},
            {PageMediaSizeName.Roll04Inch , "4-inch wide roll"},
            {PageMediaSizeName.Roll06Inch , "6-inch wide roll"},
            {PageMediaSizeName.Roll08Inch , "8-inch wide roll"},
            {PageMediaSizeName.Roll12Inch , "12-inch wide roll"},
            {PageMediaSizeName.Roll15Inch , "15-inch wide roll"},
            {PageMediaSizeName.Roll18Inch , "18-inch wide roll"},
            {PageMediaSizeName.Roll22Inch , "22-inch wide roll"},
            {PageMediaSizeName.Roll24Inch , "24-inch wide roll"},
            {PageMediaSizeName.Roll30Inch , "30-inch wide roll"},
            {PageMediaSizeName.Roll36Inch , "36-inch wide roll"},
            {PageMediaSizeName.Roll54Inch , "54-inch wide roll"},
            {PageMediaSizeName.JapanDoubleHagakiPostcard , "Double Hagaki Postcard"},
            {PageMediaSizeName.JapanDoubleHagakiPostcardRotated , "Double Hagaki Postcard Rotated"},
            {PageMediaSizeName.JapanLPhoto , "L Photo"},
            {PageMediaSizeName.Japan2LPhoto , "2L Photo"},
            {PageMediaSizeName.JapanYou1Envelope , "You 1 Envelope"},
            {PageMediaSizeName.JapanYou2Envelope , "You 2 Envelope"},
            {PageMediaSizeName.JapanYou3Envelope , "You 3 Envelope"},
            {PageMediaSizeName.JapanYou4EnvelopeRotated , "You 4 Envelope Rotated"},
            {PageMediaSizeName.JapanYou6Envelope , "You 6 Envelope"},
            {PageMediaSizeName.JapanYou6EnvelopeRotated , "You 6 Envelope Rotated"},
            {PageMediaSizeName.NorthAmerica4x6 , "4 x 6"},
            {PageMediaSizeName.NorthAmerica4x8 , "4 x 8"},
            {PageMediaSizeName.NorthAmerica5x7 , "5 x 7"},
            {PageMediaSizeName.NorthAmerica8x10 , "8 x 10"},
            {PageMediaSizeName.NorthAmerica10x12 , "10 x 12"},
            {PageMediaSizeName.NorthAmerica14x17 , "14 x 17"},
            {PageMediaSizeName.BusinessCard , "Business card"},
            {PageMediaSizeName.CreditCard , "Credit card"}
        };

        /// <summary>
        /// Converts a value. 
        /// </summary>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        /// <param name="value">The value produced by the binding source.</param><param name="targetType">The type of the binding target property.</param><param name="parameter">The converter parameter to use.</param><param name="culture">The culture to use in the converter.</param>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            PageMediaSizeName? sizeName = (PageMediaSizeName?)value;
            if (sizeName.HasValue && SizesDescription.ContainsKey(sizeName.Value))
                return SizesDescription[sizeName.Value];
            return "Custom Page Size";
        }

        /// <summary>
        /// Converts a value. 
        /// </summary>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        /// <param name="value">The value that is produced by the binding target.</param><param name="targetType">The type to convert to.</param><param name="parameter">The converter parameter to use.</param><param name="culture">The culture to use in the converter.</param>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Data templates selector for margin combo box
    /// </summary>
    public class PageMarginDataTemplateSelector : DataTemplateSelector
    {
        // Owner control with resources
        private UserControl owner;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="owner">Owner control with resources</param>
        public PageMarginDataTemplateSelector(UserControl owner)
        {
            this.owner = owner;
        }

        /// <summary>
        /// When overridden in a derived class, returns a <see cref="T:System.Windows.DataTemplate"/> based on custom logic.
        /// </summary>
        /// <returns>
        /// Returns a <see cref="T:System.Windows.DataTemplate"/> or null. The default value is null.
        /// </returns>
        /// <param name="item">The data object for which to select the template.</param><param name="container">The data-bound object.</param>
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            DependencyObject parent = VisualTreeHelper.GetParent(container);

            while (parent != null)
            {
                if (parent is ComboBox) return owner.Resources["PageMarginsSelectedDataTemplate"] as DataTemplate;
                parent = VisualTreeHelper.GetParent(parent);
            }

            return owner.Resources["PageMarginsDataTemplate"] as DataTemplate;
        }
    }

    #endregion

    /// <summary>
    /// Interaction logic for PrintTab.xaml
    /// </summary>
    public partial class PrintTab : UserControl
    {
        #region Properties

        /// <summary>
        /// Gets or sets current printing percentage
        /// </summary>
        public string PrintingPercentage
        {
            get { return (string)GetValue(PrintingPercentageProperty); }
            set { SetValue(PrintingPercentageProperty, value); }
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for PrintingPercentage.  This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty PrintingPercentageProperty =
            DependencyProperty.Register("PrintingPercentage", typeof(string), typeof(PrintTab), new UIPropertyMetadata(null));

        /// <summary>
        /// gets or sets current printing status bar item control
        /// </summary>
        public object PrintingContent
        {
            get { return (object)GetValue(PrintingContentProperty); }
            set { SetValue(PrintingContentProperty, value); }
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for PrintingContent.  This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty PrintingContentProperty =
            DependencyProperty.Register("PrintingContent", typeof(object), typeof(PrintTab), new UIPropertyMetadata(null));

        /// <summary>
        /// Gets visualizer
        /// </summary>
        public Visualizer Visualizer
        {
            get { return ((Visualizer)printPreviewPage.PageContent); }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructors
        /// </summary>
        public PrintTab()
        {
            InitializeComponent();

            // Finding printers
            LocalPrintServer server = new LocalPrintServer();
            PrintQueue defaultQueue = server.DefaultPrintQueue;
            List<PrinterInfo> printers = new List<PrinterInfo>();
            // Finding faxes
            printers.AddRange(server.GetPrintQueues(new EnumeratedPrintQueueTypes[] { EnumeratedPrintQueueTypes.Fax }).Select(x => new PrinterInfo(x, x.FullName == defaultQueue.FullName, PrinterType.Fax)));
            // Finding local printers
            printers.AddRange(server.GetPrintQueues(new EnumeratedPrintQueueTypes[] { EnumeratedPrintQueueTypes.Local }).Where(x => printers.Count(y => y.Queue.FullName == x.FullName) == 0).Select(x => new PrinterInfo(x, x.FullName == defaultQueue.FullName, PrinterType.Local)));
            // Finding network printers
            printers.AddRange(server.GetPrintQueues(new EnumeratedPrintQueueTypes[] { EnumeratedPrintQueueTypes.Connections }).Where(x => printers.Count(y => y.Queue.FullName == x.FullName) == 0).Select(x => new PrinterInfo(x, x.FullName == defaultQueue.FullName, PrinterType.Network)));
            // Fill printers combo box
            printersComboBox.ItemsSource = printers.OrderBy(x => x.Queue.Name);
            // Select default printer
            printersComboBox.SelectedItem = printers.First(x => x.Queue.FullName == defaultQueue.FullName);

            // Fill margins combobox
            List<PageMargin> margins = new List<PageMargin>();
            margins.Add(new PageMargin(3, 2, 1.5, 2, "Last Custom Setting Margin", "pack://application:,,,/;component/Images/ExclusiveMargins.png"));
            margins.Add(new PageMargin(3, 2, 1.5, 2, "Normal", "pack://application:,,,/;component/Images/NormalMargins.png"));
            margins.Add(new PageMargin(1.27, 1.27, 1.27, 1.27, "Narrow", "pack://application:,,,/;component/Images/NarrowMargins.png"));
            margins.Add(new PageMargin(1.91, 2.54, 1.91, 2.54, "Moderate", "pack://application:,,,/;component/Images/ModerateMargins.png"));
            margins.Add(new PageMargin(5.08, 2.54, 5.08, 2.54, "Wide", "pack://application:,,,/;component/Images/WideMargins.png"));
            // margins.Add(new PageMargin(3.18, 2.54, 2.54, 2.54, "Mirrored", "pack://application:,,,/;component/Images/MirroredMargins.png"));            
            pageMarginsComboBox.ItemsSource = margins;
            // Select normal margin
            pageMarginsComboBox.SelectedIndex = 1;
            // Set data template selector
            pageMarginsComboBox.ItemTemplateSelector = new PageMarginDataTemplateSelector(this);
        }

        #endregion

        #region Event Handling

        // handles printer changing
        private void OnPrinterChanged(object sender, EventArgs e)
        {
            if (printersComboBox.SelectedItem == null) return;
            // Find selected printer page sizes
            PrintQueue queue = (printersComboBox.SelectedItem as PrinterInfo).Queue;
            var data = queue.GetPrintCapabilities().PageMediaSizeCapability.Select(
                x =>
                new PageSize(x.PageMediaSizeName, x.Width.Value, x.Height.Value, GetPageImage(x.PageMediaSizeName), x)).ToList();
            // Add custom size
            data.Add(new PageSize(null, 10, 10, null, new PageMediaSize(10, 10)));
            pageSizesComboBox.ItemsSource = data;
            // Find A4 size and select it
            PageSize selectedSize = data.FirstOrDefault(x => x.Name.Value == PageMediaSizeName.ISOA4);
            if (selectedSize == null) pageSizesComboBox.SelectedIndex = 0;
            else pageSizesComboBox.SelectedIndex = Array.IndexOf(data.ToArray(), selectedSize);
        }

        // Handles page size changed
        private void OnPageSizeChanged(object sender, EventArgs e)
        {
            if ((pageSizesComboBox == null) || (pageSizesComboBox.SelectedItem == null)) return;
            PageSize size = pageSizesComboBox.SelectedItem as PageSize;
            if (orientationComboBox.SelectedIndex == 0)
            {
                printPreviewPage.PageWidth = size.Width;
                printPreviewPage.PageHeight = size.Height;
            }
            else if (orientationComboBox.SelectedIndex == 1)
            {
                printPreviewPage.PageWidth = size.Height;
                printPreviewPage.PageHeight = size.Width;
            }
        }

        // Handles printer properties click
        private void OnPrinterPropertiesClick(object sender, RoutedEventArgs e)
        {
            PrintQueue selectedPrintQueue = (printersComboBox.SelectedItem as PrinterInfo).Queue;
            selectedPrintQueue.UserPrintTicket.PageOrientation = orientationComboBox.SelectedIndex == 0 ?
              PageOrientation.Portrait : PageOrientation.Landscape;
            PrintTicketConverter ptc = new PrintTicketConverter(selectedPrintQueue.FullName, selectedPrintQueue.ClientPrintSchemaVersion);
            IntPtr mainWindowPtr = new WindowInteropHelper(Window.GetWindow(this)).Handle;

            byte[] myDevMode = ptc.ConvertPrintTicketToDevMode(selectedPrintQueue.UserPrintTicket, BaseDevModeType.UserDefault);
            GCHandle pinnedDevMode = GCHandle.Alloc(myDevMode, GCHandleType.Pinned);
            IntPtr pDevMode = pinnedDevMode.AddrOfPinnedObject();
            DocumentProperties(mainWindowPtr, IntPtr.Zero, selectedPrintQueue.FullName, pDevMode, pDevMode, 14);
            selectedPrintQueue.UserPrintTicket = ptc.ConvertDevModeToPrintTicket(myDevMode);
            pinnedDevMode.Free();
        }

        // Handles print click
        private void OnPrintClick(object sender, RoutedEventArgs e)
        {
            PrintQueue queue = (printersComboBox.SelectedItem as PrinterInfo).Queue;

            // Set printer parameters
            queue.UserPrintTicket.PageMediaSize = (pageSizesComboBox.SelectedItem as PageSize).PageMediaSize;
            queue.UserPrintTicket.PageOrientation = orientationComboBox.SelectedIndex == 0 ?
              PageOrientation.Portrait : PageOrientation.Landscape;
            queue.UserPrintTicket.CopyCount = (int)copyCountSpinner.Value;

            // Renderes visualizer to bitmpa
            RenderTargetBitmap bitmap = new RenderTargetBitmap((int)(printPreviewPage.PageWidth - printPreviewPage.PageBorder.Left - printPreviewPage.PageBorder.Right), (int)(printPreviewPage.PageHeight - printPreviewPage.PageBorder.Top - printPreviewPage.PageBorder.Bottom), 96, 96, PixelFormats.Pbgra32);
            Visualizer.RenderTo(bitmap);

            // Create canvas for page and put image to it with correct margins
            Canvas canvas = new Canvas();
            canvas.Width = printPreviewPage.PageWidth;
            canvas.Height = printPreviewPage.PageHeight;
            canvas.Measure(new Size(printPreviewPage.PageWidth, printPreviewPage.PageHeight));
            canvas.Arrange(new Rect(0, 0, printPreviewPage.PageWidth, printPreviewPage.PageHeight));
            Image img = new Image();
            img.Source = bitmap;
            img.Width = printPreviewPage.PageWidth - printPreviewPage.PageBorder.Left - printPreviewPage.PageBorder.Right;
            img.Height = printPreviewPage.PageHeight - printPreviewPage.PageBorder.Top - printPreviewPage.PageBorder.Bottom;
            canvas.Children.Add(img);
            Canvas.SetLeft(img, printPreviewPage.PageBorder.Left);
            Canvas.SetTop(img, printPreviewPage.PageBorder.Top);

            // Init printing
            var writer = PrintQueue.CreateXpsDocumentWriter(queue);
            writer.WritingProgressChanged += OnWritingProgressChanged;
            writer.WritingCompleted += OnWritingComplete;
            PrintingContent = new PrintingStatusBarContent(writer);
            PrintingPercentage = "0 %";

            // Print page
            writer.WriteAsync(canvas);
        }

        // Handles writing complete
        private void OnWritingComplete(object sender, WritingCompletedEventArgs e)
        {
            // removes indication
            PrintingContent = null;
            PrintingPercentage = null;
            if (e.Error != null) MessageBox.Show(e.Error.Message);
        }

        // Handles printing
        private void OnWritingProgressChanged(object sender, WritingProgressChangedEventArgs e)
        {
            // Set printing percentage
            PrintingPercentage = e.ProgressPercentage + " %";
        }

        // Handles margin changed
        private void OnPageMarginchanged(object sender, SelectionChangedEventArgs e)
        {
            if (printPreviewPage == null) return;
            PageMargin margin = pageMarginsComboBox.SelectedItem as PageMargin;
            double scale = 96 * 1.0 / 2.54;
            printPreviewPage.PageBorder = new Thickness(margin.Left * scale, margin.Top * scale, margin.Right * scale, margin.Bottom * scale);
        }

        // Handles screen tip opened
        private void OnPrinterScreenTipOpened(object sender, RoutedEventArgs e)
        {
            // Get printer information
            ScreenTip screenTip = sender as ScreenTip;
            PrinterInfo printer = printersComboBox.SelectedItem as PrinterInfo;
            printer.Queue.Refresh();
            screenTip.Text = "Status: " + (string)(new PrintStatusConverter()).Convert(printer.Queue.QueueStatus, typeof(string), null, CultureInfo.InvariantCulture) + Environment.NewLine;
            screenTip.Text += "Type: " + printer.Queue.Name + Environment.NewLine;
            screenTip.Text += "Where: " + printer.Queue.QueuePort.Name + Environment.NewLine;
            screenTip.Text += "Comment: " + printer.Queue.Comment;
        }

        // Handles page setup click
        private void OnPageSetupClick(object sender, RoutedEventArgs e)
        {
            PageSetupWindow wnd = new PageSetupWindow(this);
            wnd.Owner = Window.GetWindow(this);
            wnd.ShowDialog();
        }

        #endregion

        #region Methods

        // Get image for page size
        private string GetPageImage(PageMediaSizeName? name)
        {
            if (name.HasValue)
            {
                if (name.Value == PageMediaSizeName.ISOA3) return "pack://application:,,,/;component/Images/A3.png";
                if (name.Value == PageMediaSizeName.ISOA4) return "pack://application:,,,/;component/Images/A4.png";
                if (name.Value == PageMediaSizeName.ISOA5) return "pack://application:,,,/;component/Images/A5.png";
            }
            return null;
        }

        /// <summary>
        /// Updates page preview
        /// </summary>
        internal void UpdatePreview()
        {
            if ((pageSizesComboBox == null) || (pageSizesComboBox.SelectedItem == null)) return;
            PageSize size = pageSizesComboBox.SelectedItem as PageSize;
            if (orientationComboBox.SelectedIndex == 0)
            {
                printPreviewPage.PageWidth = size.Width;
                printPreviewPage.PageHeight = size.Height;
            }
            else if (orientationComboBox.SelectedIndex == 1)
            {
                printPreviewPage.PageWidth = size.Height;
                printPreviewPage.PageHeight = size.Width;
            }

            if (printPreviewPage == null) return;
            PageMargin margin = pageMarginsComboBox.SelectedItem as PageMargin;
            double scale = 96 * 1.0 / 2.54;
            printPreviewPage.PageBorder = new Thickness(margin.Left * scale, margin.Top * scale, margin.Right * scale, margin.Bottom * scale);
        }

        #endregion

        #region Interop

        [DllImport("winspool.Drv", EntryPoint = "DocumentPropertiesW", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        static extern int DocumentProperties(IntPtr hwnd, IntPtr hPrinter, [MarshalAs(UnmanagedType.LPWStr)] string pDeviceName, IntPtr pDevModeOutput, IntPtr pDevModeInput, int fMode);

        #endregion
    }
}
