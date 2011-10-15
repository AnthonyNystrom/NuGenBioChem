using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using NuGenBioChem.Data;
using NuGenBioChem.Visualization.Primitives;
using Material = System.Windows.Media.Media3D.Material;

namespace NuGenBioChem.Visualization
{
    /// <summary>
    /// Visual representation of an atom
    /// </summary>
    public class Atom : ModelVisual3D
    {
        #region Fields

        // Data
        Data.Atom data = null;
        Data.Style style = null;
        // Sphere
        Sphere sphere = new Sphere();
        // Material
        Material material = null;

        #endregion

        #region Properties

        #region Data

        /// <summary>
        /// Gets or sets data of the atom
        /// </summary>
        public Data.Atom Data
        {
            get { return data; }
            set
            {
                if (data != null) data.PropertyChanged -= OnDataChanged;
                data = value;
                if (data != null)
                {
                    UpdateVisualModel();
                    data.PropertyChanged += OnDataChanged;
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

        public void OnGeometryStyleChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "AtomSize" || e.PropertyName == "AtomSizeStyle")
            {
                double radius = GetAtomRadius();
                if (radius <= 0.001)
                {
                    Children.Clear();
                }
                else
                {
                    if (Children.Count == 0)
                    {
                        UpdateMaterial();
                        Children.Add(sphere);
                    }
                    sphere.Radius = radius;
                }
            }
        }

        #endregion

        #region Radius

        /// <summary>
        /// Gets actual radius of the visual atom
        /// </summary>
        public double Radius
        {
            get { return sphere.Radius; }
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

        #endregion

        #region Methods

        void UpdateVisualModel()
        {
            double radius = GetAtomRadius();
            if (radius <= 0.001)
            {
                Children.Clear();
                return;
            }

            sphere.Center = data.Position;
            sphere.Radius = radius;
            UpdateMaterial();

            Children.Clear();
            Children.Add(sphere);
        }

        void OnDataChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Position") sphere.Center = data.Position;
            else 
                UpdateVisualModel();
        }

        /// <summary>
        /// Calculates radius of a visual atom
        /// </summary>
        /// <param name="atom">Data of the atom</param>
        /// <param name="geometryStyle">Geometry Style</param>
        /// <returns>Radius</returns>
        public static double GetAtomRadius(Data.Atom atom, GeometryStyle geometryStyle)
        {
            switch (geometryStyle.AtomSizeStyle)
            {
                case AtomSizeStyle.VanderWaals:
                    return atom.Element.VanderWaalsRadius * geometryStyle.AtomSize;
                case AtomSizeStyle.Uniform:
                    return 0.35 * geometryStyle.AtomSize;
                case AtomSizeStyle.Empirical:
                    return atom.Element.EmpiricalRadius * geometryStyle.AtomSize;
                case AtomSizeStyle.Covalent:
                    return atom.Element.CovalentRadius * geometryStyle.AtomSize;
                case AtomSizeStyle.Calculated:
                    return atom.Element.CalculatedRadius * geometryStyle.AtomSize;
            }

            return 1.0;
        }

        double GetAtomRadius()
        {
            if (style == null) return 1;
            return GetAtomRadius(data, style.GeometryStyle);
        }

        void UpdateMaterial()
        {
            if (style == null) sphere.Material = material;
            else sphere.Material = material ?? style.ColorStyle.ColorScheme[data.Element].VisualMaterial;
        }

        #endregion
    }
}
