using System;
using System.Windows.Controls;
using System.Windows.Media.Media3D;

namespace NuGenVisChem.Visualization
{
    public class Browser
    {
        #region Fields

        // Distance to the object
        double radius = 1.0f;

        // Angles, in radians
        double verticalRotation = 0.0f;
        double horisontalRotation = 0.0f;

        Viewport3D viewport;
        PerspectiveCamera camera;

        #endregion

        #region Properties

        /// <summary>
        /// Angle around target, in radians
        /// </summary>
        public double HorisontalRotation
        {
            get { return horisontalRotation; }
            set
            {
                horisontalRotation = value;

                // Keeps value between 0 and 2*PI 
                horisontalRotation = horisontalRotation > (float)Math.PI * 2 ? horisontalRotation - (float)Math.PI * 2 : horisontalRotation;
                horisontalRotation = horisontalRotation < 0 ? horisontalRotation + (float)Math.PI * 2 : horisontalRotation;
                Invalidate();
            }
        }

        /// <summary>
        /// Angle up/down, in radians
        /// </summary>
        public double VerticalRotation
        {
            get { return verticalRotation; }
            set
            {
                verticalRotation = value;

                // Keeps value between 0 and 2*PI 
                verticalRotation = verticalRotation > (float)Math.PI * 2 ? verticalRotation - (float)Math.PI * 2 : verticalRotation;
                verticalRotation = verticalRotation < 0 ? verticalRotation + (float)Math.PI * 2 : verticalRotation;
                Invalidate();
            }
        }

        /// <summary>
        /// The distance to the target
        /// </summary>
        public double Distance
        {
            get { return radius; }
            set
            {
                radius = value;
                if (radius < 0.01f) radius = 0.01f;
                Invalidate();
            }
        }

       /* /// <summary>
        /// The observer's position
        /// </summary>
        public override Vector3D Position
        {
            get
            {
                Vector3D position = new Vector3D();
                position.X = (float)(radius * Math.Cos(verticalRotation) * Math.Cos(horisontalRotation));
                position.Y = (float)(radius * Math.Sin(verticalRotation));
                position.Z = (float)(radius * Math.Cos(verticalRotation) * Math.Sin(horisontalRotation));
                position += target;
                return position;
            }
        }*/

        void Invalidate()
        {
        }


        #endregion

        public Browser()
        {
            
        }

        public void Attach(Viewport3D vewport)
        {
            this.viewport = viewport;
            
        }

        public void Detach()
        {
            
        }
    }
}
