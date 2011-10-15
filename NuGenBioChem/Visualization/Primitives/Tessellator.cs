using System;
using System.Collections.Generic;
using System.Windows.Media.Media3D;

namespace NuGenBioChem.Visualization.Primitives
{
    /// <summary>
    /// This is a class which performs 
    /// tesselation with the given parameters
    /// </summary>
    public class Tessellator
    {
        #region Properties

        /// <summary>
        /// The main parametrized function
        /// </summary>
        public Func<double, double, Point3D> Function = null;

        /// <summary>
        /// The derivative of the function by U.
        /// It is required for noramals calculation
        /// </summary>
        public Func<double, double, Vector3D> DerivativeU = null;


        /// <summary>
        /// The derivative of the function by V.
        /// It is required for noramals calculation
        /// </summary>
        public Func<double, double, Vector3D> DerivativeV = null;

        /// <summary>
        /// Resolution of tesselation by U
        /// </summary>
        public double dU = 0.3;

        /// <summary>
        /// Resolution of tesselation by V
        /// </summary>
        public double dV = 0.3;

        /// <summary>
        /// The start value of U
        /// </summary>
        public double StartU = 0;

        /// <summary>
        /// The start value of V
        /// </summary>
        public double StartV = 0;
        
        /// <summary>
        /// The end value of U
        /// </summary>
        public double EndU = 1;

        /// <summary>
        /// The end value of V
        /// </summary>
        public double EndV = 1;

        /// <summary>
        /// Are noramls required to generate?
        /// </summary>
        public bool NeedNormals = true;
        
        #endregion

        #region Methods

        /// <summary>
        /// Performs tesselation
        /// </summary>
        /// <returns>mesh object</returns>
        public void Tesselate(MeshGeometry3D mesh)
        {
            // The function is required, the derivative can be missed
            if (Function == null)
            {
                throw new ArgumentException("Tesselation requires the function, please set the proper field");
            }
            // Start values must be lower then end values
            if ((StartU > EndU)||(StartV > EndV))
            {
                throw new ArgumentException("Tesselation required that start values must be lower then end values");
            }
            
            // Recalculate dU and dV in order to get reduction ratio
            int numberU = (int)((EndU - StartU) / dU);
            int numberV = (int)((EndV - StartV) / dV);
            double dUrecalulated = dU + ((EndU - StartU) - dU * numberU) / numberU;
            double dVrecalulated = dV + ((EndV - StartV) - dV * numberV) / numberV;

            // Validating
            if ((numberU == 0) || (numberV == 0)) throw new ArgumentException("Tessellator error. Check the end, start and dU/dV parameters, (EndX - StartX) / dX = 0");

            // Generating vertices
            double u = StartU - dUrecalulated;
            do
            {
                u += dUrecalulated;
                double v = StartV - dVrecalulated;
                do
                {
                    v += dVrecalulated;
                    mesh.Positions.Add(GetPosition(u, v));
                    mesh.Normals.Add(GetNormal(u, v));
                } 
                while (v + 0.0005 < EndV);
            }
            while (u + 0.0005 < EndU);

            // Generating faces
            for(int i = 0; i < numberU; i++)
            {
                for (int j = 0; j < numberV; j++)
                {
                    // Clockwise enumeration (!)
                    mesh.TriangleIndices.Add((i + 0) * (numberV + 1) + (j + 1));
                    mesh.TriangleIndices.Add((i + 0) * (numberV + 1) + (j + 0));
                    mesh.TriangleIndices.Add((i + 1) * (numberV + 1) + (j + 1));
                    mesh.TriangleIndices.Add((i + 1) * (numberV + 1) + (j + 0));
                    mesh.TriangleIndices.Add((i + 1) * (numberV + 1) + (j + 1));
                    mesh.TriangleIndices.Add((i + 0) * (numberV + 1) + (j + 0));
                }
            }
        }

        /// <summary>
        /// Tesselates only a cap using U parameter
        /// </summary>
        /// <param name="fixedV">A fixed value of V</param>
        /// <param name="faceInversion">Inversion of faces</param>
        /// <param name="mesh">Mesh</param>
        /// <returns>mesh object</returns>
        public void GetCapU(MeshGeometry3D mesh, double fixedV, bool faceInversion)
        {
            GetCap(mesh, 1, fixedV, faceInversion);
        }

