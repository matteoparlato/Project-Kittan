using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using Project_Kittan.ViewModels;

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
        /// Verify if there are new updates available for Project Kittan.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
#if !APPX
            await UpdateExtensions.Check();
#endif
        }

        /// <summary>
        /// Method invoked when the user clicks on About menu item.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
#if !APPX
            Process.Start("https://github.com/matteoparlato/Project-Kittan");
#else
            //Windows.System.Launcher.LaunchUriAsync(uri);
#endif
        }

        /// <summary>
        /// Method invoked when the user drag and drop a file on left pane.
        /// Add dropped *.txt files to Files collection.
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

        private void FilterTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = ((TextBox)sender);

            if (!string.IsNullOrWhiteSpace(textBox.Text))
            {
                textBox.SelectAll();

                try
                {
                    Clipboard.SetText(textBox.Text);
                }
                catch (COMException ex)
                {
                    var result = System.Windows.Forms.MessageBox.Show("An error occured during the copy operation." + Environment.NewLine + Environment.NewLine + ex.Message, Properties.Resources.AppName, System.Windows.Forms.MessageBoxButtons.RetryCancel, System.Windows.Forms.MessageBoxIcon.Error);
                    if (result == System.Windows.Forms.DialogResult.Retry)
                    {
                        textBox.SelectAll();
                        Clipboard.SetText(textBox.Text);
                    }
                }

                ((Workspace)this.DataContext).ProgressText = string.Format("{0} filter copied to clipboard", textBox.Tag);
            }
        }
    }
}
