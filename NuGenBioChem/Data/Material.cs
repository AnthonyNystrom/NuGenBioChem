using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using NuGenBioChem.Data.Transactions;

namespace NuGenBioChem.Data
{
    /// <summary>
    /// Represents material definition
    /// </summary>
    public class Material : INotifyPropertyChanged
    {
        #region Events

        /// <summary>
        /// Occurs when property has been changed
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region Static Fields

        // Brush used for drawing reflections
        private static BitmapImage reflectionImage;
        // Brush used for drawing noise
        private static BitmapImage bumpImage;

        #endregion

        #region Fields

        // Color brushes
        private readonly Transactable<Color> ambient = new Transactable<Color>(Colors.Black);
        private readonly Transactable<Color> diffuse = new Transactable<Color>(Colors.White);
        private readonly Transactable<Color> specular = new Transactable<Color>(Colors.White);
        private readonly Transactable<Color> emissive = new Transactable<Color>(Colors.Black);
        
        // Parameters
        private readonly Transactable<double> glossiness = new Transactable<double>(0.0);
        private readonly Transactable<double> specularPower = new Transactable<double>(0.5);
        private readonly Transactable<double> reflectionLevel = new Transactable<double>(0.0);
        private readonly Transactable<double> bumpLevel = new Transactable<double>(0.0);
        private readonly Transactable<double> emissiveLevel = new Transactable<double>(0.0);

        // Materials
        private MaterialGroup material;
        private DiffuseMaterial diffuseMaterial;
        private DiffuseMaterial enviromentMaterial;
        private DiffuseMaterial bumpMaterial;
        private EmissiveMaterial emissiveMaterial;
        private SpecularMaterial specularMaterial;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets ambient color
        /// </summary>
        public Color Ambient
        {
            get { return ambient.Value; }
            set
            {
                using (Transaction action = new Transaction(
                       String.Format("Change Ambient Color")))
                {
                    ambient.Value = value;
                    action.Commit();
                }
            }
        }

        /// <summary>
        /// Gets or sets diffuse color
        /// </summary>
        public Color Diffuse
        {
            get { return diffuse.Value; }
            set
            {
                using (Transaction action = new Transaction(
                       String.Format("Change Diffuse Color")))
                {
                    diffuse.Value = value; 
                    action.Commit();
                }
            }
        }

        /// <summary>
        /// Gets or sets specular color
        /// </summary>
        public Color Specular
        {
            get { return specular.Value; }
            set
            {
                using (Transaction action = new Transaction(
                       String.Format("Change Specular Color")))
                {
                    specular.Value = value; 
                    action.Commit();
                }
            }
        }

        /// <summary>
        /// Gets or sets emissive color
        /// </summary>
        public Color Emissive
        {
            get { return emissive.Value; }
            set
            {
                using (Transaction action = new Transaction(
                       String.Format("Change Emissive Color")))
                {
                    emissive.Value = value; 
                    action.Commit();
                }
            }
        }

        /// <summary>
        /// Gets or sets glossiness. Glossiness affects the size of the specular area [0..1]
        /// </summary>
        public double Glossiness
        {
            get { return glossiness.Value; }
            set
            {
                using (Transaction action = new Transaction(
                       String.Format("Change Glossiness from {0:0}% to {1:0}%", glossiness.Value * 100.0, value * 100.0)))
                {
                    glossiness.Value = value; 
                    action.Commit();
                }
            }
        }

        /// <summary>
        /// Gets or sets specular level. Specular level affects the intensity of the glossiness [0..1]
        /// </summary>
        public double SpecularPower
        {
            get { return specularPower.Value; }
            set
            {
                using (Transaction action = new Transaction(
                       String.Format("Change Specular Power from {0:0}% to {1:0}%", specularPower.Value * 100.0, value * 100.0)))
                {
                    specularPower.Value = value;
                    action.Commit();
                }
            }
        }

        /// <summary>
        /// Gets or sets reflection level
        /// </summary>
        public double ReflectionLevel
        {
            get { return reflectionLevel.Value; }
            set
            {
                using (Transaction action = new Transaction(
                       String.Format("Change Reflection Level from {0:0}% to {1:0}%", reflectionLevel.Value * 100.0, value * 100.0)))
                {
                    reflectionLevel.Value = value;
                    action.Commit();
                }
            }
        }

        /// <summary>
        /// Gets or sets bump level
        /// </summary>
        public double BumpLevel
        {
            get { return bumpLevel.Value; }
            set
            {
                using (Transaction action = new Transaction(
                       String.Format("Change Bump Level from {0:0}% to {1:0}%", bumpLevel.Value * 100.0, value * 100.0)))
                {
                    bumpLevel.Value = value;
                    action.Commit();
                }
            }
        }

