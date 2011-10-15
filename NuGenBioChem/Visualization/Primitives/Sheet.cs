using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace NuGenBioChem.Visualization.Primitives
{
    /// <summary>
    /// Represents a sheet primitive.
    /// The sheet is an extruded primitive with rectangular profile
    /// </summary>
    public sealed class Sheet : ExtrudedSurface 
    {
        #region Properties

        #region Width

        /// <summary>
        /// Width - dependency property
        /// </summary>
        public static readonly DependencyProperty WidthProperty =
            DependencyProperty.Register("Width", typeof(double), typeof(Sheet),
            new PropertyMetadata(0.0, OnWidthChanged));

        /// <summary>
        /// Gets or sets the width of the sheet
        /// </summary>
        public double Width
        {
            get { return (double)GetValue(WidthProperty); }
            set { SetValue(WidthProperty, value); } 
        }       

        static void OnWidthChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            Sheet sheet = (Sheet)sender;
            sheet.UpdateGeometry(); 
        }

        #endregion  

        #region Height

        /// <summary>
        /// Height dependency property
        /// </summary>
        public static readonly DependencyProperty HeightProperty =
            DependencyProperty.Register("Height", typeof(double), typeof(Sheet),
            new PropertyMetadata(0.0, OnHeightChanged));

        /// <summary>
        /// Gets or sets the height of the sheet
        /// </summary>
        public double Height
        {
            get { return (double)GetValue(HeightProperty); }
            set { SetValue(HeightProperty, value); }
        }

        static void OnHeightChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            Sheet sheet = (Sheet)sender;
            sheet.UpdateGeometry();
        }

        #endregion  

        #region Arrow

        public static readonly DependencyProperty IsArrowProperty =
            DependencyProperty.Register("IsArrow", typeof(bool), typeof(Sheet),
            new PropertyMetadata(false, OnIsArrowChanged));

        /// <summary>
        /// Gets of sets this sheet as arrow
        /// </summary>
        public bool IsArrow
        {
            get { return (bool)GetValue(IsArrowProperty); }
            set { SetValue(IsArrowProperty, value); } 
        }

        static void OnIsArrowChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            Sheet sheet = sender as Sheet;
            sheet.UpdateGeometry();
        }

        #endregion

        #endregion

        #region Initialization

        /// <summary>
        /// Default constructor
        /// </summary>
        public Sheet() 
        {
            Geometry = new MeshGeometry3D(); 
        }
       
        #endregion

        #region Methods

        /// <summary>
        /// Updates geometry of the sheet
        /// </summary>
        protected override void UpdateGeometry()
        {
            MeshGeometry3D geometry = (MeshGeometry3D)this.Geometry;
            geometry.Clear();             
          
            // Get trajectory of the sheet sweep surface
            Point3D[] centers = Centers;
            Vector3D[] horizontalVectors = HorizontalVectors;
            Vector3D[] verticalVectors = VerticalVectors;

            // Sheet parameters
            double halfWidth = 0.5 * Width;
            double halfHeight = 0.5 * Height;            

            // Check the data of the primitive
            if (!IsValid(centers, horizontalVectors, verticalVectors, halfWidth, halfHeight)) return;   

            bool isArrow = IsArrow;

            // Calculate positions            
            for (int i = 0; i < centers.Length; i++)
            {
                Point3D center = centers[i];

                double actualWidth = isArrow ? (centers.Length - 1 - i) * halfWidth / (centers.Length - 1) : halfWidth;

                Vector3D horizontalVector = actualWidth * horizontalVectors[i];
                Vector3D verticalVector = halfHeight * verticalVectors[i];

                double normilizedHeight = i / (centers.Length - 1.0); 

                geometry.Positions.Add(center + horizontalVector - verticalVector);
                geometry.Normals.Add(horizontalVectors[i]);
                geometry.TextureCoordinates.Add(new Point(0.0, normilizedHeight)); 
                geometry.Positions.Add(center + horizontalVector + verticalVector);
                geometry.Normals.Add(horizontalVectors[i]);
                geometry.TextureCoordinates.Add(new Point(1.0,normilizedHeight));

                geometry.Positions.Add(center + horizontalVector + verticalVector);
                geometry.Normals.Add(verticalVectors[i]);
                geometry.TextureCoordinates.Add(new Point(0.0,normilizedHeight));
                geometry.Positions.Add(center - horizontalVector + verticalVector);
                geometry.Normals.Add(verticalVectors[i]);
                geometry.TextureCoordinates.Add(new Point(1.0,normilizedHeight));

                geometry.Positions.Add(center - horizontalVector + verticalVector);
                geometry.Normals.Add(-horizontalVectors[i]);
                geometry.TextureCoordinates.Add(new Point(0.0, normilizedHeight));
                geometry.Positions.Add(center - horizontalVector - verticalVector);
                geometry.Normals.Add(-horizontalVectors[i]);
                geometry.TextureCoordinates.Add(new Point(1.0, normilizedHeight));

                geometry.Positions.Add(center - horizontalVector - verticalVector);
                geometry.Normals.Add(-verticalVectors[i]);
                geometry.TextureCoordinates.Add(new Point(0.0, normilizedHeight));
                geometry.Positions.Add(center + horizontalVector - verticalVector);
                geometry.Normals.Add(-verticalVectors[i]);
                geometry.TextureCoordinates.Add(new Point(1.0, normilizedHeight));
            }
            
            // Add triangles
            for (int i = 1; i < centers.Length; i++)
            {
                int index = 8 * i;
                for (int j = 0; j < 4; j++)
                {
                    geometry.TriangleIndices.Add(index);
                    geometry.TriangleIndices.Add(index - 8);
                    geometry.TriangleIndices.Add(index - 7);

                    geometry.TriangleIndices.Add(index);
                    geometry.TriangleIndices.Add(index - 7);
                    geometry.TriangleIndices.Add(index + 1);
                    
                    index += 2;
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
            return IsArrow ? null : CreateCap(false);
        }

        // Create the cap of sheet primitive
        Polygon CreateCap(bool isLower)
        {
            Point3D[] centers = Centers;
            Vector3D[] horizontalVectors = HorizontalVectors;
            Vector3D[] verticalVectors = VerticalVectors;
            double halfWidth = 0.5 * Width;
            double halfHeight = 0.5 * Height;

            // Check the data of premitives
            if(!Sheet.IsValid(centers, horizontalVectors, verticalVectors, halfWidth, halfHeight))
                return null;
            
            // Create the cap
            int index = isLower ? 0 : centers.Length - 1;
            Point3D center = centers[index];
            Vector3D horizontalVector = halfWidth * horizontalVectors[index];
            Vector3D verticalVector = halfHeight * verticalVectors[index]; 

            Point3D[] positions = new Point3D[4];
            positions[0] = center - horizontalVector + verticalVector;
            positions[1] = center - horizontalVector - verticalVector;
            positions[2] = center + horizontalVector - verticalVector;
            positions[3] = center + horizontalVector + verticalVector;

            // Reverse points for lower cap
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
