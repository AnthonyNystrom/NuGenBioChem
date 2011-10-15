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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using NuGenBioChem.Data;

namespace NuGenBioChem
{
    /// <summary>
    /// Represents preview for color scheme
    /// </summary>
    public partial class ColorSchemePreview : UserControl
    {
        #region Properties

        /// <summary>
        /// Gets or sets name of the color scheme
        /// </summary>
        public string ColorSchemeName
        {
            get { return (string)GetValue(ColorSchemeNameProperty); }
            set { SetValue(ColorSchemeNameProperty, value); }
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for ColorSchemeName.  
        /// This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty ColorSchemeNameProperty =
            DependencyProperty.Register("ColorSchemeName", typeof(string), typeof(ColorSchemePreview), 
            new UIPropertyMetadata(null, OnColorSchemeChanged));

        static void OnColorSchemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ColorSchemePreview colorSchemePreview = (ColorSchemePreview) d;
            if (colorSchemePreview.IsVisible) colorSchemePreview.LoadColorScheme((string)e.NewValue);
        }

        #endregion

        /// <summary>
        /// Default construcotr
        /// </summary>
        public ColorSchemePreview()
        {
            InitializeComponent();
            IsVisibleChanged += OnIsVisibleChanged;

            // Load stub
            LoadColorScheme(null);
        }

        void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue && ColorSchemeName != null && panel.Children.Count == 0)
            {
                Dispatcher.BeginInvoke((Action) (() => LoadColorScheme(ColorSchemeName)),
                                       DispatcherPriority.SystemIdle); 
            }
        }

        #region Methods

        void LoadColorScheme(string name)
        {
            ColorScheme colorScheme = name == null ? null : new ColorScheme(name);
            panel.Children.Clear();

            AddElement("H", colorScheme);
            AddElement("C", colorScheme);
            AddElement("N", colorScheme);
            AddElement("O", colorScheme);
            AddElement("S", colorScheme);
            AddElement("P", colorScheme);
        }

        void AddElement(string elementName, ColorScheme colorScheme)
        {
            Color elementColor = colorScheme == null ? Colors.White : colorScheme[elementName].Diffuse;
            Brush brush = new SolidColorBrush(elementColor);
            Brush foreground = (((double)elementColor.R + (double)elementColor.B + (double)elementColor.G) / 3.0 < 128) ? Brushes.White : Brushes.Black;

            Border border = new Border
            {
                Margin = new Thickness(1,1,0,1),
                Width = 18,
                Height = 18,
                BorderBrush = Brushes.Silver,
                BorderThickness = new Thickness(1),
                Background = brush,
            };

            TextBlock textBlock = new TextBlock
            {
                Text = elementName,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Foreground = foreground,
            };

            border.Child = textBlock;
            panel.Children.Add(border);
        }

        

        #endregion
    }
}
