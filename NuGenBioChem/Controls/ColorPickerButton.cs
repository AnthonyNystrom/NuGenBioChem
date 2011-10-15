using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Fluent;

namespace NuGenBioChem.Controls
{
    /// <summary>
    /// Brush info for color picker button
    /// </summary>
    internal class BrushInfo
    {
        public SolidColorBrush Color{ get; set;}
        public string Group{ get; set;}
        public bool HasTopPadding { get; set; }
        public bool HasBottomPadding { get; set; }
    }

    /// <summary>
    /// Represents colors picker button
    /// </summary>
    public class ColorPickerButton:RibbonControl
    {
        #region Static fields
        
        // All colors in galleries
        private static ObservableCollection<BrushInfo> data;

        #endregion

        #region Fields

        // In-style gallery
        private Gallery gallery;

        private MenuItem noColorMenuItem;

        #endregion

        #region Properties

        /// <summary>
        /// Gets ors sets selected brush
        /// </summary>
        public SolidColorBrush SelectedBrush
        {
            get { return (SolidColorBrush)GetValue(SelectedBrushProperty); }
            set { SetValue(SelectedBrushProperty, value); }
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for SelectedBrush.  This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty SelectedBrushProperty =
            DependencyProperty.Register("SelectedBrush", typeof(SolidColorBrush), typeof(ColorPickerButton), new UIPropertyMetadata(Brushes.Black, OnSelectedBrushChanged));

        // handles selection brush changes
        private static void OnSelectedBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ColorPickerButton button = (d as ColorPickerButton);
            if ((e.NewValue as SolidColorBrush).Color== Colors.Transparent)
            {
                button.SelectedBrushInfo = null;
                if (button.noColorMenuItem != null) button.noColorMenuItem.IsChecked = true;
            }
            else
            {
                if (data.Count(x => x.Color.Color == ((SolidColorBrush) e.NewValue).Color) > 0)
                    button.SelectedBrushInfo =
                        (BrushInfo) data.First(x => x.Color.Color == ((SolidColorBrush) e.NewValue).Color);
                else
                {
                    BrushInfo info = new BrushInfo()
                                         {
                                             Color = (SolidColorBrush) e.NewValue,
                                             Group = "Recent Colors",
                                             HasBottomPadding = true,
                                             HasTopPadding = true
                                         };
                    data.Add(info);
                    button.SelectedBrushInfo = info;
                }
                if (button.noColorMenuItem!=null) button.noColorMenuItem.IsChecked = false;
            }
            if (button.SelectionChanged != null) button.SelectionChanged(button, EventArgs.Empty);
        }

        /// <summary>
        /// Gets or sets selected brush info
        /// </summary>
        internal BrushInfo SelectedBrushInfo
        {
            get { return (BrushInfo)GetValue(SelectedBrushInfoProperty); }
            set { SetValue(SelectedBrushInfoProperty, value); }
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for SelectedBrushInfo.  This enables animation, styling, binding, etc...
        /// </summary>
        internal static readonly DependencyProperty SelectedBrushInfoProperty =
            DependencyProperty.Register("SelectedBrushInfo", typeof(BrushInfo), typeof(ColorPickerButton), new UIPropertyMetadata(null, OnSelectedBrushInfoChanged));

        // Handles selected brush info changes
        private static void OnSelectedBrushInfoChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null) return;
            ColorPickerButton button = (d as ColorPickerButton);
            if (button.SelectedBrush.Color != (e.NewValue as BrushInfo).Color.Color) button.SelectedBrush = (e.NewValue as BrushInfo).Color;
        }

