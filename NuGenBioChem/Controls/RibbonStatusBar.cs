using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Fluent;

namespace NuGenBioChem.Controls
{
    /// <summary>
    /// Represents ribbon status bar
    /// </summary>
    public class RibbonStatusBar : StatusBar
    {
        #region Fields

        // Context menu
        private Fluent.ContextMenu contextMenu = new Fluent.ContextMenu();

        private System.Windows.Window ownerWindow;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets whether window is maximized
        /// </summary>
        public bool IsWindowMaximized
        {
            get { return (bool)GetValue(IsWindowMaximizedProperty); }
            set { SetValue(IsWindowMaximizedProperty, value); }
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for IsWindowMaximized.  This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty IsWindowMaximizedProperty =
            DependencyProperty.Register("IsWindowMaximized", typeof(bool), typeof(RibbonStatusBar), new UIPropertyMetadata(false));        

        #endregion

        #region Constructors

        /// <summary>
        /// Static constructor
        /// </summary>
        static RibbonStatusBar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RibbonStatusBar), new FrameworkPropertyMetadata(typeof(RibbonStatusBar)));
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public RibbonStatusBar()
        {
            RecreateMenu();
            ContextMenu = contextMenu;

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (ownerWindow != null)
            {
                ownerWindow.StateChanged -= OnWindowStateChanged;
                ownerWindow = null;
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (ownerWindow == null) ownerWindow = Window.GetWindow(this);
            if (ownerWindow != null)
            {
                ownerWindow.StateChanged += OnWindowStateChanged;
                if ((ownerWindow.ResizeMode==ResizeMode.CanResizeWithGrip) && (ownerWindow.WindowState == WindowState.Maximized)) IsWindowMaximized = true;
                else IsWindowMaximized = false;
            }
        }

        private void OnWindowStateChanged(object sender, EventArgs e)
        {
            if ((ownerWindow.ResizeMode==ResizeMode.CanResizeWithGrip) && (ownerWindow.WindowState == WindowState.Maximized)) IsWindowMaximized = true;
            else IsWindowMaximized = false;
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Creates or identifies the element that is used to display the given item.
        /// </summary>
        /// <returns>The element that is used to display the given item.</returns>
        protected override DependencyObject GetContainerForItemOverride()
        {
            return new RibbonStatusBarItem();
        }

        /// <summary>
        /// Determines if the specified item is (or is eligible to be) its own container.
        /// </summary>
        /// <param name="item">The item to check.</param>
        /// <returns>true if the item is (or is eligible to be) its own container; otherwise, false.</returns>
        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return (item is RibbonStatusBarItem) || (item is Separator);
        }

        /// <summary>
        /// Invoked when the <see cref="P:System.Windows.Controls.ItemsControl.Items"/> property changes.
        /// </summary>
        /// <param name="e">Information about the change.</param>
        protected override void OnItemsChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);
            switch(e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        for (int i = 0; i < e.NewItems.Count;i++ )
                        {
                            RibbonStatusBarItem item = e.NewItems[i] as RibbonStatusBarItem;
                            if (item != null)
                            {
                                item.Checked += OnItemChecked;
                                item.Unchecked += OnItemUnchecked;
                                contextMenu.Items.Insert(e.NewStartingIndex + i + 1, new RibbonStatusBarMenuItem(item));
                            }
                            else contextMenu.Items.Insert(e.NewStartingIndex + i + 1, new Separator());
                        }
                        break;
                    }
                case NotifyCollectionChangedAction.Move:
                    {
                        for (int i = 0; i < e.NewItems.Count; i++)
                        {
                            UIElement menuItem = contextMenu.Items[e.OldStartingIndex + 1];
                            contextMenu.Items.RemoveAt(e.OldStartingIndex + 1);
                            contextMenu.Items.Insert(e.NewStartingIndex + i + 1, menuItem);
                        }
                        break;
                    }
                case NotifyCollectionChangedAction.Remove:
                    {
                        for (int i = 0; i < e.OldItems.Count; i++)
                        {
                            RibbonStatusBarMenuItem menuItem = contextMenu.Items[e.OldStartingIndex + 1] as RibbonStatusBarMenuItem;
                            if(menuItem!=null)
                            {
                                menuItem.StatusBarItem.Checked += OnItemChecked;
                                menuItem.StatusBarItem.Unchecked += OnItemUnchecked;
                            }
                            contextMenu.Items.RemoveAt(e.OldStartingIndex+1);
                        }
                        break;
                    }
                case NotifyCollectionChangedAction.Replace:
                    {
                        for (int i = 0; i < e.OldItems.Count; i++)
                        {
                            RibbonStatusBarMenuItem menuItem = contextMenu.Items[e.OldStartingIndex + 1] as RibbonStatusBarMenuItem;
                            if (menuItem != null)
                            {
                                menuItem.StatusBarItem.Checked += OnItemChecked;
                                menuItem.StatusBarItem.Unchecked += OnItemUnchecked;
                            }
                            contextMenu.Items.RemoveAt(e.OldStartingIndex + 1);
                        }
                        for (int i = 0; i < e.NewItems.Count; i++)
                        {
                            RibbonStatusBarItem item = e.NewItems[i] as RibbonStatusBarItem;
                            if (item != null)
                            {
                                item.Checked += OnItemChecked;
                                item.Unchecked += OnItemUnchecked;
                                contextMenu.Items.Insert(e.NewStartingIndex + i + 1, new RibbonStatusBarMenuItem(item));
                            }
                            else contextMenu.Items.Insert(e.NewStartingIndex + i + 1, new Separator());
                        }
                        break;
                    }
                case NotifyCollectionChangedAction.Reset:
                    {
                        RecreateMenu();
                        break;
                    }
            }
        }

        private void OnItemUnchecked(object sender, RoutedEventArgs e)
        {
            UpdateSeparartorsVisibility();
        }

        private void OnItemChecked(object sender, RoutedEventArgs e)
        {
            UpdateSeparartorsVisibility();
        }

        #endregion

        #region Private Methods

        // Creates menu
        private void RecreateMenu()
        {
            contextMenu.Items.Clear();
            // Adding header separator
            contextMenu.Items.Add(new GroupSeparatorMenuItem(){Text="Customize Status Bar"});
            for(int i=0;i<Items.Count;i++)
            {
                RibbonStatusBarItem item = Items[i] as RibbonStatusBarItem;
                if (item != null)
                {
                    item.Checked += OnItemChecked;
                    item.Unchecked += OnItemUnchecked;
                    contextMenu.Items.Add(new RibbonStatusBarMenuItem(item));
                }
                else contextMenu.Items.Add(new Separator());
            }

            UpdateSeparartorsVisibility();
        }

        // Updates separators visibility, to not duplicate
        private void UpdateSeparartorsVisibility()
        {
            bool isPrevSeparator = false;
            bool isFirstVsible = true;
            for (int i = 0; i < Items.Count; i++)
            {
                Separator item = Items[i] as Separator;
                if (item != null)
                {
                    if (isPrevSeparator || isFirstVsible) item.Visibility = Visibility.Collapsed;
                    else item.Visibility = Visibility.Visible;
                    isPrevSeparator = true;
                    isFirstVsible = false;
                }
                else if (Items[i] is RibbonStatusBarItem) if ((Items[i] as RibbonStatusBarItem).Visibility == Visibility.Visible)
                {
                    isPrevSeparator = false;
                    isFirstVsible = false;
                }
            }
        }

        #endregion
    }
}
