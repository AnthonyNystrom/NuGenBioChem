using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace NuGenBioChem.Visualization.Primitives
{
    /// <summary>
    /// Represents base class of a simple extruded surface. Extruded surface is the 
    /// surface which obtained by moving one curve segment (named profile) 
    /// along another curve segment (named trajectory)
    /// </summary>
    public abstract class ExtrudedSurface : Primitive
    {
        #region Properties

        #region Centers

        /// <summary>
        /// Dependency property
        /// </summary>
        public static readonly DependencyProperty CentersProperty =
            DependencyProperty.Register("Centers", typeof(Point3D[]), typeof(ExtrudedSurface),
            new PropertyMetadata(new Point3D[0], OnCentersChanged));

        /// <summary>
        /// Gets or sets center points of the profile curve
        /// </summary>
        public Point3D[] Centers
        {
            get { return (Point3D[])GetValue(CentersProperty); }
            set { SetValue(CentersProperty, value); }
        }

        static void OnCentersChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ExtrudedSurface primitive = sender as ExtrudedSurface;
            primitive.UpdateGeometry(); 
        }

        #endregion

        #region Vertical vectors

        /// <summary>
        /// Dependency property
        /// </summary>
        public static readonly DependencyProperty VerticalVectorsProperty =
            DependencyProperty.Register("VerticalVectors", typeof(Vector3D[]), typeof(ExtrudedSurface),
            new PropertyMetadata(new Vector3D[0], OnVerticalVectorsChanged));

        /// <summary>
        /// Gets or sets the vertical vectors at the trajectory points
        /// </summary>
        public Vector3D[] VerticalVectors
        {
            get { return (Vector3D[])GetValue(VerticalVectorsProperty); }
            set { SetValue(VerticalVectorsProperty, value); }
        }

        static void OnVerticalVectorsChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ExtrudedSurface primitive = sender as ExtrudedSurface;
            primitive.UpdateGeometry(); 
        }

        #endregion

        #region Horizontal vectors

        /// <summary>
        /// Dependency property
        /// </summary>
        public static readonly DependencyProperty HorizontalVectorsProperty =
           DependencyProperty.Register("HorizontalVectors", typeof(Vector3D[]), typeof(ExtrudedSurface),
           new PropertyMetadata(new Vector3D[0], OnHorizontalVectorsChanged));

        /// <summary>
        /// Gets or sets horizontal vectors at the trajectory points
        /// </summary>
        public Vector3D[] HorizontalVectors
        {
            get { return (Vector3D[])GetValue(HorizontalVectorsProperty); }
            set { SetValue(HorizontalVectorsProperty, value); }
        }

        static void OnHorizontalVectorsChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ExtrudedSurface primitive = sender as ExtrudedSurface;
            primitive.UpdateGeometry(); 
        }

        #endregion   
       
        #endregion

        #region Initialization
        #endregion

        #region Overridable

        /// <summary>
        /// Updates the geometry of the primitive
        /// </summary>
        protected abstract void UpdateGeometry();

        /// <summary>
        /// Creates representation of the lower cap of the primitive
        /// </summary>
        /// <returns>Primitive or null if not allowed</returns>
        public abstract Primitive CreateLowerCap();

        /// <summary>
        /// Creates representation of the upper cap of the primitve
        /// </summary>
        /// <returns>Primitive or null if not allowed</returns>
        public abstract Primitive CreateUpperCap(); 

      
        #endregion
    }
}
