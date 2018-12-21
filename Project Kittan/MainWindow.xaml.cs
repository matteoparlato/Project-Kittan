using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using Project_Kittan.Models;

namespace Project_Kittan
{
    /// <summary>
    /// MainWindow class
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<string> _files = new List<string>();

        private string _path = string.Empty;

        private bool _avoidUpdateDateTime = false;

        /// <summary>
        /// Parameterless constructor of MainWindow class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

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

            SelectedFolderTextBlock.Text = _path = executablePath.Substring(0, executablePath.Length - Path.GetFileName(executablePath).Length);

            FoundFilesListBox.ItemsSource = _files = FindFiles(_path);

            if (_files.Count == 0)
            {
                if (System.Windows.MessageBox.Show("No file found. Do you want to select another folder?", "Project Kittan", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                {
                    SelectedFolderTextBlock.Text = "Pick a folder to continue";
                    NAVVersionComboBox.IsEnabled = false;
                    //DateFormatComboBox.IsEnabled = false;
                    DateTimeUpdateCheckBox.IsEnabled = false;
                    VersionTextTextBox.IsEnabled = false;
                    GoButton.IsEnabled = false;
                    return;
                }
                else
                {
                    Button_Click(null, null);
                }
            }

            NAVVersionComboBox.IsEnabled = true;
            //DateFormatComboBox.IsEnabled = true;
            DateTimeUpdateCheckBox.IsEnabled = true;
            VersionTextTextBox.IsEnabled = true;
            GoButton.IsEnabled = true;
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
                    SelectedFolderTextBlock.Text = _path = dialog.SelectedPath;

                    FoundFilesListBox.ItemsSource = _files = FindFiles(_path);

                    if (_files.Count == 0)
                    {
                        if (System.Windows.MessageBox.Show("No file found. Do you want to select another folder?", "Project Kittan", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                        {
                            SelectedFolderTextBlock.Text = "Pick a folder to continue";
                            NAVVersionComboBox.IsEnabled = false;
                            //DateFormatComboBox.IsEnabled = false;
                            DateTimeUpdateCheckBox.IsEnabled = false;
                            VersionTextTextBox.IsEnabled = false;
                            GoButton.IsEnabled = false;
                            return;
                        }
                        else
                        {
                            Button_Click(null, null);
                        }
                    }

                    NAVVersionComboBox.IsEnabled = true;
                    //DateFormatComboBox.IsEnabled = true;
                    DateTimeUpdateCheckBox.IsEnabled = true;
                    VersionTextTextBox.IsEnabled = true;
                    GoButton.IsEnabled = true;
                }
            }
        }

        /// <summary>
        /// Method invoked when the user clicks on GO button.
        /// Process all text files in the working folder.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            StatusTextBlock.Text = "Checking files availability...";

            FoundFilesListBox.ItemsSource = _files = FindFiles(_path);
            //if (!_files.Equals(filesAvailable))
            //{
            //    if (System.Windows.MessageBox.Show("The number of files to modify has changed. Do you want to continue?", "Project Kittan", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
            //    {
            //        return;
            //    }
            //}

            foreach (string file in _files)
            {
                StatusTextBlock.Text = "Processing file \"" + file + "\"...";
                StringBuilder builder = new StringBuilder();

                using (FileStream stream = new FileStream(file, FileMode.Open, FileAccess.Read))
                using (StreamReader reader = new StreamReader(stream, Encoding.GetEncoding(1252))) // Encoding 1252 is the same used by NAV (Windows-1252)
                {
                    string line;

                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line.Equals("  OBJECT-PROPERTIES"))
                        {
                            builder.AppendLine(line); // Add "  OBJECT-PROPERTIES"
                            builder.AppendLine(reader.ReadLine()); // Add "  {"

                            line = reader.ReadLine();
                            if (line.Contains("Date") && !_avoidUpdateDateTime)
                            {
                                switch (DateFormatComboBox.SelectedIndex)
                                {
                                    case 0:
                                        {
                                            builder.AppendLine(string.Format("    Date={0:MM.dd.yy};", DateTime.Today));
                                            break;
                                        }
                                    default:
                                        {
                                            builder.AppendLine(string.Format("    Date={0:dd.MM.yy};", DateTime.Today));
                                            break;
                                        }
                                }
                            }
                            else
                            {
                                builder.AppendLine(line);
                            }

                            line = reader.ReadLine();
                            if (line.Contains("Time") && !_avoidUpdateDateTime)
                            {
                                builder.AppendLine(string.Format("    Time={0:HH:mm:ss};", DateTime.Now)); // Add "Time..."
                            }
                            else
                            {
                                builder.AppendLine(line);
                            }

                            line = reader.ReadLine();
                            if (line.Contains("Modified"))
                            {
                                builder.AppendLine(line); // Add "Modified..."
                                line = reader.ReadLine();
                            }

                            if (line.Contains("Version List"))
                            {
                                // Check if the user defined tag is already in the "Version List"
                                if (!string.IsNullOrWhiteSpace(VersionTextTextBox.Text) && line.IndexOf(VersionTextTextBox.Text, StringComparison.OrdinalIgnoreCase) == -1)
                                {
                                    line = line.Replace(";", ",");
                                    line = line + VersionTextTextBox.Text + ";";
                                }

                                string temp = line.Substring(17);
                                temp = temp.Substring(0, temp.Length - 1); // Remove "    Version List=...;"

                                bool avoidInsert = false;

                                switch (NAVVersionComboBox.SelectedIndex)
                                {
                                    case 0: // NAV 2013 and below
                                        {
                                            if (temp.Length > 80)
                                            {
                                                RequestDialog dialog = new RequestDialog(temp, 80);
                                                if (dialog.ShowDialog() == true)
                                                {
                                                    builder.AppendLine("    Version List=" + dialog.VersionList + ";");
                                                    avoidInsert = true;
                                                }
                                            }
                                            break;
                                        }
                                    default: // NAV 2015 and above
                                        {
                                            if (temp.Length > 250)
                                            {
                                                RequestDialog dialog = new RequestDialog(temp, 250);
                                                if (dialog.ShowDialog() == true)
                                                {
                                                    builder.AppendLine("    Version List=" + dialog.VersionList + ";");
                                                    avoidInsert = true;
                                                }
                                            }
                                            break;
                                        }
                                }

                                if (!avoidInsert)
                                {
                                    builder.AppendLine(line);
                                }
                            }
                        }
                        else
                        {
                            builder.AppendLine(line);
                        }
                    }
                }

