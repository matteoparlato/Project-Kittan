using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;

namespace Project_Kittan
{
    /// <summary>
    /// SettingsWindow class
    /// </summary>
    public partial class SettingsWindow : Window
    {
        /// <summary>
        /// Parameterless constructor of SettingsWindow class.
        /// Load user defined app settings.
        /// </summary>
        public SettingsWindow()
        {
            InitializeComponent();

            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            //// Encoding settings
            if(Properties.Settings.Default.DefaultEncoding == 0)
            {
                AutomaticEncodingRadioButton.IsChecked = AutomaticEncodingOptionsStackPanel.IsEnabled = true;
                ManualEncodingRadioButton.IsChecked = ManualEncodingOptionsStackPanel.IsEnabled = false;
            }
            else
            {
                AutomaticEncodingRadioButton.IsChecked = AutomaticEncodingOptionsStackPanel.IsEnabled = false;
                ManualEncodingRadioButton.IsChecked = ManualEncodingOptionsStackPanel.IsEnabled =  true;

                Encoding encoding = Encoding.GetEncoding(Properties.Settings.Default.DefaultEncoding);
                BrowseFileEncodingTextBlock.Text = UserDefindedEncodingTextBlock.Text = "Using " + encoding.BodyName + " encoding";
            }
        }

        #region Encoding

        /// <summary>
        /// Method invoked when the user types in UserDefinedEncodingTextBox textbox.
        /// Opens a text file to read it's encoding then sets it as default encoding.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BrowseFileEncodingButton_Click(object sender, RoutedEventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "txt Files (*.txt)|*.txt";
                DialogResult result = openFileDialog.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(openFileDialog.FileName))
                {
                    using (FileStream stream = new FileStream(openFileDialog.FileName, FileMode.Open, FileAccess.Read))
                    using (StreamReader reader = new StreamReader(stream, Encoding.Default, true))
                    {
                        if (reader.Peek() >= 0)
                        {
                            reader.Read();
                        }

                        BrowseFileEncodingTextBlock.Text = UserDefindedEncodingTextBlock.Text = reader.CurrentEncoding.BodyName + " saved";

                        Properties.Settings.Default.DefaultEncoding = reader.CurrentEncoding.CodePage;
                        Properties.Settings.Default.Save();
                    }
                }
            }
        }

        /// <summary>
        /// Method invoked when the user types in UserDefinedEncodingTextBox textbox.
        /// If a valid Code Page is typed then it is set as default encoding.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserDefinedEncodingTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            int.TryParse(UserDefinedEncodingTextBox.Text, out int code);
            if (code != 0)
            {
                try
                {
                    Encoding encoding = Encoding.GetEncoding(code);
                    UserDefindedEncodingTextBlock.Text = BrowseFileEncodingTextBlock.Text = encoding.BodyName + " saved";

                    Properties.Settings.Default.DefaultEncoding = encoding.CodePage;
                    Properties.Settings.Default.Save();
                }
                catch(Exception)
                {
                    UserDefindedEncodingTextBlock.Text = "Defined encoding not found";
                }
            }
            else
            {
                UserDefindedEncodingTextBlock.Text = "Defined encoding not found";
            }
        }

        /// <summary>
        /// Method invoked when the user clicks on AutomaticEncodingRadioButton radio button.
        /// Set DefaultEncoding to 0 (Encoding.Default) and updates the UI.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AutomaticEncodingRadioButton_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.DefaultEncoding = 0; // Zero refers to Encoding.Default
            Properties.Settings.Default.Save();

            ManualEncodingRadioButton.IsChecked = ManualEncodingOptionsStackPanel.IsEnabled = false;
            BrowseFileEncodingTextBlock.Text = "No file selected";
            UserDefindedEncodingTextBlock.Text = "Write the code of the encoding";
            UserDefinedEncodingTextBox.Text = string.Empty;
            AutomaticEncodingOptionsStackPanel.IsEnabled = true;
        }

        /// <summary>
        /// Method invoked when the user clicks on ManualEncodingRadioButton radio button.
        /// Updates the UI. No settings changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ManualEncodingRadioButton_Click(object sender, RoutedEventArgs e)
        {
            ManualEncodingOptionsStackPanel.IsEnabled = true;
            AutomaticEncodingOptionsStackPanel.IsEnabled = false;
        }

        /// <summary>
        /// Method invoked when the user clicks on How automatic dection works? textblock.
        /// Navigates to the specified web page with the dafault browser.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Process.Start("https://docs.microsoft.com/en-us/dotnet/api/system.text.encoding?view=netframework-4.7.1#remarks");
        }

        /// <summary>
        /// Method invoked when the user clicks on Look for encoding codes on .NET Framework Documentation textblock.
        /// Navigates to the specified web page with the dafault browser.
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBlock_MouseDown_1(object sender, MouseButtonEventArgs e)
        {
            Process.Start("https://docs.microsoft.com/en-us/dotnet/api/system.io.streamreader.currentencoding?view=netframework-4.7.1_");
        }

        #endregion Encoding

        #region Reset

        /// <summary>
        /// Method invoked when the user clicks on Reset.
        /// Reset user defined settings to app default.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ResetSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Reset();
            App.Current.Shutdown();
        }

        #endregion Reset

        #region Shell

        /// <summary>
        /// Method invoked when the usre clicks on Enable split file shell integration textblock.
        /// Runs Project Kittan with administrator prileges with /enableSplitFileShellIntegration parameter.
        /// A registry key is added to HKEY_CLASSES_ROOT\textfile\shell in order to add a context menu item
        /// which is capable of running Project Kittan with split file parameter.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EnableSplitShellTextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Process elevatedProcess = new Process();
            elevatedProcess.StartInfo.FileName = Assembly.GetEntryAssembly().Location;
            elevatedProcess.StartInfo.Arguments = "/enableSplitFileShellIntegration";
            elevatedProcess.StartInfo.UseShellExecute = true;
            elevatedProcess.StartInfo.Verb = "runas";
            elevatedProcess.Start();
        }

        /// <summary>
        /// Method invoked when the usre clicks on Disable split file shell integration textblock.
        /// Removes the previously inserted registry key from HKEY_CLASSES_ROOT\textfile\shell in order to
        /// disable Project Kittan shell integration.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DisableSplitShellTextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Process elevatedProcess = new Process();
            elevatedProcess.StartInfo.FileName = Assembly.GetEntryAssembly().Location;
            elevatedProcess.StartInfo.Arguments = "/disableSplitFileShellIntegration";
            elevatedProcess.StartInfo.UseShellExecute = true;
            elevatedProcess.StartInfo.Verb = "runas";
            elevatedProcess.Start();
        }

        #endregion Shell
    }
}
