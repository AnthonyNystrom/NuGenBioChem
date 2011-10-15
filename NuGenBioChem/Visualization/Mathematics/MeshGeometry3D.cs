using System.Collections.Generic;

namespace System.Windows.Media.Media3D
{
    /// <summary>
    /// Extension methods for MeshGeometry3D
    /// </summary>
    public static class MeshGeometry3DMethods
    {
        #region Merge
        
        /// <summary>
        /// Adds the data of the given mesh
        /// </summary>
        /// <param name="thismesh">This mesh</param>
        /// <param name="mesh">Another mesh</param>
        public static void Merdge(this MeshGeometry3D thismesh, MeshGeometry3D mesh)
        {
            for (int i = 0; i < mesh.Positions.Count; i++) thismesh.Positions.Add(mesh.Positions[i]);
            for (int i = 0; i < mesh.Normals.Count; i++) thismesh.Normals.Add(mesh.Normals[i]);
            for (int i = 0; i < mesh.TextureCoordinates.Count; i++) thismesh.TextureCoordinates.Add(mesh.TextureCoordinates[i]);
            for (int i = 0; i < mesh.TriangleIndices.Count; i++) thismesh.TriangleIndices.Add(mesh.TriangleIndices[i]);
        }

        #endregion

        #region Transform

        /// <summary>
        /// Transforms the given mesh
        /// </summary>
        /// <param name="mesh">This mesh</param>
        /// <param name="transform">Transform</param>
        public static void Transform(this MeshGeometry3D mesh, Matrix3D transform)
        {
            Matrix3D rotationOnly = transform;
            rotationOnly.OffsetX = rotationOnly.OffsetY = rotationOnly.OffsetZ = 0;

            // Transform positions
            for(int i = 0; i < mesh.Positions.Count; i++)
                mesh.Positions[i] = transform.Transform(mesh.Positions[i]);
            // Transform normals
            for (int i = 0; i < mesh.Normals.Count; i++)
                mesh.Normals[i] = rotationOnly.Transform(mesh.Normals[i]);
        }

        #endregion

        #region Clear

        /// <summary>
        /// Clears data of the given mesh
        /// </summary>
        /// <param name="mesh">This mesh</param>
        public static void Clear(this MeshGeometry3D mesh)
        {
            mesh.Positions.Clear();
            mesh.Normals.Clear();
            mesh.TextureCoordinates.Clear();
            mesh.TriangleIndices.Clear();
        }

        #endregion

        #region Texture Coordinates

        /// <summary>
        /// Generates texture coordinates as if mesh were a cylinder.
        /// </summary>
        /// <param name="mesh">The mesh</param>
        /// <param name="cylinderAxis">The axis of rotation for the cylinder</param>
        /// <returns>The generated texture coordinates</returns>
        public static void GenerateCylindricalTextureCoordinates(this MeshGeometry3D mesh, Vector3D cylinderAxis)
        {
            Rect3D bounds = mesh.Bounds;
            int count = mesh.Positions.Count;
            PointCollection texcoords = new PointCollection(count);
            IEnumerable<Point3D> positions = TransformPoints(ref bounds, mesh.Positions, ref cylinderAxis);

            mesh.TextureCoordinates.Clear();
            foreach (Point3D vertex in positions)
            {
                mesh.TextureCoordinates.Add(new Point(
                    GetUnitCircleCoordinate(-vertex.Z, vertex.X),
                    1.0 - GetPlanarCoordinate(vertex.Y, bounds.Y, bounds.SizeY)));
            }
        }

        /// <summary>
        /// Generates texture coordinates as if mesh were a sphere.
        /// </summary>
        /// <param name="mesh">The mesh</param>
        /// <param name="axisRotationSphere">The axis of rotation for the sphere</param>
        /// <returns>The generated texture coordinates</returns>
        public static void GenerateSphericalTextureCoordinates(this MeshGeometry3D mesh, Vector3D axisRotationSphere)
        {
            Rect3D bounds = mesh.Bounds;
            int count = mesh.Positions.Count;
            IEnumerable<Point3D> positions = TransformPoints(ref bounds, mesh.Positions, ref axisRotationSphere);

            mesh.TextureCoordinates.Clear();
            foreach (Point3D vertex in positions)
            {
                // Don't need to do 'vertex - center' since TransformPoints put us
                // at the origin
                Vector3D radius = new Vector3D(vertex.X, vertex.Y, vertex.Z);
                if (radius != new Vector3D()) radius.Normalize();

                mesh.TextureCoordinates.Add(new Point(
                    GetUnitCircleCoordinate(-radius.Z, radius.X),
                    1.0 - (Math.Asin(radius.Y) / Math.PI + 0.5)));
            }
        }

