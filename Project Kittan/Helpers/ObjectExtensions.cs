using Project_Kittan.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Project_Kittan.Helpers
{
    /// <summary>
    /// ObjectExtensions class
    /// </summary>
    internal static class ObjectExtensions
    {
        internal const string ObjectSplitterPattern = @"(OBJECT \w* \d* .*
{)";

        internal static readonly char[] InvalidFileNameChars = Path.GetInvalidFileNameChars();

        /// <summary>
        /// Method which adds a tag to the Version List of passed NAV object files.
        /// </summary>
        /// <param name="files">The collection of files to update</param>
        /// <param name="version">The target version of NAV</param>
        /// <param name="tag">The tag to add to the version list</param>
        /// <param name="updateDateTime">Specifies whether Date and Time must be updated</param>
        /// <param name="progress">The progress of the operation</param>
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
                        foreach (string navObject in Split(lines))
                        {
                            using (StringReader reader = new StringReader(navObject))
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

        /// <summary>
        /// Method which removes a tag from the Version List of passed NAV object files.
        /// </summary>
        /// <param name="files">The collection of files to update</param>
        /// <param name="tag">The tag to remove to the version list</param>
        /// <param name="ignoreCase">Specify whether use case-sensitive replace</param>
        /// <param name="progress">The progress of the operation</param>
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
                        foreach (string navObject in Split(lines))
                        { 
                            using (StringReader reader = new StringReader(navObject))
                            {
                                string line;
                                while (!((line = reader.ReadLine()) == null))
                                {
                                    if (line.StartsWith("    Version List="))
                                    {
                                        string versionList = line.Substring(17);  // Get version list tags
                                        versionList = versionList.Substring(0, versionList.Length - 1);  // Remove leading ;

                                        string[] tags = versionList.Split(',');
                                        if (!(Array.IndexOf(tags, tag) == -1))
                                        {
                                            tags = tags.Where(str => !str.Equals(tag)).ToArray();
                                            StringBuilder versionBuilder = new StringBuilder(string.Join(",", tags));
                                            versionBuilder.Insert(0, "    Version List=");
                                            versionBuilder.Append(";");
                                            versionBuilder.Replace(",;", ";");
                                            versionBuilder.Replace(",,", ",");
                                            versionBuilder.Replace("=,", "=");
                                            line = versionBuilder.ToString();

                                            builder.AppendLine(line);
                                            builder.Append(reader.ReadToEnd());
                                        }
                                    }
                                    else
                                    {
                                        builder.AppendLine(line);
                                    }
                                }
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

        /// <summary>
        /// Method which finds in which NAV objects there is the passed keyword.
        /// </summary>
        /// <param name="files">The collection of files to check</param>
        /// <param name="keyword">The search keyword</param>
        /// <param name="progress">The progress of the operation</param>
        /// <param name="token">The cancellation token</param>
        /// <returns>The NAV object where an occurrence of the keyword is found</returns>
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
                        foreach (string navObject in Split(lines))
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
        /// Method which reads passed NAV objects.
        /// </summary>
        /// <param name="files">The collection of files to check</param>
        /// <param name="progress">The progress of the operation</param>
        /// <param name="token">The cancellation token</param>
        /// <returns>The NAV object details</returns>
        public static IEnumerable<NAVObject> GetObjects(Models.File[] files, IProgress<KeyValuePair<double, string>> progress, CancellationToken token)
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
                        foreach (string navObject in Split(lines))
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

        /// <summary>
        /// Method which splits and stores a text file containing multiple NAV objects into single NAV object text files.
        /// </summary>
        /// <param name="filePath">The path of the file to split</param>
        /// <param name="progress">The progress of the operation</param>
        /// <param name="token">The cancellation token</param>
        /// <returns>The path of the destination folder</returns>
        public static string SplitAndStore(string filePath, IProgress<KeyValuePair<bool, string>> progress, CancellationToken token)
        {
            return SplitAndStore(filePath, Path.GetDirectoryName(filePath), progress, token);
        }

        /// <summary>
        /// Method which splits and stores a text file containing multiple NAV objects into single NAV object text files.
        /// </summary>
        /// <param name="filePath">The path of the file to split</param>
        /// <param name="destinationFolder">The folder where to store splitted files</param>
        /// <param name="progress">The progress of the operation</param>
        /// <param name="token">The cancellation token</param>
        /// <returns>The path of the destination folder</returns>
        public static string SplitAndStore(string filePath, string destinationFolder, IProgress<KeyValuePair<bool, string>> progress, CancellationToken token)
        {
            string folderName = Path.GetFileNameWithoutExtension(filePath) + "_" + string.Format("{0:yyyyMMdd}", DateTime.Now);

            string extractionFolderPath = Path.Combine(Path.Combine(destinationFolder, folderName));
            if (!Directory.Exists(extractionFolderPath))
            {
                Directory.CreateDirectory(extractionFolderPath);
            }

            string lines;
            using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            using (StreamReader reader = new StreamReader(stream, Encoding.GetEncoding(Properties.Settings.Default.DefaultEncoding)))
            {
                lines = reader.ReadToEnd();
            }

            if (!string.IsNullOrWhiteSpace(lines) && lines.IndexOf("  OBJECT-PROPERTIES") != -1)
            {
                Parallel.ForEach(Split(lines), navObject =>
                {
                    progress.Report(new KeyValuePair<bool, string>(true, "Splitting file..."));

                    if (token.IsCancellationRequested)
                    {
                        progress.Report(new KeyValuePair<bool, string>(false, "Operation aborted"));
                        token.ThrowIfCancellationRequested();
                    }

                    string firstLine = navObject.Substring(0, navObject.IndexOf(Environment.NewLine));
                    string fileName = string.Join("_", firstLine.Trim().Split(InvalidFileNameChars)).Substring(7);

                    string destFolder = Path.Combine(extractionFolderPath, firstLine.Split(' ')[1]);
                    if (!Directory.Exists(destFolder))
                    {
                        Directory.CreateDirectory(destFolder);
                    }

                    using (FileStream stream = new FileStream(Path.Combine(destFolder, fileName + ".txt"), FileMode.Create, FileAccess.Write))
                    using (StreamWriter writer = new StreamWriter(stream, Encoding.GetEncoding(Properties.Settings.Default.DefaultEncoding)))
                    {
                        writer.Write(navObject);
                    }
                });
            }

            return extractionFolderPath;
        }

        /// <summary>
        /// Method which composes NAV object name given the words of the first line of a NAV object file.
        /// </summary>
        /// <param name="words">The words of the first line of the file</param>
        /// <returns>The name of the object</returns>
        private static string GetObjectNameFromFirstLine(string[] words)
        {
            string objectName = "";
            for (int i = 3; i < words.Length; i++)
            {
                objectName += words[i] + ' ';
            }
            return objectName.Trim();
        }

        /// <summary>
        /// Method which return an array containing splitted NAV objects.
        /// </summary>
        /// <param name="lines">The string containing multiple objects</param>
        /// <returns>The array containig splitted objects</returns>
        public static IEnumerable<string> Split(string lines)
        {
            string[] splittedParts = Regex.Split(lines, ObjectSplitterPattern, RegexOptions.Compiled).Skip(1).ToArray();

            for (int i = 0; i < splittedParts.Length; i++)
            {
                yield return (splittedParts[i++] + splittedParts[i]);
            }
        }
    }
}