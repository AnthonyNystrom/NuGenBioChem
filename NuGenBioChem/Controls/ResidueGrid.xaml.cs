using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using NuGenBioChem.Data;
using Style = NuGenBioChem.Data.Style;

namespace NuGenBioChem.Controls
{
    public class ResidueColorConverter : IMultiValueConverter
    {        
        #region Implementation of IMultiValueConverter

        /// <summary>
        /// Converts source values to a value for the binding target. The data binding engine calls this method when it propagates the values from source bindings to the binding target.
        /// </summary>
        /// <returns>
        /// A converted value.If the method returns null, the valid null value is used.A return value of <see cref="T:System.Windows.DependencyProperty"/>.<see cref="F:System.Windows.DependencyProperty.UnsetValue"/> indicates that the converter did not produce a value, and that the binding will use the <see cref="P:System.Windows.Data.BindingBase.FallbackValue"/> if it is available, or else will use the default value.A return value of <see cref="T:System.Windows.Data.Binding"/>.<see cref="F:System.Windows.Data.Binding.DoNothing"/> indicates that the binding does not transfer the value or use the <see cref="P:System.Windows.Data.BindingBase.FallbackValue"/> or the default value.
        /// </returns>
        /// <param name="values">The array of values that the source bindings in the <see cref="T:System.Windows.Data.MultiBinding"/> produces. The value <see cref="F:System.Windows.DependencyProperty.UnsetValue"/> indicates that the source binding has no value to provide for conversion.</param><param name="targetType">The type of the binding target property.</param><param name="parameter">The converter parameter to use.</param><param name="culture">The culture to use in the converter.</param>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            Residue residue = values[0] as Residue;
            Style substanceStyle = values[1] as Style;
            if (residue == null) return null;
            SecondaryStructureType type = residue.GetStructureType();
            if (substanceStyle == null) return null;
            if (type == SecondaryStructureType.Helix) return new SolidColorBrush(substanceStyle.ColorStyle.HelixMaterial.Diffuse);
            if (type == SecondaryStructureType.Sheet) return new SolidColorBrush(substanceStyle.ColorStyle.SheetMaterial.Diffuse);
            if (type == SecondaryStructureType.NotDefined) return new SolidColorBrush(substanceStyle.ColorStyle.TurnMaterial.Diffuse);
            return null;
        }

        /// <summary>
        /// Converts a binding target value to the source binding values.
        /// </summary>
        /// <returns>
        /// An array of values that have been converted from the target value back to the source values.
        /// </returns>
        /// <param name="value">The value that the binding target produces.</param><param name="targetTypes">The array of types to convert to. The array length indicates the number and types of values that are suggested for the method to return.</param><param name="parameter">The converter parameter to use.</param><param name="culture">The culture to use in the converter.</param>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
    /// <summary>
    /// Interaction logic for ResidueGrid.xaml
    /// </summary>
    public partial class ResidueGrid : UserControl
    {
        #region Fields

        private Point selectionStartPosition;

        private List<ListBoxItem> previousSelectedItems = new List<ListBoxItem>();

        private bool isSelectionChanged;

        #endregion

        #region Propertires

        /// <summary>
        /// Gets or sets molecules collection
        /// </summary>
        public Substance Substance
        {
            get { return (Substance)GetValue(SubstanceProperty); }
            set { SetValue(SubstanceProperty, value); }
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for Molecules.  This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty SubstanceProperty =
            DependencyProperty.Register("Substance", typeof(Substance), typeof(ResidueGrid), new UIPropertyMetadata(null, OnSubstanceChanged));

        private static void OnSubstanceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ResidueGrid grid = d as ResidueGrid;
            if (e.OldValue != null) (e.OldValue as Substance).Molecules.CollectionChanged -= grid.OnMoleculesCollectionChanged;
            Substance substance = e.NewValue as Substance;
            if (substance != null)
            {
                substance.Molecules.CollectionChanged += grid.OnMoleculesCollectionChanged;
                substance.SelectedResidues.CollectionChanged += grid.OnSelectionChanged;
            }
            grid.RefreshContent();
        }

        private void OnSelectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if(!isSelectionChanged)
            {
                switch(e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        foreach (var newItem in e.NewItems)
                        {
                            residueListBox.SelectedItems.Add(newItem);
                        }
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        foreach (var oldItem in e.OldItems)
                        {
                            residueListBox.SelectedItems.Remove(oldItem);
                        }
                        break;
                }
            }
        }

