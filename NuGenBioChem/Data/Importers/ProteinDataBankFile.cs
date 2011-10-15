using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media.Media3D;
using System.Globalization;


namespace NuGenBioChem.Data.Importers
{
    /// <summary>
    /// Provides protein molecule import from the .PDB file format
    /// </summary>
    public class ProteinDataBankFile : IFileImporter
    {
        #region Fields

        // Message describes the loading process
        string message = "";
        // Indicates wether loading is succesful
        bool isSuccessful;
        // Molecules from the file
        readonly MoleculeCollection molecules = new MoleculeCollection();

        // Chain's table
        readonly Dictionary<char, Chain> chains = new Dictionary<char, Chain>();
        // List of the atom
        readonly List<Atom> atoms = new List<Atom>();

        #endregion

        #region Properties

        /// <summary>
        /// Indicates whether loading were succesful
        /// </summary>
        public bool IsSuccessful
        {
            get { return isSuccessful; }
        }

        /// <summary>
        /// Gets information about loading process
        /// </summary>
        public string Messages
        {
            get { return message; }
        }

        /// <summary>
        /// Gets molecules from the file
        /// </summary>
        public MoleculeCollection Molecules
        {
            get { return molecules; }
        }

        #endregion

        #region Initialzation

        /// <summary>
        /// Loads molecule models from the specified file
        /// </summary>
        /// <param name="path">Filename</param>
        public ProteinDataBankFile(string path)
        {
            try
            {
                Parse(File.ReadLines(path));
            }
            catch (Exception exception)
            {
                message = exception.Message;
                isSuccessful = false;
            }
        }

        /// <summary>
        /// Loads molecule models from the text lines
        /// </summary>
        /// <param name="lines">Lines</param>
        public ProteinDataBankFile(string[] lines)
        {
            try
            {
                Parse(lines);
            }
            catch (Exception exception)
            {
                message = exception.Message;
                isSuccessful = false;
            }
        }

        #endregion

        #region Methods

        // Parses through all pdb-file's lines
        void Parse(IEnumerable<string> pdbLines)
        {
            foreach (string pdbLine in pdbLines)
            {
                if (pdbLine.StartsWith("REMARK"))
                    continue;
                if (pdbLine.StartsWith("ATOM"))
                {
                    ParseAtom(pdbLine);
                }
                else if (pdbLine.StartsWith("HETATM"))
                {
                    ParseHetAtom(pdbLine);
                }
                else if (pdbLine.StartsWith("HELIX"))
                {
                    ParseHelix(pdbLine);
                }
                else if (pdbLine.StartsWith("SHEET"))
                {
                    ParseSheet(pdbLine);
                }
                else if (pdbLine.StartsWith("ENDMDL") || pdbLine.StartsWith("END"))
                {
                    Molecule molecule = new Molecule();
                    molecule.Atoms.AddRange(atoms);
                    molecule.Chains.AddRange(chains.Values);

                    molecule.CalculateBonds();
                    molecules.Add(molecule);

                    atoms.Clear();
                    chains.Clear();
                }
            }

        }

        // Parses residue's atom specified by the pdb-file line
        // returns residue number where atom has been added (for residue changing control)
        void ParseAtom(string pdbLine)
        {
            Atom atom = new Atom();
            atoms.Add(atom);

            Chain chain;
            char chainId = pdbLine[21];
            // FIXME: name of the chain must be gotten from DBREF record (?)
            if (!chains.TryGetValue(chainId, out chain)) chains[chainId] = chain = new Chain() { Name = chainId.ToString() };

            int number = int.Parse(pdbLine.Substring(22, 4), CultureInfo.InvariantCulture);

            Residue residue;
            if (chain.Residues.Count == 0 || (residue = chain.Residues[chain.Residues.Count - 1]).SequenceNumber != number)
            {
                residue = new Residue();
                chain.Residues.Add(residue);
                residue.SequenceNumber = number;
                residue.Name = pdbLine.Substring(17, 3);
            }
            residue.Atoms.Add(atom);

            atom.Position = GetAtomPosition(pdbLine);
            string atomName = pdbLine.Substring(12, 4).Trim();
            if (atomName == "CA" || atomName == "C1")
            {
                // Alfa-carbon has been got
                atom.Element = Element.GetBySymbol("C");
                residue.AlfaCarbon = atom;
            }
            else
            {
                atom.Element = GetAtomElement(pdbLine);
            }
        }

        // Parses heterogenius atom specified by the pdb-file line
        void ParseHetAtom(string pdbLine)
        {
            Atom atom = new Atom();
            atoms.Add(atom);
            atom.Position = GetAtomPosition(pdbLine);
            atom.Element = GetAtomElement(pdbLine);
        }

        // Parses helix secondary structure object specified by the pdb-file line
        void ParseHelix(string pdbLine)
        {
            Chain chain;
            char chainId = pdbLine[19];
            if (!chains.TryGetValue(chainId, out chain)) chains[chainId] = chain = new Chain() { Name = chainId.ToString() };

            SecondaryStructure helix = new SecondaryStructure();
            helix.StructureType = SecondaryStructureType.Helix;
            helix.FirstResidueSequenceNumber = int.Parse(pdbLine.Substring(21, 4), CultureInfo.InvariantCulture);
            helix.LastResidueSequenceNumber = int.Parse(pdbLine.Substring(33, 4), CultureInfo.InvariantCulture);

            chain.SecondaryStructures.Add(helix);
        }

        // Parses sheet secondary structure object specified by the pdb-file line
        void ParseSheet(string pdbLine)
        {
            Chain chain;
            char chainId = pdbLine[21];
            if (!chains.TryGetValue(chainId, out chain)) chains[chainId] = chain = new Chain() { Name = chainId.ToString() };

            SecondaryStructure sheet = new SecondaryStructure();
            sheet.StructureType = SecondaryStructureType.Sheet;
            sheet.FirstResidueSequenceNumber = int.Parse(pdbLine.Substring(22, 4), CultureInfo.InvariantCulture);
            sheet.LastResidueSequenceNumber = int.Parse(pdbLine.Substring(33, 4), CultureInfo.InvariantCulture);

            chain.SecondaryStructures.Add(sheet);
        }

        // Parses position of the atom specified by the pdb-file line
        static Point3D GetAtomPosition(string pdbLine)
        {
            Point3D position = new Point3D();
            position.X = double.Parse(pdbLine.Substring(30, 8), CultureInfo.InvariantCulture);
            position.Y = double.Parse(pdbLine.Substring(38, 8), CultureInfo.InvariantCulture);
            position.Z = double.Parse(pdbLine.Substring(46, 8), CultureInfo.InvariantCulture);
            return position;
        }

        // Creates element of the atom specified by the pdb-file line
        static Element GetAtomElement(string pdbLine)
        {
            string symbol = pdbLine.Substring(76, 2).Trim();
            if (symbol.Length == 2) symbol = symbol.Substring(0, 1) + symbol.Substring(1, 1).ToLowerInvariant();
            Element result = Element.GetBySymbol(symbol);
            if (result == null)
            {
                symbol = pdbLine.Substring(12, 2).Trim().Substring(0, 1);
                result = Element.GetBySymbol(symbol);
            }
            return result;
        }

        #endregion
    }
}