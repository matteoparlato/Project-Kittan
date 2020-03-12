using Project_Kittan.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Project_Kittan.Helpers
{
    /// <summary>
    /// ObjectExtensions class
    /// </summary>
    internal static class ObjectExtensions
    {
        public static ObservableCollection<ObjectElements> Conflicts { get; private set; } = new ObservableCollection<ObjectElements>();

        public static ObservableCollection<ObjectElements> Found { get; private set; } = new ObservableCollection<ObjectElements>();

        // private static Object listAccessLock = new Object();

        /// <summary>
        /// Method which return the type of file (if count is higher than one the file
        /// contains more than one object otherewise only one).
        /// </summary>
        /// <param name="text">The text of the file</param>
        /// <param name="pattern">The text to search for occurrences count</param>
        /// <returns>The type of file</returns>
        private static int GetStringOccurrences(string text, string pattern)
        {
            int count = 0, i = 0;

            while ((i = text.IndexOf(pattern, i, StringComparison.OrdinalIgnoreCase)) != -1)
            {
                i += pattern.Length;
                count++;

                if (count > 1) return count;
            }
            return count;
        }

        #region Properties updater

        /// <summary>
        /// Method which updates OBJECT-PROPERTIES with passed information.
        /// </summary>
        /// <param name="files">The collection of files to update</param>
        /// <param name="avoidUpdateDateTime"></param>
        /// <param name="version">The version tag to add</param>
        /// <param name="navVersion">The version of NAV used</param>
        internal async static Task UpdateObjects(Models.File[] files, bool avoidUpdateDateTime, string version, int navVersion)
        {
            double step = (double)100 / files.Length;

            for (int i = 0; i < files.Length; i++)
            {
                if (System.IO.File.Exists(files[i].FilePath))
                {
                    MainWindow.Current.StatusTextBlock.Text = "Updating " + files[i].FileName + " properties..."; // Update status

                    StringBuilder builder = new StringBuilder();

                    using (FileStream stream = new FileStream(files[i].FilePath, FileMode.Open, FileAccess.Read))
                    using (StreamReader reader = new StreamReader(stream, Encoding.GetEncoding(Properties.Settings.Default.DefaultEncoding)))
                    {
                        string line;

                        while ((line = await reader.ReadLineAsync()) != null)
                        {
                            if (line.Equals("  OBJECT-PROPERTIES"))
                            {
                                builder.AppendLine(line); // Add "  OBJECT-PROPERTIES"
                                builder.AppendLine(await reader.ReadLineAsync()); // Add "  {"

                                line = await reader.ReadLineAsync();
                                if (line.Contains("Date") && !avoidUpdateDateTime)
                                {
                                    builder.AppendLine(string.Format("    Date={0:dd.MM.yy};", DateTime.Today));
                                }
                                else
                                {
                                    builder.AppendLine(line);
                                }

                                line = await reader.ReadLineAsync();
                                if (line.Contains("Time") && !avoidUpdateDateTime)
                                {
                                    builder.AppendLine(string.Format("    Time={0:HH:mm:ss};", DateTime.Now)); // Add "Time..."
                                }
                                else
                                {
                                    builder.AppendLine(line);
                                }

                                line = await reader.ReadLineAsync();
                                if (line.Contains("Modified"))
                                {
                                    builder.AppendLine(line); // Add "Modified..."
                                    line = await reader.ReadLineAsync();
                                }

                                if (line.Contains("Version List"))
                                {
                                    // Check if the user defined tag is already in the "Version List"
                                    if (!string.IsNullOrWhiteSpace(version) && line.IndexOf(version, StringComparison.OrdinalIgnoreCase) == -1)
                                    {
                                        line = line.Replace(";", ",");
                                        line = line + version + ";";

                                        if (line.Contains("=,"))
                                        {
                                            StringBuilder versionBuilder = new StringBuilder(line);
                                            versionBuilder.Replace("=,", "=");
                                            line = versionBuilder.ToString();
                                        }
                                    }

                                    string temp = line.Substring(17);
                                    temp = temp.Substring(0, temp.Length - 1); // Remove "    Version List=...;"

                                    bool avoidInsert = false;

                                    switch (navVersion)
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

                                    if (!avoidInsert) builder.AppendLine(line);
                                }
                            }
                            else
                            {
                                builder.AppendLine(line);
                            }
                        }

                        MainWindow.Current.StatusProgressBar.Value += step; // Update status
                    }

                    using (FileStream stream = new FileStream(files[i].FilePath, FileMode.Truncate, FileAccess.Write))
                    using (StreamWriter writer = new StreamWriter(stream, Encoding.GetEncoding(Properties.Settings.Default.DefaultEncoding)))
                    {
                        MainWindow.Current.StatusTextBlock.Text = "Saving " + files[i].FileName + " changes..."; // Update status
                        await writer.WriteAsync(builder.ToString());
                    }
                }
            }
        }

        #endregion

        #region Tag remover

        /// <summary>
        /// Method which removes a tag from a Version List.
        /// </summary>
        /// <param name="files">The collection of files to update</param>
        /// <param name="tag">The version tag to remove</param>
        internal async static Task RemoveTag(Models.File[] files, string tag, bool ignoreCase)
        {
            double step = (double)100 / files.Length;

            for (int i = 0; i < files.Length; i++)
            {
                if (System.IO.File.Exists(files[i].FilePath))
                {
                    MainWindow.Current.StatusTextBlock.Text = "Removing " + tag + " from file " + files[i].FileName + "..."; // Update status

                    StringBuilder builder = new StringBuilder();

                    using (FileStream stream = new FileStream(files[i].FilePath, FileMode.Open, FileAccess.Read))
                    using (StreamReader reader = new StreamReader(stream, Encoding.GetEncoding(Properties.Settings.Default.DefaultEncoding)))
                    {
                        string line;

                        while ((line = await reader.ReadLineAsync()) != null)
                        {
                            if (line.Equals("  OBJECT-PROPERTIES"))
                            {
                                builder.AppendLine(line); // Add "  OBJECT-PROPERTIES"
                                builder.AppendLine(await reader.ReadLineAsync()); // Add "  {"

                                builder.AppendLine(await reader.ReadLineAsync());
                                builder.AppendLine(await reader.ReadLineAsync());

                                line = await reader.ReadLineAsync();
                                if (line.Contains("Modified"))
                                {
                                    builder.AppendLine(line); // Add "Modified..."
                                    line = await reader.ReadLineAsync();
                                }

                                if (line.Contains("Version List"))
                                {
                                    string temp = line.Substring(17);
                                    temp = temp.Substring(0, temp.Length - 1); // Remove "    Version List=...;"

                                    if (ignoreCase)
                                    {
                                        line = Regex.Replace(temp, tag, string.Empty, RegexOptions.IgnoreCase);
                                    }
                                    else
                                    {
                                        line = temp.Replace(tag, string.Empty);
                                    }

                                    line = "    Version List=" + line + ";";
                                    line = line.Replace(",,", ",");
                                    line = line.Replace(",;", ";");
                                }

                                builder.AppendLine(line);
                            }
                            else
                            {
                                builder.AppendLine(line);
                            }
                        }

                        MainWindow.Current.StatusProgressBar.Value += step; // Update status
                    }

                    using (FileStream stream = new FileStream(files[i].FilePath, FileMode.Truncate, FileAccess.Write))
                    using (StreamWriter writer = new StreamWriter(stream, Encoding.GetEncoding(Properties.Settings.Default.DefaultEncoding)))
                    {
                        MainWindow.Current.StatusTextBlock.Text = "Saving " + files[i].FileName + " changes..."; // Update status
                        await writer.WriteAsync(builder.ToString());
                    }
                }
            }
        }

        #endregion

        #region Finder

        /// <summary>
        /// Method which finds occurrences of the passed string in NAV objects in working directory.
        /// </summary>
        /// <param name="files">The files to check</param>
        /// <param name="pattern">The string to search</param>
        /// <returns></returns>
        internal async static Task FindWhere(Models.File[] files, string pattern)
        {
            double progressStep = (double)100 / files.Length;

            Found.Clear();

            //ConcurrentBag<ObjectElements> concurrentFound = new ConcurrentBag<ObjectElements>();

            for (int i = 0; i < files.Length; i++)
            {
                //Parallel.ForEach(files, async (file) =>
                //{
                if (System.IO.File.Exists(files[i].FilePath))
                {
                    //await System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    // {
                    //     MainWindow.Current.StatusTextBlock.Text = "Searching occurrences in " + file.FileName + "..."; // Update status
                    // }), DispatcherPriority.Background);

                    MainWindow.Current.StatusTextBlock.Text = "Searching occurrences in " + files[i].FileName + "..."; // Update status

                    using (FileStream stream = new FileStream(files[i].FilePath, FileMode.Open, FileAccess.Read))
                    using (StreamReader reader = new StreamReader(stream, Encoding.GetEncoding(Properties.Settings.Default.DefaultEncoding)))
                    {
                        string lines = await reader.ReadToEndAsync();

                        if (GetStringOccurrences(lines, "  OBJECT-PROPERTIES") > 1) // The file contains multiple objects
                        {
                            string[] objects = ObjectSplitterExtensions.Split(lines);

                            for (int j = 1; j < objects.Length; j++)
                            {
                                if (GetStringOccurrences(objects[j], pattern) != 0)
                                {
                                    var startingLine = objects[j].Split(new string[] { Environment.NewLine }, StringSplitOptions.None)[0].Split(' ');

                                    string objectName = string.Empty;
                                    for (int k = 2; k < startingLine.Length; k++) objectName += startingLine[k] + ' ';

                                    //lock (listAccessLock)
                                    //{
                                    Found.Add(new ObjectElements(startingLine[0], startingLine[1], objectName, files[i].FileName));
                                    //}
                                }
                            }
                        }
                        else // The file contains one object
                        {
                            if (GetStringOccurrences(lines, pattern) != 0)
                            {
                                var startingLine = lines.Split(new string[] { Environment.NewLine }, StringSplitOptions.None)[0].Split(' ');

                                string objectName = string.Empty;
                                for (int j = 3; j < startingLine.Length; j++) objectName += startingLine[j] + ' ';

                                //lock (listAccessLock)
                                //{
                                Found.Add(new ObjectElements(startingLine[1], startingLine[2], objectName, files[i].FileName));
                                //}
                            }
                        }
                    }

                    //await System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    //{
                    //    MainWindow.Current.StatusProgressBar.Value += progressStep; // Update status
                    //}), DispatcherPriority.Background);

                    MainWindow.Current.StatusProgressBar.Value += progressStep; // Update status
                }
                //});
            }

            MainWindow.Current.StatusTextBlock.Text = Found.Count != 0 ? "Found " + Found.Count + " occurrences in " + files.Length + " files" : "No occurences found for the string " + pattern; // Update status
        }

        #endregion
    }
}