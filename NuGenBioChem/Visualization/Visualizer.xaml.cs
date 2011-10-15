using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Xps;
using NuGenBioChem.Data;
using NuGenBioChem.Data.Commands;
using NuGenBioChem.Visualization.Primitives;
using Polygon = NuGenBioChem.Visualization.Primitives.Polygon;
using Style = NuGenBioChem.Data.Style;

namespace NuGenBioChem.Visualization
{
    /// <summary>
    /// Represents the main visualizer
    /// </summary>
    public partial class Visualizer : UserControl
    {
        #region Events

        /// <summary>
        /// Occurs when IsAttached property is changed
        /// </summary>
        public event EventHandler IsAttachedChanged;

        #endregion

        #region Fields

        // Data
        Substance substance;
        Style style;

        // Camera
        ProjectionCamera camera;

        // It's for dragging
        double mouseX = Double.NaN;
        double mouseY = Double.NaN;

        // Containers
        ModelVisual3D moleculesContainer = new ModelVisual3D();
        ModelVisual3D lightsContainer = new ModelVisual3D();
        ModelVisual3D shadowContainer = new ModelVisual3D();

        // Light from eyes
        DirectionalLight lightFromEyes = new DirectionalLight(Colors.White, new Vector3D(1,0,0));
        AmbientLight lightAmbient = new AmbientLight(Colors.White);

        // Brush for the shadow
        ImageBrush shadowBrush = new ImageBrush();

        // This flag is set true when refresh has been started 
        // (it is false when it is completed)
        bool shadowRefreshStarted;
        
        // Flag determines that data were attached to the presentation
        bool dataAttached;

        #endregion

        #region Properties
        
        #region Zoom

        /// <summary>
        /// Gets or sets zoom
        /// </summary>
        public double Zoom
        {
            get { return (double)GetValue(ZoomProperty); }
            set { SetValue(ZoomProperty, value); }
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for Zoom.  
        /// This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty ZoomProperty =
            DependencyProperty.Register("Zoom", typeof(double), typeof(Visualizer), 
            new UIPropertyMetadata(1.0d, OnZoomChanged));

        // Zoom changing handler
        static void OnZoomChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        
        }

        #endregion

        #region CameraProjectionMode

        /// <summary>
        /// Gets or sets projection mode of the camera
        /// </summary>
        public CameraProjectionMode CameraProjectionMode
        {
            get { return (CameraProjectionMode)GetValue(CameraProjectionModeProperty); }
            set { SetValue(CameraProjectionModeProperty, value); }
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for CameraProjectionMode.  
        /// This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty CameraProjectionModeProperty =
            DependencyProperty.Register("CameraProjectionMode", typeof(CameraProjectionMode), typeof(Visualizer), 
            new UIPropertyMetadata(CameraProjectionMode.Perspective, OnCameraProjectionModeChanged));

        static void OnCameraProjectionModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Visualizer visualizer = (Visualizer)d;
            CameraProjectionMode cameraProjectionMode = (CameraProjectionMode)e.NewValue;
            if (cameraProjectionMode == CameraProjectionMode.Perspective)
            {
                OrthographicCamera orthographicCamera = (OrthographicCamera) visualizer.camera;
                PerspectiveCamera perspectiveCamera = new PerspectiveCamera(visualizer.camera.Position, visualizer.camera.LookDirection, visualizer.camera.UpDirection, 
                    visualizer.FieldOfView);
                visualizer.viewport.Camera = visualizer.camera = perspectiveCamera;
            }
            if (cameraProjectionMode == CameraProjectionMode.Orthographic)
            {
                PerspectiveCamera perspectiveCamera = (PerspectiveCamera)visualizer.camera;
                OrthographicCamera orthographicCamera = new OrthographicCamera(visualizer.camera.Position, visualizer.camera.LookDirection, visualizer.camera.UpDirection,
                    2.0 * visualizer.Distance * Math.Tan(((perspectiveCamera.FieldOfView * Math.PI) / 180.0) / 2.0));
                visualizer.viewport.Camera = visualizer.camera = orthographicCamera;
            }
        }
        
        #endregion

        #region Substance & Style

        /// <summary>
        /// Gets or sets data
        /// </summary>
        public Substance Substance
        {
            get { return substance; }
            set
            {
                if (substance == value) return;
                
                DetachData();
                substance = value;

                if (IsVisible && style != null) AttachData();   
            }
        }

        /// <summary>
        /// Gets or sets style
        /// </summary>
        public Style SubstanceStyle
        {
            get { return style; }
            set
            {
                if (style == value) return;

                DetachStyle();
                style = value;

                if (IsVisible && !IsAttached && substance != null && style != null) AttachData();
                if (IsVisible && style != null) AttachStyle();
            }
        }

        void AttachStyle()
        {
            if (style == null) return;
            style.GeometryStyle.PropertyChanged += OnGeometryStylePropertyChanged;

            foreach (Molecule molecule in moleculesContainer.Children)
            {
                molecule.Style = style;
            }

            RefreshShadow();
        }

        void DetachStyle()
        {
            if (style == null) return;
            style.GeometryStyle.PropertyChanged -= OnGeometryStylePropertyChanged;

            foreach (Molecule molecule in moleculesContainer.Children)
            {
                molecule.Style = null;
            }
        }

        #endregion

        #region Properties of the Camera

        #region Field of View

