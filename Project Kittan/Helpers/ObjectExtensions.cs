using Project_Kittan.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;

namespace Project_Kittan.Helpers
{
    /// <summary>
    /// ObjectExtensions class
    /// </summary>
    internal static class ObjectExtensions
    {
        public static ObservableCollection<NAVObject> Found { get; private set; } = new ObservableCollection<NAVObject>();

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

        #region Add tag

        /// <summary>
        /// Method which updates OBJECT-PROPERTIES with passed information.
        /// </summary>
        /// <param name="files">The collection of files to update</param>
        /// <param name="avoidUpdateDateTime"></param>
        /// <param name="tag">The version tag to add</param>
        /// <param name="version">The version of NAV used</param>
        internal static void AddTag(Models.File[] files, int version, string tag, bool avoidUpdateDateTime, IProgress<KeyValuePair<double, string>> progress)
        {
            double progressStep = (double)100 / files.Length;
            double step = 0;

            foreach (Models.File file in files)
            {
                step += progressStep;
                progress.Report(new KeyValuePair<double, string>(step, string.Format("Adding tag {0} to {1}", tag, file.Name)));

                if (System.IO.File.Exists(file.Path))
                {
                    StringBuilder builder = new StringBuilder();

                    using (FileStream stream = new FileStream(file.Path, FileMode.Open, FileAccess.Read))
                    using (StreamReader reader = new StreamReader(stream, Encoding.GetEncoding(Properties.Settings.Default.DefaultEncoding)))
                    {
                        string line;

                        while ((line = reader.ReadLine()) != null)
                        {
                            if (line.Equals("  OBJECT-PROPERTIES"))
                            {
                                builder.AppendLine(line); // Add "  OBJECT-PROPERTIES"
                                builder.AppendLine(reader.ReadLine()); // Add "  {"

                                line = reader.ReadLine();
                                if (line.Contains("Date") && !avoidUpdateDateTime)
                                {
                                    builder.AppendLine(string.Format("    Date={0:dd.MM.yy};", DateTime.Today));
                                }
                                else
                                {
                                    builder.AppendLine(line);
                                }

                                line = reader.ReadLine();
                                if (line.Contains("Time") && !avoidUpdateDateTime)
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
                                    if (!string.IsNullOrWhiteSpace(tag) && line.IndexOf(tag, StringComparison.OrdinalIgnoreCase) == -1)
                                    {
                                        line = line.Replace(";", ",");
                                        line = line + tag + ";";

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

                                    switch (version)
                                    {
                                        case 0: // NAV 2013 and below
                                            {
                                                if (temp.Length > 80)
                                                {
                                                    Application.Current.Dispatcher.Invoke(delegate
                                                    {
                                                        RequestDialog dialog = new RequestDialog(temp, 80);
                                                        if ((bool)dialog.ShowDialog())
                                                        {
                                                            builder.AppendLine("    Version List=" + dialog.VersionList + ";");
                                                            avoidInsert = true;
                                                        }
                                                    });
                                                }
                                                break;
                                            }
                                        default: // NAV 2015 and above
                                            {
                                                if (temp.Length > 250)
                                                {
                                                    RequestDialog dialog = new RequestDialog(temp, 250);
                                                    if ((bool)dialog.ShowDialog())
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
                    }

                    using (FileStream stream = new FileStream(file.Path, FileMode.Truncate, FileAccess.Write))
                    using (StreamWriter writer = new StreamWriter(stream, Encoding.GetEncoding(Properties.Settings.Default.DefaultEncoding)))
                    {
                        try
                        {
                            writer.Write(builder.ToString());
                        }
                        catch (OutOfMemoryException) // OutOfMemoryException thrown when reading large files
                        {
                            GC.Collect();
                            writer.Write(builder.ToString());
                        }
                    }
                }
            }
        }

        #endregion

        #region Remove tag

        /// <summary>
        /// Method which removes a tag from a Version List.
        /// </summary>
        /// <param name="files">The collection of files to update</param>
        /// <param name="tag">The version tag to remove</param>
        internal static void RemoveTag(Models.File[] files, string tag, bool ignoreCase, IProgress<KeyValuePair<double, string>> progress)
        {
            double progressStep = (double)100 / files.Length;
            double step = 0;

            foreach (Models.File file in files)
            {
                step += progressStep;
                progress.Report(new KeyValuePair<double, string>(step, string.Format("Removing tag {0} from {1}", tag, file.Name)));

                if (System.IO.File.Exists(file.Path))
                {
                    StringBuilder builder = new StringBuilder();

                    using (FileStream stream = new FileStream(file.Path, FileMode.Open, FileAccess.Read))
                    using (StreamReader reader = new StreamReader(stream, Encoding.GetEncoding(Properties.Settings.Default.DefaultEncoding)))
                    {
                        string line;

                        while ((line = reader.ReadLine()) != null)
                        {
                            if (line.Equals("  OBJECT-PROPERTIES"))
                            {
                                builder.AppendLine(line); // Add "  OBJECT-PROPERTIES"
                                builder.AppendLine(reader.ReadLine()); // Add "  {"

                                builder.AppendLine(reader.ReadLine());
                                builder.AppendLine(reader.ReadLine());

                                line = reader.ReadLine();
                                if (line.Contains("Modified"))
                                {
                                    builder.AppendLine(line); // Add "Modified..."
                                    line = reader.ReadLine();
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

                        //MainWindow.Current.StatusProgressBar.Value += step; // Update status
                    }

                    using (FileStream stream = new FileStream(file.Path, FileMode.Truncate, FileAccess.Write))
                    using (StreamWriter writer = new StreamWriter(stream, Encoding.GetEncoding(Properties.Settings.Default.DefaultEncoding)))
                    {
                        writer.Write(builder.ToString());
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
        /// <param name="keyword">The string to search</param>
        /// <returns></returns>
        internal static IEnumerable<NAVObject> FindWhere(Models.File[] files, string keyword, IProgress<KeyValuePair<double,string>> progress, CancellationToken token)
        {
            double progressStep = (double)100 / files.Length;
            double step = 0;
            
            foreach (Models.File file in files)
            {
                step += progressStep;
                progress.Report(new KeyValuePair<double, string>(step, string.Format("Searching for {0} in {1}", keyword, file.Name)));

                if (token.IsCancellationRequested)
                {
                    progress.Report(new KeyValuePair<double, string>(0, "Operation aborted"));
                    token.ThrowIfCancellationRequested();
                }

                if (System.IO.File.Exists(file.Path))
                {
                    using (FileStream stream = new FileStream(file.Path, FileMode.Open, FileAccess.Read))
                    using (StreamReader reader = new StreamReader(stream, Encoding.GetEncoding(Properties.Settings.Default.DefaultEncoding)))
                    {
                        string lines;
                        try
                        {
                            lines = reader.ReadToEnd();
                        }
                        catch(OutOfMemoryException) // OutOfMemoryException thrown when reading large files
                        {
                            GC.Collect();
                            lines = reader.ReadToEnd();
                        }

                        int multipleObjects = GetStringOccurrences(lines, "  OBJECT-PROPERTIES");
                        if (multipleObjects > 1) // The file contains multiple objects
                        {
                            string[] navObjects = ObjectSplitterExtensions.Split(lines);

                            foreach (string navObject in navObjects)
                            {
                                if (GetStringOccurrences(navObject, keyword) != 0) // The file contains the keyword
                                {
                                    using (StringReader stringReader = new StringReader(navObject))
                                    {
                                        string[] startingLineWords = new StringReader(navObject).ReadLine().Split(' '); // Get the first line of the file

                                        yield return new NAVObject(startingLineWords[1], startingLineWords[2], GetObjectNameFromFirstLine(startingLineWords), file.Path);
                                    }
                                }
                            }
                        }
                        else if (multipleObjects == 1) // The file contains only one object
                        {
                            if (GetStringOccurrences(lines, keyword) != 0) // The file contains the keyword
                            {
                                using (StringReader stringReader = new StringReader(lines))
                                {
                                    string[] startingLineWords = new StringReader(lines).ReadLine().Split(' '); // Get the first line of the file

                                    yield return new NAVObject(startingLineWords[1], startingLineWords[2], GetObjectNameFromFirstLine(startingLineWords), file.Path);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Method which finds occurrences of the passed string in NAV objects in working directory.
        /// </summary>
        /// <param name="files">The files to check</param>
        /// <param name="keyword">The string to search</param>
        /// <returns></returns>
        internal static IEnumerable<NAVObject> PrepareData(Models.File[] files, IProgress<KeyValuePair<double, string>> progress, CancellationToken token)
        {
            double progressStep = (double)100 / files.Length;
            double step = 0;

            foreach (Models.File file in files)
            {
                step += progressStep;
                progress.Report(new KeyValuePair<double, string>(step, string.Format("Preparing filters...", file.Name)));

                if (token.IsCancellationRequested)
                {
                    progress.Report(new KeyValuePair<double, string>(0, "Operation aborted"));
                    token.ThrowIfCancellationRequested();
                }

                if (System.IO.File.Exists(file.Path))
                {
                    using (FileStream stream = new FileStream(file.Path, FileMode.Open, FileAccess.Read))
                    using (StreamReader reader = new StreamReader(stream, Encoding.GetEncoding(Properties.Settings.Default.DefaultEncoding)))
                    {
                        string lines;
                        try
                        {
                            lines = reader.ReadToEnd();
                        }
                        catch (OutOfMemoryException) // OutOfMemoryException thrown when reading large files
                        {
                            GC.Collect();
                            lines = reader.ReadToEnd();
                        }

                        int multipleObjects = GetStringOccurrences(lines, "  OBJECT-PROPERTIES");
                        if (multipleObjects > 1) // The file contains multiple objects
                        {
                            string[] navObjects = ObjectSplitterExtensions.Split(lines);

                            foreach (string navObject in navObjects)
                            {
                                using (StringReader stringReader = new StringReader(navObject))
                                {
                                    string[] startingLineWords = new StringReader(navObject).ReadLine().Split(' '); // Get the first line of the file

                                    yield return new NAVObject(startingLineWords[1], startingLineWords[2], GetObjectNameFromFirstLine(startingLineWords), file.Path);
                                }
                            }
                        }
                        else if (multipleObjects == 1) // The file contains only one object
                        {
                            using (StringReader stringReader = new StringReader(lines))
                            {
                                string[] startingLineWords = new StringReader(lines).ReadLine().Split(' '); // Get the first line of the file

                                yield return new NAVObject(startingLineWords[1], startingLineWords[2], GetObjectNameFromFirstLine(startingLineWords), file.Path);
                            }
                        }
                    }
                }
            }
        }

        private static string GetObjectNameFromFirstLine(string[] words)
        {
            string objectName = string.Empty;
            for (int i = 3; i < words.Length; i++)
            {
                objectName += words[i] + ' ';
            }
            return objectName.Trim();
        }

        #endregion
    }
}