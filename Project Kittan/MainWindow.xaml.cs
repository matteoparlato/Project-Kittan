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

        private HashSet<Models.File> Files { get; set; } = new HashSet<Models.File>();

        private string Path = string.Empty;

        /// <summary>
        /// Parameterless constructor of MainWindow class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            Current = this;

            WindowStartupLocation = WindowStartupLocation.CenterScreen;
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
        /// Method invoked when the user clicks on Use current folder button.
        /// Set current Project Kittan.exe directory as working directory.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            string executablePath = Process.GetCurrentProcess().MainModule.FileName;

            OpenFolder(executablePath.Substring(0, executablePath.Length - System.IO.Path.GetFileName(executablePath).Length));
        }
        
        /// <summary>
        /// Method invoked when the user clicks on Browse Folder button.
        /// Opens OpenFolderDialog for choosing the working directory.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            using (System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.SelectedPath))
                {
                    OpenFolder(dialog.SelectedPath);
                }
            }
        }

        private void OpenFolder(string selectedPath)
        {
            SelectedFolderTextBlock.Text = Path = selectedPath;

            Files.Clear();
            FoundFilesListBox.ClearValue(ItemsControl.ItemsSourceProperty);

            FoundFilesListBox.ItemsSource = Files = FilesExtensions.FindFiles(Path);

            if (Files.Count == 0)
            {
                if (MessageBox.Show("No file found. Do you want to select another folder?", "Project Kittan", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                {
                    SelectedFolderTextBlock.Text = "Pick a folder and/or drag and drop files to continue";
                    TaggerExpander.IsEnabled = false;
                    ConflictsExpander.IsEnabled = false;
                    SearchExpander.IsEnabled = false;
                    return;
                }
                else
                {
                    Button_Click(null, null);
                }
            }

            TaggerExpander.IsEnabled = true;
            ConflictsExpander.IsEnabled = true;
            SearchExpander.IsEnabled = true;
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
            StatusOverlayProgressBar.IsIndeterminate = true;
            System.Windows.Forms.Application.UseWaitCursor = true;
            ActionsScrollViewer.IsEnabled = false;

            await ObjectExtensions.UpdateObjects(Files.ToArray(), (bool)DateTimeUpdateCheckBox.IsChecked, VersionTextTextBox.Text, NAVVersionComboBox.SelectedIndex);

            ActionsScrollViewer.IsEnabled = true;
            StatusTextBlock.Text = "Done";
            StatusProgressBar.IsIndeterminate = false;
            StatusOverlayProgressBar.IsIndeterminate = false;
            System.Windows.Forms.Application.UseWaitCursor = false;
        }

        /// <summary>
        /// Method invoked when the user clicks on Search for conflicts button.
        /// Start conflict search on all text files in working directory.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Button_Click_3(object sender, RoutedEventArgs e)
        {
            StatusProgressBar.Value = 0;
            StatusOverlayProgressBar.IsIndeterminate = true;
            System.Windows.Forms.Application.UseWaitCursor = true;
            ActionsScrollViewer.IsEnabled = false;
            OutputTabControl.SelectedIndex = 0;

            await ObjectExtensions.ConflictsFinder(Files.ToArray());

            ActionsScrollViewer.IsEnabled = true;
            StatusProgressBar.IsIndeterminate = false;
            StatusOverlayProgressBar.IsIndeterminate = false;
            System.Windows.Forms.Application.UseWaitCursor = false;
        }

        /// <summary>
        /// Method invoked when the user clicks on Find more about Project Kittan on GitHub textblock.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Process.Start("https://github.com/matteoparlato/Project-Kittan");
        }

        /// <summary>
        /// Method invoked when the user clicks on Open issue on GitHub textblock.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBlock_MouseDown_1(object sender, MouseButtonEventArgs e)
        {
            Process.Start("https://github.com/matteoparlato/Project-Kittan/issues");
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

            FoundFilesListBox.ClearValue(ItemsControl.ItemsSourceProperty);
            FoundFilesListBox.ItemsSource = Files;
        }

        /// <summary>
        /// Method invoked when the user clicks on Clear textblock.
        /// Clear Files collection and FoundFilesListBox.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBlock_MouseDown_2(object sender, MouseButtonEventArgs e)
        {
            TaggerExpander.IsEnabled = false;
            ConflictsExpander.IsEnabled = false;
            SearchExpander.IsEnabled = false;

            Files.Clear();
            FoundFilesListBox.ClearValue(ItemsControl.ItemsSourceProperty);
            FoundFilesListBox.ItemsSource = Files;
            Path = string.Empty;
            SelectedFolderTextBlock.Text = "Pick a folder and/or drag and drop files to continue";
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
        /// Method invoked when the user clicks on Search for occurencies button.
        /// Start occurencies search on all text files in working directory.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Button_Click_4(object sender, RoutedEventArgs e)
        {
            if(!string.IsNullOrWhiteSpace(PatternTextBox.Text))
            {
                PatternFoundInTextBlock.Text = PatternTextBox.Text + " found in:";
                StatusProgressBar.Value = 0;
                StatusOverlayProgressBar.IsIndeterminate = true;
                System.Windows.Forms.Application.UseWaitCursor = true;
                ActionsScrollViewer.IsEnabled = false;
                OutputTabControl.SelectedIndex = 1;

                await ObjectExtensions.FindWhere(Files.ToArray(), PatternTextBox.Text);

                ActionsScrollViewer.IsEnabled = true;
                StatusProgressBar.IsIndeterminate = false;
                StatusOverlayProgressBar.IsIndeterminate = false;
                System.Windows.Forms.Application.UseWaitCursor = false;
            }
        }
    }
}
