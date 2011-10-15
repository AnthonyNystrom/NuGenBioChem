using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace NuGenBioChem.Controls
{
    /// <summary>
    /// Represents undo items for undo button
    /// </summary>
    public class UndoItem:ContentControl
    {
        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether undo item is selected
        /// </summary>
        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for IsSelected.  This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(UndoItem), new UIPropertyMetadata(false));
       
        #endregion

        #region Constructors

        /// <summary>
        /// Static constructor
        /// </summary>
        static UndoItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(UndoItem), new FrameworkPropertyMetadata(typeof(UndoItem)));
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Invoked when an unhandled <see cref="E:System.Windows.Input.Mouse.MouseEnter"/> attached event is raised on this element. Implement this method to add class handling for this event. 
        /// </summary>
        /// <param name="e">The <see cref="T:System.Windows.Input.MouseEventArgs"/> that contains the event data.</param>
        protected override void OnMouseEnter(System.Windows.Input.MouseEventArgs e)
        {
            UndoButton parent = Parent as UndoButton;
            if (parent != null) parent.Select(this);
            base.OnMouseEnter(e);
        }

        /// <summary>
        /// Invoked when an unhandled <see cref="E:System.Windows.Input.Mouse.MouseLeave"/> attached event is raised on this element. Implement this method to add class handling for this event. 
        /// </summary>
        /// <param name="e">The <see cref="T:System.Windows.Input.MouseEventArgs"/> that contains the event data.</param>
        protected override void OnMouseLeave(System.Windows.Input.MouseEventArgs e)
        {
            UndoButton parent = Parent as UndoButton;
            if (parent != null) parent.Deselect();
            base.OnMouseLeave(e);
        }

        /// <summary>
        /// Invoked when an unhandled <see cref="E:System.Windows.UIElement.MouseLeftButtonUp"/> routed event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. 
        /// </summary>
        /// <param name="e">The <see cref="T:System.Windows.Input.MouseButtonEventArgs"/> that contains the event data. The event data reports that the left mouse button was released.</param>
        protected override void OnMouseLeftButtonUp(System.Windows.Input.MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            if(e.ClickCount==1)
            {
                UndoButton parent = Parent as UndoButton;
                if (parent != null) parent.ItemClick(this);
            }
        }

        #endregion
    }
}
