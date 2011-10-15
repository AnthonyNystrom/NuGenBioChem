

using System;
using System.Globalization;
using System.IO;
using System.Windows.Media.Media3D;

namespace NuGenBioChem.Data.Importers
{
    /// <summary>
    /// A MDL Molfile is a file format created by MDL and now owned by Symyx,
    /// for holding information about the atoms, bonds, connectivity and coordinates of
    /// a molecule. The molfile consists of some header information, the Connection Table (CT)
    ///  containing atom info, then bond connections and types, 
    /// followed by sections for more complex information
    /// (see more http://en.wikipedia.org/wiki/MDL_Molfile and in the attached specification)
    /// </summary>
    public class Molefile : IFileImporter 
    {
        #region Fields

        // Molecules in the file
        readonly MoleculeCollection molecules = new MoleculeCollection();
        // Message if something was wrong
        string messages = String.Empty;
        // Is loading successful?
        bool successful = true;

        #endregion

        #region Properties

        /// <summary>
        /// Gets molecules in the file
        /// </summary>
        public MoleculeCollection Molecules
        {
            get { return molecules; }
        }

        /// <summary>
        /// Gets whether loading was successful
        /// </summary>
        public bool IsSuccessful
        {
            get { return successful; }
        }

        /// <summary>
        /// Gets messages (errors)
        /// </summary>
        public string Messages
        {
            get { return messages; }
        }

        #endregion

        #region Initilization

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="path">Filename</param>
        public Molefile(string path)
        {
            try
            {
                string data = File.ReadAllText(path);
                data = data.Replace("\r", "");

                string[] parts = data.Split(new string[] {"$$$$\n"}, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0) throw new Exception("There is no data in the file");
                messages += String.Format("Loaded {0} chars from the file, defined {1} parts", data.Length, parts.Length); 

                for (int i = 0; i < parts.Length; i++)
                {
                    try
                    {
                        molecules.Add(Parse(parts[i]));
                    }
                    catch (Exception exception)
                    {
                        messages += "\nError in the molecule #" + i + " (" + exception.Message + ")";
                        successful = false;
                    }
                }
            }
            catch (Exception exception)
            {
                messages += exception.Message;
                successful = false;
            }
        }

        #endregion

        /// <summary>
        /// Parses MDL file
        /// </summary>
        /// <param name="data">Text</param>
        /// <returns>Molecule</returns>
        public static Molecule Parse(string data)
        {   
            string[] lines = data.Split(new char[] {'\n'});
            if (lines.Length < 4) new Exception("Invalid file format");
            if (lines[3].Contains("V3000")) throw new NotSupportedException("The Extended Connection Table (V3000) is not supported");
            if (!lines[3].Contains("V2000")) /* JMol, for example, do not write V2000 text */;


            // First 3 lines are header block
            Molecule molecule = new Molecule();
            molecule.Name = String.IsNullOrEmpty(lines[0]) ? "Untitled" : lines[0];

            // Fourth line contains how much atoms and bons has the molecule
            int atomCount, bondCount;
            if (!Int32.TryParse(lines[3].Substring(0, 3), NumberStyles.Integer, CultureInfo.InvariantCulture, out atomCount))
                new Exception("Invalid header data");
            if (!Int32.TryParse(lines[3].Substring(3, 3), NumberStyles.Integer, CultureInfo.InvariantCulture, out bondCount))
                new Exception("Invalid header data");

            // Add atoms
            for (int i = 4; i < 4 + atomCount; i++)
            {
                double x = 0, y = 0, z = 0;
                if ((!Double.TryParse(lines[i].Substring(0, 10), NumberStyles.Any, CultureInfo.InvariantCulture, out x)) ||
                    (!Double.TryParse(lines[i].Substring(10, 10), NumberStyles.Any, CultureInfo.InvariantCulture, out y)) ||
                    (!Double.TryParse(lines[i].Substring(20, 10), NumberStyles.Any, CultureInfo.InvariantCulture, out z)))
                    new Exception("Invalid atom data");

                string atomSymbol = lines[i].Substring(31, Math.Min(3, lines[i].Length - 31)).Trim();
                molecule.Atoms.Add(new Atom() { Element=Element.GetBySymbol(atomSymbol), Position = new Point3D(x,y,z) } );
            }

            // Add bonds
            int beginIndex = 0;
            int endIndex = 0;
            for (int i = 4 + atomCount; i < 4 + atomCount + bondCount; i++)
            {
                // Index 1-based?
                if ((!Int32.TryParse(lines[i].Substring(0, 3), out beginIndex)) ||
                    (!Int32.TryParse(lines[i].Substring(3, 3), out endIndex)))
                    new Exception("Invalid bonds data");
                molecule.Bonds.Add(new Bond() { Begin = molecule.Atoms[beginIndex - 1], End = molecule.Atoms[endIndex - 1] });
            }

            return molecule;
        }

    }
}
