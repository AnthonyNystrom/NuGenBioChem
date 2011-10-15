using System;
using System.Windows.Media.Media3D;
using NuGenBioChem.Visualization.Primitives;

namespace NuGenBioChem.Visualization
{
    /// <summary>
    /// Contains common used meshes
    /// </summary>
    public class Meshes
    {
        #region Quality

        /// <summary>
        /// Occurs when qulity has been changed
        /// </summary>
        static event Action QualityChanged;

        // Quality of the meshes
        static double quality = 0.2;

        /// <summary>
        /// Gets or sets quality of the meshes
        /// </summary>
        public static double Quality
        {
            get { return quality; }
            set
            {
                if (quality != value)
                {
                    quality = value;
                    if (QualityChanged != null) QualityChanged();
                }
            }
        }

        #endregion

      /*  #region Sphere

        static MeshGeometry3D sphere;

        /// <summary>
        /// Gets mesh representation of shpere
        /// </summary>
        public static MeshGeometry3D Sphere
        {
            get
            {
                if (sphere == null)
                {
                    sphere = new MeshGeometry3D();
                    UpdateShpere();
                    QualityChanged += UpdateShpere;
                }
                return sphere;
            }
        }

        static void UpdateShpere()
        {
            Primitives.Sphere sphereGenerator = new Sphere(new Point3D(), 1.0);
            sphereGenerator.Quality = quality;

            sphere.Clear();
            sphereGenerator.Generate(sphere);
        }

        #endregion*/

        /*#region Cylinder

        static MeshGeometry3D cylider;

        /// <summary>
        /// Gets mesh representation of cylider
        /// </summary>
        public static MeshGeometry3D Cylider
        {
            get
            {
                if (cylider == null)
                {
                    cylider = new MeshGeometry3D();
                    UpdateCylider();
                    QualityChanged += UpdateCylider;
                }
                return cylider;
            }
        }

        static void UpdateCylider()
        {
            Primitives.Cylinder cyliderGenerator = new Primitives.Cylinder(new Point3D(), 1.0, 1.0);
            cyliderGenerator.Quality = quality;

            cylider.Clear();
            cyliderGenerator.Generate(cylider);
        }

        #endregion*/
    }
}