        /// <summary>
        /// Tesselates only a cap using V parameter
        /// </summary>
        /// <param name="fixedU">A fixed value of U</param>
        /// <param name="faceInversion">Inversion of faces</param>
        /// <param name="mesh">Mesh</param>
        /// <returns>mesh object</returns>
        public void GetCapV(MeshGeometry3D mesh, double fixedU, bool faceInversion)
        {
            GetCap(mesh, 2, fixedU, faceInversion);
        }

        /// <summary>
        /// Tesselates only a cap using U parameter
        /// </summary>
        /// <param name="parameter">1 - u, 2 - v</param>
        /// <param name="fixedValue">A fixed value on other parameter</param>
        /// <param name="faceInversion">Inversion of faces</param>
        /// <param name="mesh">mesh mesh</param>
        void GetCap(MeshGeometry3D mesh, int parameter, double fixedValue, bool faceInversion)
        {
            // The function is required, the derivative can be missed
            if (Function == null)
            {
                throw new ArgumentException("Tesselation requires the function, please set the proper field");
            }
            // Start values must be lower then end values
            if ((StartU > EndU) || (StartV > EndV))
            {
                throw new ArgumentException("Tesselation required that start values must be lower then end values");
            }


            // Recalculate dU and dV in order to get reduction ratio
            int numberU = (int)((EndU - StartU) / dU);
            int numberV = (int)((EndV - StartV) / dV);
            double dUrecalulated = dU + ((EndU - StartU) - dU * numberU) / numberU;
            double dVrecalulated = dV + ((EndV - StartV) - dV * numberV) / numberV;

            // Generating vertices
            double t = (parameter == 1) ? StartU - dUrecalulated : StartV - dVrecalulated;
            do
            {
                t += (parameter == 1) ? dUrecalulated : dVrecalulated;
                if (parameter == 1)
                {
                    mesh.Positions.Add(GetPosition(t, fixedValue));
                    mesh.Normals.Add(GetNormal(t, fixedValue));
                }
                else
                {
                    mesh.Positions.Add(GetPosition(fixedValue, t));
                    mesh.Normals.Add(GetNormal(fixedValue, t));
                }
            }
            while (t + 0.0005 < ((parameter == 1) ? EndU : EndV));
            
            // Correcting normals
            if (faceInversion)
            {
                Vector3D normal = Vector3D.CrossProduct(
                    mesh.Positions[0] - mesh.Positions[1], 
                    mesh.Positions[1] - mesh.Positions[2]);
                for (int i = 0; i < mesh.Positions.Count; i++)
                {
                    mesh.Normals[i].Normalize();
                }


                for (int i = 1; i < mesh.Positions.Count - 1; i++)
                {
                    mesh.TriangleIndices.Add(0);
                    mesh.TriangleIndices.Add(i + 1);
                    mesh.TriangleIndices.Add(i);
                }
            }
            else
            {
                Vector3D normal = Vector3D.CrossProduct(
                    mesh.Positions[0] - mesh.Positions[1],
                    mesh.Positions[2] - mesh.Positions[1]);
                for (int i = 0; i < mesh.Positions.Count; i++)
                {
                    mesh.Normals[i].Normalize();
                }

                for (int i = 1; i < mesh.Positions.Count - 1; i++)
                {
                    mesh.TriangleIndices.Add(0);
                    mesh.TriangleIndices.Add(i);
                    mesh.TriangleIndices.Add(i + 1);
                }
            }
        }

        Vector3D GetNormal(double u, double v)
        {
            Vector3D derivativeU = DerivativeU(u, v);
            Vector3D derivativeV = DerivativeV(u, v);
            
            Vector3D crossProduct = Vector3D.CrossProduct(derivativeU, derivativeV);
            if (crossProduct == new Vector3D(0,0,0)) return new Vector3D(0,1,0);
            crossProduct.Normalize();
            return crossProduct;
        }

        Point3D GetPosition(double u, double v)
        {
            return Function(u, v);
        }

        double GetFractionalPart(double value)
        {
            return value - Math.Ceiling(value);
        }

        #endregion
    }
}
