
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Media;
using NuGenBioChem.Data.Commands;
using NuGenBioChem.Data.Transactions;

namespace NuGenBioChem.Data
{
    /// <summary>
    /// Represents style of the molecule visualization
    /// </summary>
    public class Style : INotifyPropertyChanged
    {
        #region Static Properties

        // Collection of all styles
        static ObservableCollection<string> styles = new ObservableCollection<string>();

        /// <summary>
        /// Collection of all styles
        /// </summary>
        public static ObservableCollection<string> Styles
        {
            get
            {
                return styles;
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
            return !styles.Contains(name) && Storage.ValidateFileName(name);
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
            if (styles.Contains(styleName))
            {
                Storage.DeleteFile(StylesStorageDirectoryName + "\\" + styleName);
                styles.Remove(styleName);
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
               "Rename Style ...",
               styleName,
               ColorSchemeNameAvailable);

            if (enteredName != null)
            {
                Storage.MoveFile(StylesStorageDirectoryName + "\\" + styleName, StylesStorageDirectoryName + "\\" + enteredName);
                int index = styles.IndexOf(styleName);
                styles.RemoveAt(index);
                styles.Insert(index, enteredName);
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
                   String.Format("Apply Style \"{0}\"", styleName)))
            {
                Deserialize(styleName);
                action.Commit();
            }
        }

        #endregion

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

        #region Constants

        /// <summary>
        /// Name of the dictionary where styles are stored
        /// </summary>
        const string StylesStorageDirectoryName = "Styles";

        #endregion

        #region Fields

        // Name of the style
        string name = "Untitled";
        // Color style
        readonly ColorStyle colorStyle = new ColorStyle();
        // Geometry style
        readonly GeometryStyle geometryStyle = new GeometryStyle();
        
        #endregion

        #region Properties
        
