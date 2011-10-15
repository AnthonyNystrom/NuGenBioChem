using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;

namespace NuGenBioChem.Data
{
    /// <summary>
    /// Represents application settings
    /// </summary>
    public class Settings: INotifyPropertyChanged
    {
        #region Constants

        private const string RecentFilesPath = "RecentFiles";
        private const string RecentDirectoriesPath = "RecentDirectories";

        #endregion

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

        #region Properties

        #region RecentFiles

        private RecentItems recentFiles;

        /// <summary>
        /// Gets recent files collection
        /// </summary>
        public RecentItems RecentFiles
        {
            get
            {
                if (recentFiles == null)
                {
                    recentFiles = new RecentItems(RecentFilesPath);         
                }
                return recentFiles;
            }
        }

        #endregion

        #region RecentDirectories

        private RecentItems recentDirectories;

        /// <summary>
        /// Gets recent directories collection
        /// </summary>
        public RecentItems RecentDirectories
        {
            get
            {
                if (recentDirectories == null)
                {
                    recentDirectories = new RecentItems(RecentDirectoriesPath);
                }
                return recentDirectories;
            }
        }

        #endregion

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        public Settings()
        {
            
        }

        #endregion
        
        #region Methods

        /// <summary>
        /// Add recent file
        /// </summary>
        /// <param name="path">Recent file path</param>
        public void AddRecentFile(string path)
        {
            path = Path.GetFullPath(path);
            RecentFiles.Add(new RecentItem(path));
            RecentDirectories.Add(new RecentItem(Path.GetDirectoryName(path)));
        }

        #endregion
    }
}
