using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace NuGenBioChem.Visualization.Primitives
{
    /// <summary>
    /// This is a class for a sphere primitive
    /// </summary>
    public class Sphere : Primitive
    {
        #region Meshes

        static MeshGeometry3D sphereMesh = null;

        static MeshGeometry3D GetMesh()
        {
            if (sphereMesh == null)
            {
                sphereMesh = new MeshGeometry3D();
                Generate(sphereMesh, 4);
                sphereMesh.Freeze();
            }
            return sphereMesh;
        }

        #endregion

        #region Fields
        
        readonly TranslateTransform3D translateTransform = new TranslateTransform3D();
        readonly ScaleTransform3D scaleTransform = new ScaleTransform3D();

        #endregion

        #region Properties

        #region Radius

        /// <summary>
        /// Gets or sets radius of the sphere
        /// </summary>
        public double Radius
        {
            get { return (double)GetValue(RadiusProperty); }
            set { SetValue(RadiusProperty, value); }
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for Radius.  
        /// This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty RadiusProperty =
            DependencyProperty.Register("Radius", typeof(double), typeof(Sphere), 
            new UIPropertyMetadata(1.0d, OnRadiusChanged));

        static void OnRadiusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Sphere sphere = (Sphere)d;
            sphere.scaleTransform.ScaleX =
            sphere.scaleTransform.ScaleY =
            sphere.scaleTransform.ScaleZ = (double)e.NewValue;
        }

        #endregion

        #region Center

        /// <summary>
        /// Gets or sets center of the sphere
        /// </summary>
        public Point3D Center
        {
            get { return (Point3D)GetValue(CenterProperty); }
            set { SetValue(CenterProperty, value); }
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for Center.  
        /// This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty CenterProperty =
            DependencyProperty.Register("Center", typeof(Point3D), typeof(Sphere),
            new UIPropertyMetadata(new Point3D(), OnCenterChanged));

        static void OnCenterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Sphere sphere = (Sphere)d;
            Point3D center = (Point3D)e.NewValue;
            sphere.translateTransform.OffsetX = center.X;
            sphere.translateTransform.OffsetY = center.Y;
            sphere.translateTransform.OffsetZ = center.Z;
        }

        #endregion

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public Sphere() : base()
        {
            GeometryTransform.Children.Add(scaleTransform);
            GeometryTransform.Children.Add(translateTransform);
            
            Material = new DiffuseMaterial(Brushes.Blue);
            Geometry = GetMesh();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="center">Center of the sphere</param>
        /// <param name="radius">Radius of the sphere</param>
        public Sphere(Point3D center, double radius) : this()
        {
            Center = center;
            Radius = radius;
        }

        #endregion

        #region Generation
        
        // Static index (used during tessellation)
        static int nextIndex;

        // Generates sphere with radius 1 in position (0,0,0)
        static void Generate(MeshGeometry3D mesh, int divisions)
        {
            nextIndex = 0;

            AddBaseTriangle(mesh, new Point3D(0, 0, 1), new Point3D(1, 0, 0), new Point3D(0, 1, 0));
            AddBaseTriangle(mesh, new Point3D(1, 0, 0), new Point3D(0, 0, -1), new Point3D(0, 1, 0));
            AddBaseTriangle(mesh, new Point3D(0, 0, -1), new Point3D(-1, 0, 0), new Point3D(0, 1, 0));
            AddBaseTriangle(mesh, new Point3D(-1, 0, 0), new Point3D(0, 0, 1), new Point3D(0, 1, 0));
            AddBaseTriangle(mesh, new Point3D(1, 0, 0), new Point3D(0, 0, 1), new Point3D(0, -1, 0));
            AddBaseTriangle(mesh, new Point3D(0, 0, -1), new Point3D(1, 0, 0), new Point3D(0, -1, 0));
            AddBaseTriangle(mesh, new Point3D(-1, 0, 0), new Point3D(0, 0, -1), new Point3D(0, -1, 0));
            AddBaseTriangle(mesh, new Point3D(0, 0, 1), new Point3D(-1, 0, 0), new Point3D(0, -1, 0));

            for (int division = 1; division < divisions; division++) Divide(mesh);
            mesh.GenerateSphericalTextureCoordinates(new Vector3D(1, 0, 0));
        }

        /// <summary>
        /// Helper function to create a face for the base octahedron.
        /// </summary>
        /// <param name="mesh">Mesh</param>
        /// <param name="p1">Vertex 1.</param>
        /// <param name="p2">Vertex 2.</param>
        /// <param name="p3">Vertex 3.</param>
        static void AddBaseTriangle(MeshGeometry3D mesh, Point3D p1, Point3D p2, Point3D p3)
        {
            mesh.Positions.Add(p1);
            mesh.Positions.Add(p2);
            mesh.Positions.Add(p3);

            mesh.Normals.Add(new Vector3D(p1.X, p1.Y, p1.Z));
            mesh.Normals.Add(new Vector3D(p2.X, p2.Y, p2.Z));
            mesh.Normals.Add(new Vector3D(p3.X, p3.Y, p3.Z));

            mesh.TriangleIndices.Add(nextIndex++);
            mesh.TriangleIndices.Add(nextIndex++);
            mesh.TriangleIndices.Add(nextIndex++);
        }

        /// <summary>
        /// Performs the recursive subdivision.
        /// </summary>
        /// <param name="mesh">Mesh</param>
        static void Divide(MeshGeometry3D mesh)
        {
            int indexCount = mesh.TriangleIndices.Count;

            for (int indexOffset = 0; indexOffset < indexCount; indexOffset += 3)
                DivideTriangle(mesh, indexOffset);
        }

        /// <summary>
        /// Replaces a triangle at a given index buffer offset and replaces it with four triangles
        /// that compose an equilateral subdivision.
        /// </summary>
        /// <param name="mesh">Mesh</param>
        /// <param name="indexOffset">An offset into the index buffer.</param>
        static void DivideTriangle(MeshGeometry3D mesh, int indexOffset)
        {
            int i1 = mesh.TriangleIndices[indexOffset];
            int i2 = mesh.TriangleIndices[indexOffset + 1];
            int i3 = mesh.TriangleIndices[indexOffset + 2];

            Point3D p1 = mesh.Positions[i1];
            Point3D p2 = mesh.Positions[i2];
            Point3D p3 = mesh.Positions[i3];
            Point3D p4 = GetNormalizedMidpoint(p1, p2);
            Point3D p5 = GetNormalizedMidpoint(p2, p3);
            Point3D p6 = GetNormalizedMidpoint(p3, p1);

            mesh.Positions.Add(p4);
            mesh.Positions.Add(p5);
            mesh.Positions.Add(p6);

            mesh.Normals.Add(new Vector3D(p4.X, p4.Y, p4.Z));
            mesh.Normals.Add(new Vector3D(p5.X, p5.Y, p5.Z));
            mesh.Normals.Add(new Vector3D(p6.X, p6.Y, p6.Z));

            int i4 = nextIndex++;
            int i5 = nextIndex++;
            int i6 = nextIndex++;

            mesh.TriangleIndices[indexOffset] = i4;
            mesh.TriangleIndices[indexOffset + 1] = i5;
            mesh.TriangleIndices[indexOffset + 2] = i6;

            mesh.TriangleIndices.Add(i1);
            mesh.TriangleIndices.Add(i4);
            mesh.TriangleIndices.Add(i6);

            mesh.TriangleIndices.Add(i4);
            mesh.TriangleIndices.Add(i2);
            mesh.TriangleIndices.Add(i5);

            mesh.TriangleIndices.Add(i6);
            mesh.TriangleIndices.Add(i5);
            mesh.TriangleIndices.Add(i3);
        }

        /// <summary>
        /// Calculates the midpoint between two points on a unit sphere and projects the result
        /// back to the surface of the sphere.
        /// </summary>
        /// <param name="p1">Point 1.</param>
        /// <param name="p2">Point 2.</param>
        /// <returns>The normalized midpoint.</returns>
        static Point3D GetNormalizedMidpoint(Point3D p1, Point3D p2)
        {
            Vector3D vector = new Vector3D(
                (p1.X + p2.X) / 2, (p1.Y + p2.Y) / 2, (p1.Z + p2.Z) / 2);
            vector.Normalize();

            return new Point3D(vector.X, vector.Y, vector.Z);
        }

        #endregion
    }
}
