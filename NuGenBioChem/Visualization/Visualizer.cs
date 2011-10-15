using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace NuGenVisChem.Visualization
{
    /// <summary>
    /// Represents visualization of the molecules
    /// </summary>
    public class Visualizer : Viewport3D
    {
        #region Fields

        // Distance to the object
        double radius = 2.0f;

        // Angles, in radians
        double verticalRotation = 0.0f;
        double horisontalRotation = 0.0f;

        // Camera
        PerspectiveCamera camera;

        // It's for dragging
        double mouseX = Double.NaN;
        double mouseY = Double.NaN;

        #endregion

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

        /// <summary>
        /// The observer's target
        /// </summary>
        public Point3D Target
        {
            get
            {
                return new Point3D();
            }
        }

        /// <summary>
        /// The observer's position
        /// </summary>
        public Point3D Position
        {
            get
            {
                Point3D position = new Point3D(
                    (radius * Math.Cos(verticalRotation) * Math.Cos(horisontalRotation)) + Target.X,
                    (radius * Math.Sin(verticalRotation)) + Target.Y,
                    (radius * Math.Cos(verticalRotation) * Math.Sin(horisontalRotation)) + Target.Z);
                
                return position;
            }
        }

        void Invalidate()
        {
            camera.Position = Position;
            camera.LookDirection = (Target - camera.Position);
            camera.LookDirection.Normalize();
            camera.UpDirection = new Vector3D(0,1,0);
        }

        public Visualizer()
        {
            Camera = camera = new PerspectiveCamera(new Point3D(), new Vector3D(1, 0, 0), new Vector3D(0, 0, 1), 60);
            MouseMove += OnMouseMove;
            MouseWheel += OnMouseWheel;

            GeometryModel3D model = new GeometryModel3D();
            model.BackMaterial = new DiffuseMaterial(Brushes.Red);
            model.Material = new DiffuseMaterial(Brushes.Blue);
            model.Geometry = Meshes.Cylider;
            
            ModelVisual3D modelVisual3D = new ModelVisual3D();
            modelVisual3D.Content = model;

            
            ModelVisual3D lights = new ModelVisual3D();
            DirectionalLight light = new DirectionalLight(Colors.White, new Vector3D(-0.612372, -0.5, -0.612372));
            lights.Content = light;

            Children.Add(modelVisual3D);
            Children.Add(lights);
            

        }

        void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point point = e.GetPosition(this);

                if (!Double.IsNaN(mouseX))
                {
                    HorisontalRotation += (float)((point.X - mouseX) / 100);
                    VerticalRotation += (float)((point.Y - mouseY) / 100);
                }

                mouseX = point.X;
                mouseY = point.Y;
            }
            else
            {
                mouseX = Double.NaN;
                mouseY = Double.NaN;
            }
        }

        void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            Distance *= e.Delta > 0 ? 1.05 : 0.95;
        }
    }
}