        private void OnMoleculesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)(() => RefreshContent()));
        }

        /// <summary>
        /// Gets or sets substance style
        /// </summary>
        public Style SubstanceStyle
        {
            get { return (Style)GetValue(SubstanceStyleProperty); }
            set { SetValue(SubstanceStyleProperty, value); }
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for SubstanceStyle.  This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty SubstanceStyleProperty =
            DependencyProperty.Register("SubstanceStyle", typeof(Style), typeof(ResidueGrid), new UIPropertyMetadata(null));
        
        #endregion

        #region Constructors

        public ResidueGrid()
        {
            InitializeComponent();
            RefreshContent();
        }

        #endregion

        #region Methods

        private void RefreshContent()
        {
            chainsStackPanel.Children.Clear();
            residueListBox.ItemsSource = null;
            numbersListBox.Items.Clear();
            if ((Substance == null) || (Substance.Molecules == null))
            {
                Visibility = Visibility.Collapsed;
                return;
            }
            Visibility = Visibility.Visible;
            List<Residue> residuesList = new List<Residue>();
            int maxCount = 0;
            int chainsCount = 0;
            foreach (var molecule in Substance.Molecules)
            {                
                foreach (var chain in molecule.Chains)                
                {
                    maxCount = Math.Max(maxCount, chain.Residues.Count);
                    chainsCount++;
                    ToggleButton btn = new ToggleButton()
                                           {
                                               Content = chain.Name,
                                               Tag = chain,
                                           };
                    btn.Checked += OnChainChecked;
                    btn.Unchecked += OnChainUnchecked;
                    chainsStackPanel.Children.Add(btn);
                    foreach (var residue in chain.Residues)
                    {
                        residuesList.Add(residue);
                    }
                }
            }
            residueListBox.ItemsSource = residuesList;
            (residueGrid.Background as DrawingBrush).Viewport = new Rect(0, 0, 1.0 / (double)maxCount, 1.0 / (double)chainsCount);
            for (int i = 0; i < maxCount;i++ )
            {
                numbersListBox.Items.Add(i+1);
            }
            CollectionViewSource.GetDefaultView(residueListBox.ItemsSource).GroupDescriptions.Add(new PropertyGroupDescription("Chain"));
            if (chainsStackPanel.Children.Count == 0)
            {
                Visibility = Visibility.Collapsed;
                return;
            }
        }

        private void OnChainUnchecked(object sender, RoutedEventArgs e)
        {
            Chain chain = (sender as ToggleButton).Tag as Chain;
            foreach (var residue in chain.Residues)
            {
                if (residueListBox.SelectedItems.Contains(residue)) residueListBox.SelectedItems.Remove(residue);
            }
        }

        private void OnChainChecked(object sender, RoutedEventArgs e)
        {
            Chain chain = (sender as ToggleButton).Tag as Chain;
            foreach (var residue in chain.Residues)
            {
                if (!residueListBox.SelectedItems.Contains(residue)) residueListBox.SelectedItems.Add(residue);
            }
        }

        #endregion

        #region Event Handling

        private void OnPreviewMouseLeftDown(object sender, MouseButtonEventArgs e)
        {
            // Move focus to the list box
            residueListBox.Focus();

            selectionStartPosition = Mouse.GetPosition(residueListBox);
            // Save selected items
            previousSelectedItems.Clear();
            foreach (var element in residueListBox.SelectedItems)
            {
                ListBoxItem item = residueListBox.ItemContainerGenerator.ContainerFromItem(element) as ListBoxItem;
                previousSelectedItems.Add(item);
            }
            // Capture mouse
            Mouse.Capture(residueListBox);
        }

        private void OnPreviewMouseLeftUp(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.Captured == residueListBox) Mouse.Capture(null);
        }
         
        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (Mouse.Captured != residueListBox) return;
            Rect mouseSelectionRect = new Rect(selectionStartPosition, e.GetPosition(residueListBox));
            bool isControlPressed = (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;
            // Check items selection
            foreach (var element in residueListBox.Items)
            {
                ListBoxItem item = residueListBox.ItemContainerGenerator.ContainerFromItem(element) as ListBoxItem;
                Rect itemRect = new Rect(item.TranslatePoint(new Point(0, 0), residueListBox), item.TranslatePoint(new Point(item.ActualWidth, item.ActualHeight), residueListBox));
                if (itemRect.IntersectsWith(mouseSelectionRect))
                {
                    if (isControlPressed && previousSelectedItems.Contains(item)) item.IsSelected = false;
                    else item.IsSelected = true;
                }
                else if ((!isControlPressed) || (!previousSelectedItems.Contains(item))) item.IsSelected = false;
            }
        }

        private void OnResiduesSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Substance == null) return;
            isSelectionChanged = true;

            foreach (Residue addedItem in e.AddedItems)
            {
                Substance.SelectedResidues.Add(addedItem);
            }

            foreach (Residue removedItem in e.RemovedItems)
            {
                Substance.SelectedResidues.Remove(removedItem);
            }

            isSelectionChanged = false;
        }

        private void OnResidueScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            numbersScrollViewer.ScrollToHorizontalOffset(e.HorizontalOffset);
            chainsScrollViewer.ScrollToVerticalOffset(e.VerticalOffset);
        }

        private void OnResidueScrollSizeChanged(object sender, SizeChangedEventArgs e)
        {
            ScrollBar verticalScrollBar = residueScrollViewer.Template.FindName("PART_VerticalScrollBar", residueScrollViewer) as ScrollBar;
            if((verticalScrollBar!=null)&&(verticalScrollBar.Visibility == Visibility.Visible)) verticalScrollPlaceholder.Visibility = Visibility.Visible;
            else verticalScrollPlaceholder.Visibility = Visibility.Collapsed;

            ScrollBar horizontalScrollBar = residueScrollViewer.Template.FindName("PART_HorizontalScrollBar", residueScrollViewer) as ScrollBar;
            if ((horizontalScrollBar != null) && (horizontalScrollBar.Visibility == Visibility.Visible)) horizontalScrollPlaceholder.Visibility = Visibility.Visible;
            else horizontalScrollPlaceholder.Visibility = Visibility.Collapsed;
        }

        #endregion        
    }
}
