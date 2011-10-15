using System.Windows;
using System.Windows.Media.Media3D;

namespace NuGenBioChem.Visualization.Primitives
{
    /// <summary>
    ///  Represents base class for primitives
    /// </summary>
    public class Primitive : ModelVisual3D
    {
        #region Fields

        // Geometry model
        readonly GeometryModel3D geometryModel3D = new GeometryModel3D();
        
        /// <summary>
        /// Transformations for the geometry
        /// </summary>
        protected readonly Transform3DGroup GeometryTransform = new Transform3DGroup();

        #endregion

        #region Properties

        #region Geometry

        /// <summary>
        /// Gets or sets geometry of the primitive
        /// </summary>
        public Geometry3D Geometry
        {
            get { return (Geometry3D)GetValue(GeometryProperty); }
            set { SetValue(GeometryProperty, value); }
        }

        /// <summary>
        /// Dependency property for Geometry
        /// </summary>
        public static readonly DependencyProperty GeometryProperty =
            GeometryModel3D.GeometryProperty.AddOwner(typeof(Primitive),
            new PropertyMetadata(null, OnPropertyChanged));

        #endregion

        #region Material

        /// <summary>
        /// Gets or sets material of the primitive
        /// </summary>
        public Material Material
        {
            get { return (Material)GetValue(MaterialProperty); }
            set { SetValue(MaterialProperty, value); }
        }

        /// <summary>
        /// Dependency property for Material
        /// </summary>
        public static readonly DependencyProperty MaterialProperty =
            GeometryModel3D.MaterialProperty.AddOwner(typeof(Primitive),
            new PropertyMetadata(null, OnPropertyChanged));

        #endregion

        #region BackMaterial

        /// <summary>
        /// Gets or sets back material
        /// </summary>
        public Material BackMaterial
        {
            get { return (Material)GetValue(BackMaterialProperty); }
            set { SetValue(BackMaterialProperty, value); }
        }

        /// <summary>
        /// Dependency property for BackMaterial
        /// </summary>
        public static readonly DependencyProperty BackMaterialProperty =
            GeometryModel3D.BackMaterialProperty.AddOwner(typeof(Primitive),
            new PropertyMetadata(null, OnPropertyChanged));

        #endregion
        
        static void OnPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            Primitive primitive = (Primitive)obj;

            if (args.Property == GeometryProperty)
                primitive.geometryModel3D.Geometry = (Geometry3D)args.NewValue;
            else if (args.Property == MaterialProperty)
               primitive.geometryModel3D.Material = (Material)args.NewValue;
            else
               primitive.geometryModel3D.BackMaterial = (Material)args.NewValue;
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Default constructor
        /// </summary>
        public Primitive()
        {
            geometryModel3D.Transform = GeometryTransform;
            Content = geometryModel3D;
        }

        #endregion
    }
}
