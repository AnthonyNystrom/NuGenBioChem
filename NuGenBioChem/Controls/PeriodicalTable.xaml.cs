using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using NuGenBioChem.Data;
using NuGenBioChem.Data.Transactions;
using Style = NuGenBioChem.Data.Style;

namespace NuGenBioChem.Controls
{	
    /// <summary>
    /// Converts font color from back color
    /// </summary>
    public class FontColorConverter : IValueConverter
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
            SolidColorBrush brush = value as SolidColorBrush;
            if (((double)brush.Color.R + (double)brush.Color.B + (double)brush.Color.G) / 3.0 < 128) return Brushes.White;
            else return Brushes.Black;
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
    /// Interaction logic for PeriodicalTable.xaml
    /// </summary>
    public partial class PeriodicalTable : UserControl
    {
        #region Fields

        private ColorScheme colorScheme;
        private Point selectionStartPosition;

        private List<ListBoxItem> previousSelectedItems = new List<ListBoxItem>();

        private bool isSelectionChanged;
        private Material selectedMaterial;
        #endregion

        #region Events

        /// <summary>
        /// Occurs then material is changed
        /// </summary>
        public event EventHandler MaterialChanged;

        private void RaiseMaterialChanged()
        {
            if (MaterialChanged != null) MaterialChanged(this, EventArgs.Empty);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets color scheme
        /// </summary>
        public ColorScheme ColorScheme
        {
            get { return colorScheme; }
            set
            {
                colorScheme = value;
                elementsListBox.ItemsSource = Element.Elements.Select(x => new Tuple<Element, Material>(x, ColorScheme[x]));
            }
        }

        #endregion

        #region Material properties

        #region Ambient

        /// <summary>
        /// Gets or sets ambient brush for selected elements
        /// </summary>
        public SolidColorBrush Ambient
        {
            get { return (SolidColorBrush)GetValue(AmbientProperty); }
            set { SetValue(AmbientProperty, value); }
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for Ambient.  This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty AmbientProperty =
            DependencyProperty.Register("Ambient", typeof(SolidColorBrush), typeof(PeriodicalTable), new UIPropertyMetadata(Brushes.Black, OnAmbientChanged));

        private static void OnAmbientChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as PeriodicalTable).SetAmbient();
        }

        #endregion

        #region Diffuse

        /// <summary>
        /// Gets or sets diffuse brush for selected elements
        /// </summary>
        public SolidColorBrush Diffuse
        {
            get { return (SolidColorBrush)GetValue(DiffuseProperty); }
            set { SetValue(DiffuseProperty, value); }
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for Diffuse.  This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty DiffuseProperty =
            DependencyProperty.Register("Diffuse", typeof(SolidColorBrush), typeof(PeriodicalTable), new UIPropertyMetadata(Brushes.Black, OnDiffuseChanged));

        private static void OnDiffuseChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as PeriodicalTable).SetDiffuse();
        }

        #endregion

        #region Specular

