using Project_Kittan.Helpers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Media;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Shell;

namespace Project_Kittan
{
    /// <summary>
    /// SplitDialog class
    /// </summary>
    public partial class SplitDialog : Window
	{
        private bool _closable;

        private string _filePath;

        /// <summary>
        /// Constructor which initializes a SplitDialog Window with passed information.
        /// </summary>
        /// <param name="filePath">The path of the file to split</param>
		public SplitDialog(string filePath)
		{
			InitializeComponent();

            _filePath = filePath;

            UsedEncodingTextBlock.Text = "Encoding " + Encoding.GetEncoding(Properties.Settings.Default.DefaultEncoding).BodyName + " used";

			WindowStartupLocation = WindowStartupLocation.CenterScreen;
		}

        /// <summary>
        /// Method invoked the window is laid out, rendered, and ready for interaction.
        /// Starts the file splitting.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            StatusTextBlock.Text = "Splitting " + Path.GetFileName(_filePath) + "...";
            UsedEncodingTextBlock.Text = "Encoding " + Encoding.GetEncoding(Properties.Settings.Default.DefaultEncoding).BodyName + " used";

            KeyValuePair<int, string> returnValue = await ObjectSplitterExtensions.SplitAndStore(_filePath, Properties.Settings.Default.DefaultEncoding);
            _filePath = returnValue.Value;

            StatusTextBlock.Text = returnValue.Key + " files extracted in:\n" + returnValue.Value;
            StatusProgressBar.IsIndeterminate = false;
            TaskBarProgress.ProgressState = TaskbarItemProgressState.None;
            OpenFolderButton.IsEnabled = CloseButton.IsEnabled = true;

            SystemSounds.Asterisk.Play();

            _closable = true;
        }

        /// <summary>
        /// Method invoked when closing the window.
        /// If _closable = true the window closes, otherwise not.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			e.Cancel = !_closable;
		}

        /// <summary>
        /// Method invoked when the user clicks on Open Folder button.
        /// Opens the extraction folder in Explorer.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenFolderButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(_filePath);
        }

        /// <summary>
        /// Method invoked when the user clicks on Close button.
        /// Closes the current window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Method invoked when the user presses a keyboard key.
        /// If the user presses Enter close the window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                CloseButton_Click(null, null);
            }
        }
    }
}
