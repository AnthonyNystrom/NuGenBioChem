namespace System.Windows.Media.Media3D
{
    /// <summary>
    /// Extension methods for MeshGeometry3D
    /// </summary>
    public static class Rect3DExtensions
    {
        /// <summary>
        /// Transforms the axis-aligned bounding box 'bounds' by 'transform'
        /// </summary>
        /// <param name="bounds">The AABB to transform</param>
        /// <returns>Transformed AABB</returns>
        public static Rect3D Transform(this Rect3D bounds, Matrix3D transform)
        {
            double x1 = bounds.X;
            double y1 = bounds.Y;
            double z1 = bounds.Z;
            double x2 = bounds.X + bounds.SizeX;
            double y2 = bounds.Y + bounds.SizeY;
            double z2 = bounds.Z + bounds.SizeZ;

            Point3D[] points = new Point3D[] {
                new Point3D(x1, y1, z1),
                new Point3D(x1, y1, z2),
                new Point3D(x1, y2, z1),
                new Point3D(x1, y2, z2),
                new Point3D(x2, y1, z1),
                new Point3D(x2, y1, z2),
                new Point3D(x2, y2, z1),
                new Point3D(x2, y2, z2),
            };

            transform.Transform(points);

            // reuse the 1 and 2 variables to stand for smallest and largest
            Point3D p = points[0];
            x1 = x2 = p.X;
            y1 = y2 = p.Y;
            z1 = z2 = p.Z;

            for (int i = 1; i < points.Length; i++)
            {
                p = points[i];

                x1 = Math.Min(x1, p.X); y1 = Math.Min(y1, p.Y); z1 = Math.Min(z1, p.Z);
                x2 = Math.Max(x2, p.X); y2 = Math.Max(y2, p.Y); z2 = Math.Max(z2, p.Z);
            }

            return new Rect3D(x1, y1, z1, x2 - x1, y2 - y1, z2 - z1);
        }

        /// <summary>
        /// Computes the center of 'box'
        /// </summary>
        /// <param name="box">The Rect3D we want the center of</param>
        /// <returns>The center point</returns>
        public static Point3D GetCenter(this Rect3D box)
        {
            return new Point3D(box.X + box.SizeX / 2, box.Y + box.SizeY / 2, box.Z + box.SizeZ / 2);
        }
    }
}