        /// <summary>
        /// Gets or sets specular brush for selected elements
        /// </summary>
        public SolidColorBrush Specular
        {
            get { return (SolidColorBrush)GetValue(SpecularProperty); }
            set { SetValue(SpecularProperty, value); }
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for Specular.  This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty SpecularProperty =
            DependencyProperty.Register("Specular", typeof(SolidColorBrush), typeof(PeriodicalTable), new UIPropertyMetadata(Brushes.Black, OnSpecularChanged));

        private static void OnSpecularChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as PeriodicalTable).SetSpecular();
        }

        #endregion

        #region Emissive

        /// <summary>
        /// Gets or sets emissive brush for selected elements
        /// </summary>
        public SolidColorBrush Emissive
        {
            get { return (SolidColorBrush)GetValue(EmissiveProperty); }
            set { SetValue(EmissiveProperty, value); }
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for Emissive.  This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty EmissiveProperty =
            DependencyProperty.Register("Emissive", typeof(SolidColorBrush), typeof(PeriodicalTable), new UIPropertyMetadata(Brushes.Black, OnEmissiveChanged));

        private static void OnEmissiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as PeriodicalTable).SetEmissive();
        }

        #endregion

        #region Glossiness
        
        /// <summary>
        /// Gets or sets glossiness for selected element
        /// </summary>
        public double Glossiness
        {
            get { return (double)GetValue(GlossinessProperty); }
            set { SetValue(GlossinessProperty, value); }
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for Glossiness.  This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty GlossinessProperty =
            DependencyProperty.Register("Glossiness", typeof(double), typeof(PeriodicalTable), new UIPropertyMetadata(0.0, OnGlossinessChanged));

        private static void OnGlossinessChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as PeriodicalTable).SetGlossiness();
        }

        #endregion

        #region SpecularPower

        /// <summary>
        /// Gets or sets SpecularPower for selected element
        /// </summary>
        public double SpecularPower
        {
            get { return (double)GetValue(SpecularPowerProperty); }
            set { SetValue(SpecularPowerProperty, value); }
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for SpecularPower.  This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty SpecularPowerProperty =
            DependencyProperty.Register("SpecularPower", typeof(double), typeof(PeriodicalTable), new UIPropertyMetadata(0.0, OnSpecularPowerChanged));

        private static void OnSpecularPowerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as PeriodicalTable).SetSpecularPower();
        }

        #endregion

        #region ReflectionLevel

        /// <summary>
        /// Gets or sets ReflectionLevel for selected element
        /// </summary>
        public double ReflectionLevel
        {
            get { return (double)GetValue(ReflectionLevelProperty); }
            set { SetValue(ReflectionLevelProperty, value); }
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for ReflectionLevel.  This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty ReflectionLevelProperty =
            DependencyProperty.Register("ReflectionLevel", typeof(double), typeof(PeriodicalTable), new UIPropertyMetadata(0.0, OnReflectionLevelChanged));

        private static void OnReflectionLevelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as PeriodicalTable).SetReflectionLevel();
        }

        #endregion

        #region BumpLevel

        /// <summary>
        /// Gets or sets BumpLevel for selected element
        /// </summary>
        public double BumpLevel
        {
            get { return (double)GetValue(BumpLevelProperty); }
            set { SetValue(BumpLevelProperty, value); }
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for BumpLevel.  This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty BumpLevelProperty =
            DependencyProperty.Register("BumpLevel", typeof(double), typeof(PeriodicalTable), new UIPropertyMetadata(0.0, OnBumpLevelChanged));

        private static void OnBumpLevelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as PeriodicalTable).SetBumpLevel();
        }

        #endregion

        #region EmissiveLevel

        /// <summary>
        /// Gets or sets EmissiveLevel for selected element
        /// </summary>
        public double EmissiveLevel
        {
            get { return (double)GetValue(EmissiveLevelProperty); }
            set { SetValue(EmissiveLevelProperty, value); }
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for EmissiveLevel.  This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty EmissiveLevelProperty =
            DependencyProperty.Register("EmissiveLevel", typeof(double), typeof(PeriodicalTable), new UIPropertyMetadata(0.0, OnEmissiveLevelChanged));

        private static void OnEmissiveLevelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as PeriodicalTable).SetEmissiveLevel();
        }

        #endregion

        #endregion

        #region Initializing

        /// <summary>
        /// Default constructor
        /// </summary>
        public PeriodicalTable()
        {
            this.InitializeComponent();   
            visualizer.Substance = new Substance();
            visualizer.SubstanceStyle = new Style();
            Molecule molecule = new Molecule();
            molecule.Atoms.Add(new Atom() {Element = Element.GetBySymbol("H")});
            visualizer.Substance.Molecules.Add(molecule);

            // Ensure that list box will be focused, so we will be able press Ctrl + Alt to select all
            IsVisibleChanged += (s, e) => elementsListBox.Focus();
            MaterialChanged += (s, e) => UpdatePreview();
        }
        
        #endregion

        #region Materials changings

        private void SetAmbient()
        {
            if(!isSelectionChanged)
            {
                using (Transaction action = new Transaction("Change Ambient Color"))
                {
                    foreach (Tuple<Element, Material> element in elementsListBox.SelectedItems)
                    {
                        colorScheme[element.Item1].Ambient = Ambient.Color;
                    }
                    action.Commit();
                }
                RaiseMaterialChanged();
            }
        }

        private void SetDiffuse()
        {
            if (!isSelectionChanged)
            {
                using (Transaction action = new Transaction("Change Diffuse Color"))
                {
                    foreach (Tuple<Element, Material> element in elementsListBox.SelectedItems)
                    {
                        colorScheme[element.Item1].Diffuse = Diffuse.Color;
                    }
                    action.Commit();
                }
                RaiseMaterialChanged();
            }
        }

        private void SetSpecular()
        {
            if (!isSelectionChanged)
            {
                using (Transaction action = new Transaction("Change Specular Color"))
                {
                    foreach (Tuple<Element, Material> element in elementsListBox.SelectedItems)
                    {
                        colorScheme[element.Item1].Specular = Specular.Color;
                    }
                    action.Commit();
                }
                RaiseMaterialChanged();
            }
        }

        private void SetEmissive()
        {
            if (!isSelectionChanged)
            {
                using (Transaction action = new Transaction("Change Emissive Color"))
                {
                    foreach (Tuple<Element, Material> element in elementsListBox.SelectedItems)
                    {
                        colorScheme[element.Item1].Emissive = Emissive.Color;
                    }
                    action.Commit();
                }
                RaiseMaterialChanged();
            }
        }

        private void SetGlossiness()
        {
            if (!isSelectionChanged)
            {
                using (Transaction action = new Transaction(String.Format("Change Glossiness to {0:0.#}%", Glossiness * 100.0)))
                {
                    foreach (Tuple<Element, Material> element in elementsListBox.SelectedItems)
                    {
                        colorScheme[element.Item1].Glossiness = Glossiness;
                    }
                    action.Commit();
                }
                RaiseMaterialChanged();
            }
        }

        private void SetSpecularPower()
        {
            if (!isSelectionChanged)
            {
                using (Transaction action = new Transaction(String.Format("Change Specular Power to {0:0.#}%", SpecularPower * 100.0)))
                {
                    foreach (Tuple<Element, Material> element in elementsListBox.SelectedItems)
                    {
                        colorScheme[element.Item1].SpecularPower = SpecularPower;
                    }
                    action.Commit();
                }
                RaiseMaterialChanged();
            }
        }

        private void SetReflectionLevel()
        {
            if (!isSelectionChanged)
            {
                using (Transaction action = new Transaction(String.Format("Change Reflection Level to {0:0.#}%", ReflectionLevel * 100.0)))
                {
                    foreach (Tuple<Element, Material> element in elementsListBox.SelectedItems)
                    {
                        colorScheme[element.Item1].ReflectionLevel = ReflectionLevel;
                    }
                    action.Commit();
                }
                RaiseMaterialChanged();
            }
        }

        private void SetBumpLevel()
        {
            if (!isSelectionChanged)
            {
                using (Transaction action = new Transaction(String.Format("Change Bump Level to {0:0.#}%", BumpLevel * 100.0)))
                {
                    foreach (Tuple<Element, Material> element in elementsListBox.SelectedItems)
                    {
                        colorScheme[element.Item1].BumpLevel = BumpLevel;
                    }
                    action.Commit();
                }
                RaiseMaterialChanged();
            }
        }

        private void SetEmissiveLevel()
        {
            if (!isSelectionChanged)
            {
                using (Transaction action = new Transaction(String.Format("Change Emissive Level to {0:0.#}%", EmissiveLevel * 100.0)))
                {
                    foreach (Tuple<Element, Material> element in elementsListBox.SelectedItems)
                    {
                        colorScheme[element.Item1].EmissiveLevel = EmissiveLevel;
                    }
                    action.Commit();
                }
                RaiseMaterialChanged();
            }
        }

        #endregion

        #region Selection Drag Handling

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(elementsListBox.SelectedItems.Count>0)
            {
                materialGrid.IsEnabled = true;
                
                if (selectedMaterial != null) selectedMaterial.PropertyChanged -= OnSelectedMaterialPropertyChanged;

                selectedMaterial = (elementsListBox.SelectedItems[0] as Tuple<Element, Material>).Item2;

                UpdateSelectedMaterial();    

                if (selectedMaterial != null) selectedMaterial.PropertyChanged += OnSelectedMaterialPropertyChanged;
                

                
            }
            else
            {
                if (selectedMaterial != null) selectedMaterial.PropertyChanged -= OnSelectedMaterialPropertyChanged;
                materialGrid.IsEnabled = false;
                selectedMaterial = null;
            }
        }

        private void OnSelectedMaterialPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            UpdateSelectedMaterial();
        }

        void UpdateSelectedMaterial()
        {
            isSelectionChanged = true;
            if (selectedMaterial.Ambient!=Ambient.Color) Ambient = new SolidColorBrush(selectedMaterial.Ambient);
            if (selectedMaterial.Diffuse != Diffuse.Color) Diffuse = new SolidColorBrush(selectedMaterial.Diffuse);
            if (selectedMaterial.Specular != Specular.Color) Specular = new SolidColorBrush(selectedMaterial.Specular);
            if (selectedMaterial.Emissive != Emissive.Color) Emissive = new SolidColorBrush(selectedMaterial.Emissive);
            if (selectedMaterial.Glossiness != Glossiness) Glossiness = selectedMaterial.Glossiness;
            if (selectedMaterial.SpecularPower != SpecularPower) SpecularPower = selectedMaterial.SpecularPower;
            if (selectedMaterial.ReflectionLevel != ReflectionLevel) ReflectionLevel = selectedMaterial.ReflectionLevel;
            if (selectedMaterial.BumpLevel != BumpLevel) BumpLevel = selectedMaterial.BumpLevel;
            if (selectedMaterial.EmissiveLevel != EmissiveLevel) EmissiveLevel = selectedMaterial.EmissiveLevel;

            isSelectionChanged = false;
            
            UpdatePreview();
        }

        void UpdatePreview()
        {
            Transaction.Suspend();
            try
            {               
                Material sphereMaterial =
                    visualizer.SubstanceStyle.ColorStyle.ColorScheme[visualizer.Substance.Molecules[0].Atoms[0].Element];
                sphereMaterial.Ambient = selectedMaterial.Ambient;
                sphereMaterial.Diffuse = selectedMaterial.Diffuse;
                sphereMaterial.Specular = selectedMaterial.Specular;
                sphereMaterial.Emissive = selectedMaterial.Emissive;
                sphereMaterial.Glossiness = selectedMaterial.Glossiness;
                sphereMaterial.SpecularPower = selectedMaterial.SpecularPower;
                sphereMaterial.ReflectionLevel = selectedMaterial.ReflectionLevel;
                sphereMaterial.BumpLevel = selectedMaterial.BumpLevel;
                sphereMaterial.EmissiveLevel = selectedMaterial.EmissiveLevel;
            }
            finally
            {
                Transaction.Resume();
            }
        }

        #endregion

        #region Event Handling

        private void OnPreviewMouseLeftDown(object sender, MouseButtonEventArgs e)
        {
            // Move focus to the list box
            elementsListBox.Focus();

            selectionStartPosition = Mouse.GetPosition(elementsListBox);
            // Save selected items
            previousSelectedItems.Clear();
            foreach (var element in elementsListBox.SelectedItems)
            {
                ListBoxItem item = elementsListBox.ItemContainerGenerator.ContainerFromItem(element) as ListBoxItem;
                previousSelectedItems.Add(item);
            }
            // Capture mouse
            Mouse.Capture(elementsListBox);
        }

        private void OnPreviewMouseLeftUp(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture(null);
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (Mouse.Captured != elementsListBox) return;           
            Rect mouseSelectionRect = new Rect(selectionStartPosition, e.GetPosition(elementsListBox));
            bool isControlPressed = (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;
            // Check items selection
            foreach (var element in elementsListBox.Items)
            {
                ListBoxItem item = elementsListBox.ItemContainerGenerator.ContainerFromItem(element) as ListBoxItem;
                Rect itemRect = new Rect(item.TranslatePoint(new Point(0, 0), elementsListBox), item.TranslatePoint(new Point(item.ActualWidth, item.ActualHeight), elementsListBox));
                if (itemRect.IntersectsWith(mouseSelectionRect)) item.IsSelected = true;
                else if ((!isControlPressed) || (!previousSelectedItems.Contains(item))) item.IsSelected = false;
            }            
        }

        #endregion
    }
}