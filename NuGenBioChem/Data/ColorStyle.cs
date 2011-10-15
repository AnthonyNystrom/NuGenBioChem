using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Media;
using NuGenBioChem.Data.Commands;
using Media3D = System.Windows.Media.Media3D;
using NuGenBioChem.Data.Transactions;

namespace NuGenBioChem.Data
{
    /// <summary>
    /// Represents color style
    /// </summary>
    public class ColorStyle : INotifyPropertyChanged
    {
        #region Static Properties

        // Collection of all color styles
        static readonly ObservableCollection<string> colorStyles = new ObservableCollection<string>();

        /// <summary>
        /// Collection of all color styles
        /// </summary>
        public static ObservableCollection<string> ColorStyles
        {
            get
            {
                return colorStyles;
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
               "Save Current Color Style As ...",
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
            return !ColorStyles.Contains(name) && Storage.ValidateFileName(name);
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
            if (colorStyles.Contains(styleName))
            {
                Storage.DeleteFile(ColorStylesStorageDirectoryName + "\\" + styleName);
                colorStyles.Remove(styleName);
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
               "Rename Color Style ...",
               styleName,
               ColorSchemeNameAvailable);

            if (enteredName != null)
            {
                Storage.MoveFile(ColorStylesStorageDirectoryName + "\\" + styleName, ColorStylesStorageDirectoryName + "\\" + enteredName);
                int index = colorStyles.IndexOf(styleName);
                colorStyles.RemoveAt(index);
                colorStyles.Insert(index, enteredName);
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
                       String.Format("Apply Color Style \"{0}\"", styleName)))
            {
                Deserialize(styleName);
                action.Commit();
            }
        }

        #endregion

        #endregion

        #region Constants

        /// <summary>
        /// Name of the dictionary where color styles are stored
        /// </summary>
        const string ColorStylesStorageDirectoryName = "ColorStyles";

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

        // Name of the color style
        string name = "Untitled";
        // Color scheme
        readonly ColorScheme colorScheme = new ColorScheme("Default");
        // Bond color style
        readonly Transactable<bool> useSingleBondMaterial = new Transactable<bool>(false);
        // Bond material (will be used when BondColorStyle == Constant)
        readonly Material bondMaterial = new Material();

        // Helix, sheet and turn material
        readonly Material helixMaterial = new Material(Colors.Red);
        readonly Material turnMaterial = new Material(Colors.LightGreen);
        readonly Material sheetMaterial = new Material(Colors.LightGray);
        
        #endregion

        #region Properties
        
        /// <summary>
        /// Gets or sets name of the color scheme
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
        /// Gets or sets color scheme
        /// </summary>
        public ColorScheme ColorScheme
        {
            get { return colorScheme; }
        }

        /// <summary>
        /// Gets or sets whether it should be used single bond material
        /// </summary>
        public bool UseSingleBondMaterial
        {
            get { return useSingleBondMaterial.Value; }
            set
            {
                using (Transaction action = new Transaction(
                       value ? "Use Single Bond Material" : "Use Different Bond Material"))
                {
                    useSingleBondMaterial.Value = value;
                    action.Commit();
                }
            }
        }

        /// <summary>
        /// Gets bond material
        /// </summary>
        public Material BondMaterial
        {
            get { return bondMaterial; }
        }

        /// <summary>
        /// Gets helix material
        /// </summary>
        public Material HelixMaterial
        {
            get { return helixMaterial; }
        }

        /// <summary>
        /// Gets sheet material
        /// </summary>
        public Material SheetMaterial
        {
            get { return sheetMaterial; }
        }

        /// <summary>
        /// Gets turn material
        /// </summary>
        public Material TurnMaterial
        {
            get { return turnMaterial; }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Default constructor
        /// </summary>
        public ColorStyle()
        {
            useSingleBondMaterial.Changed += (s, a) => RaisePropertyChanged("UseSingleBondMaterial");
        }

        static ColorStyle()
        {
            if (Storage.DirectoryExists(ColorStylesStorageDirectoryName))
            {
                string[] files = Storage.GetFileNames(ColorStylesStorageDirectoryName + "\\*");
                for (int i = 0; i < files.Length; i++)
                {
                    colorStyles.Add(Path.GetFileNameWithoutExtension(files[i]));
                }
            }
            else
            {
                Storage.CreateDirectory(ColorStylesStorageDirectoryName);

                #region Add default color styles

                ColorStyle style = new ColorStyle();
                style.ColorScheme.LoadCoreyPaulingKoltunScheme();
                style.SaveAs("Default");

                #endregion
            }
        }
        
        #endregion

        #region Methods

        /// <summary>
        /// Saves color style to other file (this style will be not changed)
        /// </summary>
        /// <param name="newname">Name</param>
        public void SaveAs(string newname)
        {
            string fileName = ColorStylesStorageDirectoryName + "\\" + newname;

            using (StreamWriter writer = new StreamWriter(Storage.OpenFile(fileName, FileMode.Create, FileAccess.Write)))
            {
                writer.Write(SerializeToString());
                writer.Flush();
            }

            colorStyles.Add(newname);
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
            string path = ColorStylesStorageDirectoryName + "\\" + styleName;

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
            stringBuilder.AppendLine(useSingleBondMaterial.Value.ToString(CultureInfo.InvariantCulture));
            stringBuilder.AppendLine(bondMaterial.SerializeToString());
            stringBuilder.AppendLine(helixMaterial.SerializeToString());
            stringBuilder.AppendLine(sheetMaterial.SerializeToString());
            stringBuilder.AppendLine(turnMaterial.SerializeToString());
            stringBuilder.Append("\nColorScheme:\n");
            stringBuilder.Append(colorScheme.SerializeToString());
            
            return stringBuilder.ToString();
        }

        /// <summary>
        /// Deserializes from string
        /// </summary>
        /// <param name="data">String</param>
        public void DeserializeFromString(string data)
        {
            const string partSeparator = "\nColorScheme:\n";
            int partSeparatorIndex = data.IndexOf(partSeparator);
            if (partSeparatorIndex == -1) return;
            
            // Deserialize bond data
            string[] lines = data.Substring(0, partSeparatorIndex + 1).Split('\n');
            useSingleBondMaterial.Value = Boolean.Parse(lines[0]);
            bondMaterial.DeserializeFromString(lines[1]);
            // Deserialize cartoon data
            if (lines.Length >= 5)
            {
                helixMaterial.DeserializeFromString(lines[2]);
                sheetMaterial.DeserializeFromString(lines[3]);
                turnMaterial.DeserializeFromString(lines[4]);
            }
            // Deserialize color scheme
            colorScheme.DeserializeFromString(data.Substring(partSeparatorIndex + partSeparator.Length));
        }

        #endregion
    }
}

