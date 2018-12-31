using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;
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
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                DialogResult result = dialog.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.SelectedPath))
                {
                    OpenFolder(dialog.SelectedPath);
                }
            }
        }

        private void OpenFolder(string selectedPath)
        {
            SelectedFolderTextBlock.Text = Path = selectedPath;

            FoundFilesListBox.ItemsSource = Files = FilesExtensions.FindFiles(Path);

            if (Files.Count == 0)
            {
                if (System.Windows.MessageBox.Show("No file found. Do you want to select another folder?", "Project Kittan", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                {
                    SelectedFolderTextBlock.Text = "Pick a folder to continue";
                    TaggerExpander.IsEnabled = false;
                    ConflictsExpander.IsEnabled = false;
                    return;
                }
                else
                {
                    Button_Click(null, null);
                }
            }

            TaggerExpander.IsEnabled = true;
            ConflictsExpander.IsEnabled = true;
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

            await ObjectExtensions.UpdateObjects(Files.ToArray(), (bool)DateTimeUpdateCheckBox.IsChecked, VersionTextTextBox.Text, NAVVersionComboBox.SelectedIndex);

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

            await ObjectExtensions.ConflictsFinder(Files.ToArray());

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
    }
}
