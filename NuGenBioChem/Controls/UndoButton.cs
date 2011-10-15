using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Fluent;

namespace NuGenBioChem.Controls
{
    /// <summary>
    /// Represents undo button for quick access toolbar
    /// </summary>
    public class UndoButton : ItemsControl, IQuickAccessItemProvider, ICommandSource
    {
        #region Fields

        // Hint text block
        private TextBlock hintTextBlock;

        // Cached can execute value
        private bool currentCanExecute = true;

        #endregion

        #region Properties

        #region IsOpen

        /// <summary>
        /// Gets or sets a value indicating popup is open
        /// </summary>
        public bool IsOpen
        {
            get { return (bool)GetValue(IsOpenProperty); }
            set { SetValue(IsOpenProperty, value); }
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for IsOpen.  This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty IsOpenProperty =
            DependencyProperty.Register("IsOpen", typeof(bool), typeof(UndoButton), new UIPropertyMetadata(false));

        #endregion

        #region Click

        /// <summary>
        /// Occurs when is clicked.
        /// </summary>
        [Category("Behavior")]
        public event RoutedEventHandler Click
        {
            add
            {
                AddHandler(ClickEvent, value);
            }
            remove
            {
                RemoveHandler(ClickEvent, value);
            }
        }
        /// <summary>
        /// Identifies the Click routed event.
        /// </summary>
        public static readonly RoutedEvent ClickEvent = EventManager.RegisterRoutedEvent("Click", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(UndoButton));

        #endregion

        #region Command

        /// <summary>
        /// Gets or sets the command to invoke when this button is pressed. This is a dependency property.
        /// </summary>
        [Category("Action"), Localizability(LocalizationCategory.NeverLocalize), Bindable(true)]
        public ICommand Command
        {
            get
            {
                return (ICommand)GetValue(CommandProperty);
            }
            set
            {
                SetValue(CommandProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the parameter to pass to the System.Windows.Controls.Primitives.ButtonBase.Command property. This is a dependency property.
        /// </summary>
        [Bindable(true), Localizability(LocalizationCategory.NeverLocalize), Category("Action")]
        public object CommandParameter
        {
            get
            {
                return GetValue(CommandParameterProperty);
            }
            set
            {
                SetValue(CommandParameterProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the element on which to raise the specified command. This is a dependency property.
        /// </summary>
        [Bindable(true), Category("Action")]
        public IInputElement CommandTarget
        {
            get
            {
                return (IInputElement)GetValue(CommandTargetProperty);
            }
            set
            {
                SetValue(CommandTargetProperty, value);
            }
        }

        /// <summary>
        /// Identifies the System.Windows.Controls.Primitives.ButtonBase.CommandParameter dependency property.
        /// </summary>
        public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register("CommandParameter", typeof(object), typeof(UndoButton), new FrameworkPropertyMetadata(null));
        /// <summary>
        /// Identifies the routed System.Windows.Controls.Primitives.ButtonBase.Command dependency property.
        /// </summary>
        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register("Command", typeof(ICommand), typeof(UndoButton), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnCommandChanged)));

        /// <summary>
        /// Identifies the System.Windows.Controls.Primitives.ButtonBase.CommandTarget dependency property.
        /// </summary>
        public static readonly DependencyProperty CommandTargetProperty = DependencyProperty.Register("CommandTarget", typeof(IInputElement), typeof(UndoButton), new FrameworkPropertyMetadata(null));

        // Keep a copy of the handler so it doesn't get garbage collected.
        private static EventHandler canExecuteChangedHandler;

        /// <summary>
        /// Handles Command changed
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            UndoButton control = d as UndoButton;
            EventHandler handler = control.OnCommandCanExecuteChanged;
            if (e.OldValue != null)
            {
                (e.OldValue as ICommand).CanExecuteChanged -= handler;
            }
            if (e.NewValue != null)
            {
                handler = new EventHandler(control.OnCommandCanExecuteChanged);
                canExecuteChangedHandler = handler;
                (e.NewValue as ICommand).CanExecuteChanged += handler;
            }
            control.UpdateCanExecute(); ;
        }
        /// <summary>
        /// Handles Command CanExecute changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCommandCanExecuteChanged(object sender, EventArgs e)
        {
            UpdateCanExecute();
        }

        private void UpdateCanExecute()
        {
            bool canExecute = Command != null && CanExecuteCommand();
            if (currentCanExecute != canExecute)
            {
                currentCanExecute = canExecute;
                CoerceValue(IsEnabledProperty);
            }
        }

        /// <summary>
        /// Execute command
        /// </summary>
        protected void ExecuteCommand()
        {
            ICommand command = Command;
            if (command != null)
            {
                object commandParameter = CommandParameter;
                RoutedCommand routedCommand = command as RoutedCommand;
                if (routedCommand != null)
                {
                    if (routedCommand.CanExecute(commandParameter, CommandTarget))
                    {
                        routedCommand.Execute(commandParameter, CommandTarget);
                    }
                }
                else if (command.CanExecute(commandParameter))
                {
                    command.Execute(commandParameter);
                }
            }
        }

        /// <summary>
        /// Determines whether the Command can be executed
        /// </summary>
        /// <returns>Returns Command CanExecute</returns>
        protected bool CanExecuteCommand()
        {
            ICommand command = Command;
            if (command == null)
            {
                return false;
            }
            object commandParameter = CommandParameter;
            RoutedCommand routedCommand = command as RoutedCommand;
            if (routedCommand == null)
            {
                return command.CanExecute(commandParameter);
            }
            return routedCommand.CanExecute(commandParameter, CommandTarget);
        }

        #endregion

        #region IsEnabled

        /// <summary>
        /// Handles IsEnabled changes
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e">The event data.</param>
        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        /// <summary>
        /// Coerces IsEnabled 
        /// </summary>
        /// <param name="d"></param>
        /// <param name="basevalue"></param>
        /// <returns></returns>
        private static object CoerceIsEnabled(DependencyObject d, object basevalue)
        {
            UndoButton control = (d as UndoButton);            
            UIElement parent = LogicalTreeHelper.GetParent(control) as UIElement;
            bool parentIsEnabled = parent == null || parent.IsEnabled;
            bool commandIsEnabled = control.Command == null || control.currentCanExecute;

            // We force disable if parent is disabled or command cannot be executed
            return (bool)basevalue && parentIsEnabled && commandIsEnabled;

        }

        #endregion

        #region HintText

        /// <summary>
        /// Gets or sets hint text
        /// </summary>
        public string HintText
        {
            get { return (string)GetValue(HintTextProperty); }
            set { SetValue(HintTextProperty, value); }
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for HintText.  This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty HintTextProperty =
            DependencyProperty.Register("HintText", typeof(string), typeof(UndoButton), new UIPropertyMetadata(""));

        #endregion

        #region Icon

        /// <summary>
        /// Gets or sets button icon
        /// </summary>
        public BitmapSource Icon
        {
            get { return (BitmapSource)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for Icon.  This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(BitmapSource), typeof(UndoButton), new UIPropertyMetadata(null));

        #endregion

        #endregion

        #region Constructors

        /// <summary>
        /// Static constructor
        /// </summary>
        static UndoButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(UndoButton), new FrameworkPropertyMetadata(typeof(UndoButton)));
            IsEnabledProperty.AddOwner(typeof(UndoButton), new FrameworkPropertyMetadata(OnIsEnabledChanged, CoerceIsEnabled));
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public UndoButton()
        {

        }

        #endregion

        #region Overrides

        /// <summary>
        /// Creates or identifies the element that is used to display the given item.
        /// </summary>
        /// <returns>The element that is used to display the given item.</returns>
        protected override DependencyObject GetContainerForItemOverride()
        {
            UndoItem item = new UndoItem();
            this.AddLogicalChild(item);
            return item;
        }

        /// <summary>
        /// Determines if the specified item is (or is eligible to be) its own container.
        /// </summary>
        /// <param name="item">The item to check.</param>
        /// <returns>true if the item is (or is eligible to be) its own container; otherwise, false.</returns>
        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return (item is UndoItem);
        }

        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or internal processes call <see cref="M:System.Windows.FrameworkElement.ApplyTemplate"/>.
        /// </summary>
        public override void OnApplyTemplate()
        {
            hintTextBlock = GetTemplateChild("PART_TextHint") as TextBlock;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Select item
        /// </summary>
        /// <param name="item">Undo item</param>
        internal void Select(UndoItem item)
        {
            int itemIndex = Items.IndexOf(this.ItemContainerGenerator.ItemFromContainer(item));
            for (int i = 0; i < Items.Count; i++)
            {
                UndoItem undoItem = this.ItemContainerGenerator.ContainerFromItem(Items[i]) as UndoItem;
                if (i > itemIndex) undoItem.IsSelected = false;
                else undoItem.IsSelected = true;
            }
            hintTextBlock.Text = HintText + " " + (itemIndex + 1) + " Action";
            if (itemIndex > 0) hintTextBlock.Text += "s";
        }

        /// <summary>
        /// Deselect all items
        /// </summary>
        internal void Deselect()
        {
            for (int i = 0; i < Items.Count; i++)
            {
                UndoItem undoItem = this.ItemContainerGenerator.ContainerFromItem(Items[i]) as UndoItem;
                undoItem.IsSelected = false;
            }
            hintTextBlock.Text = "Cancel";
        }

        /// <summary>
        /// Click on item
        /// </summary>
        /// <param name="item">Undo item</param>
        internal void ItemClick(UndoItem item)
        {
            if (Command != null)
            {
                ICommand command = Command;
                if (command != null)
                {
                    object commandParameter = this.ItemContainerGenerator.ItemFromContainer(item);
                    IInputElement commandTarget = CommandTarget;
                    RoutedCommand routedCommand = command as RoutedCommand;
                    if (routedCommand != null)
                    {
                        if (commandTarget == null)
                        {
                            commandTarget = this as IInputElement;
                        }
                        if (routedCommand.CanExecute(commandParameter, commandTarget))
                        {
                            routedCommand.Execute(commandParameter, commandTarget);
                        }
                    }
                    else if (command.CanExecute(commandParameter))
                    {
                        command.Execute(commandParameter);
                    }
                }
            }
            IsOpen = false;
        }

        #endregion

        #region Implementation of IQuickAccessItemProvider

        /// <summary>
        /// Gets control which represents shortcut item.
        ///             This item MUST be syncronized with the original 
        ///             and send command to original one control.
        /// </summary>
        /// <returns>
        /// Control which represents shortcut item
        /// </returns>
        public FrameworkElement CreateQuickAccessItem()
        {
            UndoButton button = new UndoButton();

            Binding binding = new Binding("Icon");
            binding.Mode = BindingMode.OneWay;
            binding.Source = this;
            button.SetBinding(IconProperty, binding);

            binding = new Binding("ItemsSource");
            binding.Mode = BindingMode.OneWay;
            binding.Source = this;
            button.SetBinding(ItemsSourceProperty, binding);

            binding = new Binding("Command");
            binding.Mode = BindingMode.OneWay;
            binding.Source = this;
            button.SetBinding(CommandProperty, binding);

            binding = new Binding("CommandTarget");
            binding.Mode = BindingMode.OneWay;
            binding.Source = this;
            button.SetBinding(CommandTargetProperty, binding);

            binding = new Binding("CommandParameter");
            binding.Mode = BindingMode.OneWay;
            binding.Source = this;
            button.SetBinding(CommandParameterProperty, binding);

            binding = new Binding("HintText");
            binding.Mode = BindingMode.OneWay;
            binding.Source = this;
            button.SetBinding(HintTextProperty, binding);

            return button;
        }

        /// <summary>
        /// Gets or sets whether control can be added to quick access toolbar
        /// </summary>
        public bool CanAddToQuickAccessToolBar
        {
            get { return (bool)GetValue(CanAddToQuickAccessToolBarProperty); }
            set { SetValue(CanAddToQuickAccessToolBarProperty, value); }
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for CanAddToQuickAccessToolBar.  This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty CanAddToQuickAccessToolBarProperty =
            DependencyProperty.Register("CanAddToQuickAccessToolBar", typeof(bool), typeof(UndoButton), new UIPropertyMetadata(true));

        #endregion
    }
}
