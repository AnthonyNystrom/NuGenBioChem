using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace NuGenBioChem.Visualization.Primitives
{
    /// <summary>
    /// This is a class for a cylinder primitive
    /// </summary>
    public class Cylinder : Primitive
    {
        #region Meshes

        static MeshGeometry3D mesh = null;

        static MeshGeometry3D GetMesh()
        {
            if (mesh == null)
            {
                mesh = new MeshGeometry3D();
                Generate(mesh);
                mesh.Freeze();
            }
            return mesh;
        }

        #endregion

        #region Fields
        
        // Transformations
        readonly TranslateTransform3D translateTransform = new TranslateTransform3D();
        readonly ScaleTransform3D scaleTransform = new ScaleTransform3D();
        readonly MatrixTransform3D orientationTransform = new MatrixTransform3D();

        #endregion

        #region Properties

        #region Radius

        /// <summary>
        /// Gets or sets radius of the Cylinder
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
            DependencyProperty.Register("Radius", typeof(double), typeof(Cylinder), 
            new UIPropertyMetadata(1.0d, OnRadiusChanged));

        static void OnRadiusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Cylinder cylinder = (Cylinder)d;
            cylinder.scaleTransform.ScaleX =
            cylinder.scaleTransform.ScaleZ = (double)e.NewValue;
        }

        #endregion

        #region Height

        /// <summary>
        /// Gets or sets Height of the Cylinder
        /// </summary>
        public double Height
        {
            get { return (double)GetValue(HeightProperty); }
            set { SetValue(HeightProperty, value); }
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for Height.  
        /// This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty HeightProperty =
            DependencyProperty.Register("Height", typeof(double), typeof(Cylinder), 
            new UIPropertyMetadata(1.0d, OnHeightChanged));

        static void OnHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Cylinder cylinder = (Cylinder)d;
            cylinder.scaleTransform.ScaleY = (double)e.NewValue;
        }

        #endregion

        #region Position

        /// <summary>
        /// Gets or sets Position of the Cylinder
        /// </summary>
        public Point3D Position
        {
            get { return (Point3D)GetValue(PositionProperty); }
            set { SetValue(PositionProperty, value); }
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for Position.  
        /// This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty PositionProperty =
            DependencyProperty.Register("Position", typeof(Point3D), typeof(Cylinder), 
            new UIPropertyMetadata(new Point3D(), OnPositionChanged));

        static void OnPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Cylinder cylinder = (Cylinder)d;
            Point3D position = (Point3D) e.NewValue;
            cylinder.translateTransform.OffsetX = position.X;
            cylinder.translateTransform.OffsetY = position.Y;
            cylinder.translateTransform.OffsetZ = position.Z;
        }

        #endregion
 
        #region Direction

        /// <summary>
        /// Gets or sets Direction of the Cylinder
        /// </summary>
        public Vector3D Direction
        {
            get { return (Vector3D)GetValue(DirectionProperty); }
            set { SetValue(DirectionProperty, value); }
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for Direction.  
        /// This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty DirectionProperty =
            DependencyProperty.Register("Direction", typeof(Vector3D), typeof(Cylinder), 
            new UIPropertyMetadata(new Vector3D(0, 1, 0), OnDirectionChanged));

        static void OnDirectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Cylinder cylinder = (Cylinder)d;
            Vector3D direction = (Vector3D) e.NewValue;
            Matrix3D orientationMatrix = Matrix3D.Identity;
            orientationMatrix = orientationMatrix.TransformAlongTo(direction);
            cylinder.orientationTransform.Matrix = orientationMatrix;
        }

        #endregion

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public Cylinder()
        {
            GeometryTransform.Children.Add(scaleTransform);
            GeometryTransform.Children.Add(orientationTransform);
            GeometryTransform.Children.Add(translateTransform);
            
            Material = new DiffuseMaterial(Brushes.Blue);
            
            Geometry = GetMesh();
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="begin">Begin point</param>
        /// <param name="end">End point</param>
        /// <param name="radius">Radius</param>
        public Cylinder(Point3D begin, Point3D end, double radius) : this()
        {
            Position = begin;
            Vector3D direction = end - begin;
            Height = direction.Length;
            direction.Normalize();
            Direction = direction;
            Radius = radius;
        }

        #endregion

        #region Generation

         /// <summary>
        /// Generates a mesh model
        /// </summary>
        /// <param name="mesh">Mesh</param>
        static void Generate(MeshGeometry3D mesh)
        {
             const int slices = 12;
             const double slice = (2.0 * Math.PI) / (double)slices;

             for(int i = 0; i < slices; i++)
             {
                 int secondIndex = (i == slice - 1) ? 0 : i + 1;
                 Point3D topFirst = Function(1, slice * i);
                 Point3D topSecond = Function(1, slice * secondIndex);
                 Point3D bottomFirst = Function(0, slice * i);
                 Point3D bottomSecond = Function(0, slice * secondIndex);

                 Vector3D firstNormal = new Vector3D(bottomFirst.X, bottomFirst.Y, bottomFirst.Z);
                 Vector3D secondNormal = new Vector3D(bottomSecond.X, bottomSecond.Y, bottomSecond.Z);

                 /*0*/ mesh.Positions.Add(topFirst);
                 /*1*/ mesh.Positions.Add(topSecond);
                 /*2*/ mesh.Positions.Add(bottomFirst);
                 /*3*/ mesh.Positions.Add(bottomSecond);

                 mesh.Normals.Add(firstNormal);
                 mesh.Normals.Add(secondNormal);
                 mesh.Normals.Add(firstNormal);
                 mesh.Normals.Add(secondNormal);

                 mesh.TriangleIndices.Add(i * 4 + 0);
                 mesh.TriangleIndices.Add(i * 4 + 3);
                 mesh.TriangleIndices.Add(i * 4 + 1);

                 mesh.TriangleIndices.Add(i * 4 + 0);
                 mesh.TriangleIndices.Add(i * 4 + 2);
                 mesh.TriangleIndices.Add(i * 4 + 3);
             }

             mesh.GenerateCylindricalTextureCoordinates(new Vector3D(0, 1, 0));
        }

        // Cylinder equation
        static Point3D Function(double height, double angle)
        {
            Point3D result = new Point3D(
                Math.Sin(angle),
                height,
                Math.Cos(angle));

            return result;
        }

        static Vector3D dU(double u, double v)
        {
            return new Vector3D(0.0,1.0,0.0);
        }

        static Vector3D dV(double u, double v)
        {
            return new Vector3D(
                -Math.Cos(v),
                0.0,
                Math.Sin(v));
        }

        #endregion
    }
}