        /// <summary>
        /// Gets or sets field of view of the camera
        /// </summary>
        public double FieldOfView
        {
            get { return (double)GetValue(FieldOfViewProperty); }
            set { SetValue(FieldOfViewProperty, value); }
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for FieldOfView.  
        /// This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty FieldOfViewProperty =
            DependencyProperty.Register("FieldOfView", typeof(double), typeof(Visualizer),
            new UIPropertyMetadata(60.0d, OnFieldOfViewChanged));

        static void OnFieldOfViewChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Visualizer visualizer = (Visualizer)d;
            PerspectiveCamera perspectiveCamera = visualizer.camera as PerspectiveCamera;
            if (perspectiveCamera != null) perspectiveCamera.FieldOfView = (double)e.NewValue;
            else
            {
                // Orthographic case
                visualizer.UpdateCamera();
            }
        }

        #endregion

        #region HorizontalRotation

        /// <summary>
        /// Gets or sets angle around target, in radians
        /// </summary>
        public double HorizontalRotation
        {
            get { return (double)GetValue(HorizontalRotationProperty); }
            set { SetValue(HorizontalRotationProperty, value); }
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for HorizontalRotation.  
        /// This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty HorizontalRotationProperty =
            DependencyProperty.Register("HorizontalRotation", typeof(double), typeof(Visualizer),
            new UIPropertyMetadata(0.0d, null, null));
        
        static object CoerceHorizontalRotation(DependencyObject d, object basevalue)
        {
            double horisontalRotation = (double) basevalue;

            // Keeps value between 0 and 2*PI 
            horisontalRotation = horisontalRotation > Math.PI * 2.0 ? horisontalRotation - Math.PI * 2.0 : horisontalRotation;
            horisontalRotation = horisontalRotation < 0 ? horisontalRotation + Math.PI * 2.0 : horisontalRotation;

            return horisontalRotation; 
        }

        #endregion

        #region VerticalRotation

        /// <summary>
        /// Gets or sets angle around target, in radians
        /// </summary>
        public double VerticalRotation
        {
            get { return (double)GetValue(VerticalRotationProperty); }
            set { SetValue(VerticalRotationProperty, value); }
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for VerticalRotation.  
        /// This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty VerticalRotationProperty =
            DependencyProperty.Register("VerticalRotation", typeof(double), typeof(Visualizer),
            new UIPropertyMetadata(0.0d, null, CoerceVerticalRotation));


        static object CoerceVerticalRotation(DependencyObject d, object basevalue)
        {
            double verticalRotation = (double)basevalue;

            // Keeps value between 0 and 2*PI 
            verticalRotation = verticalRotation > (Math.PI / 2.0 - 0.01) ? (Math.PI / 2.0 - 0.01) : verticalRotation;
            verticalRotation = verticalRotation < (-Math.PI / 2.0 + 0.01) ? (-Math.PI / 2.0 + 0.01) : verticalRotation;

            return verticalRotation;
        }

        #endregion

        #region Distance

        /// <summary>
        /// Gets or sets distance to the camera's target
        /// </summary>
        public double Distance
        {
            get { return (double)GetValue(DistanceProperty); }
            set { SetValue(DistanceProperty, value); }
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for Distance.  
        /// This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty DistanceProperty =
            DependencyProperty.Register("Distance", typeof(double), typeof(Visualizer), 
            new UIPropertyMetadata(1.0d, null, CoerceDistance));

        static object CoerceDistance(DependencyObject d, object basevalue)
        {
            double distance = (double) basevalue;
            if (distance == 0) return 0.001;
            return distance;
        }
        
        #endregion

        #region Target
        
        /// <summary>
        /// Gets or sets camera's target
        /// </summary>
        public Point3D Target
        {
            get { return (Point3D)GetValue(TargetProperty); }
            set { SetValue(TargetProperty, value); }
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for Target.  
        /// This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty TargetProperty =
            DependencyProperty.Register("Target", typeof(Point3D), typeof(Visualizer), 
            new UIPropertyMetadata(new Point3D(), OnTargetChanged));

        static void OnTargetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Visualizer)d).UpdateCamera();
        }

        #endregion

        #region Prosition

        /// <summary>
        /// Gets camera position
        /// </summary>
        public Point3D Position
        {
            get { return (Point3D)GetValue(PositionProperty); }
            private set { SetValue(PositionProperty, value); }
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for Position. 
        /// This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty PositionProperty =
            DependencyProperty.Register("Position", typeof(Point3D), typeof(Visualizer), 
            new UIPropertyMetadata(new Point3D()));


        #endregion

        #region Direction

        /// <summary>
        /// Gets camera direction
        /// </summary>
        public Vector3D Direction
        {
            get { return (Vector3D)GetValue(DirectionProperty); }
            private set { SetValue(DirectionProperty, value); }
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for Direction. 
        /// This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty DirectionProperty =
            DependencyProperty.Register("Direction", typeof(Vector3D), typeof(Visualizer),
            new UIPropertyMetadata(new Vector3D(1,0,0)));


        #endregion
        
        void UpdateCamera()
        {
            // Update position
            double verticalRotation = actualVeticalRotation;
            double horisontalRotation = actualHorizontalRotation;
            Position = new Point3D(
                (actualDistance * Math.Cos(verticalRotation) * Math.Cos(horisontalRotation)) + actualTarget.X,
                (actualDistance * Math.Sin(verticalRotation)) + actualTarget.Y,
                (actualDistance * Math.Cos(verticalRotation) * Math.Sin(horisontalRotation)) + actualTarget.Z);

            Vector3D direction = actualTarget - Position;
            direction.Normalize();

            // Update camera
            camera.Position = Position;
            camera.UpDirection = new Vector3D(0, 1, 0);
            Vector3D side = Vector3D.CrossProduct(direction, new Vector3D(0,1,0));
            Vector3D up = Vector3D.CrossProduct(side, direction);
            up.Normalize();
            camera.UpDirection = up;

            // 
            Direction = direction;
            camera.LookDirection = direction;
            lightFromEyes.Direction = direction;

            OrthographicCamera orthographicCamera = camera as OrthographicCamera;
            if (orthographicCamera != null)
            {
                orthographicCamera.Width = 2.0 * actualDistance * Math.Tan(((FieldOfView * Math.PI) / 180.0) / 2.0);
            }
        }

        #endregion
        
        /// <summary>
        /// Gets or sets whether the data is attached to visualizer
        /// </summary>
        public bool IsAttached
        {
            get { return dataAttached; }
        }

        #endregion

        #region Commands
        
        #region Show All

        // Command
        RelayCommand showAllComand;

        /// <summary>
        /// Gets show all command
        /// </summary>
        public RelayCommand ShowAllComand
        {
            get
            {
                if (showAllComand == null)
                {
                    showAllComand = new RelayCommand(x => ShowAll());
                }
                return showAllComand;
            }
        }

        /// <summary>
        /// Centers and zooms to show all content
        /// </summary>
        public void ShowAll()
        {
            if (dataAttached)
            {
                Zoom = 1.0;
                Rect3D boundingBox = VisualTreeHelper.GetDescendantBounds(moleculesContainer);
                if (!boundingBox.IsEmpty) Target = boundingBox.GetCenter();
            }
            else IsAttachedChanged += OnShowAllDelayed;
        }

        void OnShowAllDelayed(object sender, EventArgs args)
        {
            IsAttachedChanged -= OnShowAllDelayed;
            ShowAll();
        }

        #endregion

        #region Look At

        // Command
        RelayCommand lookAtComand;

        /// <summary>
        /// Gets look at command
        /// </summary>
        public RelayCommand LookAtComand
        {
            get
            {
                if (lookAtComand == null)
                {
                    lookAtComand = new RelayCommand(x => OnLookAtCommand(x));
                }
                return lookAtComand;
            }
        }

        void OnLookAtCommand(object parameter)
        {
            string plane = parameter as string;
            switch (plane)
            {
                case "+X":
                    HorizontalRotation = Math.PI;
                    VerticalRotation = 0;
                    break;
                case "+Y":
                    HorizontalRotation = 2.5 * Math.PI;
                    VerticalRotation = -0.5 * Math.PI;
                    break;
                case "+Z":
                    HorizontalRotation = 1.5 * Math.PI;
                    VerticalRotation = 0;
                    break;
                case "-X":
                    HorizontalRotation = 2.0 * Math.PI;
                    VerticalRotation = 0;
                    break;
                case "-Y":
                    HorizontalRotation = 2.5 * Math.PI;
                    VerticalRotation = 0.5 * Math.PI;
                    break;
                case "-Z":
                    HorizontalRotation = 0.5 * Math.PI;
                    VerticalRotation = 0;
                    break;
                
                case "Bias":
                    HorizontalRotation = 1.86;
                    VerticalRotation = 0.56;
                    break;
            }
        }

        #endregion

        #region Navigate To

        // Command
        RelayCommand navigateToComand;

        /// <summary>
        /// Gets show all command
        /// </summary>
        public RelayCommand NavigateToComand
        {
            get
            {
                if (navigateToComand == null)
                {
                    navigateToComand = new RelayCommand(x => NavigateTo((Data.Molecule)x));
                }
                return navigateToComand;
            }
        }

        #endregion
        
        #endregion

        #region Initialization

        /// <summary>
        /// Default constructor
        /// </summary>
        public Visualizer()
        {
            InitializeComponent();

            HorizontalRotation = 1.86;
            VerticalRotation = 0.56;

            IsVisibleChanged += OnIsVisibleChanged;

            viewport.Camera = camera = new PerspectiveCamera(new Point3D(), new Vector3D(1, 0, 0), new Vector3D(0, 0, 1), FieldOfView);

            lightsContainer.Content = lightFromEyes;
            Transform3DGroup transform3DGroup = new Transform3DGroup();
            transform3DGroup.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 1, 0), 20)));
            transform3DGroup.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1, 0, 0), -20)));
            lightsContainer.Transform = transform3DGroup;

            // Add ambient light
            viewport.Children.Add(new ModelVisual3D() { Content = lightAmbient });

            #region Axis (for debug only)

            /*Cylinder xaxis = new Cylinder(new Point3D(), new Point3D(100, 0,0), 1);
            Cylinder yaxis = new Cylinder(new Point3D(), new Point3D(0, 100, 0), 1);
            Cylinder zaxis = new Cylinder(new Point3D(), new Point3D(0, 0, 100), 1);
            xaxis.Material = new DiffuseMaterial(new SolidColorBrush(Colors.Red));
            yaxis.Material = new DiffuseMaterial(new SolidColorBrush(Colors.Green));
            zaxis.Material = new DiffuseMaterial(new SolidColorBrush(Colors.Blue));
            lightsContainer.Children.Add(xaxis);
            lightsContainer.Children.Add(yaxis);
            lightsContainer.Children.Add(zaxis);*/

            #endregion

            viewport.Children.Add(moleculesContainer);
            viewport.Children.Add(lightsContainer);
            viewport.Children.Add(shadowContainer);
        }

        // This value for debug purpose.
        // Total performance depends from how many of attached visulizers now
        static int attachedVisualizerCount = 0;

        // Create visual controls for the data
        void AttachData()
        {
            if (substance == null) return;
            if (dataAttached) return;
            dataAttached = true;

            // Add molecules
            moleculesContainer.Children.Clear();
            foreach (Data.Molecule item in substance.Molecules)
                moleculesContainer.Children.Add(new Molecule { Data = item, Style = style });

            // Subscribe to events
            substance.Molecules.CollectionChanged += OnMoleculesChanged;


            if (IsAttachedChanged != null) IsAttachedChanged(this, EventArgs.Empty);

            #region Debug

            attachedVisualizerCount++;
            Debug.WriteLine(String.Format("Visualizer attached (count = {0})", attachedVisualizerCount));

            #endregion
        }

        // Remove visual controls for data,
        // it must be invoked to avoid memory leaks
        void DetachData()
        {
            if (substance == null) return;
            if (!dataAttached) return;
            dataAttached = false;
            
            // Unsubscribe from data events
            substance.Molecules.CollectionChanged -= OnMoleculesChanged;

            // Detach from data to prevent memory leaks
            foreach (Molecule item in moleculesContainer.Children)
            {
                item.Data = null;
                item.Style = null;
            }

            moleculesContainer.Children.Clear();

            #region Debug

            attachedVisualizerCount--;
            Debug.WriteLine(String.Format("Visualizer detached (count = {0})", attachedVisualizerCount));

            #endregion
        }

        void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                CompositionTarget.Rendering += OnAnimation;
                AttachData();
                AttachStyle();
            }
            else
            {
                // Unsubscribe to prevent memory leaks
                CompositionTarget.Rendering -= OnAnimation;
                DetachData();
                DetachStyle();
            }
        }
       
        
        #endregion

        #region Smooth Changing

        // Animated variables
        double actualDistance = Double.NaN;
        double actualHorizontalRotation = Double.NaN;
        double actualVeticalRotation = Double.NaN;
        Point3D actualTarget = new Point3D(Double.NaN, 0,0);
        // Shadow target opacity
        double shadowTargetOpacity = 0;

        /// <summary>
        /// Immediately completes the animation
        /// </summary>
        public void CompleteAnimation()
        {
            RenderOptions.SetEdgeMode(this, EdgeMode.Unspecified);

            actualDistance = Distance;
            actualHorizontalRotation = HorizontalRotation;
            actualVeticalRotation = VerticalRotation;
            actualTarget = Target;
            shadowBrush.Opacity = shadowTargetOpacity;

            UpdateCamera();
        }

        void OnAnimation(object sender, EventArgs e)
        {
            if (substance == null) return;

            // Update distance
            double showAllDistance = GetShowAllDistance();
            Distance = showAllDistance / Zoom;

            bool updateRequired = false;

            UpdateValue(Distance, ref actualDistance, ref updateRequired, 0.1);
            UpdateValue(HorizontalRotation, ref actualHorizontalRotation, ref updateRequired, 0.18);
            UpdateValue(VerticalRotation, ref actualVeticalRotation, ref updateRequired, 0.18);
            UpdateValue(Target, ref actualTarget, ref updateRequired, 0.18);
            
            // Update shadow
            double actualShadowOpacity = shadowBrush.Opacity;
            UpdateValue(shadowTargetOpacity, ref actualShadowOpacity, ref updateRequired, 0.1);
            shadowBrush.Opacity = actualShadowOpacity;

            if (updateRequired)
            {
                RenderOptions.SetEdgeMode(this, EdgeMode.Aliased);
                UpdateCamera();
            }
            else
            {
                EdgeMode edgeMode = RenderOptions.GetEdgeMode(this);
                if (edgeMode != EdgeMode.Unspecified)
                {
                    RenderOptions.SetEdgeMode(this, EdgeMode.Unspecified);
                    InvalidateVisual();
                }
            }

            #region Draw Selections

            if (substance != null && substance.SelectedAtoms.Count != 0)
            {
                // Get transform from 3D to 2D screen of the control
                Matrix3D transform = viewport.Get3DTo2DTransform();

                // Fill with ellipses
                int count = substance.SelectedAtoms.Count - canvas.Children.Count;
                for (int i = 0; i < count; i++)
                {
                    Ellipse ellipse = new Ellipse();
                    ellipse.Fill = null;
                    ellipse.Stroke = Brushes.White;
                    ellipse.StrokeDashArray.Add(4);
                    ellipse.StrokeDashArray.Add(2);
                    ellipse.StrokeThickness = 1;
                    canvas.Children.Add(ellipse);
                }
                count = canvas.Children.Count - substance.SelectedAtoms.Count;
                canvas.Children.RemoveRange(canvas.Children.Count - count, count);

                for (int i = 0; i < substance.SelectedAtoms.Count; i++)
                {
                    Point3D transformed = transform.Transform(substance.SelectedAtoms[i].Position);
                    Point3D transformedEdgePoint = transform.Transform(substance.SelectedAtoms[i].Position + camera.UpDirection * GetVisualAtom(substance.SelectedAtoms[i]).Radius);
                    double radius = (transformed - transformedEdgePoint).Length;

                    Ellipse ellipse = (Ellipse) canvas.Children[i];
                    ellipse.Width = ellipse.Height = radius * 2;
                    
                    Canvas.SetLeft(ellipse, transformed.X - radius);
                    Canvas.SetTop(ellipse, transformed.Y - radius);
                }
            }
            else canvas.Children.Clear();

            #endregion
        }

        Atom GetVisualAtom(Data.Atom data)
        {
            return (from Molecule molecule in moleculesContainer.Children select molecule.GetVisualAtom(data)).FirstOrDefault(result => result != null);
        }

        void UpdateValue(double targetValue, ref double currentValue, ref bool updateRequired, double velocity)
        {
            if (Double.IsNaN(currentValue) || Double.IsInfinity(currentValue) || IsApproxEquals(currentValue, targetValue))
            {
                if (currentValue != targetValue) updateRequired = true;
                currentValue = targetValue; 
            }
            else
            {
                currentValue = currentValue * (1.0 - velocity) + targetValue * velocity;
                updateRequired = true;
            }
        }

        void UpdateValue(Point3D targetValue, ref Point3D currentValue, ref bool updateRequired, double velocity)
        {
            if (Double.IsNaN(currentValue.X) || IsApproxEquals(currentValue, targetValue))
            {
                if (currentValue != targetValue) updateRequired = true;
                currentValue = targetValue;
            }
            else
            {
                currentValue.X = currentValue.X * (1.0 - velocity) + targetValue.X * velocity;
                currentValue.Y = currentValue.Y * (1.0 - velocity) + targetValue.Y * velocity;
                currentValue.Z = currentValue.Z * (1.0 - velocity) + targetValue.Z * velocity;
                updateRequired = true;
            }
        }

        static bool IsApproxEquals(double a, double b)
        {
            return Math.Abs(a - b) < 0.001;
        }

        static bool IsApproxEquals(Point3D a, Point3D b)
        {
            return IsApproxEquals(a.X, b.X) && IsApproxEquals(a.Y, b.Y) && IsApproxEquals(a.Z, b.Z);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Zooms to the given molecule
        /// </summary>
        public void NavigateTo(Data.Molecule molecule)
        {
            foreach (Molecule child in moleculesContainer.Children)
            {
                if (child.Data == molecule)
                {
                    Rect3D boundingBox = VisualTreeHelper.GetDescendantBounds(child);
                    ZoomTo(boundingBox);
                    return;
                }
            }
        }

        void ZoomTo(Rect3D boundingBox)
        {
            Target = new Point3D(boundingBox.Location.X + boundingBox.SizeX / 2.0,
                                 boundingBox.Location.Y + boundingBox.SizeY / 2.0,
                                 boundingBox.Location.Z + boundingBox.SizeZ / 2.0);

            double boundingSphereRadius = new Vector3D(
                boundingBox.Size.X / 2.0,
                boundingBox.Size.Y / 2.0,
                boundingBox.Size.Z / 2.0).Length;

            double screenAspect = 1;
            if (ActualHeight != 0 && ActualWidth != 0)
                screenAspect = (ActualWidth > ActualHeight) ? ActualWidth / ActualHeight : ActualHeight / ActualWidth;

            Distance = 0.85 * screenAspect * boundingSphereRadius / Math.Tan((FieldOfView * Math.PI / 180.0) / 2.0);
        }

        double GetShowAllDistance()
        {
            Rect3D boundingBox = VisualTreeHelper.GetDescendantBounds(moleculesContainer);

            double boundingSphereRadius = new Vector3D(
                boundingBox.Size.X / 2.0,
                boundingBox.Size.Y / 2.0,
                boundingBox.Size.Z / 2.0).Length;

            double screenAspect = 1;
            if (ActualHeight != 0 && ActualWidth != 0)
                screenAspect = (ActualWidth > ActualHeight) ? ActualWidth / ActualHeight : ActualHeight / ActualWidth;

            return 0.85 * screenAspect * boundingSphereRadius / Math.Tan((FieldOfView * Math.PI / 180.0) / 2.0);
        }

        /// <summary>
        /// Creates preview of this control (automatically setup the camera)
        /// </summary>
        public void CreatePreview(RenderTargetBitmap renderTargetBitmap)
        {
            bool wasDetached = !dataAttached;
            AttachData();

            if (moleculesContainer.Children.Count == 0)
            {
                if (wasDetached) DetachData();
                return;
            }

            ProjectionCamera backupCamera = camera;
            Double backupWidth = Width;
            Double backupHeight = Height;

            // Change size of the visualizer
            Width = renderTargetBitmap.PixelWidth;
            Height = renderTargetBitmap.PixelHeight;
            Measure(new Size(renderTargetBitmap.PixelWidth, renderTargetBitmap.PixelHeight));
            Arrange(new Rect(0, 0, renderTargetBitmap.PixelWidth, renderTargetBitmap.PixelHeight));

            // Change camera
            // Prepare bounding box
            Rect3D boundingBox = VisualTreeHelper.GetDescendantBounds(moleculesContainer);
            if (boundingBox.IsEmpty)
            {
                renderTargetBitmap.Render(this);
                boundingBox = VisualTreeHelper.GetDescendantBounds(moleculesContainer);
                if (boundingBox.IsEmpty) return;
            }
            OrthographicCamera orthographicCamera = new OrthographicCamera();

            #region Accomodate camera to fit content

            if (boundingBox.SizeZ < boundingBox.SizeX && boundingBox.SizeZ < boundingBox.SizeY)
            {
                orthographicCamera.Position = new Point3D(boundingBox.Location.X + boundingBox.SizeX / 2.0,
                                                          boundingBox.Location.Y + boundingBox.SizeY / 2.0,
                                                          boundingBox.Location.Z);
                orthographicCamera.LookDirection = new Vector3D(0, 0, 1);
                orthographicCamera.UpDirection = new Vector3D(0, 1, 0);
                orthographicCamera.Width = Math.Max(boundingBox.SizeX, boundingBox.SizeY) * (Width / Height);
                
            }
            else if (boundingBox.SizeX < boundingBox.SizeZ && boundingBox.SizeX < boundingBox.SizeY)
            {
                orthographicCamera.Position = new Point3D(boundingBox.Location.X,
                                                          boundingBox.Location.Y + boundingBox.SizeY / 2.0,
                                                          boundingBox.Location.Z + boundingBox.SizeZ / 2.0);
                orthographicCamera.LookDirection = new Vector3D(1, 0, 0);
                orthographicCamera.UpDirection = new Vector3D(0, 1, 0);
                orthographicCamera.Width = Math.Max(boundingBox.SizeY, boundingBox.SizeZ);
            }
            else
            {
                orthographicCamera.Position = new Point3D(boundingBox.Location.X + boundingBox.SizeX / 2.0,
                                                          boundingBox.Location.Y,
                                                          boundingBox.Location.Z + boundingBox.SizeZ / 2.0);
                orthographicCamera.LookDirection = new Vector3D(0, 1, 0);
                orthographicCamera.UpDirection = new Vector3D(1, 0, 0);
                orthographicCamera.Width = Math.Max(boundingBox.SizeX, boundingBox.SizeZ);
            }

            #endregion

            orthographicCamera.NearPlaneDistance = 0;
            
            // Set the camera & correct lights
            viewport.Camera = orthographicCamera;
            lightFromEyes.Direction = orthographicCamera.LookDirection;
            RenderOptions.SetEdgeMode(this, EdgeMode.Unspecified);
            UpdateLayout();
            renderTargetBitmap.Render(this);

            // Restore original values of the properties
            viewport.Camera = camera = backupCamera;
            Width = backupWidth;
            Height = backupHeight;

            if (wasDetached) DetachData();
        }

        /// <summary>
        /// Renders visualizer to bitmap
        /// </summary>
        public void RenderTo(RenderTargetBitmap renderTargetBitmap)
        {
            bool wasDetached = !dataAttached;
            AttachData();

            if (moleculesContainer.Children.Count == 0)
            {
                if (wasDetached) DetachData();
                return;
            }

            ProjectionCamera backupCamera = camera;
            Double backupWidth = Width;
            Double backupHeight = Height;

            // Change size of the visualizer
            Width = renderTargetBitmap.PixelWidth;
            Height = renderTargetBitmap.PixelHeight;
            Measure(new Size(renderTargetBitmap.PixelWidth, renderTargetBitmap.PixelHeight));
            Arrange(new Rect(0, 0, renderTargetBitmap.PixelWidth, renderTargetBitmap.PixelHeight));
            
            // Finish the animations
            CompleteAnimation();

            RenderOptions.SetEdgeMode(this, EdgeMode.Unspecified);
            UpdateLayout();
            renderTargetBitmap.Render(this);

            // Restore original values of the properties
            viewport.Camera = camera = backupCamera;
            Width = backupWidth;
            Height = backupHeight;

            if (wasDetached) DetachData();
        }

        #endregion

        #region Data Event Handlers

        void OnGeometryStylePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName != "Name") RefreshShadow();
        }

        void OnMoleculesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (!dataAttached) return;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Reset:
                    foreach (Molecule molecule in moleculesContainer.Children)
                    {
                        molecule.Data = null;
                        molecule.Style = null;
                    }
                    moleculesContainer.Children.Clear(); 
                    break;
                case NotifyCollectionChangedAction.Add:
                    foreach(Data.Molecule item in e.NewItems)
                        moleculesContainer.Children.Add(new Molecule { Data = item, Style = style});
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (Data.Molecule item in e.OldItems)
                        foreach (Molecule molecule in moleculesContainer.Children)
                            if (molecule.Data == item)
                            {
                                molecule.Data = null;
                                molecule.Style = null;
                                moleculesContainer.Children.Remove(molecule);
                                break;
                            }
                    break;    
            }
        }



        #endregion

        #region Mouse Event Handlers

        // Position where mouse were down
        Point mouseDownPosition;
        MouseButton mouseDownButton;

        // Mouse down handling
        void OnMouseDown(object sender, MouseEventArgs e)
        {
            mouseDownPosition = e.GetPosition(this);
            if (e.MouseDevice.MiddleButton == MouseButtonState.Pressed) mouseDownButton = MouseButton.Middle;
            if (e.MouseDevice.RightButton == MouseButtonState.Pressed) mouseDownButton = MouseButton.Right;
            if (e.MouseDevice.LeftButton == MouseButtonState.Pressed) mouseDownButton = MouseButton.Left; 
            mouseX = mouseDownPosition.X;
            mouseY = mouseDownPosition.Y;
            if (e.MiddleButton == MouseButtonState.Pressed) Cursor = Cursors.SizeAll;
            Mouse.Capture(this);
        }

        // Mouse up handling
        void OnMouseUp(object sender, MouseEventArgs e)
        {
            Cursor = Cursors.Arrow;
            Mouse.Capture(null);

            Point point = e.GetPosition(this);
            // If mouse were moved only a little
            if (Math.Abs(mouseDownPosition.X - point.X) < 2 && Math.Abs(mouseDownPosition.Y - point.Y) < 2)
            {
                Atom atom = HitTestAtom(e.GetPosition(this));
                bool isCtrlPressed = Keyboard.Modifiers == ModifierKeys.Control;

                if (mouseDownButton == MouseButton.Left)
                {
                    if (atom != null)
                    {
                        // Select atoms
                        if (isCtrlPressed)
                        {
                             if(substance.SelectedAtoms.Contains(atom.Data)) substance.SelectedAtoms.Remove(atom.Data);
                             else substance.SelectedAtoms.Add(atom.Data);
                        }
                        else 
                        {
                            for (int i = substance.SelectedAtoms.Count - 1; i >= 1; i--) substance.SelectedAtoms.RemoveAt(i);
                            if (substance.SelectedAtoms.Count == 0) substance.SelectedAtoms.Add(atom.Data);
                            else substance.SelectedAtoms[0] = atom.Data;
                        }
                    }
                    else
                    {
                        if (!isCtrlPressed) substance.SelectedAtoms.Clear();
                    }
                }
            }
        }

        // Mouse move handling
        void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (Mouse.Captured != this) return;
            
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point point = e.GetPosition(this);

                if (!Double.IsNaN(mouseX))
                {
                    HorizontalRotation += (float)((point.X - mouseX) / 100);
                    VerticalRotation += (float)((-mouseY + point.Y) / 100);
                }
                
                mouseX = point.X;
                mouseY = point.Y;
            }
            else if (e.MiddleButton == MouseButtonState.Pressed)
            {
                Point point = e.GetPosition(this);

                if (!Double.IsNaN(mouseX) && ActualWidth != 0 && ActualHeight != 0)
                {
                    Vector3D upVector = camera.UpDirection;
                    Vector3D sideVector = Vector3D.CrossProduct(camera.UpDirection, camera.LookDirection);

                    double proportion = Distance / camera.NearPlaneDistance;
                    double aspectRatio = ActualHeight / ActualWidth;
                    double mousedx = 0.25 * (point.X - mouseX) / ActualWidth;
                    double mousedy = 0.25 * (point.Y - mouseY) / ActualHeight;

                    Target = Target + upVector * proportion * aspectRatio * mousedy + sideVector * proportion * mousedx;
                }
                mouseX = point.X;
                mouseY = point.Y;
            }
        }

        // Mouse wheel handling
        void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            Zoom *= e.Delta > 0 ? 1.05 : 0.95;
        }
        
        #endregion

        #region Hit Test

        Atom HitTestAtom(Point mousePoint)
        {
            /*mousePoint.X = mousePoint.X / ActualWidth;
            mousePoint.Y = mousePoint.Y / ActualHeight;

            Vector3D rayDirection = new Vector3D();
            Point3D rayOrigin = new Point3D();

            Matrix3D view = camera.GetViewMatrix();
            view.Invert();
            Matrix3D projection = camera.GetProjectionMatrix(ActualWidth / ActualHeight);

            Vector3D v = new Vector3D();
            v.X = (mousePoint.X * 2.0f - 1.0f) / projection.M11;
            v.Y = -(mousePoint.Y * 2.0f - 1.0f) / projection.M22;
            v.Z = -camera.NearPlaneDistance;

            // Transforms our vector with the inversed view matrix
            view.OffsetX = view.OffsetY = view.OffsetZ = 0;
            rayDirection = view.Transform(v);
            /*rayDirection.X = v.X * view.M11 + v.Y * view.M12 + v.Z * view.M13;
            rayDirection.Y = v.X * view.M21 + v.Y * view.M22 + v.Z * view.M23;
            rayDirection.Z = v.X * view.M31 + v.Y * view.M32 + v.Z * view.M33;*/
            
            // Gets the eye position
            /*rayOrigin.X = view.OffsetX;
            rayOrigin.Y = view.OffsetY;
            rayOrigin.Z = view.OffsetZ;
            RayHitTestParameters rayparams = new RayHitTestParameters(rayOrigin, rayDirection);*/

            hittedAtom = null;
            PointHitTestParameters pointparams = new PointHitTestParameters(mousePoint);
            VisualTreeHelper.HitTest(viewport, null, HitTestAtomResultCallback,
                                     pointparams);

            return hittedAtom;
        }

        Atom hittedAtom = null;

        HitTestResultBehavior HitTestAtomResultCallback(HitTestResult result)
        {
            RayHitTestResult rayResult = result as RayHitTestResult;    
            if (rayResult != null)    
            {
                hittedAtom = VisualTreeHelper.GetParent(rayResult.VisualHit) as Atom;
                if (hittedAtom != null) return HitTestResultBehavior.Stop;
            }    
            return HitTestResultBehavior.Continue;
        }

        #endregion

        #region Private Methods
        
        void RefreshShadow()
        {
            if (shadowRefreshStarted) return;
            shadowRefreshStarted = true;
            shadowTargetOpacity = 0;

            Thread shadowUpdateThread = new Thread(UpdateShadow);
            shadowUpdateThread.Priority = ThreadPriority.Lowest;
            shadowUpdateThread.SetApartmentState(ApartmentState.STA);
            shadowUpdateThread.IsBackground = true;
            shadowUpdateThread.Start();
        }


        void UpdateShadow()
        {
            if (substance == null) return;
            if (style == null) return;

            Thread.Sleep(300);

            // Black material
            DiffuseMaterial material = new DiffuseMaterial(Brushes.Black);
            material.Freeze();

            // Create molecules
            ModelVisual3D container = new ModelVisual3D();
            foreach (Data.Molecule molecule in substance.Molecules)
            {
                foreach (Data.Atom atom in molecule.Atoms)
                {
                    if (style.ColorStyle.ColorScheme[atom.Element].Diffuse.A < 5) continue;
                    Sphere sphere = new Sphere();
                    sphere.Material = material;
                    sphere.Radius = Atom.GetAtomRadius(atom, style.GeometryStyle);
                    sphere.Center = atom.Position;
                    container.Children.Add(sphere);
                }
                double bondRadius = Bond.GetBondRadius(style.GeometryStyle);
                foreach (Data.Bond bond in molecule.Bonds)
                {
                    if (style.ColorStyle.UseSingleBondMaterial)
                    {
                        if (style.ColorStyle.BondMaterial.Diffuse.A < 5) continue;
                    }
                    else if (style.ColorStyle.ColorScheme[bond.Begin.Element].Diffuse.A < 5 || 
                             style.ColorStyle.ColorScheme[bond.End.Element].Diffuse.A < 5) continue;
                    Cylinder cylinder = new Cylinder(bond.Begin.Position, bond.End.Position, bondRadius);
                    cylinder.Material = material;
                    container.Children.Add(cylinder);
                }

                #region Build approximation of ribbon

                double radius = 0.45;
                foreach (Data.Chain chain in molecule.Chains)
                {
                    for (int i = 0; i < chain.Residues.Count; i++)
                    {
                        if (chain.Residues[i].GetStructureType() == SecondaryStructureType.Helix)
                           if (style.GeometryStyle.HelixHeight < 0.05 || style.GeometryStyle.HelixWidth < 0.05) continue;
                           else radius = Residue.HelixWidth * ((style.GeometryStyle.HelixHeight + style.GeometryStyle.HelixWidth) / 2.0);
                        if (chain.Residues[i].GetStructureType() == SecondaryStructureType.Sheet)
                           if (style.GeometryStyle.SheetHeight < 0.05 || style.GeometryStyle.SheetWidth < 0.05) continue;
                           else radius = Residue.SheetWidth * ((style.GeometryStyle.SheetHeight + style.GeometryStyle.SheetWidth) / 2.0);
                        if (chain.Residues[i].GetStructureType() == SecondaryStructureType.NotDefined)
                           if (style.GeometryStyle.TurnHeight < 0.05 || style.GeometryStyle.TurnWidth < 0.05) continue;
                           else radius = Residue.TurnWidth * ((style.GeometryStyle.TurnHeight + style.GeometryStyle.TurnWidth) / 2.0);
                        
                        if (chain.Residues[i].GetStructureType() == SecondaryStructureType.Helix && style.ColorStyle.HelixMaterial.Diffuse.A < 5) continue;
                        if (chain.Residues[i].GetStructureType() == SecondaryStructureType.Sheet && style.ColorStyle.SheetMaterial.Diffuse.A < 5) continue;
                        if (chain.Residues[i].GetStructureType() == SecondaryStructureType.NotDefined && style.ColorStyle.TurnMaterial.Diffuse.A < 5) continue;

                        Data.Atom alfaCarbon = chain.Residues[i].AlfaCarbon;
                        if (alfaCarbon != null)
                        {
                            Point3D begin = alfaCarbon.Position;
                            alfaCarbon = null;
                            for (int j = i + 1; j < chain.Residues.Count; j++)
                            {
                                alfaCarbon = chain.Residues[j].AlfaCarbon;
                                if (alfaCarbon != null) break;
                            }
                            if (alfaCarbon != null)
                            {
                                Point3D end = alfaCarbon.Position;
                                Cylinder cylinder = new Cylinder(begin, end, radius);
                                container.Children.Add(cylinder);
                            }
                        }
                    }
                }

                #endregion
            }
               
            // Get bounding box
            Rect3D boundingBox = VisualTreeHelper.GetDescendantBounds(container);
            if (boundingBox.IsEmpty)
            {
                shadowRefreshStarted = false;
                return;
            }

            #region Render Shadow

            const double blurSize = 25;
            const int renderTargetWidth = 200;
            int renderTargetHeight = (int)(200.0 * (boundingBox.SizeX / boundingBox.SizeY));
            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap(renderTargetWidth, renderTargetHeight, 96, 96, PixelFormats.Pbgra32);
            

            Viewport3D shadowViewport3D = new Viewport3D();
            Border border = new Border();
            border.Padding = new Thickness(blurSize);
            border.Child = shadowViewport3D;

            // Change size of the visualizer
            border.Width = renderTargetBitmap.PixelWidth;
            border.Height = renderTargetBitmap.PixelHeight;
            border.Measure(new Size(renderTargetBitmap.PixelWidth, renderTargetBitmap.PixelHeight));
            border.Arrange(new Rect(0, 0, renderTargetBitmap.PixelWidth, renderTargetBitmap.PixelHeight));


            shadowViewport3D.Children.Add(container);

            // Create camera
            OrthographicCamera orthographicCamera = new OrthographicCamera();

            #region Accomodate camera to fit content


            orthographicCamera.Position = new Point3D(boundingBox.Location.X + boundingBox.SizeX / 2.0,
                                                      boundingBox.Location.Y,
                                                      boundingBox.Location.Z + boundingBox.SizeZ / 2.0);
            orthographicCamera.LookDirection = new Vector3D(0, 1, 0);
            orthographicCamera.UpDirection = new Vector3D(-1, 0, 0);
            orthographicCamera.Width = boundingBox.SizeZ;

            #endregion
            
            orthographicCamera.NearPlaneDistance = 0;

            // Set the camera & correct lights
            shadowViewport3D.Camera = orthographicCamera;
            
            BlurEffect blurEffect = new BlurEffect();
            blurEffect.Radius = blurSize;

            border.Effect = blurEffect;

            renderTargetBitmap.Render(border);
            renderTargetBitmap.Freeze();
            
            #endregion

            // Invoke in main thread
            Dispatcher.BeginInvoke((Action) delegate
            {
                #region Create Plane

                Vector3D margin = new Vector3D(boundingBox.SizeX * 0.4, 0, boundingBox.SizeZ * 0.4);
                Point3D[] points = new Point3D[]
                {
                    boundingBox.Location + new Vector3D(-margin.X, -margin.Y, -margin.Z),
                    boundingBox.Location +
                    new Vector3D(margin.X + boundingBox.SizeX, -margin.Y, -margin.Z),
                    boundingBox.Location +
                    new Vector3D(margin.X + boundingBox.SizeX, -margin.Y,
                                margin.Z + boundingBox.SizeZ),
                    boundingBox.Location +
                    new Vector3D(-margin.X, -margin.Y, margin.Z + boundingBox.SizeZ)
                };

                Polygon shadowPlane = new Polygon();
                shadowPlane.Positions = points;
                shadowPlane.TextureCoordinates = new Point[] { new Point(0, 0), new Point(0, 1), new Point(1, 1), new Point(1, 0) };

                #endregion
                shadowBrush.ImageSource = renderTargetBitmap;
                shadowBrush.Stretch = Stretch.Fill;
                shadowTargetOpacity = 0.8;

                shadowPlane.Material = new DiffuseMaterial(shadowBrush);
                shadowContainer.Children.Clear();
                shadowContainer.Children.Add(shadowPlane);

                // Update shadow hash
                shadowRefreshStarted = false;

              }, DispatcherPriority.SystemIdle);
        }

        #endregion     
    }

    /// <summary>
    /// Camera projection mode
    /// </summary>
    public enum CameraProjectionMode
    {
        /// <summary>
        /// Perspective projection
        /// </summary>
        Perspective,

        /// <summary>
        /// Orthogonal projection
        /// </summary>
        Orthographic
    }
}