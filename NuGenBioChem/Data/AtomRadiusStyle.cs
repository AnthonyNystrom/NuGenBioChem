

namespace NuGenBioChem.Data
{
    /// <summary>
    /// Defines what radius must be used during visualization
    /// </summary>
    public enum AtomSizeStyle
    {
        /// <summary>
        /// All atoms must have equal radiuses
        /// </summary>
        Uniform,

        /// <summary>
        /// Calculated radius of the element must be used
        /// </summary>
        Calculated,

        /// <summary>
        /// Empirical radius of the element must be used
        /// </summary>
        Empirical,

        /// <summary>
        /// Covalent radius of the element must be used
        /// </summary>
        Covalent,

        /// <summary>
        /// Van der Waals radius of the element must be used
        /// </summary>
        VanderWaals
    }
}
