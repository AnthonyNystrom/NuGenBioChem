using System;
using System.Windows;
using System.Windows.Controls;

namespace NuGenBioChem
{
    /// <summary>
    /// Window for user to enter name for something
    /// </summary>
    public partial class EnterNameWindow : System.Windows.Window
    {
        #region Fields

        Func<string, bool> nameAllowed = null;

        #endregion

        #region Initialization

        /// <summary>
        /// Default constructor
        /// </summary>
        public EnterNameWindow()
        {
            InitializeComponent();
            Loaded += (s, e) => { textBox.SelectAll(); textBox.Focus(); };
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows dialog to user where he can enter the name
        /// </summary>
        /// <param name="owner">Window owner</param>
        /// <param name="title">Title of the window</param>
        /// <param name="initialName">Initial name in the textbox</param>
        /// <param name="nameAllowed">Function to determinate whether name is allowed</param>
        /// <returns>Name or null if user pressed cancel button</returns>
        public static string RequestName(System.Windows.Window owner, string title, string initialName, Func<string, bool> nameAllowed)
        {
            EnterNameWindow enterNameWindow = new EnterNameWindow();
            enterNameWindow.nameAllowed = nameAllowed;
            enterNameWindow.Title = title;
            enterNameWindow.textBox.Text = initialName;
            enterNameWindow.Owner = owner;

            return enterNameWindow.ShowDialog() == true ? enterNameWindow.textBox.Text : null;
        }

        #endregion

        #region Event Handlers

        void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (nameAllowed == null) return;
            buttonOk.IsEnabled = nameAllowed(textBox.Text);
        }

        void OkButtonClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        #endregion
    }
}
