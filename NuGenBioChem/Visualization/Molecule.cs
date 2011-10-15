using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Media.Media3D;

namespace NuGenBioChem.Visualization
{
    /// <summary>
    /// Visual representation of an molecule
    /// </summary>
    public class Molecule : ModelVisual3D
    {
        #region Events


        #endregion

        #region Fields

        // Data
        Data.Molecule data = null;
        Data.Style style = null;

        // Atoms and bonds
        Dictionary<Data.Atom, Atom> atoms = new Dictionary<Data.Atom, Atom>();
        List<Bond> bonds = new List<Bond>();
        List<Chain> chains = new List<Chain>();

        #endregion

        #region Properties

        #region Molecule

        /// <summary>
        /// Gets or sets data of the molecule
        /// </summary>
        public Data.Molecule Data
        {
            get { return data; }
            set
            {
                if (data == value) return;
                data = value;
                if (data != null) UpdateVisualModel();
                else
                {
                    foreach (Atom atom in atoms.Values)
                    {
                        atom.Data = null;
                        atom.Style = null;
                    }
                    foreach (Bond bond in bonds)
                    {
                        bond.Data = null;
                        bond.Style = null;
                    }
                    foreach (Chain chain in chains)
                    {
                        chain.Data = null;
                        chain.Style = null;
                    }
                    Children.Clear();
                    atoms.Clear();
                    bonds.Clear();
                }
            }
        }

        #endregion
        
        #region Style

        /// <summary>
        /// Gets or sets style of the molecule
        /// </summary>
        public Data.Style Style
        {
            get { return style; }
            set
            {
                if (style == value) return;
                if (style != null)
                {
                    style.GeometryStyle.PropertyChanged -= OnGeometryStyleChanged;
                    style.ColorStyle.PropertyChanged -= OnColorStyleChanged;
                }
                style = value;
                if (style != null)
                {
                    style.GeometryStyle.PropertyChanged += OnGeometryStyleChanged;
                    style.ColorStyle.PropertyChanged += OnColorStyleChanged;
                }
                
                foreach (Atom atom in atoms.Values)
                {
                    atom.Style = style;
                }
                foreach (Bond bond in bonds)
                {
                    bond.Style = style;
                }
                foreach (Chain chain in chains)
                {
                    chain.Style = style;
                }   
            }
        }

        void OnColorStyleChanged(object sender, PropertyChangedEventArgs e)
        {
            foreach (Bond bond in bonds)
            {
                bond.OnColorStyleChanged(sender, e);
            }
        }

        void OnGeometryStyleChanged(object sender, PropertyChangedEventArgs e)
        {
            foreach (Atom atom in atoms.Values)
            {
                atom.OnGeometryStyleChanged(sender, e);
            }
            foreach (Bond bond in bonds)
            {
                bond.OnGeometryStyleChanged(sender, e);
            }
        }

        #endregion

        #endregion

        #region Initialization

        #endregion

        #region Methods

        /// <summary>
        /// Gets visual atom entity by data (or null if this molecule doesn't contains the atom)
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public Atom GetVisualAtom(Data.Atom data)
        {
            Atom atom = null;
            atoms.TryGetValue(data, out atom);
            return atom;
        }

        void UpdateVisualModel()
        {
            Children.Clear();
            atoms.Clear();
            bonds.Clear();

            // Add atoms
            foreach (Data.Atom atomData in data.Atoms)
            {
                Atom atom = new Atom();
                atom.Data = atomData;
                atom.Style = style;
                atoms.Add(atomData, atom);
                Children.Add(atom);
            }

            // Add bonds
            foreach (Data.Bond bondData in data.Bonds)
            {
                Bond bond = new Bond();
                bond.Data = bondData;
                bond.Style = style;
                bonds.Add(bond);
                Children.Add(bond);
            }

            // Add chains
            foreach (Data.Chain chainData in data.Chains)
            {
                Chain chain = new Chain();
                chain.Data = chainData;
                chain.Style = style;
                chains.Add(chain);
                Children.Add(chain);
            }         
             
        }

        #endregion        
    }
}
