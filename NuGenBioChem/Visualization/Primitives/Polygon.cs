using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace NuGenBioChem.Visualization.Primitives
{
    /// <summary>
    /// This is a class for a polygon primitive
    /// </summary>
    public class Polygon : Primitive
    {
        #region Fields
        

        #endregion

        #region Properties

        #region Points

        /// <summary>
        /// Gets or sets points of the polygon
        /// </summary>
        public Point3D[] Positions
        {
            get { return (Point3D[])GetValue(PositionsProperty); }
            set { SetValue(PositionsProperty, value); }
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for Points.  
        /// This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty PositionsProperty =
            DependencyProperty.Register("Points", typeof(Point3D[]), typeof(Polygon), 
            new UIPropertyMetadata(new Point3D[0], OnPointsChanged));

        static void OnPointsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Polygon polygon = (Polygon)d;
            polygon.UpdatePositions();
        }

        #endregion

        #region TextureCoordinates

        /// <summary>
        /// Gets or sets texture coordinates
        /// </summary>
        public Point[] TextureCoordinates
        {
            get { return (Point[])GetValue(TextureCoordinatesProperty); }
            set { SetValue(TextureCoordinatesProperty, value); }
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for TextureCoordinates.  
        /// This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty TextureCoordinatesProperty =
            DependencyProperty.Register("TextureCoordinates", typeof(Point[]), typeof(Polygon), 
            new UIPropertyMetadata(new Point[0], OnTextureCoordinatesChanged));

        static void OnTextureCoordinatesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Polygon polygon = (Polygon)d;
            polygon.UpdateTextureCoordinates();
        }

        #endregion

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public Polygon()
        {
            Geometry = new MeshGeometry3D();   
        }

        #endregion

        #region Generation

        void UpdatePositions()
        {
            Point3D[] points = Positions;
            MeshGeometry3D meshGeometry3D = (MeshGeometry3D) Geometry;
            meshGeometry3D.Positions.Clear();
            meshGeometry3D.Normals.Clear();


            if (points.Length < 3)
            {
                Geometry = null;
                return;
            }

            Vector3D normal = Vector3D.CrossProduct(points[0] - points[2], points[0] - points[1]).GetUnit();
            for (int i = 0; i < points.Length; i++)
            {
                meshGeometry3D.Positions.Add(points[i]);
                meshGeometry3D.Normals.Add(normal);
                if (i >= 2)
                {
                    meshGeometry3D.TriangleIndices.Add(0);
                    meshGeometry3D.TriangleIndices.Add(i);
                    meshGeometry3D.TriangleIndices.Add(i-1);
                }
            }
        }

        void UpdateTextureCoordinates()
        {
            Point3D[] points = Positions;
            MeshGeometry3D meshGeometry3D = (MeshGeometry3D)Geometry;
            meshGeometry3D.TextureCoordinates.Clear();

            Point[] textureCoordinates = TextureCoordinates;
            for (int i = 0; i < textureCoordinates.Length; i++) 
                meshGeometry3D.TextureCoordinates.Add(textureCoordinates[i]);
        }

        #endregion
    }
}
