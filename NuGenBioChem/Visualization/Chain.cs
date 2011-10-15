using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Windows.Media.Media3D;
using NuGenBioChem.Data;
using Material = System.Windows.Media.Media3D.Material;

namespace NuGenBioChem.Visualization
{
    /// <summary>
    /// Visual representation of the protein chain
    /// </summary>
    public class Chain : ModelVisual3D
    {
        #region Fields

        // Chains data
        Data.Chain data = null;
        Data.Style style = null;
  
        // Matching between the visual residues in the chain and the residue data
        Dictionary<Residue, Data.Residue> residues = new Dictionary<Residue, Data.Residue>();  
        
        #endregion

        #region Properties

        #region Ribbon

        /// <summary>
        /// Ribbon of the chain
        /// </summary>
        public Ribbon Ribbon
        {
            get;
            private set;
        }

        #endregion

        #region Data

        /// <summary>
        /// Gets or sets chains data
        /// </summary>
        public Data.Chain Data
        {
            get { return data; }
            set
            {
                if (data != null) data.PropertyChanged -= OnDataChanged;
                data = value;
                if (data != null)
                {
                    data.PropertyChanged += OnDataChanged;
                    UpdateVisualModel();
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
                style = value;
                
                foreach (Residue residue in residues.Keys)
                {
                    residue.Style = value;
                }
                UpdateMaterials();
            }
        }
        
        #endregion

        #endregion

        #region Initialization

        /// <summary>
        /// Default constructor
        /// </summary>
        public Chain() { }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the data of the given residue
        /// </summary>
        /// <param name="residue">Residue</param>
        /// <returns>Residue data</returns>
        public Data.Residue GetResidueData(Residue residue)
        {
            Data.Residue result;
            return this.residues.TryGetValue(residue, out result) ? result : null;    
        }

        void OnDataChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "")
            {
                // TODO:
                UpdateVisualModel();
            }
        }

        void UpdateVisualModel()
        {
            Children.Clear();
            residues.Clear();


            this.Ribbon = new Ribbon(this);
            if (this.Ribbon.IsSuccessful)
            {
                foreach (var residueData in this.data.Residues)
                {
                    Residue visualResidue = new Residue();
                    this.residues.Add(visualResidue, residueData);
                    visualResidue.Chain = this;
                    visualResidue.Style = style;
                    Children.Add(visualResidue);
                }
            }

            UpdateMaterials();
        }

        void UpdateMaterials()
        {
            Material helixMaterial = null;
            Material sheetMaterial = null;
            Material turnMaterial = null;
            if (style != null)
            {
                helixMaterial = style.ColorStyle.HelixMaterial.VisualMaterial;
                sheetMaterial = style.ColorStyle.SheetMaterial.VisualMaterial;
                turnMaterial = style.ColorStyle.TurnMaterial.VisualMaterial;
            }

            foreach (Residue residue in Children)
            {
                SecondaryStructureType type = residue.Data.GetStructureType();
                if (type == SecondaryStructureType.Helix) residue.Material = helixMaterial;
                else if (type == SecondaryStructureType.Sheet) residue.Material = sheetMaterial;
                else if (type == SecondaryStructureType.NotDefined) residue.Material = turnMaterial;
            }
        }
        

        #endregion        
    }
}
