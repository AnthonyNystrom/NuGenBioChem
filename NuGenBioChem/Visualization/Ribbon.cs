using System;
using System.Collections.Generic;
using System.Windows.Media.Media3D;

namespace NuGenBioChem.Visualization
{
    /// <summary>
    /// The ribbon object provides methods for geometry evaluation of the protein chain
    /// </summary>
    public class Ribbon
    {
        #region Static fields

        // Count of the line segment of the residue bacbone
        static int LineSegmentCount = 10;

        #endregion

        #region Fields

        // Chain
        Chain chain;

        // Types of the residues in the chain
        Data.SecondaryStructureType[] residueTypes = null;
        // Dictionary used to match residues and indeces of the control points
        Dictionary<Data.Residue, int> residueIndexTable = new Dictionary<Data.Residue, int>();

        List<Point3D> controlPoints = new List<Point3D>();
        List<Point3D> torsionPoints = new List<Point3D>();

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the chain of the ribbon
        /// </summary>
        public Chain Chain
        {
            get { return this.chain; }
            set
            {
                this.chain = value;
                Update();
            }
        }

        /// <summary>
        /// Indicates whether creation of the ribbon is successful
        /// </summary>
        public bool IsSuccessful
        {
            get;
            private set;
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Default constructor
        /// </summary>
        public Ribbon()
        { }

        /// <summary>
        /// Constructor
        /// Creates the ribbon for the specified chain
        /// </summary>
        /// <param name="chain">Chain</param>
        public Ribbon(Chain chain)
        {
            Chain = chain;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the backbone of the given residue
        /// </summary>
        /// <param name="residue">Residue</param>
        /// <param name="backbonePoints">Points of the residue backbone</param>
        /// <param name="unitTorsionVectors">Torsion vectors of the backbone curve</param>
        /// <param name="unitNormalVectors">Normal vectors of the backbone curve</param>
        public void GetResidueBackbone(Residue residue,
            out Point3D[] backbonePoints,
            out Vector3D[] unitTorsionVectors,
            out Vector3D[] unitNormalVectors)
        {
            int index;
            if (this.residueIndexTable.TryGetValue(residue.Data, out index))
            {
                int count = 9;
                int first = index * (count - 1);
                backbonePoints = new Point3D[count];
                unitTorsionVectors = new Vector3D[count];
                unitNormalVectors = new Vector3D[count];
                for (int i = 0; i < count - 1; i++)
                {
                    backbonePoints[i] = this.controlPoints[first + i];
                    CalculateBackboneVectors(backbonePoints[i],
                        this.controlPoints[first + i + 1],
                        this.torsionPoints[first + i],
                        out unitTorsionVectors[i],
                        out unitNormalVectors[i]);
                }
                int last = first + count - 1;
                backbonePoints[count - 1] = this.controlPoints[last];
                if (last == this.controlPoints.Count - 1)
                {
                    unitTorsionVectors[count - 1] = unitTorsionVectors[count - 2];
                    unitNormalVectors[count - 1] = unitNormalVectors[count - 2];
                }
                else
                {
                    CalculateBackboneVectors(this.controlPoints[last],
                        this.controlPoints[last + 1],
                        this.torsionPoints[last],
                        out unitTorsionVectors[count - 1],
                        out unitNormalVectors[count - 1]);
                }
            }
            else
            {
                backbonePoints = null;
                unitTorsionVectors = null;
                unitNormalVectors = null;
            }

        }

        /// <summary>
        /// Determines whether given residue is the end of a structure
        /// </summary>
        /// <param name="residue">Residue</param>
        /// <returns>true - if the resdiue is start of a helix, a sheet or a turn</returns>
        public bool IsStructureEnd(Residue residue)
        {
            int index;
            if (this.residueIndexTable.TryGetValue(residue.Data, out index))
            {
                if (index == this.residueTypes.Length - 1) return true;
                if (this.residueTypes[index + 1] != this.residueTypes[index]) return true;
                return false;
            }
            else throw new ArgumentException();
        }

        /// <summary>
        /// Determines whether given residue is the begin of a structure
        /// </summary>
        /// <param name="residue">Residue</param>
        /// <returns>true - if the resdiue is the begin of a helix, a sheet or a turn</returns>        
        public bool IsStructureBegin(Residue residue)
        {
            int index;
            if (this.residueIndexTable.TryGetValue(residue.Data, out index))
            {
                if (index == 0) return true;
                if (this.residueTypes[index - 1] != this.residueTypes[index]) return true;
                return false;
            }
            else throw new ArgumentException();
        }

        // Updates when the chain of the ribbon has been changed
        void Update()
        {
            this.IsSuccessful = true;
            this.residueIndexTable.Clear();

            List<Point3D> acList = new List<Point3D>(this.chain.Data.Residues.Count);
            List<Point3D> oList = new List<Point3D>(this.chain.Data.Residues.Count);
            List<Data.SecondaryStructureType> typeList = new List<Data.SecondaryStructureType>();

            foreach (var residueData in this.chain.Data.Residues)
            {
                Data.Atom alfaCarbon = residueData.AlfaCarbon ?? residueData.FindAtom("C");
                Data.Atom oxygen = residueData.FindAtom("O");
                if (alfaCarbon != null && oxygen != null)
                {
                    this.residueIndexTable.Add(residueData, acList.Count);
                    acList.Add(alfaCarbon.Position);
                    oList.Add(oxygen.Position);
                    typeList.Add(chain.Data.GetStructureType(residueData));
                }
            }

            if (acList.Count < 4)
            {
                this.IsSuccessful = false;
                return;
            }

            this.residueTypes = typeList.ToArray();

            acList.Insert(0, Ribbon.Lerp(acList[1], acList[2], -1.0));
            acList.Add(Ribbon.Lerp(acList[acList.Count - 2], acList[acList.Count - 1], 2.0));

            oList.Insert(0, Ribbon.Lerp(oList[1], oList[2], -1.0));
            oList.Add(Ribbon.Lerp(oList[oList.Count - 2], oList[oList.Count - 1], 2.0));


            Vector3D prevD = new Vector3D();
            for (int i = 1; i < acList.Count; i++)
            {
                Point3D ac1 = acList[i - 1];
                Point3D ac2 = acList[i];

                Vector3D a = ac2 - ac1;
                Vector3D n = Vector3D.CrossProduct(a, oList[i - 1] - ac1);
                Vector3D d = Vector3D.CrossProduct(n, a);

                if (Vector3D.DotProduct(prevD, d) < 0.0) d.Negate();
                prevD = d;

                d.Normalize();
                n.Normalize();

                Point3D p = Ribbon.Lerp(ac1, ac2, 0.5);

                this.controlPoints.Add(p);
                this.torsionPoints.Add(p + d);
            }

            this.controlPoints = Ribbon.SmoothList(this.controlPoints);
            this.torsionPoints = Ribbon.SmoothList(this.torsionPoints);

            this.controlPoints = Ribbon.SmoothList(this.controlPoints);
            this.torsionPoints = Ribbon.SmoothList(this.torsionPoints);

            this.controlPoints = Ribbon.SmoothList(this.controlPoints);
            this.torsionPoints = Ribbon.SmoothList(this.torsionPoints);

        }

        // Calculates the normal and the torsion unit vectors for the specified point, the up point and the torsion point
        static void CalculateBackboneVectors(Point3D point, Point3D upPoint, Point3D torsionPoint,
            out Vector3D unitTorsionVector, out Vector3D unitNormalVector)
        {
            Vector3D tangentVector = upPoint - point;
            unitTorsionVector = torsionPoint - point;
            unitNormalVector = Vector3D.CrossProduct(tangentVector, unitTorsionVector);
            unitTorsionVector = Vector3D.CrossProduct(unitNormalVector, tangentVector);
            unitNormalVector.Normalize();
            unitTorsionVector.Normalize();
        }

        // Smooth between points p1 and p2 and between points p2 and p3 by construct circumscribed circle,
        // select middle points on chords and getting up it to the circle
        static void Smoothing(Point3D p1, Point3D p2, Point3D p3, out Point3D p12, out Point3D p23)
        {
            Vector3D r1 = p1 - p2;
            Vector3D r2 = p3 - p2;

            // Calculates middle chords points
            p12 = p2 + 0.5 * r1;
            p23 = p2 + 0.5 * r2;

            double r11 = r1.LengthSquared;
            double r22 = r2.LengthSquared;
            double r12 = Vector3D.DotProduct(r1, r2);

            double det = r12 * r12 - r11 * r22;

            if (det < 1e-3)
            {
                // Calculates center and radius of the circle
                double a = 0.5 * r22 * (r12 - r11) / det;
                double b = 0.5 * r11 * (r12 - r22) / det;
                Point3D center = p2 + a * r1 + b * r2;
                double radius = (center - p2).Length;

                Vector3D d = p12 - center;
                d *= radius / d.Length;
                p12 = center + d;

                d = p23 - center;
                d *= radius / d.Length;
                p23 = center + d;
            }

        }

        // Smooth list by circle smoothing
        static List<Point3D> SmoothList(List<Point3D> points)
        {
            List<Point3D> result = new List<Point3D>();
            Point3D p1 = points[0];
            Point3D p2 = points[1];
            Point3D p3 = points[2];
            Point3D p12, p23;
            Smoothing(p1, p2, p3, out p12, out p23);
            result.Add(p1);
            result.Add(p12);
            result.Add(p2);
            Point3D prev = p23;
            for (int i = 3; i < points.Count; i++)
            {
                p1 = points[i - 2];
                p2 = points[i - 1];
                p3 = points[i];
                Smoothing(p1, p2, p3, out p12, out p23);
                p12 = Ribbon.Lerp(p12, prev, 0.5);
                result.Add(p12);
                result.Add(p2);
                prev = p23;
            }
            result.Add(prev);
            result.Add(p3);
            return result;
        }

        // Linear interpolation between two points
        static Point3D Lerp(Point3D p1, Point3D p2, double t)
        {
            return new Point3D((1.0 - t) * p1.X + t * p2.X,
                (1.0 - t) * p1.Y + t * p2.Y,
                (1.0 - t) * p1.Z + t * p2.Z);
        }

        #endregion

    }
}