using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text;
using NuGenBioChem.Data.Commands;
using NuGenBioChem.Data.Transactions;

namespace NuGenBioChem.Data
{
    /// <summary>
    /// Represents geometry style
    /// </summary>
    public class GeometryStyle : INotifyPropertyChanged
    {
        #region Static Properties

        // Collection of all geometry styles
        static readonly ObservableCollection<string> geometryStyles = new ObservableCollection<string>();

        /// <summary>
        /// Collection of all geometry styles
        /// </summary>
        public static ObservableCollection<string> GeometryStyles
        {
            get
            {
                return geometryStyles;
            }
        }

        #endregion

        #region Commands

        #region Save As Commands

        // Command
        RelayCommand saveAsCommand;

        /// <summary>
        /// Gets Save As command
        /// </summary>
        public RelayCommand SaveAsCommand
        {
            get
            {
                if (saveAsCommand == null)
                {
                    saveAsCommand = new RelayCommand(x => OnSaveAs());
                }
                return saveAsCommand;
            }
        }

        void OnSaveAs()
        {
            string enteredName = EnterNameWindow.RequestName(null,
               "Save Current Geometry Style As ...",
               "Untitled",
               ColorSchemeNameAvailable);

            if (enteredName != null)
            {
                SaveAs(enteredName);
            }
        }

        // Check color scheme name availability
        static bool ColorSchemeNameAvailable(string name)
        {
            return !GeometryStyles.Contains(name) && Storage.ValidateFileName(name);
        }

        #endregion

        #region Remove Command

        // Command
        static RelayCommand removeCommand;

        /// <summary>
        /// Gets Apply Command
        /// </summary>
        public static RelayCommand RemoveCommand
        {
            get
            {
                if (removeCommand == null)
                {
                    removeCommand = new RelayCommand(x => OnRemove(x as string));
                }
                return removeCommand;
            }
        }

        static void OnRemove(string styleName)
        {
            if (styleName == null) return;
            if (geometryStyles.Contains(styleName))
            {
                Storage.DeleteFile(GeometryStylesStorageDirectoryName + "\\" + styleName);
                geometryStyles.Remove(styleName);
            }
        }

        #endregion

        #region Rename Command

        // Command
        static RelayCommand renameCommand;

        /// <summary>
        /// Gets Rename Command
        /// </summary>
        public static RelayCommand RenameCommand
        {
            get
            {
                if (renameCommand == null)
                {
                    renameCommand = new RelayCommand(x => OnRename(x as string));
                }
                return renameCommand;
            }
        }

        static void OnRename(string styleName)
        {
            if (styleName == null) return;

            string enteredName = EnterNameWindow.RequestName(null,
               "Rename Geometry Style ...",
               styleName,
               ColorSchemeNameAvailable);

            if (enteredName != null)
            {
                Storage.MoveFile(GeometryStylesStorageDirectoryName + "\\" + styleName, GeometryStylesStorageDirectoryName + "\\" + enteredName);
                int index = geometryStyles.IndexOf(styleName);
                geometryStyles.RemoveAt(index);
                geometryStyles.Insert(index, enteredName);
            }
        }

        #endregion

        #region Apply Command

        // Command
        RelayCommand applyCommand;

        /// <summary>
        /// Gets Apply Command
        /// </summary>
        public RelayCommand ApplyCommand
        {
            get
            {
                if (applyCommand == null)
                {
                    applyCommand = new RelayCommand(x => OnApply(x.ToString()));
                }
                return applyCommand;
            }
        }

        void OnApply(string styleName)
        {
            using (Transaction action = new Transaction(
                       String.Format("Apply Geometry Style \"{0}\"", styleName)))
            {
                Deserialize(styleName);
                action.Commit();
            }
        }

        #endregion

        #endregion

        #region Constants

        /// <summary>
        /// Name of the dictionary where geometry styles are stored
        /// </summary>
        const string GeometryStylesStorageDirectoryName = "GeometryStyles";

        #endregion

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

