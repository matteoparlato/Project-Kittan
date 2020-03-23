using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Project_Kittan
{
    /// <summary>
    /// RequestDialog class
    /// </summary>
    public partial class RequestDialog : Window
	{
        public string VersionList
        {
            get { return VersionListTextBox.Text; }
            set { VersionListTextBox.Text = value; }
        }

        private bool _closable;

		private int _maxLength;

        /// <summary>
        /// Constructor which initializes a RequestDialog Window with passed information.
        /// </summary>
        /// <param name="versionList">The "Verison List" string</param>
        /// <param name="maxLength">The maximum length of the "Version List"</param>
		public RequestDialog(string versionList, int maxLength)
		{
			InitializeComponent();

            VersionList = versionList;
			_maxLength = maxLength;

			CharactersLeftTextBlock.Text = string.Format("The current version list exceeds the maximum length by {0} characters.", VersionListTextBox.Text.Length - _maxLength);

			WindowStartupLocation = WindowStartupLocation.CenterScreen;
		}

        /// <summary>
        /// Method invoked when the user clicks on Save and continue button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		private void Button_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = true;
			_closable = true;
		}

        /// <summary>
        /// Method invoked when the text in VersionListTextBox changes.
        /// Check the length of the "Version List" and enables the Save and continue button
        /// if the length is correct.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (string.IsNullOrWhiteSpace(VersionListTextBox.Text) || VersionListTextBox.Text.Length > _maxLength)
			{
                CharactersLeftTextBlock.Text = string.Format("The current \"Version List\" exceeds the maximum length by {0} characters.", VersionListTextBox.Text.Length - _maxLength);
                ContinueButton.IsEnabled = false;
				_closable = false;
				return;
			}
            else
            {
                CharactersLeftTextBlock.Text = string.Format("The current \"Version List\" length is valid. You can add {0} characters.", Math.Abs(VersionListTextBox.Text.Length - _maxLength));
            }

			ContinueButton.IsEnabled = true;
			_closable = true;
		}

        /// <summary>
        /// Method invoked when the user types in VersionListTextBox.
        /// Saves and continue after Enter or Return key is pressed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.Enter || e.Key == Key.Return) && !(string.IsNullOrWhiteSpace(VersionListTextBox.Text) || VersionListTextBox.Text.Length > _maxLength)) Button_Click(null, null);
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
    }
}
