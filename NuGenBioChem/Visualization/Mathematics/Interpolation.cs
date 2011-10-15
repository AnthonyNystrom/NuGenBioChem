using System;
using System.Windows.Media.Media3D;

namespace NuGenBioChem.Visualization
{
    /// <summary>
    /// Provides methods for smoothing point data
    /// </summary>
    public static class Interpolation
    {
        /// <summary>
        /// Gets the linear interpolation point between the given points
        /// </summary>
        /// <param name="p1">First point (0.0 parameter value)</param>
        /// <param name="p2">Second point (1.0 parameter value)</param>
        /// <param name="t">Parameter value</param>
        /// <returns>Interpolated point</returns>
        public static Point3D Line(Point3D p1, Point3D p2, double t)
        {
            double s = 1.0 - t;
            return new Point3D(s * p1.X + t * p2.X,
                s * p1.Y + t * p2.Y,
                s * p1.Z + t * p2.Z);
        }

        /// <summary>
        /// Gets the point of the quadratic curve passes through the given points
        /// </summary>
        /// <param name="p1">First parabola point (0.0 parameter value)</param>
        /// <param name="p2">Second parabola point (0.5 parameter value)</param>
        /// <param name="p3">Third parabola point (1.0 parameter value)</param>
        /// <param name="t">Parameter value</param>
        /// <returns>Interpolated point</returns>
        public static Point3D QuadraticCurve(Point3D p1, Point3D p2, Point3D p3, double t)
        {
            double t1 = 1.0 - t * (3.0 - 2.0 * t);
            double t2 = 4.0 * t * (1.0 - t);
            double t3 = t * (2.0 * t - 1.0);
            return new Point3D(t1 * p1.X + t2 * p2.X + t3 * p3.X,
                t1 * p1.Y + t2 * p2.Y + t3 * p3.Y,
                t1 * p1.Z + t2 * p2.Z + t3 * p3.Z);
        }

        /// <summary>
        /// Gets the point of the cubic curve passes through given points
        /// </summary>
        /// <param name="p1">First point (0.0 parameter value)</param>
        /// <param name="p2">Second point (1/3 parameter value)</param>
        /// <param name="p3">Third point (2/3 parameter value)</param>
        /// <param name="p4">Fourth point (1.0 parameter value)</param>
        /// <param name="t">Parameter value</param>
        /// <returns>Interpolated Point</returns>
        public static Point3D CubicCurve(Point3D p1, Point3D p2, Point3D p3, Point3D p4, double t)
        {
            double t1 = t * (t * (-4.5 * t + 9.0) - 5.5) + 1.0;
            double t2 = t * (t * (13.5 * t - 22.5) + 9.0);
            double t3 = t * (t * (-13.5 * t + 18.0) - 4.5);
            double t4 = t * (t * (4.5 * t - 4.5) + 1.0);
            return new Point3D(t1 * p1.X + t2 * p2.X + t3 * p3.X + t4 * p4.X,
                t1 * p1.Y + t2 * p2.Y + t3 * p3.Y + t4 * p4.Y,
                t1 * p1.Z + t2 * p2.Z + t3 * p3.Z + t4 * p4.Z);
        }

        /// <summary>
        /// Interpolates points by blending two parabolas (p1-p2-p3 and p2-p3-p4)
        /// on the segment p2-p3 with linear interpolation
        /// </summary>
        /// <param name="p1">First point</param>
        /// <param name="p2">Second point (0.0 parameter value)</param>
        /// <param name="p3">Third point (1.0 parameter value)</param>
        /// <param name="p4">Fourth point</param>
        /// <param name="t">Parameter value</param>
        /// <returns>Interpolated point</returns>
        public static Point3D QuadraticBlend(Point3D p1, Point3D p2, Point3D p3, Point3D p4, double t)
        {
            return Line(
                QuadraticCurve(p1, p2, p3, 0.5 + 0.5 * t),
                QuadraticCurve(p2, p3, p4, 0.5 * t), 
                t);
        }

        /// <summary>
        /// Gets the point of the Hermite curve segment with the specified
        /// end positions and the derivative values
        /// </summary>
        /// <param name="p1">Position of the segment start (0.0 parameter value)</param>
        /// <param name="p2">Position of the segment end (1.0 parameter value)</param>
        /// <param name="d1">Derivatives at the segment start</param>
        /// <param name="d2">Derivatives at the segment end</param>
        /// <param name="t">Parameter value</param>
        /// <returns>Interpolated point</returns>
        public static Point3D HermiteCurve(Point3D p1, Point3D p2, Vector3D d1, Vector3D d2, double t)
        {
            double tp1 = 1.0 + t * t * (2.0 * t - 3.0);
            double tp2 = t * t * (3.0 - 2.0 * t);
            double td1 = t + t * t * (t - 2.0);
            double td2 = t * t * (t - 1.0);
            return new Point3D(tp1 * p1.X + tp2 * p2.X + td1 * d1.X + td2 * d2.X,
                tp1 * p1.Y + tp2 * p2.Y + td1 * d1.Y + td2 * d2.Y,
                tp1 * p1.Z + tp2 * p2.Z + td1 * d1.Z + td2 * d2.Z);
        }      
    }
}