        /// <summary>
        /// Generates texture coordinates as if mesh were a plane.
        /// </summary>
        /// <param name="mesh">The mesh</param>
        /// <param name="planeNormal">The normal of the plane</param>
        /// <returns>The generated texture coordinates</returns>
        public static void GeneratePlanarTextureCoordinates(this MeshGeometry3D mesh, Vector3D planeNormal)
        {
            Rect3D bounds = mesh.Bounds;
            int count = mesh.Positions.Count;
            IEnumerable<Point3D> positions = TransformPoints(ref bounds, mesh.Positions, ref planeNormal);

            mesh.TextureCoordinates.Clear();
            foreach (Point3D vertex in positions)
            {
                // The plane is looking along positive Y, so Z is really Y
                mesh.TextureCoordinates.Add(new Point(
                    GetPlanarCoordinate(vertex.X, bounds.X, bounds.SizeX),
                    GetPlanarCoordinate(vertex.Z, bounds.Z, bounds.SizeZ)));
            }
        }

        static double GetPlanarCoordinate(double end, double start, double width)
        {
            return (end - start) / width;
        }

        static double GetUnitCircleCoordinate(double y, double x)
        {
            return Math.Atan2(y, x) / (2.0 * Math.PI) + 0.5;
        }

        /// <summary>
        /// Finds the transform from 'meshOrientation' to '<0, 1, 0>' and transforms 'bounds' and 'points' by it.
        /// </summary>
        /// <param name="bounds">The bounds to transform</param>
        /// <param name="points">The vertices to transform</param>
        /// <param name="meshOrientation">The orientation of the mesh</param>
        /// <returns>The transformed points. If 'meshOrientation' is already '<0, 1, 0>' then this will equal 'points.'
        /// </returns>
        static IEnumerable<Point3D> TransformPoints(ref Rect3D bounds, Point3DCollection points, ref Vector3D meshOrientation)
        {
            Vector3D yAxis = new Vector3D(0, 1, 0);
            Vector3D xAxis = new Vector3D(1, 0, 0);

            if (meshOrientation == yAxis)
            {
                return points;
            }

            Vector3D rotAxis = Vector3D.CrossProduct(meshOrientation, yAxis);
            double rotAngle = Vector3D.AngleBetween(meshOrientation, yAxis);
            Quaternion q;

            if (rotAxis.X != 0 || rotAxis.Y != 0 || rotAxis.Z != 0)
            {
                q = new Quaternion(rotAxis, rotAngle);
            }
            else
            {
                q = new Quaternion(xAxis, rotAngle);
            }

            Vector3D center = new Vector3D(
                bounds.X + bounds.SizeX / 2,
                bounds.Y + bounds.SizeY / 2,
                bounds.Z + bounds.SizeZ / 2);

            Matrix3D t = Matrix3D.Identity;
            t.Translate(-center);
            t.Rotate(q);

            int count = points.Count;
            Point3D[] transformedPoints = new Point3D[count];

            for (int i = 0; i < count; i++)
            {
                transformedPoints[i] = t.Transform(points[i]);
            }

            // Finally, transform the bounds too
            bounds = bounds.Transform(t);

            return transformedPoints;
        }

        #endregion

        #region Saving (for debugging)

        /// <summary>
        /// Saves the mesh to wavefront(.obj) file format that allows
        /// to analyze the geometry with other very powerful tools
        /// </summary>
        /// <param name="mesh">Mesh geometry</param>
        /// <param name="path">Filename</param>
        public static void SaveToWavefrontObj(this MeshGeometry3D mesh, string path)
        {
            using (var writer = new IO.StreamWriter(path, false, Text.Encoding.ASCII))
            {
                var format = System.Globalization.CultureInfo.InvariantCulture;
                foreach (Point3D position in mesh.Positions)
                {                    
                    writer.WriteLine("v {0} {1} {2}",
                        position.X.ToString(format),
                        position.Y.ToString(format),
                        position.Z.ToString(format)); 
                }
                foreach (Vector3D normals in mesh.Normals)
                {                   
                    writer.WriteLine("vn {0} {1} {2}",
                        normals.X.ToString(format),
                        normals.Y.ToString(format),
                        normals.Z.ToString(format));
                }
                foreach (Point textureCoordinate in mesh.TextureCoordinates)
                {
                    writer.WriteLine("vt {0} {1}",
                        textureCoordinate.X.ToString(format),
                        textureCoordinate.Y.ToString(format));
                }
                string formatString;
                if (mesh.TextureCoordinates.Count == mesh.Positions.Count)
                {
                    if (mesh.Normals.Count == mesh.Positions.Count) formatString = "f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}";
                    else formatString = "f {0}/{0} {1}/{1} {2}/{2}";
                }
                else
                {
                    if (mesh.Normals.Count == mesh.Positions.Count) formatString = "f {0}//{0} {1}//{1} {2}//{2}";
                    else formatString = "f {0} {1} {2}";
                }
                for (int i = 2; i < mesh.TriangleIndices.Count; i += 3)
                {
                    writer.WriteLine(formatString,
                        (mesh.TriangleIndices[i - 2] + 1).ToString(format),
                        (mesh.TriangleIndices[i - 1] + 1).ToString(format),
                        (mesh.TriangleIndices[i] + 1).ToString(format));
                }
            }
        }

        #endregion
    }
}
