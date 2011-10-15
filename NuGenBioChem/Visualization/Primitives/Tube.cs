using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace NuGenBioChem.Visualization.Primitives
{
    /// <summary>
    /// Representation of an elliptical tube primitive.
    /// The tube is an extruded primitive with elliptical profile.
    /// </summary>
    public sealed class Tube : ExtrudedSurface 
    {
        #region Static fields

        /// <summary>
        /// Count of the radial segement in tube visualisation
        /// </summary>
        static int RadialSegementCount = 10;

        #endregion

        #region Properties

        #region Width

        /// <summary>
        /// Width dependency property
        /// </summary>
        public static readonly DependencyProperty WidthProperty =
            DependencyProperty.Register("Width", typeof(double), typeof(Tube),
            new PropertyMetadata(0.0, OnWidthChanged));

        /// <summary>
        /// Gets or sets the width of the tube
        /// </summary>
        public double Width
        {
            get { return (double)GetValue(WidthProperty); }
            set { SetValue(WidthProperty, value); }
        }

        static void OnWidthChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            Tube tube = sender as Tube;
            tube.UpdateGeometry();
        }

        #endregion

        #region Height

        /// <summary>
        /// Height dependency property
        /// </summary>
        public static readonly DependencyProperty HeightProperty =
            DependencyProperty.Register("Height", typeof(double), typeof(Tube),
            new PropertyMetadata(0.0, OnHeightChanged));

        /// <summary>
        /// Gets or sets the height of the tube
        /// </summary>
        public double Height
        {
            get { return (double)GetValue(HeightProperty); }
            set { SetValue(HeightProperty, value); }
        }

        static void OnHeightChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            Tube tube = sender as Tube;
            tube.UpdateGeometry();
        }

        #endregion   

        #endregion

        #region Initialization

        /// <summary>
        /// Default constructor
        /// </summary>
        public Tube() { }       

        #endregion

        #region Methods

        /// <summary>
        /// Updates the geometry of the tube
        /// </summary>
        protected override void UpdateGeometry()
        {
            // Get properties of the tube
            Point3D[] centers = Centers;
            Vector3D[] verticalVectors = VerticalVectors;
            Vector3D[] horizontalVectors = HorizontalVectors;
            double halfWidth = 0.5 * Width;
            double halfHeight = 0.5 * Height;
            
            // Check the properties of the tubes
            if (!Tube.IsValid(centers, horizontalVectors, verticalVectors, halfWidth, halfHeight)) return;
            
            // Create tube geometry
            MeshGeometry3D geometry = new MeshGeometry3D();

            for (int i = 0; i < centers.Length; i++)
            {
                Point3D center = centers[i];
                Vector3D horizontalVector = horizontalVectors[i];
                Vector3D verticalVector = verticalVectors[i];
                for (int j = 0; j <= Tube.RadialSegementCount; j++)
                {
                    double normilizedAngle = (double)j / Tube.RadialSegementCount;
                    double angle = 2.0 * Math.PI * normilizedAngle;
                    double cos = Math.Cos(angle);
                    double sin = Math.Sin(angle);
                    geometry.Positions.Add(center + (halfWidth * cos) * horizontalVector + (halfHeight * sin) * verticalVector);
                    Vector3D normal = (halfHeight * cos) * horizontalVector + (halfWidth * sin) * verticalVector;
                    normal.Normalize();
                    geometry.Normals.Add(normal);
                    geometry.TextureCoordinates.Add(new Point(normilizedAngle, i / (centers.Length - 1))); 
                }
            }

            for (int i = 1; i < centers.Length; i++)
            {
                int offset = Tube.RadialSegementCount + 1;
                int index = i * offset;
                for (int j = 1; j <= Tube.RadialSegementCount; j++)
                {
                    geometry.TriangleIndices.Add(index + j);
                    geometry.TriangleIndices.Add(index + j - 1);
                    geometry.TriangleIndices.Add(index + j - 1 - offset);

                    geometry.TriangleIndices.Add(index + j);
                    geometry.TriangleIndices.Add(index + j - 1 - offset);
                    geometry.TriangleIndices.Add(index + j - offset);
                }
            }

            Geometry = geometry;
        }

        /// <summary>
        /// Creates representation of the lower cap of the primitive
        /// </summary>
        /// <returns>Primitive</returns>
        public override Primitive CreateLowerCap()
        {
            return CreateCap(true);
        }

        /// <summary>
        /// Creates representation of the upper cap of the primitive
        /// </summary>
        /// <returns>Primitive</returns>
        public override Primitive CreateUpperCap()
        {
            return CreateCap(false);
        }

        // Create the cap of primitive
        Polygon CreateCap(bool isLower)
        {
            Point3D[] centers = Centers;
            Vector3D[] horizontalVectors = HorizontalVectors;
            Vector3D[] verticalVectors = VerticalVectors;
            double halfWidth = 0.5 * Width;
            double halfHeight = 0.5 * Height;

            // Check the data of primitives
            if (!Tube.IsValid(centers, horizontalVectors, verticalVectors, halfWidth, halfHeight))
                return null;

            // Create the cap
            int index = isLower ? 0 : centers.Length - 1;
            Point3D center = centers[index];
            Vector3D horizontalVector = halfWidth * horizontalVectors[index];
            Vector3D verticalVector = halfHeight * verticalVectors[index];

            // Calculate positions
            Point3D[] positions = new Point3D[Tube.RadialSegementCount];
            for (int i = 0; i < Tube.RadialSegementCount; i++)
            {
                double angle = i * 2 * Math.PI / Tube.RadialSegementCount;
                positions[i] = center + Math.Cos(angle) * horizontalVector + Math.Sin(angle) * verticalVector;
            }

            // Reverse points for upper cap
            if (!isLower) Array.Reverse(positions);

            Polygon result = new Polygon();
            result.Positions = positions;
            // Set the same material
            result.Material = Material;
            return result;
        }

        // Check the data of the primitive
        static bool IsValid(Point3D[] trajectoryPoints,
            Vector3D[] horizontalVectors,
            Vector3D[] verticalVectors,
            double width,
            double height)
        {
            if (trajectoryPoints == null || trajectoryPoints.Length < 2)
                return false;
            if (verticalVectors == null || verticalVectors.Length != trajectoryPoints.Length)
                return false;
            if (horizontalVectors == null || horizontalVectors.Length != trajectoryPoints.Length)
                return false;
            if (width <= 0.0 || height <= 0.0)
                return false;
            return true;
        }

        #endregion    
    }
}

