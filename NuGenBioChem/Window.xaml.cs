using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using Fluent;
using Microsoft.Win32;
using NuGenBioChem.Data;
using NuGenBioChem.Data.Importers;
using System.Windows.Media.Media3D;
using NuGenBioChem.Data.Transactions;
using NuGenBioChem.Visualization;
using Material = NuGenBioChem.Data.Material;
using Style = NuGenBioChem.Data.Style;

namespace NuGenBioChem
{
    /// <summary>
    /// Represents the main window of the application
    /// </summary>
    public partial class Window : RibbonWindow
    {
        #region Fields

        /// <summary>
        /// Main data of the application
        /// </summary>
        readonly Substance substance = new Substance();
        readonly Style style = new Style();

        // Transaction context of this window (to undo/redo)
        TransactionContext transactionContext = new TransactionContext();

        // List of active tool windows
        List<System.Windows.Window> toolWindows = new List<System.Windows.Window>();

        // Name of opened file
        private string fileName = null;

        #endregion

        #region Properties

        /// <summary>
        /// Gets main data of the application
        /// </summary>
        public Substance Substance { get { return substance; } }

        /// <summary>
        /// Gets style of the substance
        /// </summary>
        public NuGenBioChem.Data.Style SubstanceStyle { get { return style; } }

        /// <summary>
        /// Gets transaction context of this window
        /// </summary>
        public TransactionContext TransactionContext { get { return transactionContext; } }

        #region Enumeration Values

        /// <summary>
        /// Gets available modes of the camera for binding
        /// </summary>
        public Array CameraModesValues
        {
            get { return Enum.GetValues(typeof(Visualization.CameraProjectionMode)); }
        }

        /// <summary>
        /// Gets available modes of atom radius styles
        /// </summary>
        public Array AtomRadiusStyleEnumValues
        {
            get { return Enum.GetValues(typeof(Data.AtomSizeStyle)); }
        }


        #endregion

        #region Status Bar Values

        #region SelectionInfo