        // Name of the color scheme
        readonly Transactable<string> name = new Transactable<string>("Untitled");
        // Atom radius
        readonly Transactable<double> atomSize = new Transactable<double>(1.0);
        // Atom radius style
        readonly Transactable<AtomSizeStyle> atomSizeStyle = new Transactable<AtomSizeStyle>(AtomSizeStyle.VanderWaals);
        // Bond radius
        readonly Transactable<double> bondSize = new Transactable<double>(0.1);

        // Sheet's, Helix's, Arrow's widths and heights
        readonly Transactable<double> sheetWidth = new Transactable<double>(1.0d);
        readonly Transactable<double> sheetHeight = new Transactable<double>(1.0d);
        readonly Transactable<double> arrowWidth = new Transactable<double>(1.0d);
        readonly Transactable<double> turnWidth = new Transactable<double>(1.0d);
        readonly Transactable<double> turnHeight = new Transactable<double>(1.0d);
        readonly Transactable<double> helixWidth = new Transactable<double>(1.0d);
        readonly Transactable<double> helixHeight = new Transactable<double>(1.0d);

        #endregion

        #region Properties
        
        /// <summary>
        /// Gets or sets name of the geometry style
        /// </summary>
        public string Name
        {
            get { return name.Value; }
            set
            {
                name.Value = value;
            }
        }

        /// <summary>
        /// Gets or sets atom radius
        /// </summary>
        public double AtomSize
        {
            get { return atomSize.Value; }
            set
            {
                using (Transaction action = new Transaction(
                    String.Format("Change Size of Atoms to {0:0}%", value * 100.0)))
                {
                    atomSize.Value = value;
                    action.Commit();
                }
            }
        }

        /// <summary>
        /// Gets or sets atom radius style
        /// </summary>
        public AtomSizeStyle AtomSizeStyle
        {
            get { return atomSizeStyle.Value; }
            set
            {
                using (Transaction action = new Transaction(
                       String.Format("Change Style of Size of Atoms to {0}", value)))
                {
                    atomSizeStyle.Value = value;
                    action.Commit();
                }
            }
        }

        /// <summary>
        /// Gets or sets bond radius
        /// </summary>
        public double BondSize
        {
            get { return bondSize.Value; }
            set
            {
                using (Transaction action = new Transaction(
                       String.Format("Change Size of Bonds to {0:0}%", value * 100.0)))
                {
                    bondSize.Value = value;
                    action.Commit();
                }
            }
        }

        /// <summary>
        /// Gets or sets width of sheets
        /// </summary>
        public double SheetWidth
        {
            get { return sheetWidth.Value; } 
            set
            {
                using(Transaction action = new Transaction(
                      String.Format("Change Sheet's Width to {0:0.0}%", value * 100.0)))
                {
                    sheetWidth.Value = value;
                    action.Commit();
                }
            }
        }

        /// <summary>
        /// Gets or sets height of sheets
        /// </summary>
        public double SheetHeight
        {
            get { return sheetHeight.Value; }
            set
            {
                using (Transaction action = new Transaction(
                       String.Format("Change Sheet's Height to {0:0.0}%", value * 100.0)))
                {
                    sheetHeight.Value = value;
                    action.Commit();
                }
            }
        }

        /// <summary>
        /// Gets or sets width of arrows
        /// </summary>
        public double ArrowWidth
        {
            get { return arrowWidth.Value; }
            set
            {
                using (Transaction action = new Transaction(
                       String.Format("Change Arrow's Width to {0:0.0}%", value * 100.0)))
                {
                    arrowWidth.Value = value;
                    action.Commit();
                }
            }
        }

        /// <summary>
        /// Gets or sets width of turn
        /// </summary>
        public double TurnWidth
        {
            get { return turnWidth.Value; }
            set
            {
                using (Transaction action = new Transaction(
                       String.Format("Change Turn's Width to {0:0.0}%", value * 100.0)))
                {
                    turnWidth.Value = value;
                    action.Commit();
                }
            }
        }

