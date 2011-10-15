using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using NuGenBioChem.Data;

namespace NuGenBioChem.Controls.Backstage
{
    #region Converters
    
 /// <summary>
    /// Represents file name converter
    /// </summary>
    public class FileNameConverter : IValueConverter
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
            string path = value.ToString();
            return System.IO.Path.GetFileName(path);
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
    /// Represents file icon converter
    /// </summary>
    public class FileIconConverter : IValueConverter
    {
        #region Interop

        [StructLayout(LayoutKind.Sequential)]
        internal struct SHFILEINFO
        {
            public IntPtr hIcon;
            public IntPtr iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        };

        const uint SHGFI_ICON = 0x100;
        const uint SHGFI_SYSICONINDEX = 16384;
        const uint SHGFI_USEFILEATTRIBUTES = 16;

        /// <summary>
        /// Get Icons that are associated with files.
        /// To use it, use (System.Drawing.Icon myIcon = System.Drawing.Icon.FromHandle(shinfo.hIcon));
        /// hImgSmall = SHGetFileInfo(fName, 0, ref shinfo,(uint)Marshal.SizeOf(shinfo),Win32.SHGFI_ICON |Win32.SHGFI_SMALLICON);
        /// </summary>
        [DllImport("shell32.dll")]
        static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes,
                                                  ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);

        #endregion

        #region Caching
        
        // Icons cache
        private static Dictionary<string, BitmapSource> iconCache = new Dictionary<string, BitmapSource>();

        #endregion

        #region Methods

        /// <summary>
        /// Return large file icon of the specified file.
        /// </summary>
        internal static BitmapSource GetFileIcon(string fileName)
        {
            string extension = System.IO.Path.GetExtension(fileName).ToLower();
            if (iconCache.ContainsKey(extension)) return iconCache[extension];
            
            SHFILEINFO shinfo = new SHFILEINFO();

            uint flags = SHGFI_SYSICONINDEX;
            if (fileName.IndexOf(":") == -1)
                flags = flags | SHGFI_USEFILEATTRIBUTES;
            flags = flags | SHGFI_ICON;

            SHGetFileInfo(fileName, 0, ref shinfo, (uint)Marshal.SizeOf(shinfo), flags);
            BitmapSource result = null;
            if (shinfo.hIcon == IntPtr.Zero)
                result = new BitmapImage(new Uri("pack://application:,,,/;component/Images/RecentFile.png"));
            else result = Imaging.CreateBitmapSourceFromHIcon(shinfo.hIcon, new Int32Rect(0, 0, 32, 32), BitmapSizeOptions.FromEmptyOptions());
            
            iconCache.Add(extension, result);

            return result;
        }

        #endregion

        /// <summary>
        /// Converts a value. 
        /// </summary>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        /// <param name="value">The value produced by the binding source.</param><param name="targetType">The type of the binding target property.</param><param name="parameter">The converter parameter to use.</param><param name="culture">The culture to use in the converter.</param>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string path = value.ToString();
            return GetFileIcon(path);
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

    #endregion

    /// <summary>
    /// Interaction logic for RecentTab.xaml
    /// </summary>
    public partial class RecentTab : UserControl
    {
        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public RecentTab()
        {
            InitializeComponent();
            // Set group selectors
            recentFilesListBox.GroupStyleSelector = SelectFilesGroupStyle;
            recentDirectoriesListBox.GroupStyleSelector = SelectDirectoriesGroupStyle;
        }

        #endregion

        #region Croup Selectors

        // group selector for recent files
        private GroupStyle SelectFilesGroupStyle(CollectionViewGroup group, int level)
        {
            return SelectGroupStyle(recentFilesListBox.ItemsSource);
        }

        // group selector for recent dirs
        private GroupStyle SelectDirectoriesGroupStyle(CollectionViewGroup group, int level)
        {
            return SelectGroupStyle(recentDirectoriesListBox.ItemsSource);
        }

        // Group selector for items  source
        private GroupStyle SelectGroupStyle(IEnumerable itemsSource)
        {
            if (itemsSource == null) return (GroupStyle)FindResource("recentItemsEmptyGroup");
            ICollectionView view = CollectionViewSource.GetDefaultView(itemsSource);
            if (view.Groups.Count < 2) return (GroupStyle)FindResource("recentItemsEmptyGroup");
            if (((view.Groups[0] as CollectionViewGroup).ItemCount == 0) || ((view.Groups[1] as CollectionViewGroup).ItemCount == 0)) return (GroupStyle)FindResource("recentItemsEmptyGroup");

            return (GroupStyle)FindResource("recentItemsGroup");
        }

        #endregion

        #region Event Handling

        // Handles recent file click
        private void OnRecentFileClick(object sender, RoutedEventArgs e)
        {
            Window wnd = Window.GetWindow(this) as Window;
            wnd.OpenFile((sender as Button).Tag.ToString());
            wnd.ribbon.IsBackstageOpen = false;
        }

        // handles recent dir click
        private void OnRecentDirClick(object sender, RoutedEventArgs e)
        {
            Window wnd = Window.GetWindow(this) as Window;
            if (wnd.OpenDir((sender as Button).Tag.ToString())) wnd.ribbon.IsBackstageOpen = false;
        }

        #endregion        
    }
}
