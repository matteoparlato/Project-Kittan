using Project_Kittan.Helpers;
using Project_Kittan.ViewModels;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace Project_Kittan.Views
{
    /// <summary>
    /// MainWindow class
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Parameterless constructor of MainWindow class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Method invoked the window is laid out, rendered, and ready for interaction.
        /// Verify if there are new updates available for Project Kittan on GitHub.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
#if !APPX
            if (await UpdateExtensions.Check())
            {
                if (MessageBox.Show("There's a new version of Project Kittan available on GitHub. Do you want to download it?", Properties.Resources.AppName, MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                {
                    Process.Start("https://github.com/matteoparlato/Project-Kittan/releases");
                }
            }
#endif
        }

        /// <summary>
        /// Method invoked when the user clicks on GitHub button.
        /// Navigates to Project Kittan GitHub page.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
#if !APPX
            Process.Start("https://github.com/matteoparlato/Project-Kittan");
#endif
        }

        /// <summary>
        /// Method invoked when the user drag and drop a file on the left pane.
        /// Adds dropped *.txt files to the Workspace.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StackPanel_Drop(object sender, DragEventArgs e)
        {
            Workspace workspace = (Workspace)DataContext;
            if (workspace.Command_CanExecute(null))
            {
                workspace.DropFile_Action(e);
            }
        }

        /// <summary>
        /// Method invoked when the Filters controls get the focus.
        /// Copies the text of the selected textbox in Filters.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FilterTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = ((TextBox)sender);

            if (!string.IsNullOrWhiteSpace(textBox.Text))
            {
                textBox.SelectAll();

                try
                {
                    Clipboard.SetText(textBox.Text);

                    ((Workspace)DataContext).ProgressText = string.Format("{0} filter copied to clipboard", textBox.Tag);
                }
                catch (Exception)
                {
                    ((Workspace)DataContext).ProgressText = string.Format("An error occurred during the copy operation");
                }
            }
        }
    }
}
