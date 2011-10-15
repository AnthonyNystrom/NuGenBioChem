
using System.Diagnostics;

namespace System.Windows.Media.Media3D
{
    /// <summary>
    /// Extension methods for camera class
    /// </summary>
    public static class CameraMethods
    {
        #region GetProjectionMatrix

        /// <summary>
        /// Computes the effective projection matrix for the given camera.
        /// </summary>
        /// <param name="camera">Camera</param>
        /// <param name="aspectRatio">Aspect ratio</param>
        public static Matrix3D GetProjectionMatrix(this Camera camera, double aspectRatio)
        {
            if (camera == null)
            {
                throw new ArgumentNullException("camera");
            }

            PerspectiveCamera perspectiveCamera = camera as PerspectiveCamera;

            if (perspectiveCamera != null)
            {
                return GetProjectionMatrix(perspectiveCamera, aspectRatio);
            }

            OrthographicCamera orthographicCamera = camera as OrthographicCamera;

            if (orthographicCamera != null)
            {
                return GetProjectionMatrix(orthographicCamera, aspectRatio);
            }

            MatrixCamera matrixCamera = camera as MatrixCamera;

            if (matrixCamera != null)
            {
                return matrixCamera.ProjectionMatrix;
            }

            throw new ArgumentException(String.Format("Unsupported camera type '{0}'.", camera.GetType().FullName), "camera");
        }
        
        static Matrix3D GetProjectionMatrix(OrthographicCamera camera, double aspectRatio)
        {
            Debug.Assert(camera != null,
                "Caller needs to ensure camera is non-null.");

            // This math is identical to what you find documented for
            // D3DXMatrixOrthoRH with the exception that in WPF only
            // the camera's width is specified.  Height is calculated
            // from width and the aspect ratio.

            double w = camera.Width;
            double h = w / aspectRatio;
            double zn = camera.NearPlaneDistance;
            double zf = camera.FarPlaneDistance;

            double m33 = 1 / (zn - zf);
            double m43 = zn * m33;

            return new Matrix3D(
                2 / w, 0, 0, 0,
                  0, 2 / h, 0, 0,
                  0, 0, m33, 0,
                  0, 0, m43, 1);
        }

        static Matrix3D GetProjectionMatrix(PerspectiveCamera camera, double aspectRatio)
        {
            // This math is identical to what you find documented for
            // D3DXMatrixPerspectiveFovRH with the exception that in
            // WPF the camera's horizontal rather the vertical
            // field-of-view is specified.

            double hFoV = DegreesToRadians(camera.FieldOfView);
            double zn = camera.NearPlaneDistance;
            double zf = camera.FarPlaneDistance;

            double xScale = 1 / Math.Tan(hFoV / 2);
            double yScale = aspectRatio * xScale;
            double m33 = (zf == double.PositiveInfinity) ? -1 : (zf / (zn - zf));
            double m43 = zn * m33;

            return new Matrix3D(
                xScale, 0, 0, 0,
                     0, yScale, 0, 0,
                     0, 0, m33, -1,
                     0, 0, m43, 0);
        }

        #endregion

        #region GetViewMatrix

        /// <summary>
        /// Computes the effective view matrix for the given camera.
        /// </summary>
        /// <param name="camera">Camera</param>
        public static Matrix3D GetViewMatrix(this Camera camera)
        {
            if (camera == null)
            {
                throw new ArgumentNullException("camera");
            }

            MatrixCamera matrixCamera = camera as MatrixCamera;
            if (matrixCamera != null)
            {
                return matrixCamera.ViewMatrix;
            }

            ProjectionCamera projectionCamera = camera as ProjectionCamera;
            if (projectionCamera != null)
            {
                // This math is identical to what you find documented for
                // D3DXMatrixLookAtRH with the exception that WPF uses a
                // LookDirection vector rather than a LookAt point.

                Vector3D zAxis = -projectionCamera.LookDirection;
                zAxis.Normalize();

                Vector3D xAxis = Vector3D.CrossProduct(projectionCamera.UpDirection, zAxis);
                xAxis.Normalize();

                Vector3D yAxis = Vector3D.CrossProduct(zAxis, xAxis);

                Vector3D position = (Vector3D)projectionCamera.Position;
                double offsetX = -Vector3D.DotProduct(xAxis, position);
                double offsetY = -Vector3D.DotProduct(yAxis, position);
                double offsetZ = -Vector3D.DotProduct(zAxis, position);

                return new Matrix3D(
                    xAxis.X, yAxis.X, zAxis.X, 0,
                    xAxis.Y, yAxis.Y, zAxis.Y, 0,
                    xAxis.Z, yAxis.Z, zAxis.Z, 0,
                    offsetX, offsetY, offsetZ, 1);
            }

            throw new ArgumentException(String.Format("Unsupported camera type '{0}'.", camera.GetType().FullName), "camera");
        }


        #endregion

        #region Private Methods

        static double GetAspectRatio(Size size)
        {
            return size.Width / size.Height;
        }

        static double DegreesToRadians(double degrees)
        {
            return degrees * (Math.PI / 180.0);
        }

        #endregion
    }
}
