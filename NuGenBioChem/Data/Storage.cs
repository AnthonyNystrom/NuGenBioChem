using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;

namespace NuGenBioChem.Data
{
    /// <summary>
    /// Encapsulates isolated storage functionality
    /// </summary>
    public static class Storage
    {
        #region Fields

        // Isolated storage
        static IsolatedStorageFile storage = null;
        // Root directory in storage
        static string rootDirectory = "NuGenBioChemPreviewJuly2010";

        #endregion

        #region Initialization

        static Storage()
        {
            storage = IsolatedStorageFile.GetUserStoreForAssembly();
            if (!storage.DirectoryExists(rootDirectory)) storage.CreateDirectory(rootDirectory);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Deletes all directories and files in the storage
        /// </summary>
        public static void Clear()
        {
            DeleteDirectoryWithSubdirectories(rootDirectory);
        }

        /// <summary>
        /// Determinates whether the given file name is an appropriate one
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static bool ValidateFileName(string fileName)
        {
            return fileName.IndexOfAny(new char[] { '/', '\\', ':', '*', '?', '"', '<', '>', '|'}) == -1;
        }

        /// <summary>
        /// Checks whether the directory exists
        /// </summary>
        /// <param name="directoryName"></param>
        /// <returns></returns>
        public static bool DirectoryExists(string directoryName)
        {
            string path = rootDirectory + "\\" + directoryName;
            return storage.DirectoryExists(path);
        }

        /// <summary>
        /// Creates a new directory
        /// </summary>
        /// <param name="directoryName"></param>
        public static void CreateDirectory(string directoryName)
        {
            string path = rootDirectory + "\\" + directoryName;
            storage.CreateDirectory(path);
        }

        /// <summary>
        /// Checks whether the file exists
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static bool FileExists(string fileName)
        {
            string path = rootDirectory + "\\" + fileName;
            return storage.FileExists(path);
        }

        /// <summary>
        /// Creates a new file
        /// </summary>
        /// <param name="fileName"></param>
        public static void CreateFile(string fileName)
        {
            string path = rootDirectory + "\\" + fileName;
            storage.CreateFile(path);
        }

        /// <summary>
        /// Opens a file
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <param name="fileMode">File mode</param>
        /// <param name="fileAccess">File Access</param>
        /// <returns>Stream</returns>
        public static IsolatedStorageFileStream OpenFile(string fileName, FileMode fileMode, FileAccess fileAccess)
        {
            string path = rootDirectory + "\\" + fileName;
            IsolatedStorageFileStream stream = new IsolatedStorageFileStream(path, fileMode, fileAccess, storage);
            return stream;
        }

        /// <summary>
        /// Moves a file
        /// </summary>
        /// <param name="sourceFileName">Source file name</param>
        /// <param name="destinationFileName">Destination file name</param>
        public static void MoveFile(string sourceFileName, string destinationFileName)
        {
            storage.MoveFile(rootDirectory + "\\" + sourceFileName, rootDirectory + "\\" + destinationFileName);
        }

        /// <summary>
        /// Gets file names that match a search pattern
        /// </summary>
        /// <param name="searchPattern">Search pattern</param>
        /// <returns>File names</returns>
        public static string[] GetFileNames(string searchPattern)
        {
            return storage.GetFileNames(rootDirectory + "\\" + searchPattern);
        }

        /// <summary>
        /// Deletes a file
        /// </summary>
        /// <param name="fileName">Name of the file</param>
        public static void DeleteFile(string fileName)
        {
            storage.DeleteFile(rootDirectory + "\\" + fileName);
        }

        /// <summary>
        /// Deletes a directory
        /// </summary>
        /// <param name="path">Name of the directory</param>
        public static void DeleteDirectory(string path)
        {
            DeleteDirectoryWithSubdirectories(rootDirectory + "\\" + path);
        }

        #endregion

        #region Private

        /// <summary>
        /// Deletes a directory
        /// </summary>
        /// <param name="path">Name of the directory</param>
        public static void DeleteDirectoryWithSubdirectories(string path)
        {
            string[] subdirectories = storage.GetDirectoryNames(path + "\\*");
            foreach (string subdirectory in subdirectories) DeleteDirectoryWithSubdirectories(path + "\\" + subdirectory);
            string[] files = storage.GetFileNames(path + "\\*");
            foreach (string file in files) storage.DeleteFile(path + "\\" + file);
            storage.DeleteDirectory(path);
        }

        #endregion
    }
}