        /// <summary>
        /// Gets or sets height of turn
        /// </summary>
        public double TurnHeight
        {
            get { return turnHeight.Value; }
            set
            {
                using (Transaction action = new Transaction(
                       String.Format("Change Turn's Height to {0:0.0}%", value * 100.0)))
                {
                    turnHeight.Value = value;
                    action.Commit();
                }
            }
        }

        /// <summary>
        /// Gets or sets width of helix
        /// </summary>
        public double HelixWidth
        {
            get { return helixWidth.Value; }
            set
            {
                using (Transaction action = new Transaction(
                       String.Format("Change Helix's Width to {0:0.0}%", value * 100.0)))
                {
                    helixWidth.Value = value;
                    action.Commit();
                }
            }
        }

        /// <summary>
        /// Gets or sets height of helix
        /// </summary>
        public double HelixHeight
        {
            get { return helixHeight.Value; }
            set
            {
                using (Transaction action = new Transaction(
                       String.Format("Change Helix's Height to {0:0.0}%", value * 100.0)))
                {
                    helixHeight.Value = value;
                    action.Commit();
                }
            }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Default constructor
        /// </summary>
        public GeometryStyle()
        {
            name.Changed += (s, a) => RaisePropertyChanged("Name");
            atomSize.Changed += (s, a) => RaisePropertyChanged("AtomSize");
            atomSizeStyle.Changed += (s, a) => RaisePropertyChanged("AtomSizeStyle");
            bondSize.Changed += (s, a) => RaisePropertyChanged("BondSize");

            sheetWidth.Changed += (s, a) => RaisePropertyChanged("SheetWidth");
            sheetHeight.Changed += (s, a) => RaisePropertyChanged("SheetHeight");
            arrowWidth.Changed += (s, a) => RaisePropertyChanged("ArrowWidth");
            turnWidth.Changed += (s, a) => RaisePropertyChanged("TurnWidth");
            turnHeight.Changed += (s, a) => RaisePropertyChanged("TurnHeight");
            helixWidth.Changed += (s, a) => RaisePropertyChanged("HelixWidth");
            helixHeight.Changed += (s, a) => RaisePropertyChanged("HelixHeight");
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name of the style</param>
        public GeometryStyle(string name) : this()
        {
            Deserialize(name);
        }

        static GeometryStyle()
        {
            if (Storage.DirectoryExists(GeometryStylesStorageDirectoryName))
            {
                string[] files = Storage.GetFileNames(GeometryStylesStorageDirectoryName + "\\*");
                for (int i = 0; i < files.Length; i++)
                {
                    geometryStyles.Add(Path.GetFileNameWithoutExtension(files[i]));
                }
            }
            else
            {
                Storage.CreateDirectory(GeometryStylesStorageDirectoryName);

                #region Add default geometry styles

                GeometryStyle style = new GeometryStyle();
                style.atomSize.Value = 1;
                style.atomSizeStyle.Value = AtomSizeStyle.VanderWaals;
                style.bondSize.Value = 0;
                style.sheetHeight.Value = 0;
                style.turnWidth.Value = 0;
                style.helixWidth.Value = 0;
                style.SaveAs("Space filled");

                style = new GeometryStyle();
                style.atomSize.Value = 0.80;
                style.atomSizeStyle.Value = AtomSizeStyle.Calculated;
                style.bondSize.Value = 0.50;
                style.sheetHeight.Value = 0;
                style.turnWidth.Value = 0;
                style.helixWidth.Value = 0;
                style.SaveAs("Ball-and-Stick (Calculated)");

                style = new GeometryStyle();
                style.atomSize.Value = 0.80;
                style.atomSizeStyle.Value = AtomSizeStyle.Empirical;
                style.bondSize.Value = 0.50;
                style.sheetHeight.Value = 0;
                style.turnWidth.Value = 0;
                style.helixWidth.Value = 0;
                style.SaveAs("Ball-and-Stick (Empirical)");

                style = new GeometryStyle();
                style.atomSize.Value = 0.5;
                style.atomSizeStyle.Value = AtomSizeStyle.Uniform;
                style.bondSize.Value = 1;
                style.sheetHeight.Value = 0;
                style.turnWidth.Value = 0;
                style.helixWidth.Value = 0;
                style.SaveAs("Wireframe");

                style = new GeometryStyle();
                style.atomSize.Value = 0;
                style.atomSizeStyle.Value = AtomSizeStyle.VanderWaals;
                style.bondSize.Value = 1;
                style.sheetHeight.Value = 1;
                style.turnWidth.Value = 1;
                style.helixWidth.Value = 1;
                style.SaveAs("Cartoon");

                #endregion
            }
        }
        
        #endregion

        #region Methods

        /// <summary>
        /// Saves geometry style to other file (this style will be not changed)
        /// </summary>
        /// <param name="newname">Name</param>
        public void SaveAs(string newname)
        {
            string fileName = GeometryStylesStorageDirectoryName + "\\" + newname;

            using (StreamWriter writer = new StreamWriter(Storage.OpenFile(fileName, FileMode.Create, FileAccess.Write)))
            {
                writer.Write(SerializeToString());
                writer.Flush();
            }

            geometryStyles.Add(newname);
        }
        
        #endregion

        #region Serialization
        
        // Deserializes color scheme from isolated storage
        void Deserialize()
        {
            Deserialize(Name);
        }

        void Deserialize(string styleName)
        {
            Name = styleName;
            string path = GeometryStylesStorageDirectoryName + "\\" + styleName;

            if (Storage.FileExists(path))
            {
                using (StreamReader reader = new StreamReader(Storage.OpenFile(path, FileMode.Open, FileAccess.Read)))
                {
                    DeserializeFromString(reader.ReadToEnd());
                }
            }
        }

        /// <summary>
        /// Serializes to string
        /// </summary>
        /// <returns></returns>
        public string SerializeToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(AtomSize.ToString(CultureInfo.InvariantCulture));
            stringBuilder.AppendLine(((int)AtomSizeStyle).ToString(CultureInfo.InvariantCulture));
            stringBuilder.AppendLine(BondSize.ToString(CultureInfo.InvariantCulture));
            
            stringBuilder.AppendLine(SheetWidth.ToString(CultureInfo.InvariantCulture));
            stringBuilder.AppendLine(SheetHeight.ToString(CultureInfo.InvariantCulture));
            stringBuilder.AppendLine(ArrowWidth.ToString(CultureInfo.InvariantCulture));
            stringBuilder.AppendLine(TurnWidth.ToString(CultureInfo.InvariantCulture));
            stringBuilder.AppendLine(TurnHeight.ToString(CultureInfo.InvariantCulture));
            stringBuilder.AppendLine(HelixWidth.ToString(CultureInfo.InvariantCulture));
            stringBuilder.AppendLine(HelixHeight.ToString(CultureInfo.InvariantCulture));

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Deserializes from string
        /// </summary>
        /// <param name="data"></param>
        public void DeserializeFromString(string data)
        {
            string[] lines = data.Split('\n');
            if (lines.Length < 3) return;

            atomSize.Value = Double.Parse(lines[0], CultureInfo.InvariantCulture);
            atomSizeStyle.Value = (AtomSizeStyle)Int32.Parse(lines[1], CultureInfo.InvariantCulture);
            bondSize.Value = Double.Parse(lines[2], CultureInfo.InvariantCulture);

            if (lines.Length < 10) return;

            sheetWidth.Value = Double.Parse(lines[3], CultureInfo.InvariantCulture);
            sheetHeight.Value = Double.Parse(lines[4], CultureInfo.InvariantCulture);
            arrowWidth.Value = Double.Parse(lines[5], CultureInfo.InvariantCulture);
            turnWidth.Value = Double.Parse(lines[6], CultureInfo.InvariantCulture);
            turnHeight.Value = Double.Parse(lines[7], CultureInfo.InvariantCulture);
            helixWidth.Value = Double.Parse(lines[8], CultureInfo.InvariantCulture);
            helixHeight.Value = Double.Parse(lines[9], CultureInfo.InvariantCulture);
        }

        #endregion
    }
}