        /// <summary>
        /// Gets or sets button large icon
        /// </summary>
        public ImageSource LargeIcon
        {
            get { return (ImageSource)GetValue(LargeIconProperty); }
            set { SetValue(LargeIconProperty, value); }
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for LargeIcon.  This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty LargeIconProperty =
            DependencyProperty.Register("LargeIcon", typeof(ImageSource), typeof(ColorPickerButton), new UIPropertyMetadata(null));

        /// <summary>
        /// Gets or sets a value indicating whether nocolor is shown on menu
        /// </summary>
        public bool ShowNoColor
        {
            get { return (bool)GetValue(ShowNoColorProperty); }
            set { SetValue(ShowNoColorProperty, value); }
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for ShowNoColor.  This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty ShowNoColorProperty =
            DependencyProperty.Register("ShowNoColor", typeof(bool), typeof(ColorPickerButton), new UIPropertyMetadata(true));
        
        #endregion

        #region Events

        /// <summary>
        /// Occurs on selection changed
        /// </summary>
        public event EventHandler SelectionChanged;

        #endregion

        #region Commands

        /// <summary>
        /// More colors command
        /// </summary>
        public static RoutedCommand MoreColorsCommand = new RoutedCommand();

        /// <summary>
        /// Set nocolors command
        /// </summary>
        public static RoutedCommand SetNoColorCommand = new RoutedCommand();
        
        #endregion

        #region Constructor

        /// <summary>
        /// Static constructor
        /// </summary>
        static ColorPickerButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ColorPickerButton), new FrameworkPropertyMetadata(typeof(ColorPickerButton)));
            CommandManager.RegisterClassCommandBinding(typeof(ColorPickerButton), new CommandBinding(ColorPickerButton.MoreColorsCommand, OnMoreColorsCommandExecuted));
            CommandManager.RegisterClassCommandBinding(typeof(ColorPickerButton), new CommandBinding(ColorPickerButton.SetNoColorCommand, OnSetNoColorCommandExecuted));

