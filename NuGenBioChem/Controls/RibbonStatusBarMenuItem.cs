using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Fluent;

namespace NuGenBioChem.Controls
{
    /// <summary>
    /// Represents menu item in ribbon status bar menu
    /// </summary>
    public class RibbonStatusBarMenuItem : Fluent.MenuItem
    {
        #region Properties

        /// <summary>
        /// Gets or sets Ribbon Status Bar menu item
        /// </summary>
        public RibbonStatusBarItem StatusBarItem
        {
            get { return (RibbonStatusBarItem)GetValue(StatusBarItemProperty); }
            set { SetValue(StatusBarItemProperty, value); }
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for StatusBarItem.  This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty StatusBarItemProperty =
            DependencyProperty.Register("StatusBarItem", typeof(RibbonStatusBarItem), typeof(RibbonStatusBarMenuItem), new UIPropertyMetadata(null));

        
        #endregion

        #region Constructors

        /// <summary>
        /// Static constructor
        /// </summary>
        static RibbonStatusBarMenuItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RibbonStatusBarMenuItem), new FrameworkPropertyMetadata(typeof(RibbonStatusBarMenuItem)));
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="item">Ribbon Status Bar menu item</param>
        public RibbonStatusBarMenuItem(RibbonStatusBarItem item)
        {
            StatusBarItem = item;
        }

        #endregion
    }
}
