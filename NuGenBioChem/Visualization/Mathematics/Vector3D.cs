namespace System.Windows.Media.Media3D
{
    /// <summary>
    /// Extension methods for MeshGeometry3D
    /// </summary>
    public static class Vector3DExtensions
    {
        /// <summary>
        /// Adds the data of the given mesh
        /// </summary>
        /// <param name="v">This vector</param>
        /// <param name="u">Another vector</param>
        /// <param name="tolerance">Tolerance</param>
        public static bool ApproxEqual(this Vector3D v, Vector3D u, double tolerance)
        {
            return  (Math.Abs(v.X - u.X) <= tolerance) &&
                    (Math.Abs(v.Y - u.Y) <= tolerance) &&
                    (Math.Abs(v.Z - u.Z) <= tolerance);
        }

        /// <summary>
        /// Gets normalized vector
        /// </summary>
        /// <param name="v">Vector</param>
        /// <returns>Normalized vector</returns>
        public static Vector3D GetUnit(this Vector3D v)
        {
            v.Normalize();
            return v;
        }
    }
}
