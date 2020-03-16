using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using Project_Kittan.Helpers;
using Project_Kittan.ViewModels;

namespace Project_Kittan
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

            Title = "Project Kittan - " + Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        /// <summary>
        /// Method invoked the window is laid out, rendered, and ready for interaction.
        /// Verify if there are new updates available for Project Kittan.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await UpdateExtensions.Check();
        }

        /// <summary>
        /// Method invoked when the user clicks on About menu item.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/matteoparlato/Project-Kittan");
        }

        /// <summary>
        /// Method invoked when the user drag and drop a file on left pane.
        /// Add dropped *.txt files to Files collection.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StackPanel_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                ((Workspace)this.DataContext).AddFilesFromDrop(files);
            }
        }

        private void FilterTextBox_GotKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            FilterTextBox_MouseUp(sender, null);
        }

        private void FilterTextBox_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            TextBox textBox = ((TextBox)sender);
            textBox.SelectAll();

            try
            {
                Clipboard.SetText(textBox.Text);
            }
            catch(COMException ex)
            {
                var result = System.Windows.Forms.MessageBox.Show("An error occured during the copy operation." + Environment.NewLine + Environment.NewLine + ex.Message, string.Empty, System.Windows.Forms.MessageBoxButtons.RetryCancel, System.Windows.Forms.MessageBoxIcon.Error);
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