        /// <summary>
        /// Gets or sets emissive level
        /// </summary>
        public double EmissiveLevel
        {
            get { return emissiveLevel.Value; }
            set
            {
                using (Transaction action = new Transaction(
                       String.Format("Change Emissive Level from {0:0}% to {1:0}%", emissiveLevel.Value * 100.0, value * 100.0)))
                {
                    emissiveLevel.Value = value;
                    action.Commit();
                }
            }
        }

        /// <summary>
        /// Gets WPF material
        /// </summary>
        public System.Windows.Media.Media3D.Material VisualMaterial
        {
            get
            {
                if (material == null) CreateMaterial();
                return material;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public Material()
        {
            ambient.Changed += (s, a) => RaisePropertyChanged("Ambient");
            diffuse.Changed += (s, a) => RaisePropertyChanged("Diffuse");
            specular.Changed += (s, a) => RaisePropertyChanged("Specular");
            emissive.Changed += (s, a) => RaisePropertyChanged("Emissive");
            glossiness.Changed += (s, a) => RaisePropertyChanged("Glossiness");
            specularPower.Changed += (s, a) => RaisePropertyChanged("SpecularPower");
            reflectionLevel.Changed += (s, a) => RaisePropertyChanged("ReflectionLevel");
            bumpLevel.Changed += (s, a) => RaisePropertyChanged("BumpLevel");
            emissiveLevel.Changed += (s, a) => RaisePropertyChanged("EmissiveLevel");

            ambient.Changed += (s, a) => UpdateMaterial();
            diffuse.Changed += (s, a) => UpdateMaterial();
            specular.Changed += (s, a) => UpdateMaterial();
            emissive.Changed += (s, a) => UpdateMaterial();
            glossiness.Changed += (s, a) => UpdateMaterial();
            specularPower.Changed += (s, a) => UpdateMaterial();
            reflectionLevel.Changed += (s, a) => UpdateMaterial();
            bumpLevel.Changed += (s, a) => UpdateMaterial();
            emissiveLevel.Changed += (s, a) => UpdateMaterial();
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public Material(Color diffuse) : this()
        {
            this.diffuse.Value = diffuse;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Blends two materials
        /// </summary>
        /// <param name="a">Material</param>
        /// <param name="b">Material</param>
        /// <returns>Material</returns>
        public static Material Blend(Material a, Material b)
        {
            Material blended = new Material();

            blended.Ambient = Blend(a.Ambient, b.Ambient);
            blended.BumpLevel = (a.BumpLevel + b.BumpLevel) / 2.0;
            blended.Diffuse = Blend(a.Diffuse, b.Diffuse);
            blended.Emissive = Blend(a.Emissive, b.Emissive);
            blended.EmissiveLevel = (a.EmissiveLevel + b.EmissiveLevel) / 2.0;
            blended.Glossiness = (a.Glossiness + b.Glossiness) / 2.0;
            blended.ReflectionLevel = (a.ReflectionLevel + b.ReflectionLevel) / 2.0;
            blended.Specular = Blend(a.Specular, b.Specular);
            blended.SpecularPower = (a.SpecularPower + b.SpecularPower) / 2.0;

            return blended;
        }

        static Color Blend(Color a, Color b)
        {
            return Color.FromArgb(
                (byte) (((int) a.A + (int) b.A) / 2.0),
                (byte) (((int) a.R + (int) b.R) / 2.0),
                (byte) (((int) a.G + (int) b.G) / 2.0),
                (byte) (((int) a.B + (int) b.B) / 2.0));
        }

        #endregion

        #region Private methods

        // Creates new material
        void CreateMaterial()
        {
            // Creates materials
            material = new MaterialGroup();
            diffuseMaterial = new DiffuseMaterial(new SolidColorBrush(diffuse.Value)){AmbientColor = ambient.Value};
            if (bumpImage == null) bumpImage = new BitmapImage(new Uri("pack://application:,,,/NuGenBioChem;component/Images/Noise.png"));
            bumpMaterial = new DiffuseMaterial(new ImageBrush(bumpImage) { TileMode = TileMode.Tile, Opacity = bumpLevel.Value }) { Color = diffuse.Value, AmbientColor = ambient.Value };
            if(reflectionImage==null) reflectionImage = new BitmapImage(new Uri("pack://application:,,,/NuGenBioChem;component/Images/Clouds.png"));
            enviromentMaterial = new DiffuseMaterial(new ImageBrush(reflectionImage) { TileMode = TileMode.Tile, Opacity = reflectionLevel.Value }) { Color = diffuse.Value, AmbientColor = ambient.Value };
            emissiveMaterial = new EmissiveMaterial(new SolidColorBrush(emissive.Value){Opacity = emissiveLevel.Value});
            specularMaterial = new SpecularMaterial(new SolidColorBrush(specular.Value){Opacity = glossiness.Value},specularPower.Value);
            
            // Fill group
            if (diffuse.Value.A > 5) // Only if diffuse is not transparent
            {
                if ((bumpLevel.Value < 1) && (reflectionLevel.Value < 1)) material.Children.Add(diffuseMaterial);
                if ((bumpLevel.Value > 0) && (reflectionLevel.Value < 1)) material.Children.Add(bumpMaterial);
                if (reflectionLevel.Value > 0) material.Children.Add(enviromentMaterial);
                if ((emissiveLevel.Value > 0) && (emissive.Value != Colors.Black))
                    material.Children.Add(emissiveMaterial);
                if ((glossiness.Value > 0) && (specular.Value != Colors.Black)) material.Children.Add(specularMaterial);
            }
            RaisePropertyChanged("VisualMaterial");
        }

        /// <summary>
        /// Updates WPF material 
        /// </summary>
        void UpdateMaterial()
        {
            if (material == null) return;
            // Update material parameters
            ((SolidColorBrush)diffuseMaterial.Brush).Color = diffuse.Value;
            diffuseMaterial.AmbientColor = ambient.Value;
            bumpMaterial.Brush.Opacity = bumpLevel.Value;
            bumpMaterial.Color = diffuse.Value;
            bumpMaterial.AmbientColor = ambient.Value;
            enviromentMaterial.Brush.Opacity = reflectionLevel.Value;
            enviromentMaterial.Color = diffuse.Value;
            enviromentMaterial.AmbientColor = ambient.Value;
            ((SolidColorBrush)emissiveMaterial.Brush).Color = emissive.Value;
            emissiveMaterial.Brush.Opacity = emissiveLevel.Value;
            ((SolidColorBrush)specularMaterial.Brush).Color = specular.Value;
            specularMaterial.Brush.Opacity = specularPower.Value;
            specularMaterial.SpecularPower = 10 + 120.0 * (1.0 - glossiness.Value);

            // Fill group
            material.Children.Clear();
            if (diffuse.Value.A > 5) // Only if diffuse is not transparent
            {
                if ((bumpLevel.Value < 1) && (reflectionLevel.Value < 1)) material.Children.Add(diffuseMaterial);
                if ((bumpLevel.Value > 0) && (reflectionLevel.Value < 1)) material.Children.Add(bumpMaterial);
                if (reflectionLevel.Value > 0) material.Children.Add(enviromentMaterial);
                if ((emissiveLevel.Value > 0) && (emissive.Value != Colors.Black)) material.Children.Add(emissiveMaterial);
                if ((glossiness.Value > 0) && (specular.Value != Colors.Black)) material.Children.Add(specularMaterial);
            }
            RaisePropertyChanged("VisualMaterial");
        }

        #endregion

        #region Serialization

        /// <summary>
        /// Serializes material to string
        /// </summary>
        /// <returns></returns>
        public string SerializeToString()
        {
            return String.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}",
                                Ambient.ToString(CultureInfo.InvariantCulture),
                                Diffuse.ToString(CultureInfo.InvariantCulture),
                                Specular.ToString(CultureInfo.InvariantCulture),
                                Emissive.ToString(CultureInfo.InvariantCulture),
                                Glossiness.ToString(CultureInfo.InvariantCulture),
                                SpecularPower.ToString(CultureInfo.InvariantCulture),
                                ReflectionLevel.ToString(CultureInfo.InvariantCulture),
                                BumpLevel.ToString(CultureInfo.InvariantCulture),
                                EmissiveLevel.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Deserialize from string
        /// </summary>
        /// <param name="line">Line with data</param>
        public void DeserializeFromString(string line)
        {
            string[] data = line.Split('|');

            ambient.Value = (Color) ColorConverter.ConvertFromString(data[0]);
            diffuse.Value = (Color) ColorConverter.ConvertFromString(data[1]);
            specular.Value = (Color) ColorConverter.ConvertFromString(data[2]);
            emissive.Value = (Color) ColorConverter.ConvertFromString(data[3]);
            glossiness.Value = Convert.ToDouble(data[4], CultureInfo.InvariantCulture);
            specularPower.Value = Convert.ToDouble(data[5], CultureInfo.InvariantCulture);
            reflectionLevel.Value = Convert.ToDouble(data[6], CultureInfo.InvariantCulture);
            bumpLevel.Value = Convert.ToDouble(data[7], CultureInfo.InvariantCulture);
            emissiveLevel.Value = Convert.ToDouble(data[8], CultureInfo.InvariantCulture);

            UpdateMaterial();
        }

        #endregion
    }
}
