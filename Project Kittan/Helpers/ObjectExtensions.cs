using Project_Kittan.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;

namespace Project_Kittan.Helpers
{
    /// <summary>
    /// ObjectExtensions class
    /// </summary>
    internal static class ObjectExtensions
    {
        #region Add tag

        /// <summary>
        /// Method which updates OBJECT-PROPERTIES with passed information.
        /// </summary>
        /// <param name="files">The collection of files to update</param>
        /// <param name="updateDateTime"></param>
        /// <param name="tag">The version tag to add</param>
        /// <param name="version">The version of NAV used</param>
        public static void AddTag(Models.File[] files, int version, string tag, bool updateDateTime, IProgress<KeyValuePair<double, string>> progress)
        {
            double progressStep = (double)100 / files.Length;
            double step = 0;

            if (string.IsNullOrWhiteSpace(tag)) return;

            foreach (Models.File file in files)
            {
                step += progressStep;
                progress.Report(new KeyValuePair<double, string>(step, string.Format("Adding tag {0} to {1}", tag, file.Name)));

                if (System.IO.File.Exists(file.Path))
                {
                    string lines;
                    using (FileStream stream = new FileStream(file.Path, FileMode.Open, FileAccess.Read))
                    using (StreamReader reader = new StreamReader(stream, Encoding.GetEncoding(Properties.Settings.Default.DefaultEncoding)))
                    {
                        lines = reader.ReadToEnd();
                    }

                    if (!string.IsNullOrWhiteSpace(lines) && lines.IndexOf("  OBJECT-PROPERTIES") != -1)
                    {
                        StringBuilder builder = new StringBuilder();
                        using (StringReader reader = new StringReader(lines))
                        {
                            string line;
                            while (!((line = reader.ReadLine()) == null))
                            {
                                if (line.StartsWith("  OBJECT-PROPERTIES"))
                                {
                                    builder.AppendLine(line);
                                    builder.AppendLine(reader.ReadLine());

                                    line = reader.ReadLine();
                                    if (line.StartsWith("    Date=") && updateDateTime)
                                    {
                                        line = string.Format("    Date={0:dd.MM.yy};", DateTime.Today);
                                    }
                                    builder.AppendLine(line);

                                    line = reader.ReadLine();
                                    if (line.StartsWith("    Time=") && updateDateTime)
                                    {
                                        line = string.Format("    Time={0:HH:mm:ss};", DateTime.Now);
                                    }
                                    builder.AppendLine(line);

                                    line = reader.ReadLine();
                                    if (line.StartsWith("    Modified="))
                                    {
                                        builder.AppendLine(line);
                                        line = reader.ReadLine();
                                    }

                                    if (line.StartsWith("    Version List="))
                                    {
                                        string versionList = line.Substring(17);  // Get version list tags
                                        versionList = versionList.Substring(0, versionList.Length - 1);  // Remove leading ;

                                        string[] tags = versionList.Split(',');
                                        if (Array.IndexOf(tags, tag) == -1)
                                        {
                                            StringBuilder versionBuilder = new StringBuilder(versionList);
                                            versionBuilder.Append(",");
                                            versionBuilder.Append(tag);
                                            versionBuilder.Replace(",,", ",");

                                            switch (version)
                                            {
                                                case 0: // NAV 2013 and below
                                                    {
                                                        if (versionBuilder.Length > 80)
                                                        {
                                                            Application.Current.Dispatcher.Invoke(delegate
                                                            {
                                                                RequestDialog dialog = new RequestDialog(versionBuilder.ToString(), 80);
                                                                if (!(bool)dialog.ShowDialog())
                                                                {
                                                                    versionBuilder = new StringBuilder(versionList);
                                                                }
                                                            });
                                                        }
                                                        break;
                                                    }
                                                default: // NAV 2015 and above
                                                    {
                                                        if (versionBuilder.Length > 250)
                                                        {
                                                            RequestDialog dialog = new RequestDialog(versionBuilder.ToString(), 250);
                                                            if (!(bool)dialog.ShowDialog())
                                                            {
                                                                versionBuilder = new StringBuilder(versionList);
                                                            }
                                                        }
                                                        break;
                                                    }
                                            }

                                            versionBuilder.Insert(0, "    Version List=");
                                            versionBuilder.Append(";");
                                            versionBuilder.Replace(",;", ";");
                                            versionBuilder.Replace(",,", ",");
                                            versionBuilder.Replace("=,", "=");
                                            line = versionBuilder.ToString();
                                        }
                                    }
                                    builder.AppendLine(line);
                                    builder.Append(reader.ReadToEnd());
                                }
                                else
                                {
                                    builder.AppendLine(line);
                                }
                            }

                            using (FileStream stream = new FileStream(file.Path, FileMode.Truncate, FileAccess.Write))
                            using (StreamWriter writer = new StreamWriter(stream, Encoding.GetEncoding(Properties.Settings.Default.DefaultEncoding)))
                            {
                                writer.Write(builder.ToString());
                            }
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
        public static void RemoveTag(Models.File[] files, string tag, bool ignoreCase, IProgress<KeyValuePair<double, string>> progress)
        {
            double progressStep = (double)100 / files.Length;
            double step = 0;

            if (string.IsNullOrWhiteSpace(tag)) return;

            foreach (Models.File file in files)
            {
                step += progressStep;
                progress.Report(new KeyValuePair<double, string>(step, string.Format("Removing tag {0} from {1}", tag, file.Name)));

                if (System.IO.File.Exists(file.Path))
                {
                    string lines;
                    using (FileStream stream = new FileStream(file.Path, FileMode.Open, FileAccess.Read))
                    using (StreamReader reader = new StreamReader(stream, Encoding.GetEncoding(Properties.Settings.Default.DefaultEncoding)))
                    {
                        lines = reader.ReadToEnd();
                    }

                    if (!string.IsNullOrWhiteSpace(lines) && lines.IndexOf("  OBJECT-PROPERTIES") != -1)
                    {
                        StringBuilder builder = new StringBuilder();
                        using (StringReader reader = new StringReader(lines))
                        {
                            string line;
                            while (!((line = reader.ReadLine()) == null))
                            {
                                if (line.StartsWith("    Version List="))
                                {
                                    line = line.Substring(17);  // Get version list tags
                                    line = line.Substring(0, line.Length - 1);  // Remove leading ;


                                    line = ignoreCase ? line.Replace(tag, "", StringComparison.OrdinalIgnoreCase) : line.Replace(tag, "");

                                    line = string.Format("    Version List={0};", line);
                                    line = line.Replace(",,", ",");
                                    line = line.Replace(",;", ";");

                                    builder.AppendLine(line);
                                    builder.AppendLine(reader.ReadToEnd());
                                }
                                else
                                {
                                    builder.AppendLine(line);
                                }
                            }

                            using (FileStream stream = new FileStream(file.Path, FileMode.Truncate, FileAccess.Write))
                            using (StreamWriter writer = new StreamWriter(stream, Encoding.GetEncoding(Properties.Settings.Default.DefaultEncoding)))
                            {
                                writer.Write(builder.ToString());
                            }
                        }
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
        public static IEnumerable<NAVObject> FindWhere(Models.File[] files, string keyword, IProgress<KeyValuePair<double,string>> progress, CancellationToken token)
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
                    string lines;
                    using (FileStream stream = new FileStream(file.Path, FileMode.Open, FileAccess.Read))
                    using (StreamReader reader = new StreamReader(stream, Encoding.GetEncoding(Properties.Settings.Default.DefaultEncoding)))
                    {
                        lines = reader.ReadToEnd();
                    }

                    if (!string.IsNullOrWhiteSpace(lines) && lines.IndexOf("  OBJECT-PROPERTIES") != -1)
                    {
                        foreach (string navObject in ObjectSplitterExtensions.Split(lines))
                        {
                            if (navObject.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) != -1) // The file contains the keyword
                            {
                                using (StringReader stringReader = new StringReader(navObject))
                                {
                                    string[] startingLineWords = new StringReader(navObject).ReadLine().Split(' '); // Get the first line of the file

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
        public static IEnumerable<NAVObject> PrepareData(Models.File[] files, IProgress<KeyValuePair<double, string>> progress, CancellationToken token)
        {
            double progressStep = (double)100 / files.Length;
            double step = 0;

            foreach (Models.File file in files)
            {
                step += progressStep;
                progress.Report(new KeyValuePair<double, string>(step, string.Format("Obtaining filters...", file.Name)));

                if (token.IsCancellationRequested)
                {
                    progress.Report(new KeyValuePair<double, string>(0, "Operation aborted"));
                    token.ThrowIfCancellationRequested();
                }

                if (System.IO.File.Exists(file.Path))
                {
                    string lines;
                    using (FileStream stream = new FileStream(file.Path, FileMode.Open, FileAccess.Read))
                    using (StreamReader reader = new StreamReader(stream, Encoding.GetEncoding(Properties.Settings.Default.DefaultEncoding)))
                    {
                        lines = reader.ReadToEnd();
                    }

                    if (!string.IsNullOrWhiteSpace(lines) && lines.IndexOf("  OBJECT-PROPERTIES") != -1)
                    {
                        foreach (string navObject in ObjectSplitterExtensions.Split(lines))
                        {
                            using (StringReader stringReader = new StringReader(navObject))
                            {
                                string[] startingLineWords = new StringReader(navObject).ReadLine().Split(' '); // Get the first line of the file

                                yield return new NAVObject(startingLineWords[1], startingLineWords[2], GetObjectNameFromFirstLine(startingLineWords), file.Path);
                            }
                        }
                    }
                }
            }
        }

        private static string GetObjectNameFromFirstLine(string[] words)
        {
            string objectName = "";
            for (int i = 3; i < words.Length; i++)
            {
                objectName += words[i] + ' ';
            }
            return objectName.Trim();
        }

        #endregion


    }
}