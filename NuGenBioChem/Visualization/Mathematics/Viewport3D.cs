using System.Diagnostics;
using System.Windows.Controls;

namespace System.Windows.Media.Media3D
{
    /// <summary>
    /// Extension methods for Viewport3D class
    /// </summary>
    public static class Viewport3DMethods
    {
        /// <summary>
        /// Computes the transform from world space to the Viewport3D 2D space.
        /// </summary>
        public static Matrix3D Get3DTo2DTransform(this Viewport3D viewport3D)
        {
            // We need a Viewport3DVisual but we only have a Viewport3D. 
            Viewport3DVisual viewport3DVisual = VisualTreeHelper.GetParent(viewport3D.Children[0]) as Viewport3DVisual;
            if (viewport3DVisual == null || viewport3DVisual.Camera == null || viewport3DVisual.Viewport == Rect.Empty)
                return Matrix3D.Identity;

            // Apply camera inverted transform
            Matrix3D result = Matrix3D.Identity;
            if (viewport3DVisual.Camera.Transform != null)
            {
                Matrix3D m = viewport3DVisual.Camera.Transform.Value;
                if (!m.HasInverse)
                {
                    return Matrix3D.Identity;
                }
                m.Invert();
                result.Append(m);
            }

            // Apply camera view matrix
            result.Append(viewport3DVisual.Camera.GetViewMatrix());

            // Apply camera projection matrix
            result.Append(viewport3DVisual.Camera.GetProjectionMatrix(GetAspectRatio(viewport3DVisual.Viewport.Size)));

            // Apply viewport size transform
            double scaleX = viewport3DVisual.Viewport.Width / 2;
            double scaleY = viewport3DVisual.Viewport.Height / 2;
            double offsetX = viewport3DVisual.Viewport.X + scaleX;
            double offsetY = viewport3DVisual.Viewport.Y + scaleY;
            Matrix3D homogeneousToViewportTransform = new Matrix3D(
                 scaleX, 0, 0, 0,
                 0, -scaleY, 0, 0,
                 0, 0, 1, 0,
                 offsetX, offsetY, 0, 1);
            result.Append(homogeneousToViewportTransform);

            return result;
        }

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
