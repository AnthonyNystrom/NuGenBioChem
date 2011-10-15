using System;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Media.Media3D;
using NuGenBioChem.Data;
using NuGenBioChem.Visualization.Primitives;
using Material = System.Windows.Media.Media3D.Material;

namespace NuGenBioChem.Visualization
{
    /// <summary>
    /// Visual representation of an bond
    /// </summary>
    public class Bond : ModelVisual3D
    {
        #region Fields

        // Bond data
        Data.Bond data = null;
        Data.Style style = null;
        // Cylinder is the representation of the bond
        readonly Cylinder cylinderBegin = new Cylinder();
        readonly Cylinder cylinderEnd = new Cylinder();
        // Material
        Material material = null;

        #endregion

        #region Properties

        #region Data

        /// <summary>
        /// Gets or sets data of the bond
        /// </summary>
        public Data.Bond Data
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
                if (style != null)
                {
                    UpdateVisualModel();
                }
            }
        }

        /// <summary>
        /// Handles color style changing
        /// (This methods must be invoked from molecule to prevent massive subscribing 
        /// to style from atoms, that is too slow when their count is tremendous)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OnColorStyleChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "UseSingleBondMaterial")
            {
                UpdateMaterial();
            }
        }

        /// <summary>
        /// Handles geometry style changing
        /// (This methods must be invoked from molecule to prevent massive subscribing 
        /// to style from atoms, that is too slow when their count is tremendous)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OnGeometryStyleChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "AtomSize" || e.PropertyName == "AtomSizeStyle" || e.PropertyName == "BondSize")
            {
                double bondRadius = GetBondRadius();
                if (data.End.Position == data.Begin.Position || bondRadius <= 0.001)
                {
                    Children.Clear();
                }
                else
                {
                    if (Children.Count == 0)
                    {
                        UpdateMaterial();
                        Children.Add(cylinderBegin);
                        Children.Add(cylinderEnd);
                    }
                    cylinderBegin.Radius = cylinderEnd.Radius = bondRadius;
                }
                
            }
        }

        #endregion

        #region Material

        /// <summary>
        /// Gets or sets material which overrides the material from data
        /// </summary>
        public Material Material
        {
            get { return material; }
            set
            {
                material = value;
                UpdateMaterial();
            }
        }

        #endregion
        
        #endregion

        #region Initialization

        /// <summary>
        /// Constructor
        /// </summary>
        public Bond()
        {
            cylinderBegin.Radius = 0.1;
            cylinderEnd.Radius = 0.1;
        }

        #endregion

        #region Methods

        void UpdateVisualModel()
        {
            double bondRadius = GetBondRadius();
            if (data.End.Position == data.Begin.Position || bondRadius <= 0.001)
            {
                Children.Clear();
                return;
            }

            Vector3D subtract = data.End.Position - data.Begin.Position;

            cylinderBegin.Position = data.Begin.Position;
            cylinderBegin.Height = subtract.Length / 2.0;
            cylinderBegin.Direction = subtract / subtract.Length;

            cylinderEnd.Position = data.Begin.Position + cylinderBegin.Direction * cylinderBegin.Height;
            cylinderEnd.Height = cylinderBegin.Height;
            cylinderEnd.Direction = cylinderBegin.Direction;

            // Set radius
            cylinderBegin.Radius = cylinderEnd.Radius = bondRadius;
            // Set material
            UpdateMaterial();

            Children.Clear();
            Children.Add(cylinderBegin);
            Children.Add(cylinderEnd);
        }

        void OnDataChanged(object sender, PropertyChangedEventArgs e)
        {
            UpdateVisualModel();
        }
        
        /// <summary>
        /// Calculates bond radius for the given style
        /// </summary>
        /// <param name="geometryStyle"></param>
        public static double GetBondRadius(GeometryStyle geometryStyle)
        {
            // Here hydrogen has taken as the smallest one
            switch (geometryStyle.AtomSizeStyle)
            {
                case AtomSizeStyle.VanderWaals:
                    return 1.2 * geometryStyle.AtomSize * geometryStyle.BondSize;
                case AtomSizeStyle.Uniform:
                    return 0.35 * geometryStyle.AtomSize * geometryStyle.BondSize;
                case AtomSizeStyle.Empirical:
                    return 0.35 * geometryStyle.AtomSize * geometryStyle.BondSize;
                case AtomSizeStyle.Covalent:
                    return 0.38 * geometryStyle.AtomSize * geometryStyle.BondSize;
                case AtomSizeStyle.Calculated:
                    return 0.53 * geometryStyle.AtomSize * geometryStyle.BondSize;
            }

            return 0.35;
        }

        double GetBondRadius()
        {
            if (style == null) return 0.35;
            return GetBondRadius(style.GeometryStyle);
        }

        void UpdateMaterial()
        {
            if (style == null)
            {
                cylinderBegin.Material = material;
                cylinderEnd.Material = material;
            }
            else
            {
                if (style.ColorStyle.UseSingleBondMaterial)
                {
                    cylinderBegin.Material = cylinderEnd.Material = 
                        material ?? style.ColorStyle.BondMaterial.VisualMaterial;
                }
                else
                {
                    cylinderBegin.Material = material ?? style.ColorStyle.ColorScheme[data.Begin.Element].VisualMaterial;
                    cylinderEnd.Material = material ?? style.ColorStyle.ColorScheme[data.End.Element].VisualMaterial;
                }
            }
        }

        #endregion
    }
}
