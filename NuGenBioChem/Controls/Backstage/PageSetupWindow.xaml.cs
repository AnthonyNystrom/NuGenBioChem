using System;
using System.Collections.Generic;
using System.Linq;
using System.Printing;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NuGenBioChem.Controls.Backstage
{
    /// <summary>
    /// Interaction logic for PageSetupWindow.xaml
    /// </summary>
    public partial class PageSetupWindow : System.Windows.Window
    {
        #region Fields
        
        // Owner print tab
        private PrintTab printTab;
        // Indicates whether value in spinners and combobox not need to refresh
        private bool isInit = true;
        // Indicates whether page magin is changed
        private bool isMarginChanged;

        #endregion

        #region Constructor
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="printTab">Owner print tab</param>
        public PageSetupWindow(PrintTab printTab)
        {
            InitializeComponent();
            this.printTab = printTab;
            
            // Initialize page margins
            PageMargin margin = printTab.pageMarginsComboBox.SelectedItem as PageMargin;
            marginLeftSpinner.Value = margin.Left;
            marginTopSpinner.Value = margin.Top;
            marginRightSpinner.Value = margin.Right;
            marginBottomSpinner.Value = margin.Bottom;
            // Initialize orientation
            if (printTab.orientationComboBox.SelectedIndex == 0) portraitRadioButton.IsChecked = true;
            else landscapeRadioButton.IsChecked = true;
            // Initialize paper size
            paperSizeComboBox.ItemsSource = printTab.pageSizesComboBox.ItemsSource;
            paperSizeComboBox.SelectedIndex = printTab.pageSizesComboBox.SelectedIndex;
            PageSize size = printTab.pageSizesComboBox.SelectedItem as PageSize;
            paperSizeWidthSpinner.Value = size.Width / PageSize.PixelToCm;
            paperSizeHeightSpinner.Value = size.Height / PageSize.PixelToCm;
            // End of initialization
            isInit = false;
        }

        #endregion

        #region Event handlers                

        // Handles ok button click
        private void OnOkClick(object sender, RoutedEventArgs e)
        {
            // Set page orientation
            if (portraitRadioButton.IsChecked == true) printTab.orientationComboBox.SelectedIndex = 0;
            else printTab.orientationComboBox.SelectedIndex = 1;

            // If margin is changed set page margins
            if(isMarginChanged)
            {
                PageMargin margin = printTab.pageMarginsComboBox.Items[0] as PageMargin;
                margin.Left = marginLeftSpinner.Value;
                margin.Top = marginTopSpinner.Value;
                margin.Right = marginRightSpinner.Value;
                margin.Bottom = marginBottomSpinner.Value;
                printTab.pageMarginsComboBox.SelectedIndex = 0;
            }   
         
            // Set custom paper size if needed
            if(paperSizeComboBox.SelectedIndex == paperSizeComboBox.Items.Count - 1)
            {
                PageSize size = paperSizeComboBox.SelectedItem as PageSize;
                size.PageMediaSize = new PageMediaSize(paperSizeWidthSpinner.Value * PageSize.PixelToCm, paperSizeHeightSpinner.Value * PageSize.PixelToCm);
                size.Width = paperSizeWidthSpinner.Value*PageSize.PixelToCm;
                size.Height = paperSizeHeightSpinner.Value * PageSize.PixelToCm;
            }
            // Set paper size
            printTab.pageSizesComboBox.SelectedIndex = paperSizeComboBox.SelectedIndex;
            // Updates preview
            printTab.UpdatePreview();

            Close();
        }

        // Handles page margin changed
        private void OnMarginChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if(!isInit)isMarginChanged = true;
        }

        // Handles page size changed
        private void OnPageSizeChanged(object sender, EventArgs e)
        {
            if (!isInit)
            {
                isInit = true;
                // Fill page size spinners
                PageSize size = paperSizeComboBox.SelectedItem as PageSize;
                paperSizeWidthSpinner.Value = size.Width/PageSize.PixelToCm;
                paperSizeHeightSpinner.Value = size.Height/PageSize.PixelToCm;
                isInit = false;
            }
        }

        // Handles page size spinners value changed
        private void OnPageSizeSpinnerChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!isInit)
            {
                isInit = true;
                // Set combo box selection to custom size
                paperSizeComboBox.SelectedIndex = paperSizeComboBox.Items.Count - 1;
                isInit = false;
            }
        }

        #endregion
    }
}