            data = new ObservableCollection<BrushInfo>(new BrushInfo[]
                                     {
                                        new BrushInfo(){Group="Standard Colors", Color=new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C00000")), HasTopPadding = true, HasBottomPadding = true},
                                        new BrushInfo(){Group="Standard Colors", Color=new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF0000")), HasTopPadding = true, HasBottomPadding = true},
                                        new BrushInfo(){Group="Standard Colors", Color=new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC000")), HasTopPadding = true, HasBottomPadding = true},
                                        new BrushInfo(){Group="Standard Colors", Color=new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFF00")), HasTopPadding = true, HasBottomPadding = true},
                                        new BrushInfo(){Group="Standard Colors", Color=new SolidColorBrush((Color)ColorConverter.ConvertFromString("#92D050")), HasTopPadding = true, HasBottomPadding = true},
                                        new BrushInfo(){Group="Standard Colors", Color=new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00B050")), HasTopPadding = true, HasBottomPadding = true},
                                        new BrushInfo(){Group="Standard Colors", Color=new SolidColorBrush((Color)ColorConverter.ConvertFromString("#26A8E1")), HasTopPadding = true, HasBottomPadding = true},
                                        new BrushInfo(){Group="Standard Colors", Color=new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1C75BD")), HasTopPadding = true, HasBottomPadding = true},
                                        new BrushInfo(){Group="Standard Colors", Color=new SolidColorBrush((Color)ColorConverter.ConvertFromString("#263B74")), HasTopPadding = true, HasBottomPadding = true},
                                        new BrushInfo(){Group="Standard Colors", Color=new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7A47A4")), HasTopPadding = true, HasBottomPadding = true},

                                        new BrushInfo(){Group="Advanced Colors", Color=new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF")), HasTopPadding = true, HasBottomPadding = true},
                                        new BrushInfo(){Group="Advanced Colors", Color=new SolidColorBrush((Color)ColorConverter.ConvertFromString("#000000")), HasTopPadding = true, HasBottomPadding = true},
                                        new BrushInfo(){Group="Advanced Colors", Color=new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EEECE1")), HasTopPadding = true, HasBottomPadding = true},
                                        new BrushInfo(){Group="Advanced Colors", Color=new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1F497D")), HasTopPadding = true, HasBottomPadding = true},
                                        new BrushInfo(){Group="Advanced Colors", Color=new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4F81BD")), HasTopPadding = true, HasBottomPadding = true},
                                        new BrushInfo(){Group="Advanced Colors", Color=new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C0504D")), HasTopPadding = true, HasBottomPadding = true},
                                        new BrushInfo(){Group="Advanced Colors", Color=new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9BBB59")), HasTopPadding = true, HasBottomPadding = true},
                                        new BrushInfo(){Group="Advanced Colors", Color=new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8064A2")), HasTopPadding = true, HasBottomPadding = true},
                                        new BrushInfo(){Group="Advanced Colors", Color=new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4BACC6")), HasTopPadding = true, HasBottomPadding = true},
                                        new BrushInfo(){Group="Advanced Colors", Color=new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F79646")), HasTopPadding = true, HasBottomPadding = true},

                                        new BrushInfo(){Group="Advanced Colors", Color=new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F2F2F2")), HasTopPadding = true},
                                        new BrushInfo(){Group="Advanced Colors", Color=new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7F7F7F")), HasTopPadding = true},
                                        new BrushInfo(){Group="Advanced Colors", Color=new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DDD9C3")), HasTopPadding = true},
                                        new BrushInfo(){Group="Advanced Colors", Color=new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C6D9F0")), HasTopPadding = true},
                                        new BrushInfo(){Group="Advanced Colors", Color=new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DBE5F1")), HasTopPadding = true},
                                        new BrushInfo(){Group="Advanced Colors", Color=new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F2DCDB")), HasTopPadding = true},
                                        new BrushInfo(){Group="Advanced Colors", Color=new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EBF1DD")), HasTopPadding = true},
                                        new BrushInfo(){Group="Advanced Colors", Color=new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E5E0EC")), HasTopPadding = true},
                                        new BrushInfo(){Group="Advanced Colors", Color=new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DBEEF3")), HasTopPadding = true},
                                        new BrushInfo(){Group="Advanced Colors", Color=new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FDEADA")), HasTopPadding = true},

                                        new BrushInfo(){Group="Advanced Colors", Color=new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D8D8D8")), HasTopPadding = false},
                                        new BrushInfo(){Group="Advanced Colors", Color=new SolidColorBrush((Color)ColorConverter.ConvertFromString("#595959")), HasTopPadding = false},
                                        new BrushInfo(){Group="Advanced Colors", Color=new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C4BD97")), HasTopPadding = false},
                                        new BrushInfo(){Group="Advanced Colors", Color=new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8DB3E2")), HasTopPadding = false},
                                        new BrushInfo(){Group="Advanced Colors", Color=new SolidColorBrush((Color)ColorConverter.ConvertFromString("#B8CCE4")), HasTopPadding = false},
                                        new BrushInfo(){Group="Advanced Colors", Color=new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E5B9B7")), HasTopPadding = false},
                                        new BrushInfo(){Group="Advanced Colors", Color=new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D7E3BC")), HasTopPadding = false},
                                        new BrushInfo(){Group="Advanced Colors", Color=new SolidColorBrush((Color)ColorConverter.ConvertFromString("#CCC1D9")), HasTopPadding = false},
                                        new BrushInfo(){Group="Advanced Colors", Color=new SolidColorBrush((Color)ColorConverter.ConvertFromString("#B7DDE8")), HasTopPadding = false},
                                        new BrushInfo(){Group="Advanced Colors", Color=new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FBD5B5")), HasTopPadding = false},

                                        new BrushInfo(){Group="Advanced Colors", Color=new SolidColorBrush((Color)ColorConverter.ConvertFromString("#BFBFBF")), HasTopPadding = false},
                                        new BrushInfo(){Group="Advanced Colors", Color=new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3F3F3F")), HasTopPadding = false},
                                        new BrushInfo(){Group="Advanced Colors", Color=new SolidColorBrush((Color)ColorConverter.ConvertFromString("#938953")), HasTopPadding = false},
                                        new BrushInfo(){Group="Advanced Colors", Color=new SolidColorBrush((Color)ColorConverter.ConvertFromString("#548DD4")), HasTopPadding = false},
                                        new BrushInfo(){Group="Advanced Colors", Color=new SolidColorBrush((Color)ColorConverter.ConvertFromString("#95B3D7")), HasTopPadding = false},
                                        new BrushInfo(){Group="Advanced Colors", Color=new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D99694")), HasTopPadding = false},
                                        new BrushInfo(){Group="Advanced Colors", Color=new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C3D69B")), HasTopPadding = false},
                                        new BrushInfo(){Group="Advanced Colors", Color=new SolidColorBrush((Color)ColorConverter.ConvertFromString("#B2A2C7")), HasTopPadding = false},
                                        new BrushInfo(){Group="Advanced Colors", Color=new SolidColorBrush((Color)ColorConverter.ConvertFromString("#92CDDC")), HasTopPadding = false},
                                        new BrushInfo(){Group="Advanced Colors", Color=new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FAC08F")), HasTopPadding = false},

                                        new BrushInfo(){Group="Advanced Colors", Color=new SolidColorBrush((Color)ColorConverter.ConvertFromString("#A5A5A5")), HasTopPadding = false},
                                        new BrushInfo(){Group="Advanced Colors", Color=new SolidColorBrush((Color)ColorConverter.ConvertFromString("#262626")), HasTopPadding = false},
                                        new BrushInfo(){Group="Advanced Colors", Color=new SolidColorBrush((Color)ColorConverter.ConvertFromString("#494429")), HasTopPadding = false},
                                        new BrushInfo(){Group="Advanced Colors", Color=new SolidColorBrush((Color)ColorConverter.ConvertFromString("#17365D")), HasTopPadding = false},
                                        new BrushInfo(){Group="Advanced Colors", Color=new SolidColorBrush((Color)ColorConverter.ConvertFromString("#366092")), HasTopPadding = false},
                                        new BrushInfo(){Group="Advanced Colors", Color=new SolidColorBrush((Color)ColorConverter.ConvertFromString("#953734")), HasTopPadding = false},
                                        new BrushInfo(){Group="Advanced Colors", Color=new SolidColorBrush((Color)ColorConverter.ConvertFromString("#76923C")), HasTopPadding = false},
                                        new BrushInfo(){Group="Advanced Colors", Color=new SolidColorBrush((Color)ColorConverter.ConvertFromString("#5F497A")), HasTopPadding = false},
                                        new BrushInfo(){Group="Advanced Colors", Color=new SolidColorBrush((Color)ColorConverter.ConvertFromString("#31859B")), HasTopPadding = false},
                                        new BrushInfo(){Group="Advanced Colors", Color=new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E36C09")), HasTopPadding = false},

                                        new BrushInfo(){Group="Advanced Colors", Color=new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7F7F7F")), HasTopPadding = false, HasBottomPadding = true},
                                        new BrushInfo(){Group="Advanced Colors", Color=new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0C0C0C")), HasTopPadding = false, HasBottomPadding = true},
                                        new BrushInfo(){Group="Advanced Colors", Color=new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1D1B10")), HasTopPadding = false, HasBottomPadding = true},
                                        new BrushInfo(){Group="Advanced Colors", Color=new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0F243E")), HasTopPadding = false, HasBottomPadding = true},
                                        new BrushInfo(){Group="Advanced Colors", Color=new SolidColorBrush((Color)ColorConverter.ConvertFromString("#244061")), HasTopPadding = false, HasBottomPadding = true},
                                        new BrushInfo(){Group="Advanced Colors", Color=new SolidColorBrush((Color)ColorConverter.ConvertFromString("#632423")), HasTopPadding = false, HasBottomPadding = true},
                                        new BrushInfo(){Group="Advanced Colors", Color=new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4F6128")), HasTopPadding = false, HasBottomPadding = true},
                                        new BrushInfo(){Group="Advanced Colors", Color=new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3F3151")), HasTopPadding = false, HasBottomPadding = true},
                                        new BrushInfo(){Group="Advanced Colors", Color=new SolidColorBrush((Color)ColorConverter.ConvertFromString("#205867")), HasTopPadding = false, HasBottomPadding = true},
                                        new BrushInfo(){Group="Advanced Colors", Color=new SolidColorBrush((Color)ColorConverter.ConvertFromString("#974806")), HasTopPadding = false, HasBottomPadding = true},                                        
                                     });
        }

        private static void OnSetNoColorCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            (sender as ColorPickerButton).SelectedBrush = Brushes.Transparent;
        }

        private static void OnMoreColorsCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            (sender as ColorPickerButton).SelectedBrush = new SolidColorBrush(Color.FromArgb(100, 250, 200, 155));
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public ColorPickerButton()
        {
            
        }

        #endregion

        #region Overrides

        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or internal processes call <see cref="M:System.Windows.FrameworkElement.ApplyTemplate"/>.
        /// </summary>
        public override void OnApplyTemplate()
        {
            gallery = GetTemplateChild("PART_Gallery") as Gallery;
            if (gallery != null)gallery.ItemsSource = data;
            noColorMenuItem = GetTemplateChild("PART_NoColorMenuItem") as MenuItem;
            if (noColorMenuItem != null) noColorMenuItem.IsChecked = SelectedBrush.Color == Colors.Transparent;
        }

        /// <summary>
        /// Gets control which represents shortcut item.
        ///             This item MUST be synchronized with the original 
        ///             and send command to original one control.
        /// </summary>
        /// <returns>
        /// Control which represents shortcut item
        /// </returns>
        public override FrameworkElement CreateQuickAccessItem()
        {
            ColorPickerButton btn = new ColorPickerButton();
            Bind(this,btn,"SelectedBrush",ColorPickerButton.SelectedBrushProperty,BindingMode.TwoWay);
            return btn;
        }

        #endregion
    }
}
