using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Windows.Data;

namespace NuGenBioChem.Data
{
    /// <summary>
    /// Represents recent item
    /// </summary>
    public class RecentItem : INotifyPropertyChanged
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

        #region Properties

        #region Path

        private string path;

        /// <summary>
        /// Gets or sets path
        /// </summary>
        public string Path
        {
            get { return path; }
        }

        #endregion

        #region IsPinned

        private bool isPinned;

        /// <summary>
        /// Gets or sets a value indicating whether file is pinned
        /// </summary>
        public bool IsPinned
        {
            get { return isPinned; }
            set
            {
                isPinned = value;
                RaisePropertyChanged("IsPinned");
            }
        }

        #endregion

        #endregion

        #region Constructor
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="path">Path</param>
        public RecentItem(string path)
        {
            this.path = path;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="path">Path</param>
        /// <param name="isPinned">Is pinned</param>
        public RecentItem(string path, bool isPinned)
        {
            this.path = path;
            this.isPinned = isPinned;
        }

        #endregion
    }

    /// <summary>
    /// Represents recent items collection
    /// </summary>
    public class RecentItems : ObservableCollection<RecentItem>
    {
        #region Constants

        private const char Separator = '|';

        #endregion

        #region Fields

        // Path for settings file
        private string path;

        #endregion

        #region Methods

        /// <summary>
        /// Inserts an item into the collection at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="item"/> should be inserted.</param><param name="item">The object to insert.</param>
        protected override void InsertItem(int index, RecentItem item)
        {
            const int maxRecentItemsCount = 22;
            RecentItem previousItem = this.FirstOrDefault(x => x.Path == item.Path);
            if (previousItem!=null) Move(IndexOf(previousItem), 0);
            else base.InsertItem(0, item);
            while (Count > maxRecentItemsCount)
            {
                RecentItem lastItem = Items.LastOrDefault(x => !x.IsPinned);
                if (lastItem != null) Remove(lastItem);
                else break;
            }
            Save();
        }

        /// <summary>
        /// Saves recent items to file
        /// </summary>
        private void Save()
        {
            using (StreamWriter writer = new StreamWriter(Storage.OpenFile(path,FileMode.Create, FileAccess.Write)))
            {
                foreach (RecentItem item in this)
                {
                    string data = string.Format("{0}|{1}", item.Path, item.IsPinned.ToString(CultureInfo.InvariantCulture));
                    writer.WriteLine(data);
                }
                writer.Flush();
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Collections.ObjectModel.ObservableCollection`1.CollectionChanged"/> event with the provided arguments.
        /// </summary>
        /// <param name="e">Arguments of the event being raised.</param>
        protected override void OnCollectionChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            base.OnCollectionChanged(e);
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (INotifyPropertyChanged item in e.NewItems)
                    {
                        item.PropertyChanged += OnItemPropertyChanged;
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (INotifyPropertyChanged item in e.OldItems)
                    {
                        item.PropertyChanged -= OnItemPropertyChanged;
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    foreach (INotifyPropertyChanged item in Items)
                    {
                        item.PropertyChanged -= OnItemPropertyChanged;
                    }
                    break;
            }
            CollectionViewSource.GetDefaultView(this).Refresh();
        }

        private void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "IsPinned") Save();
            CollectionViewSource.GetDefaultView(this).Refresh();
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Create recent item from file
        /// </summary>
        /// <param name="path">File path with recent items</param>
        public RecentItems(string path)
        {
            this.path = path;

            // Load items from file
            if(Storage.FileExists(path))
            {
                using (StreamReader reader = new StreamReader(Storage.OpenFile(path, FileMode.Open, FileAccess.Read)))
                {
                    while(!reader.EndOfStream)
                    {
                        string data = reader.ReadLine();
                        string[] dataStrings = data.Split(Separator);
                        if(dataStrings.Length != 2) throw new Exception("Incorrect recent items file format.");
                        RecentItem item = new RecentItem(dataStrings[0], Convert.ToBoolean(dataStrings[1], CultureInfo.InvariantCulture));
                        base.InsertItem(Count,item);
                    }
                }
            }

            GroupDescription description = new PropertyGroupDescription("IsPinned");
            description.GroupNames.Add(true);
            description.GroupNames.Add(false);

            CollectionViewSource.GetDefaultView(this).GroupDescriptions.Add(description);            
        }

        #endregion
    }      
}