        /// <summary>
        /// Gets or sets selection information
        /// </summary>
        public string SelectionInfo
        {
            get { return (string)GetValue(SelectionInfoProperty); }
            set { SetValue(SelectionInfoProperty, value); }
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for SelectionInfo.  
        /// This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty SelectionInfoProperty =
            DependencyProperty.Register("SelectionInfo", typeof(string), typeof(Window),
            new UIPropertyMetadata("No selection", null, CoerceSelectionInfo));

        static object CoerceSelectionInfo(DependencyObject d, object basevalue)
        {
            Window window = (Window)d;
            Substance substance = window.Substance;

            if (substance.SelectedAtoms.Count == 0) return "No selection";
            if (substance.SelectedAtoms.Count == 1) return substance.SelectedAtoms[0].Element.Name;
            return substance.SelectedAtoms.Count + " Atoms";
        }

        /// <summary>
        /// Gets or sets selection details
        /// </summary>
        public string SelectionDetails
        {
            get { return (string)GetValue(SelectionDetailsProperty); }
            set { SetValue(SelectionDetailsProperty, value); }
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for SelectionDetails.
        /// This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty SelectionDetailsProperty =
            DependencyProperty.Register("SelectionDetails", typeof(string), typeof(Window),
            new UIPropertyMetadata(null, null, CoerceSelectionDetails));

        static object CoerceSelectionDetails(DependencyObject d, object basevalue)
        {
            Window window = (Window)d;
            Substance substance = window.Substance;

            if (substance.SelectedAtoms.Count == 1)
            {
                Element element = substance.SelectedAtoms[0].Element;
                return String.Format(
                    "{0}\n" +
                    "Symbol: {1}\n" +
                    "Number: {2}\n\n" +
                    "Calculated radius: {3} Å\n" +
                    "Empirical radius: {4} Å\n" +
                    "Covalent radius: {5} Å\n" +
                    "Van der Waals radius: {6} Å",
                    element.Name,
                    element.Symbol,
                    element.Number,

                    Double.IsNaN(element.CalculatedRadius) ? "no data" : element.CalculatedRadius.ToString("0.##", CultureInfo.InvariantCulture),
                    Double.IsNaN(element.EmpiricalRadius) ? "no data" : element.EmpiricalRadius.ToString("0.##", CultureInfo.InvariantCulture),
                    Double.IsNaN(element.CovalentRadius) ? "no data" : element.CovalentRadius.ToString("0.##", CultureInfo.InvariantCulture),
                    Double.IsNaN(element.VanderWaalsRadius) ? "no data" : element.VanderWaalsRadius.ToString("0.##", CultureInfo.InvariantCulture));
            }
            return null;
        }

        #endregion

        #endregion

        #region Selected Element Material

        /// <summary>
        /// Gets or sets selected element's material
        /// </summary>
        public Material SelectedElementMaterial
        {
            get { return (Material)GetValue(SelectedElementMaterialProperty); }
            set { SetValue(SelectedElementMaterialProperty, value); }
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for SelectedElementMaterial.
        /// This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty SelectedElementMaterialProperty =
            DependencyProperty.Register("SelectedElementMaterial", typeof(Material), typeof(Window),
            new UIPropertyMetadata(null, null, CoerceSelectedElementMaterial));

        static object CoerceSelectedElementMaterial(DependencyObject d, object basevalue)
        {
            Window window = (Window)d;
            if (window.substance.SelectedAtoms.Count != 1) return null;
            return window.SubstanceStyle.ColorStyle.ColorScheme[window.Substance.SelectedAtoms[0].Element];
        }

        #endregion

        #region Element Contextual Group Visibility

        /// <summary>
        /// Gets or sets whether element tools contextual group should be visible
        /// </summary>
        public Visibility ElementContextualGroupVisibility
        {
            get { return (Visibility)GetValue(ElementContextualGroupVisibilityProperty); }
            set { SetValue(ElementContextualGroupVisibilityProperty, value); }
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for ElementContextualGroupVisibility.
        /// This enables animation, styling, binding, etc...
        /// </summary> 
        public static readonly DependencyProperty ElementContextualGroupVisibilityProperty =
            DependencyProperty.Register("ElementContextualGroupVisibility", typeof(Visibility), typeof(Window),
            new UIPropertyMetadata(Visibility.Collapsed, null, CoerceElementContextualGroupVisibility));

        static object CoerceElementContextualGroupVisibility(DependencyObject d, object basevalue)
        {
            Window window = (Window)d;
            return (window.substance.SelectedAtoms.Count == 1) ? Visibility.Visible : Visibility.Collapsed;
        }

        #endregion

        #region Settings

        /// <summary>
        /// Gets or sets settings
        /// </summary>
        public Settings Settings
        {
            get { return (Settings)GetValue(SettingsProperty); }
            set { SetValue(SettingsProperty, value); }
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for Settings.  This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty SettingsProperty =
            DependencyProperty.Register("Settings", typeof(Settings), typeof(Window), new UIPropertyMetadata(new Settings()));

        #endregion

        #endregion

        #region Initialization

        /// <summary>
        /// Default constructor
        /// </summary>
        public Window()
            : this(false)
        {

        }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="isEmpty">If true when creates empty window, else try to open files from command line</param>
        public Window(bool isEmpty)
        {
            InitializeComponent();

            // Load file
            string[] commandLineArgs = Environment.GetCommandLineArgs();
            if (!isEmpty && commandLineArgs.Length > 1 && !commandLineArgs[1].StartsWith("-"))
            {
                for (int i = 1; i < commandLineArgs.Length; i++) OpenFile(commandLineArgs[i]);
            }
            else
            {
                // FIXME: fix it in Fluent project and remove the dispatching stuff
                Dispatcher.BeginInvoke((ThreadStart) (() => { ribbon.IsBackstageOpen = true; }),
                                       DispatcherPriority.Render);
            }

            Initialize();
        }


        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="path">Path to the file</param>
        public Window(string path)
        {
            InitializeComponent();

            if (!IsExtensionSupported(Path.GetExtension(path))) Close();
            else
            {
                borderLoading.Visibility = Visibility.Visible;
                Dispatcher.BeginInvoke((Action<string>)OnDelayedOpen, DispatcherPriority.ContextIdle, path);
            }
        }

        void OnDelayedOpen(string path)
        {
            Open(path);
            Initialize();
            borderLoading.Visibility = Visibility.Collapsed;
        }

        void Initialize()
        {
            SourceInitialized += OnSourceInitialized;

            visualizer.Substance = substance;
            printTab.Visualizer.Substance = substance;
            publishTab.Visualizer.Substance = substance;
            visualizer.SubstanceStyle = style;
            printTab.Visualizer.SubstanceStyle = style;
            publishTab.Visualizer.SubstanceStyle = style;

            DataContext = this;

            substance.SelectedAtoms.CollectionChanged += OnSelectedAtomsChanged;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Determines whether the extension is supported
        /// </summary>
        /// <param name="extension">Extension (with '.', for ex. ".ext")</param>
        /// <returns>True if it is supported</returns>
        public bool IsExtensionSupported(string extension)
        {
            extension = extension.ToLowerInvariant();
            return
                extension == ".pdb" || extension == ".ent" || extension == ".brk" ||
                extension == ".mol" || extension == ".mdl" || 
                extension == ".sd"  || extension == ".sdf";
        }

        void Open()
        {
            OpenDir(null);
        }

        // Opens file with the given path
        void Open(string path)
        {
            IFileImporter importer;
            string extension = Path.GetExtension(path).ToLowerInvariant();
            if (extension == ".pdb" || extension == ".ent" || extension == ".brk")
                importer = new ProteinDataBankFile(path);
            else if (extension == ".mol" || extension == ".mdl" || extension == ".sd" || extension == ".sdf")
                importer = new Molefile(path);
            else throw new NotSupportedException();

            // Apply appropriate style
            if (importer.Molecules.Count > 0 && importer.Molecules[0].Chains.Count > 0) 
                style.LoadCartoonDeafult();
            else style.LoadBallAndStickDeafult();

            substance.Molecules.Clear();
            substance.Molecules.AddRange(importer.Molecules);
            visualizer.ShowAll();
            printTab.Visualizer.ShowAll();
            publishTab.Visualizer.ShowAll();

            // Change application title
            string[] splitted = Title.Split(new string[] { " - " }, StringSplitOptions.None);
            string applicationTitle = splitted[splitted.Length - 1];
            Title = Path.GetFileNameWithoutExtension(path) + " - " + applicationTitle;

            // Add opened file to recent files
            Settings.AddRecentFile(path);
            // Reset transaction context to clear all undo/redo
            transactionContext.Reset();

            fileName = Path.GetFullPath(path);
        }

        internal void OpenFile(string path)
        {
            string fullPath = Path.GetFullPath(path);
            foreach (var window in Application.Current.Windows)
            {
                Window wnd = window as Window;
                if ((wnd != null) && (wnd.fileName == fullPath))
                {
                    wnd.Activate();
                    return;
                }
            }

            if (substance.Molecules.Count == 0)
            {
                Open(path);
                // TODO: fix it
            }
            else
            {
                Window wnd = (new Window(path));
                wnd.Show();

                // FIXME: fix it in Fluent project and remove this line
                Dispatcher.BeginInvoke((Action) (() => wnd.Activate()), DispatcherPriority.Normal);
            }

        }

        internal bool OpenDir(string path)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (string.IsNullOrEmpty(path)) openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\NuGenBioChem";
            else openFileDialog.InitialDirectory = path;
            openFileDialog.Filter =
                        "All Supported Files|*.mol;*.mdl;*.sd;*.sdf;*.pdb;*.ent;*.brk|" +
                        "Protein Data Bank Files (*.pdb, *.ent, *.brk)|*.pdb;*.ent;*.brk|" +
                        "Structure-Data Files (*.sdf, *.sd)|*.sdf;*.sd|" +
                        "Molfiles (*.mol, *.mdl)|*.mol;*.mdl|" +
                        "All Files|*.*";
            if (openFileDialog.ShowDialog(this) == true)
            {
                OpenFile(openFileDialog.FileName);
                return true;
            }
            return false;
        }

        #endregion

        #region Event Handling

        void OnOpenClick(object sender, RoutedEventArgs e)
        {
            Open();
        }

        void OnEditColorSchemeButtonClick(object sender, RoutedEventArgs e)
        {
            ColorSchemeWindow window = toolWindows.OfType<ColorSchemeWindow>().FirstOrDefault();
            if (window == null)
            {
                window = new ColorSchemeWindow();
                window.ColorScheme = style.ColorStyle.ColorScheme;
                window.Owner = this;
                window.Show();
                window.Closed += (s, a) => toolWindows.Remove(window);
                toolWindows.Add(window);
            }
            else window.Activate();
        }

        void OnWindowActivated(object sender, EventArgs e)
        {
            // Set transaction context
            Data.Transactions.TransactionContext.Current = transactionContext;
            // Show back all tool windows
            // TODO:
        }

        void OnWindowDeactivated(object sender, EventArgs e)
        {
            // Hide all tool windows
            // TODO:
        }

        void OnEditCartoonMaterialsClick(object sender, RoutedEventArgs e)
        {
            CartoonMaterialsEditorWindow window = toolWindows.OfType<CartoonMaterialsEditorWindow>().FirstOrDefault();
            if (window == null)
            {
                window = new CartoonMaterialsEditorWindow();
                window.ColorStyle = style.ColorStyle;
                window.Owner = this;
                window.Show();
                window.Closed += (s, a) => toolWindows.Remove(window);
                toolWindows.Add(window);
            }
            else window.Activate();
        }

        private void OnFileDrop(object sender, DragEventArgs e)
        {
            string[] droppedFilePaths = e.Data.GetData(DataFormats.FileDrop, true) as string[];
            foreach (string droppedFilePath in droppedFilePaths)
            {
                OpenFile(droppedFilePath);
            }
        }

        private void OnSourceInitialized(object sender, EventArgs e)
        {
            HwndSource hwndSource = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
            hwndSource.AddHook(WndProc);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam, ref bool handled)
        {
            switch (msg)
            {
                case SingleInstance.WM_COPYDATA:
                    SingleInstance.GetArgs(this, lparam);
                    break;
            }
            return IntPtr.Zero;
        }

        void OnEditBondMaterialClick(object sender, RoutedEventArgs e)
        {
            BondMaterialEditorWindow window = toolWindows.OfType<BondMaterialEditorWindow>().FirstOrDefault();
            if (window == null)
            {
                window = new BondMaterialEditorWindow();
                window.BondMaterial = style.ColorStyle.BondMaterial;
                window.Owner = this;
                window.Show();
                window.Closed += (s, a) => toolWindows.Remove(window);
                toolWindows.Add(window);
            }
            else window.Activate();
        }

        #endregion

        #region Data Event Handlers

        // Handles selection changed
        void OnSelectedAtomsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            CoerceValue(SelectionInfoProperty);
            CoerceValue(SelectionDetailsProperty);
            CoerceValue(SelectedElementMaterialProperty);
            CoerceValue(ElementContextualGroupVisibilityProperty);

            // FIXME: binding between contextualGroupElementTools.Visibility and ElementContextualGroupVisibility 
            // does not work in second opened window. The following code is a workaround:
            contextualGroupElementTools.Visibility = (substance.SelectedAtoms.Count == 1) ? Visibility.Visible : Visibility.Collapsed;
        }

        #endregion
    }
}
