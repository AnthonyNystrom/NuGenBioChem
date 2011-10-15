
namespace NuGenBioChem.Data.Importers
{
    /// <summary>
    /// Base interface for all importers
    /// </summary>
    public interface IFileImporter
    {
        /// <summary>
        /// Gets molecule collection imported from the file
        /// </summary>
        MoleculeCollection Molecules { get; }

        /// <summary>
        /// Gets import succesful indicator
        /// </summary>
        bool IsSuccessful { get; }

        /// <summary>
        /// Gets message about import process
        /// </summary>
        string Messages { get; }
    }
}
