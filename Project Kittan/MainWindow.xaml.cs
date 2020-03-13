using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Project_Kittan.Helpers;

namespace Project_Kittan
{
    /// <summary>
    /// MainWindow class
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow Current;

        private List<Models.File> Files { get; set; } = new List<Models.File>();

        /// <summary>
        /// Parameterless constructor of MainWindow class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            Current = this;

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
        /// Method invoked when the user clicks on Update OBJECT-PROPERTIES button.
        /// Start OBJECT-PROPERTIES update on all text files in working directory.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            StatusProgressBar.Value = 0;
            ResponsiveStatusProgressBar.IsIndeterminate = true;

            await ObjectExtensions.UpdateObjects(Files.ToArray(), (bool)DateTimeUpdateCheckBox.IsChecked, VersionTextTextBox.Text, NAVVersionComboBox.SelectedIndex);

            StatusTextBlock.Text = "Done";
            StatusProgressBar.IsIndeterminate = false;
            ResponsiveStatusProgressBar.IsIndeterminate = false;
        }

        /// <summary>
        /// Method invoked when the user clicks on Update Remove tag button.
        /// Start tag removal from all text files in working directory.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Button_Click_5(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(TagTextTextBox.Text))
            {
                StatusProgressBar.Value = 0;
                ResponsiveStatusProgressBar.IsIndeterminate = true;

                await ObjectExtensions.RemoveTag(Files.ToArray(), TagTextTextBox.Text, (bool)CaseSensitiveCheckBox.IsChecked);

                StatusTextBlock.Text = "Done";
                StatusProgressBar.IsIndeterminate = false;
                ResponsiveStatusProgressBar.IsIndeterminate = false;
            }
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
                files = files.Where(i => i.EndsWith(".txt")).ToArray();
                foreach (string file in files)
                {
                    Files.Add(new Models.File(file));
                }
            }

            if(Files.Count > 0)
            {
                //TaggerExpander.IsEnabled = true;
                //TaggerRemoverExpander.IsEnabled = true;
                //SearchExpander.IsEnabled = true;
            }

            //FoundFilesListBox.ClearValue(ItemsControl.ItemsSourceProperty);
            //FoundFilesListBox.ItemsSource = Files;
        }

        /// <summary>
        /// Method invoked when the user clicks on Clear textblock.
        /// Clear Files collection and FoundFilesListBox.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBlock_MouseDown_2(object sender, MouseButtonEventArgs e)
        {
        }

        /// <summary>
        /// Method invoked when the user Copy value from ConflictsListView contex menu.
        /// Copy the right-clicked value to the clipboard.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            string clipboard = item.CommandParameter.ToString().Trim();

            Clipboard.SetText(clipboard);
            StatusTextBlock.Text = clipboard + " copied to clipboard";
        }

        /// <summary>
        /// Method invoked when the user types in PatterTextBox.
        /// Starts the search after Enter or Return is pressed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Return) Button_Click_4(null, null);
        }

        /// <summary>
        /// Method invoked when the user clicks on Search for occurrences button.
        /// Start occurrences search on all text files in working directory.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Button_Click_4(object sender, RoutedEventArgs e)
        {
            if(!string.IsNullOrWhiteSpace(PatternTextBox.Text))
            {
                PatternFoundInTextBlock.Text = PatternTextBox.Text + " found in:";
                StatusProgressBar.Value = 0;
                ResponsiveStatusProgressBar.IsIndeterminate = true;
                OutputTabControl.SelectedIndex = 1;

                await ObjectExtensions.FindWhere(Files.ToArray(), PatternTextBox.Text);

                StatusProgressBar.IsIndeterminate = false;
                ResponsiveStatusProgressBar.IsIndeterminate = false;
            }
        }

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            new SettingsWindow().ShowDialog();
        }
    }
}
