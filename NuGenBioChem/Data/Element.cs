using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Media;

namespace NuGenBioChem.Data
{
    /// <summary>
    /// Represents chemical element
    /// </summary>
    public class Element
    {
        #region Public Fields

        /// <summary>
        /// Symbol of the element
        /// </summary>
        public string Symbol { get; private set; }

        /// <summary>
        /// Name of the element
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Number
        /// </summary>
        public int Number { get; private set; }

        /// <summary>
        /// Group (0 if not present)
        /// </summary>
        public readonly int Group;

        /// <summary>
        /// Period (0 if not present)
        /// </summary>
        public readonly int Period;

        /// <summary>
        /// Van der Waals radius (Double.NaN if not present)
        /// </summary>
        public readonly double VanderWaalsRadius;

        /// <summary>
        /// Empirical radius (Double.NaN if not present)
        /// </summary>
        public readonly double EmpiricalRadius;

        /// <summary>
        /// Calculated radius (Double.NaN if not present)
        /// </summary>
        public readonly double CalculatedRadius;

        /// <summary>
        /// Calculated radius (Double.NaN if not present)
        /// </summary>
        public readonly double CovalentRadius;

        #endregion

        #region Static Fields

        // Elements by symbol
        static Dictionary<string, Element> elementsBySymbol;
        static Element[] elements;

        /// <summary>
        /// Gets all elements
        /// </summary>
        public static Element[] Elements
        {
            get { return elements; }
        }

        static Element undefined;

        /// <summary>
        /// Gets the stub element
        /// </summary>
        public static Element Undefined
        {
            get
            {
                if (undefined == null)
                {
                    undefined = new Element("Udf", "Undefined", 0, 0, 0, AverageVanderWaalsRadius, AverageEmpiricalRadius, AverageCalculatedRadius, AverageCovalentRadius);
                }
                return undefined;
            }
        }

        #endregion

        #region Row and Colum

        /// <summary>
        /// Gets real row position in table
        /// </summary>
        public int Row
        {
            get
            {
                if (Group == 0)
                {
                    return Period + 2;
                }
                else return Period-1;
            }
        }

        /// <summary>
        /// Gets real column position in table
        /// </summary>
        public int Column
        {
            get
            {
                if (Group == 0)
                {
                    if (Row == 8) return Number - 57 + 3;
                    return Number - 89 + 3;
                }
                else return Group-1;
            }
        }

        #endregion

        #region Average Radiuses

        static double averageVanderWaalsRadius = 0.0;
        static double averageEmpiricalRadius = 0.0;
        static double averageCalculatedRadius = 0.0;
        static double averageCovalentRadius = 0.0;

        /// <summary>
        /// Gets average Van der Waals Radius
        /// </summary>
        public static double AverageVanderWaalsRadius
        {
            get
            {
                if (averageVanderWaalsRadius == 0.0)
                {
                    double summ = 0;
                    double count = 0;
                    for (int i = 0; i < Elements.Length; i++)
                    {
                        double radius = Elements[i].VanderWaalsRadius;
                        if (Double.IsNaN(radius)) continue;
                        summ += radius;
                        count += 1.0;
                    }
                    averageVanderWaalsRadius = count != 0.0 ? summ / count : 0;
                }
                return averageVanderWaalsRadius;
            }
        }

        /// <summary>
        /// Gets average Empirical Radius
        /// </summary>
        public static double AverageEmpiricalRadius
        {
            get
            {
                if (averageEmpiricalRadius == 0.0)
                {
                    double summ = 0;
                    double count = 0;
                    for (int i = 0; i < Elements.Length; i++)
                    {
                        double radius = Elements[i].EmpiricalRadius;
                        if (Double.IsNaN(radius)) continue;
                        summ += radius;
                        count += 1.0;
                    }
                    averageEmpiricalRadius = count != 0.0 ? summ / count : 0;
                }
                return averageEmpiricalRadius;
            }
        }

        /// <summary>
        /// Gets average calculated radius
        /// </summary>
        public static double AverageCalculatedRadius
        {
            get
            {
                if (averageCalculatedRadius == 0.0)
                {
                    double summ = 0;
                    double count = 0;
                    for (int i = 0; i < Elements.Length; i++)
                    {
                        double radius = Elements[i].CalculatedRadius;
                        if (Double.IsNaN(radius)) continue;
                        summ += radius;
                        count += 1.0;
                    }
                    averageCalculatedRadius = count != 0.0 ? summ / count : 0;
                }
                return averageCalculatedRadius;
            }
        }

        /// <summary>
        /// Gets average covalent radius
        /// </summary>
        public static double AverageCovalentRadius
        {
            get
            {
                if (averageCovalentRadius == 0.0)
                {
                    double summ = 0;
                    double count = 0;
                    for (int i = 0; i < Elements.Length; i++)
                    {
                        double radius = Elements[i].CovalentRadius;
                        if (Double.IsNaN(radius)) continue;
                        summ += radius;
                        count += 1.0;
                    }
                    averageCovalentRadius = count != 0.0 ? summ / count : 0;
                }
                return averageCovalentRadius;
            }
        }

        #endregion

        #region Initialization

        Element(string symbol, string name, int number, int group, int period, double vanderWaalsRadius, double empiricalRadius, double calculatedRadius, double covalentRadius)
        {
            Period = period;
            Number = number;
            Symbol = symbol;
            CovalentRadius = covalentRadius;
            CalculatedRadius = calculatedRadius;
            EmpiricalRadius = empiricalRadius;
            VanderWaalsRadius = vanderWaalsRadius;
            Group = group;
            Name = name;
        }
  
        static Element()
        {
            Load();
        }

        static void Load()
        {
            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("NuGenBioChem.Resources.Elements.Elements.csv");
            StreamReader reader = new StreamReader(stream);
            string[] lines = reader.ReadToEnd().Split('\n');

            // Clears existed data
            elementsBySymbol = new Dictionary<string, Element>(120);
            elements = null;

            // (Skip title line #0)
            for (int i = 1; i < lines.Length; i++)
            {
                string[] splitted = lines[i].Split(';');
                if (splitted.Length < 9) continue; // Skip empty/incorrect lines

                Element element = new Element(
                    splitted[1],   // Symbol
                    splitted[2],   // Name
                    ParseInt(splitted[0]), // Number
                    ParseInt(splitted[7]), // Group
                    ParseInt(splitted[8]), // Period
                    ParseDouble(splitted[5]), // Van der Waals radius
                    ParseDouble(splitted[3]), // Empirical radius
                    ParseDouble(splitted[4]), // Calculated radius
                    ParseDouble(splitted[6]) // Covalent radius
                    );

                elementsBySymbol.Add(element.Symbol, element);
            }

            // Set all elements array
            elements = elementsBySymbol.Values.ToArray();
        }

        static int ParseInt(string text)
        {
            if (String.IsNullOrEmpty(text)) return 0;
            int result = 0;
            Int32.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out result);
            return result;
        }

        static double ParseDouble(string text)
        {
            if (String.IsNullOrEmpty(text)) return Double.NaN;
            double result = Double.NaN;
            Double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out result);
            return result;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets element by its symbol
        /// </summary>
        /// <param name="symbol">Symbol (must be started with upper case letter)</param>
        /// <returns>Element of null if not present</returns>
        public static Element GetBySymbol(string symbol)
        {
            Element element = null;
            elementsBySymbol.TryGetValue(symbol, out element);
            return element;
        }

        /// <summary>
        /// Gets string representation of this element
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("[{0}] {1}", Symbol, Name);
        }

        #endregion
    }
}