                StatusTextBlock.Text = "Saving file \"" + file + "\"...";
                using (FileStream stream = new FileStream(file, FileMode.Truncate, FileAccess.Write))
                using (StreamWriter writer = new StreamWriter(stream, Encoding.GetEncoding(1252)))
                {
                    writer.Write(builder.ToString());
                }
            }

            ConflictsFinder();

            StatusTextBlock.Text = "Done!";
        }

        /// <summary>
        /// 
        /// </summary>
        private void ConflictsFinder()
        {
            int lineNumber = 0;
            List<ObjectElements> elements = new List<ObjectElements>();

            foreach (string file in _files)
            {
                using (FileStream stream = new FileStream(file, FileMode.Open, FileAccess.Read))
                using (StreamReader reader = new StreamReader(stream, Encoding.GetEncoding(1252))) // Encoding 1252 is the same used by NAV (Windows-1252)
                {
                    ObjectElements element = new ObjectElements();
                    element.FilePath = Path.GetFileName(file);

                    string line;

                    while ((line = reader.ReadLine()) != null)
                    {
                        lineNumber++;
                        if (line.Equals("  CONTROLS"))
                        {
                            while ((line = reader.ReadLine()) != null && !line.Equals("  }"))
                            {
                                lineNumber++;
                                if (line.Contains("{"))
                                {
                                    var lineParts = line.Split(';');
                                    if (lineParts.Length > 0)
                                    {
                                        string id = lineParts[0].Replace('{', ' ').Trim();
                                        if (id.Length > 0 && !id.Contains("ID="))
                                        {
                                            ControlProperties properties = new ControlProperties();
                                            properties.ID = id;
                                            properties.LineNumber = lineNumber.ToString();
                                            properties.LinePreview = line;
                                            element.Controls.Add(properties);
                                        }
                                    }
                                }
                            }
                        }
                        //if(line.Equals("  CODE"))
                        //{
                        //    while ((line = reader.ReadLine()) != null && !line.Equals("  }"))
                        //    {
                        //        counter++;
                        //        if (line.Contains("PROCEDURE"))
                        //        {
                        //            var el = line.Split('@');
                        //            var temp = el[1].Split('(');

                        //            ControlProperties cp = new ControlProperties();
                        //            cp.ID = temp[0];
                        //            cp.LineNumber = counter.ToString();
                        //            cp.LinePreview = line;
                        //            objel.Controls.Add(cp);
                        //        }
                        //    }
                        //}
                    }

                    foreach (ControlProperties control in element.Controls)
                    {
                        Search(control, element.Controls, ref element);
                    }

                    element.Conflicts.Sort();

                    elements.Add(element);
                }
            }

            ConflictsListView.ItemsSource = elements;
        }

        /// <summary>
        /// Method which searches the given ControlProperties object in the passed collection of ControlObjects.
        /// Any occurrence is added to the Conflicts collection of passed ObjectElements.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="elements"></param>
        /// <param name="objp"></param>
        private void Search(ControlProperties element, List<ControlProperties> elements, ref ObjectElements objp)
        {
            foreach (ControlProperties elem in elements)
            {
                if (element.ID.Equals(elem.ID) && !element.LineNumber.Equals(elem.LineNumber))
                {
                    objp.Conflicts.Add(elem);
                }
            }
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
        /// Method which finds all *.txt files in specified folder and subfolders.
        /// </summary>
        /// <param name="Path">The path of the directory</param>
        /// <returns>The list containing all the paths of the text files found in working directory</returns>
        private List<string> FindFiles(string Path)
        {
            List<string> files = new List<string>();
            try
            {
                foreach (string f in Directory.GetFiles(Path).Where(i => i.EndsWith(".txt")))
                {
                    files.Add(f);
                }
                foreach (string d in Directory.GetDirectories(Path).Where(i => !i.EndsWith(".hg")))
                {
                    files.AddRange(FindFiles(d));
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message, "Project Kittan", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return files;
        }

        /// <summary>
        /// Method invoked when the user click on DateTimeUpdateCheckBox control.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            _avoidUpdateDateTime = (bool)DateTimeUpdateCheckBox.IsChecked;
        }
    }
}