        /// <summary>
        /// Gets or sets name of the style
        /// </summary>
        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                RaisePropertyChanged("Name");
            }
        }

        /// <summary>
        /// Gets or sets color style
        /// </summary>
        public ColorStyle ColorStyle
        {
            get { return colorStyle; }
        }


        /// <summary>
        /// Gets or sets geometry style
        /// </summary>
        public GeometryStyle GeometryStyle
        {
            get { return geometryStyle; }

        }
        
        #endregion

        #region Initialization

        /// <summary>
        /// Constructor
        /// </summary>
        public Style()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name of the style</param>
        public Style(string name) : this()
        {
            Deserialize(name);
        }

        static Style()
        {
            if (Storage.DirectoryExists(StylesStorageDirectoryName))
            {
                string[] files = Storage.GetFileNames(StylesStorageDirectoryName + "\\*");
                for (int i = 0; i < files.Length; i++)
                {
                    Styles.Add(Path.GetFileNameWithoutExtension(files[i]));
                }
            }
            else
            {
                Storage.CreateDirectory(StylesStorageDirectoryName);

                #region Add default styles

                Transaction.Suspend();
                try
                {
                    Style style = new Style();
                    style.LoadBallAndStickDeafult();
                    style.SaveAs("Default");

                    style = new Style();
                    style.GeometryStyle.AtomSize = 1;
                    style.GeometryStyle.BondSize = 0.01;
                    style.GeometryStyle.AtomSizeStyle = AtomSizeStyle.VanderWaals;
                    style.GeometryStyle.SheetHeight = 0;
                    style.GeometryStyle.HelixHeight = 0;
                    style.GeometryStyle.TurnHeight = 0;
                    style.ColorStyle.ColorScheme.LoadCoreyPaulingKoltunScheme();
                    style.SaveAs("Space-filling");

                    style = new Style();
                    style.LoadCartoonDeafult();
                    style.SaveAs("Cartoon");

                    style = new Style();
                    style.GeometryStyle.AtomSize = 0.8;
                    style.GeometryStyle.BondSize = 0.5;
                    style.GeometryStyle.AtomSizeStyle = AtomSizeStyle.Calculated;
                    style.GeometryStyle.SheetHeight = 0;
                    style.GeometryStyle.HelixHeight = 0;
                    style.GeometryStyle.TurnHeight = 0;
                    style.ColorStyle.ColorScheme.LoadCoreyPaulingKoltunScheme();
                    style.SaveAs("Ball-and-stick");

                    style = new Style();
                    style.GeometryStyle.AtomSize = 1;
                    style.GeometryStyle.BondSize = 0.6;
                    style.GeometryStyle.AtomSizeStyle = AtomSizeStyle.Uniform;
                    style.GeometryStyle.SheetHeight = 0;
                    style.GeometryStyle.HelixHeight = 0;
                    style.GeometryStyle.TurnHeight = 0;
                    style.ColorStyle.ColorScheme.LoadCoreyPaulingKoltunScheme();
                    style.SaveAs("Ball-and-stick (Uniform)");

                    style = new Style();
                    style.GeometryStyle.AtomSize = 1.0;
                    style.GeometryStyle.BondSize = 0.5;
                    style.GeometryStyle.AtomSizeStyle = AtomSizeStyle.Uniform;
                    style.GeometryStyle.SheetHeight = 0;
                    style.GeometryStyle.HelixHeight = 0;
                    style.GeometryStyle.TurnHeight = 0;
                    foreach (Element element in Element.Elements)
                    {
                        style.ColorStyle.ColorScheme[element].Diffuse = Colors.Blue;
                    }
                    style.ColorStyle.UseSingleBondMaterial = true;
                    style.ColorStyle.BondMaterial.Diffuse = Colors.White;
                    style.SaveAs("Blue");

                    style = new Style();
                    style.GeometryStyle.AtomSize = 0.75;
                    style.GeometryStyle.BondSize = 1;
                    style.GeometryStyle.AtomSizeStyle = AtomSizeStyle.Uniform;
                    style.GeometryStyle.SheetHeight = 0;
                    style.GeometryStyle.HelixHeight = 0;
                    style.GeometryStyle.TurnHeight = 0;
                    style.ColorStyle.ColorScheme.LoadCoreyPaulingKoltunScheme();
                    style.SaveAs("Wireframe");

                    style = new Style();
                    style.GeometryStyle.AtomSize = 0.45;
                    style.GeometryStyle.BondSize = 1;
                    style.GeometryStyle.AtomSizeStyle = AtomSizeStyle.Uniform;
                    style.GeometryStyle.SheetHeight = 0;
                    style.GeometryStyle.HelixHeight = 0;
                    style.GeometryStyle.TurnHeight = 0;
                    style.ColorStyle.ColorScheme.LoadCoreyPaulingKoltunScheme();
                    style.SaveAs("Wireframe (Thin)");
                }
                finally 
                {
                    Transaction.Resume();
                }

                #endregion
            }
        }

        /// <summary>
        /// Loads default cartoon style
        /// </summary>
        public void LoadCartoonDeafult()
        {
            using (Transaction action = new Transaction("Apply Default Cartoon Style"))
            {
                GeometryStyle.AtomSize = 0;
                GeometryStyle.BondSize = 0.01;
                GeometryStyle.AtomSizeStyle = AtomSizeStyle.Uniform;
                GeometryStyle.SheetHeight = 1;
                GeometryStyle.HelixHeight = 1;
                GeometryStyle.TurnHeight = 1;

                ColorStyle.ColorScheme.LoadCoreyPaulingKoltunScheme();

                action.Commit();
            }
        }

        /// <summary>
        /// Loads default Ball-and-Stick style
        /// </summary>
        public void LoadBallAndStickDeafult()
        {
            using (Transaction action = new Transaction("Apply Default Ball-and-Stick Style"))
            {
                GeometryStyle.AtomSize = 1;
                GeometryStyle.BondSize = 0;
                GeometryStyle.AtomSizeStyle = AtomSizeStyle.VanderWaals;
                GeometryStyle.SheetHeight = 0;
                GeometryStyle.HelixHeight = 0;
                GeometryStyle.TurnHeight = 0;

                ColorStyle.ColorScheme.LoadCoreyPaulingKoltunScheme();

                action.Commit();
            }
        }
        
        #endregion
        
        #region Methods

        /// <summary>
        /// Saves style to other file (this style will be not changed)
        /// </summary>
        /// <param name="newname">Name</param>
        public void SaveAs(string newname)
        {
            string fileName = StylesStorageDirectoryName + "\\" + newname;

            using (StreamWriter writer = new StreamWriter(Storage.OpenFile(fileName, FileMode.Create, FileAccess.Write)))
            {
                writer.Write(SerializeToString());
                writer.Flush();
            }

            Styles.Add(newname);
        }
        
        #endregion

        #region Serialization

        void Deserialize(string styleName)
        {
            Name = styleName;
            string path = StylesStorageDirectoryName + "\\" + styleName;

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
            
            stringBuilder.Append("\nGeometryStyle:\n");
            stringBuilder.Append(geometryStyle.SerializeToString());
            stringBuilder.Append("\nColorStyle:\n");
            stringBuilder.Append(colorStyle.SerializeToString());
            return stringBuilder.ToString();
        }

        /// <summary>
        /// Deserializes from string
        /// </summary>
        /// <param name="data">String</param>
        public void DeserializeFromString(string data)
        {
            const string partGeometryStyleSeparator = "\nGeometryStyle:\n";
            const string partColorStyleSeparator = "\nColorStyle:\n";

            int partColorStyleSeparatorIndex = data.IndexOf(partColorStyleSeparator);
            if (partColorStyleSeparatorIndex == -1) return;

            // Deserialize geometry style
            geometryStyle.DeserializeFromString(data.Substring(partGeometryStyleSeparator.Length, partColorStyleSeparatorIndex - partGeometryStyleSeparator.Length));
            // Deserialize color style
            colorStyle.DeserializeFromString(data.Substring(partColorStyleSeparatorIndex + partColorStyleSeparator.Length));
        }

        #endregion
    }
}
