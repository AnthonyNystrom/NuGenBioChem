
using System;
using System.Collections.Generic;
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
    /// Represents color scheme
    /// </summary>
    public class ColorScheme : INotifyPropertyChanged
    {
        #region Static Fields

        // Collection of all color schemes
        static ObservableCollection<string> colorSchemes = new ObservableCollection<string>();

        #endregion

        #region Static Properties
        
        /// <summary>
        /// Collection of all color schemes
        /// </summary>
        public static ObservableCollection<string> ColorSchemes
        {
            get
            {
                return colorSchemes;
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
               "Save Current Color Scheme As ...",
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
            return !ColorSchemes.Contains(name) && Storage.ValidateFileName(name);
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

        static void OnRemove(string schemeName)
        {
            if (schemeName == null) return;
            if (colorSchemes.Contains(schemeName))
            {
                Storage.DeleteFile(ColorSchemeStorageDirectoryName + "\\" + schemeName);
                colorSchemes.Remove(schemeName);
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

        static void OnRename(string schemeName)
        {
            if (schemeName == null) return;
            
            string enteredName = EnterNameWindow.RequestName(null,
               "Rename Color Scheme ...",
               schemeName,
               ColorSchemeNameAvailable);

            if (enteredName != null)
            {
                Storage.MoveFile(ColorSchemeStorageDirectoryName + "\\" + schemeName, ColorSchemeStorageDirectoryName + "\\" + enteredName);
                int index = colorSchemes.IndexOf(schemeName);
                colorSchemes.RemoveAt(index);
                colorSchemes.Insert(index, enteredName);
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

        void OnApply(string schemeName)
        {
            using (Transaction action = new Transaction(
                   String.Format("Apply Color Scheme \"{0}\"", schemeName)))
            {
                Deserialize(schemeName);
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

        #region Fields

        // Name of the color scheme
        readonly Transactable<string> name = new Transactable<string>("Untitled");
        // Loaded materials
        readonly Dictionary<Element, Material> materials = new Dictionary<Element, Material>();
        
        #endregion

        #region Constants

        /// <summary>
        /// Name of the dictionary where color schemes are stored
        /// </summary>
        const string ColorSchemeStorageDirectoryName = "ColorSchemes";

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets name of the color scheme
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
        /// Get material for element symbol
        /// </summary>
        /// <param name="symbol">Element symbol</param>
        /// <returns>Material</returns>
        public Material this[string symbol]
        {
            get { return materials[Element.GetBySymbol(symbol)]; }
        }

        /// <summary>
        /// Get material for element
        /// </summary>
        /// <param name="element">Element</param>
        /// <returns>Material</returns>
        public Material this[Element element]
        {
            get { return materials[element]; }
        }
    

        #endregion

        #region Initialization

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="name">Name of color scheme</param>
        public ColorScheme(string name)
        {
            this.name.Value = name;            
            Deserialize();
            this.name.Changed += (s, a) => RaisePropertyChanged("Name");
        }

        static ColorScheme()
        {
            if (Storage.DirectoryExists(ColorSchemeStorageDirectoryName))
            {
                string[] files = Storage.GetFileNames(ColorSchemeStorageDirectoryName + "\\*");
                for (int i = 0; i < files.Length; i++)
                {
                    colorSchemes.Add(Path.GetFileNameWithoutExtension(files[i]));
                }
            }
            else
            {
                Storage.CreateDirectory(ColorSchemeStorageDirectoryName);

                #region Add default color schemes

                ColorScheme colorScheme = new ColorScheme("Empty Color Scheme");
                colorScheme.SaveAs("Empty Color Scheme");

                colorScheme = new ColorScheme("Corey Pauling Koltun's Scheme");
                colorScheme.LoadCoreyPaulingKoltunScheme();
                
                colorScheme.SaveAs("Corey Pauling Koltun Scheme");

                #endregion
            }
        }

        /// <summary>
        /// Loads Corey Pauling Koltun's Scheme (CPK)
        /// </summary>
        public void LoadCoreyPaulingKoltunScheme()
        {
            Transaction.Suspend();
            try
            {
                foreach (var element in Element.Elements)
                {
                    Material material = this[element];
                    material.Diffuse = Colors.Pink;
                    material.Ambient = Colors.Black;
                    material.Specular = Colors.White;
                    material.BumpLevel = 0;
                    material.Glossiness = 0;
                    material.SpecularPower = 0.5;
                    material.BumpLevel = 0;
                }

                this["H"].Diffuse = Colors.White;
                this["C"].Diffuse = Colors.Silver;
                this["N"].Diffuse = Colors.DarkBlue;
                this["O"].Diffuse = Colors.Red;
                this["F"].Diffuse = Colors.Green;
                this["Cl"].Diffuse = Colors.Green;
                this["Br"].Diffuse = Colors.DarkRed;
                this["I"].Diffuse = Colors.DarkViolet;
                // Noble gases
                this["He"].Diffuse = Colors.Cyan;
                this["Ne"].Diffuse = Colors.Cyan;
                this["Ar"].Diffuse = Colors.Cyan;
                this["Xe"].Diffuse = Colors.Cyan;
                this["Kr"].Diffuse = Colors.Cyan;


                this["P"].Diffuse = Colors.Orange;
                this["S"].Diffuse = Colors.Yellow;

                // B & Most transition metalls
                this["B"].Diffuse = Colors.Salmon;

                this["Li"].Diffuse = Colors.Violet;
                this["Na"].Diffuse = Colors.Violet;
                this["K"].Diffuse = Colors.Violet;
                this["Rb"].Diffuse = Colors.Violet;
                this["Cs"].Diffuse = Colors.Violet;

                this["Be"].Diffuse = Colors.DarkGreen;
                this["Mg"].Diffuse = Colors.DarkGreen;
                this["Ca"].Diffuse = Colors.DarkGreen;
                this["Sr"].Diffuse = Colors.DarkGreen;
                this["Ba"].Diffuse = Colors.DarkGreen;
                this["Ra"].Diffuse = Colors.DarkGreen;

                this["Ti"].Diffuse = Colors.LightGray;
                this["Fe"].Diffuse = Colors.Orange;
            }
            finally
            {
                Transaction.Resume();
            }
        }

        #endregion
        
        #region Methods
        
        /// <summary>
        /// Saves color scheme to other file (this scheme will be not changed)
        /// </summary>
        /// <param name="newname">Name</param>
        public void SaveAs(string newname)
        {
            string fileName = ColorSchemeStorageDirectoryName + "\\" + newname;
            
            using (StreamWriter writer = new StreamWriter(Storage.OpenFile(fileName, FileMode.Create, FileAccess.Write)))
            {
                writer.Write(SerializeToString());
                writer.Flush();
            } 

            colorSchemes.Add(newname);
        }

        /// <summary>
        /// Loads new scheme
        /// </summary>
        /// <param name="schemeName">Scheme</param>
        public void Load(string schemeName)
        {
            Deserialize(schemeName);
        }

        #endregion

        #region Serialization / Deserialization
        
        // Serialize color scheme to string
        internal string SerializeToString()
        {
            StringBuilder builder = new StringBuilder();
            foreach (KeyValuePair<Element, Material> material in materials)
            {
                builder.AppendFormat("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}|{9}\n",
                                    material.Key.Symbol,
                                    material.Value.Ambient.ToString(CultureInfo.InvariantCulture),
                                    material.Value.Diffuse.ToString(CultureInfo.InvariantCulture),
                                    material.Value.Specular.ToString(CultureInfo.InvariantCulture),
                                    material.Value.Emissive.ToString(CultureInfo.InvariantCulture),
                                    material.Value.Glossiness.ToString(CultureInfo.InvariantCulture),
                                    material.Value.SpecularPower.ToString(CultureInfo.InvariantCulture),
                                    material.Value.ReflectionLevel.ToString(CultureInfo.InvariantCulture),
                                    material.Value.BumpLevel.ToString(CultureInfo.InvariantCulture),
                                    material.Value.EmissiveLevel.ToString(CultureInfo.InvariantCulture)
                );
            }
            return builder.ToString();
        }

        // Serialize color scheme to isolated storage
        void Serialize()
        {
            using(StreamWriter writer = new StreamWriter(Storage.OpenFile(ColorSchemeStorageDirectoryName + "\\" +Name, FileMode.Create, FileAccess.Write)))
            {
                writer.Write(SerializeToString());
                writer.Flush();
            }            
        }
        
        // Deserializes color scheme from string
        internal void DeserializeFromString(string dataString)
        {            
            Transaction.Suspend();

            try
            {
                string[] lines = dataString.Split('\n');
                for (int i = 0; i < lines.Length; i++)
                {
                    string[] data = lines[i].Split('|');
                    if (data.Length == 10)
                    {
                        Element element = Element.GetBySymbol(data[0]);
                        Material material;
                        if (materials.ContainsKey(element))
                        {
                            // FIXME: use material.Serialize methods (to fix undo/redo issues)
                            material = materials[element];
                            material.Ambient = (Color) ColorConverter.ConvertFromString(data[1]);
                            material.Diffuse = (Color) ColorConverter.ConvertFromString(data[2]);
                            material.Specular = (Color) ColorConverter.ConvertFromString(data[3]);
                            material.Emissive = (Color) ColorConverter.ConvertFromString(data[4]);
                            material.Glossiness = Convert.ToDouble(data[5], CultureInfo.InvariantCulture);
                            material.SpecularPower = Convert.ToDouble(data[6], CultureInfo.InvariantCulture);
                            material.ReflectionLevel = Convert.ToDouble(data[7], CultureInfo.InvariantCulture);
                            material.BumpLevel = Convert.ToDouble(data[8], CultureInfo.InvariantCulture);
                            material.EmissiveLevel = Convert.ToDouble(data[9], CultureInfo.InvariantCulture);
                        }
                        else
                        {
                            // FIXME: use material.Serialize methods (to fix undo/redo issues)
                            material = new Material()
                                           {
                                               Ambient = (Color) ColorConverter.ConvertFromString(data[1]),
                                               Diffuse = (Color) ColorConverter.ConvertFromString(data[2]),
                                               Specular = (Color) ColorConverter.ConvertFromString(data[3]),
                                               Emissive = (Color) ColorConverter.ConvertFromString(data[4]),
                                               Glossiness = Convert.ToDouble(data[5], CultureInfo.InvariantCulture),
                                               SpecularPower = Convert.ToDouble(data[6], CultureInfo.InvariantCulture),
                                               ReflectionLevel =
                                                   Convert.ToDouble(data[7], CultureInfo.InvariantCulture),
                                               BumpLevel = Convert.ToDouble(data[8], CultureInfo.InvariantCulture),
                                               EmissiveLevel = Convert.ToDouble(data[9], CultureInfo.InvariantCulture)
                                           };
                            materials.Add(element, material);
                        }
                    }
                }
            }
            finally
            {
                Transaction.Resume();
            }
        }

        // Deserializes color scheme from isolated storage
        void Deserialize()
        {
            Deserialize(Name);
        }

        void Deserialize(string schemeName)
        {
            Name = schemeName;
            
            if (Storage.FileExists(ColorSchemeStorageDirectoryName + "\\" + schemeName))
            {
                using (StreamReader reader = new StreamReader(Storage.OpenFile(ColorSchemeStorageDirectoryName + "\\" + schemeName, FileMode.Open, FileAccess.Read)))
                {
                    DeserializeFromString(reader.ReadToEnd());
                }
            }

            // Fill all nonsaved elements
            foreach (Element element in Element.Elements)
            {
                if (!materials.ContainsKey(element))
                {
                    Material material = new Material();
                    materials.Add(element, material);
                }
            }
        }

        #endregion

        #region ToString

        /// <summary>
        /// Returns a string that represents the current object
        /// </summary>
        /// <returns>
        /// A string that represents the current object
        /// </returns>
        public override string ToString()
        {
            return Name;
        }

        #endregion
    }
}
