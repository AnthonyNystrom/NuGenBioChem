using System;
using System.Collections.Generic;
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

namespace NuGenBioChem
{
    /// <summary>
    /// Interaction logic for PagePreview.xaml
    /// </summary>
    public partial class PagePreview : UserControl
    {
        #region Properties

        /// <summary>
        /// Gets or sets page width
        /// </summary>
        public double PageWidth
        {
            get { return (double)GetValue(PageWidthProperty); }
            set { SetValue(PageWidthProperty, value); }
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for PageWidth.  This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty PageWidthProperty =
            DependencyProperty.Register("PageWidth", typeof(double), typeof(PagePreview), new UIPropertyMetadata(250.0, OnPageSizeChanged));

        // Handles page size changed
        private static void OnPageSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as PagePreview).UpdatePageSize();
        }

        /// <summary>
        /// Gets or sets page height
        /// </summary>
        public double PageHeight
        {
            get { return (double)GetValue(PageHeightProperty); }
            set { SetValue(PageHeightProperty, value); }
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for PageHeight.  This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty PageHeightProperty =
            DependencyProperty.Register("PageHeight", typeof(double), typeof(PagePreview), new UIPropertyMetadata(352.5, OnPageSizeChanged));

        /// <summary>
        /// Gets or sets page content
        /// </summary>
        public UIElement PageContent
        {
            get { return (UIElement)GetValue(PageContentProperty); }
            set { SetValue(PageContentProperty, value); }
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for PageContent.  This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty PageContentProperty =
            DependencyProperty.Register("PageContent", typeof(UIElement), typeof(PagePreview), new UIPropertyMetadata(null, OnPageContentChanged));

        private static void OnPageContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as PagePreview).pageBorder.Child = (UIElement)e.NewValue;
        }

        /// <summary>
        /// Gets or sets page border
        /// </summary>
        public Thickness PageBorder
        {
            get { return (Thickness)GetValue(PageBorderProperty); }
            set { SetValue(PageBorderProperty, value); }
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for PageBorder.  This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty PageBorderProperty =
            DependencyProperty.Register("PageBorder", typeof(Thickness), typeof(PagePreview), new UIPropertyMetadata(new Thickness(), OnPageSizeChanged));

        #endregion

        /// <summary>
        /// Default constructor
        /// </summary>
        public PagePreview()
        {
            this.InitializeComponent();
        }

        // Handles size changed
        private void OnSizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            UpdatePageSize();
        }

        // Update page size
        private void UpdatePageSize()
        {
            double deltaX = LayoutRoot.ActualWidth / PageWidth;
            double deltaY = LayoutRoot.ActualHeight / PageHeight;
            double scale = Math.Min(deltaX, deltaY);
            pageBorder.Width = Math.Max(0, PageWidth * scale - 12);
            pageBorder.Height = Math.Max(0, PageHeight * scale - 12);

            pageBorder.Padding = new Thickness(PageBorder.Left * scale, PageBorder.Top * scale, PageBorder.Right * scale, PageBorder.Bottom * scale);
        }
    }
}