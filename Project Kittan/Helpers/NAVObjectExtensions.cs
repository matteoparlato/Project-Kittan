using Project_Kittan.Enums;
using Project_Kittan.Models;
using Project_Kittan.Views;
using System;
using System.Collections.Generic;
using System.Globalization;
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
    /// NAVObjectExtensions class
    /// </summary>
    internal static class NAVObjectExtensions
    {
        private const string ObjectSplitterPattern = @"(OBJECT \w* \d* .*
{)";

        private static readonly char[] InvalidFileNameChars = Path.GetInvalidFileNameChars();

        /// <summary>
        /// Method which adds a tag to the Version List of passed NAV object files.
        /// </summary>
        /// <param name="files">The collection of files to update</param>
        /// <param name="version">The target version of NAV</param>
        /// <param name="tag">The tag to add to the version list</param>
        /// <param name="ignoreCase">Specify whether use case-sensitive insert</param>
        /// <param name="updateDateTime">Specifies whether Date and Time must be updated</param>
        /// <param name="progress">The progress of the operation</param>
        public static void AddTag(Models.File[] files, int version, string tag, bool ignoreCase, bool updateDateTime, IProgress<KeyValuePair<double, string>> progress)
        {
            double progressStep = (double)100 / files.Length;
            double step = 0;

            DateTime dateTime = DateTime.Now;
            DateTimeFormatInfo dateTimeFormat;
            if (string.IsNullOrWhiteSpace(Properties.Settings.Default.DefaultLocale))
            {
                dateTimeFormat = new CultureInfo(Thread.CurrentThread.CurrentCulture.Name, false).DateTimeFormat;
            }
            else
            {
                dateTimeFormat = new CultureInfo(Properties.Settings.Default.DefaultLocale, false).DateTimeFormat;
            }

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

                    if (!string.IsNullOrWhiteSpace(lines) && lines.IndexOf("  OBJECT-PROPERTIES", StringComparison.Ordinal) != -1)
                    {
                        StringBuilder builder = new StringBuilder();
                        foreach (string navObjectLines in Split(lines))
                        {
                            using (StringReader reader = new StringReader(navObjectLines))
                            {
                                string line;
                                while ((line = reader.ReadLine()) != null)
                                {
                                    if (line.StartsWith("  OBJECT-PROPERTIES"))
                                    {
                                        builder.AppendLine(line);
                                        builder.AppendLine(reader.ReadLine());

                                        line = reader.ReadLine();
                                        if (line.StartsWith("    Date=") && updateDateTime)
                                        {
                                            line = string.Format("    Date={0};", Convert.ToDateTime(dateTime, dateTimeFormat).ToString(dateTimeFormat.ShortDatePattern));
                                        }
                                        builder.AppendLine(line);

                                        line = reader.ReadLine();
                                        if (line.StartsWith("    Time=") && updateDateTime)
                                        {
                                            line = string.Format("    Time={0:HH:mm:ss};", dateTime);
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

                                            StringComparison comparison = StringComparison.Ordinal;
                                            if (ignoreCase)
                                            {
                                                comparison = StringComparison.OrdinalIgnoreCase;
                                            }

                                            if (Array.FindIndex(tags, item => item.Equals(tag, comparison)) == -1)
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
                                                                    RequestDialog requestDialog = new RequestDialog(versionBuilder.ToString(), 80);
                                                                    if (requestDialog.ShowDialog() == true)
                                                                    {
                                                                        versionBuilder = new StringBuilder(requestDialog.VersionList);
                                                                    }
                                                                });
                                                            }
                                                            break;
                                                        }
                                                    default: // NAV 2015 and above
                                                        {
                                                            if (versionBuilder.Length > 250)
                                                            {
                                                                Application.Current.Dispatcher.Invoke(delegate
                                                                {
                                                                    RequestDialog requestDialog = new RequestDialog(versionBuilder.ToString(), 250);
                                                                    if (requestDialog.ShowDialog() == true)
                                                                    {
                                                                        versionBuilder = new StringBuilder(requestDialog.VersionList);
                                                                    }
                                                                });
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

            if (string.IsNullOrWhiteSpace(tag))
            {
                return;
            }

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

                    if (!string.IsNullOrWhiteSpace(lines) && lines.IndexOf("  OBJECT-PROPERTIES", StringComparison.Ordinal) != -1)
                    {
                        StringBuilder builder = new StringBuilder();
                        foreach (string navObjectLines in Split(lines))
                        {
                            using (StringReader reader = new StringReader(navObjectLines))
                            {
                                string line;
                                while ((line = reader.ReadLine()) != null)
                                {
                                    if (line.StartsWith("    Version List="))
                                    {
                                        string versionList = line.Substring(17);  // Get version list tags
                                        versionList = versionList.Substring(0, versionList.Length - 1);  // Remove leading ;

                                        string[] tags = versionList.Split(',');

                                        StringComparison comparison = StringComparison.Ordinal;
                                        if (ignoreCase)
                                        {
                                            comparison = StringComparison.OrdinalIgnoreCase;
                                        }

                                        if (Array.FindIndex(tags, item => item.Equals(tag, comparison)) != -1)
                                        {
                                            tags = tags.Where(str => !str.Equals(tag, comparison)).ToArray();
                                            StringBuilder versionBuilder = new StringBuilder(string.Join(",", tags));
                                            versionBuilder.Insert(0, "    Version List=");
                                            versionBuilder.Append(";");
                                            versionBuilder.Replace(",;", ";");
                                            versionBuilder.Replace(",,", ",");
                                            versionBuilder.Replace("=,", "=");
                                            line = versionBuilder.ToString();
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
        /// Method which finds in which NAV objects there is the passed keyword.
        /// </summary>
        /// <param name="files">The collection of files to check</param>
        /// <param name="keyword">The search keyword</param>
        /// <param name="progress">The progress of the operation</param>
        /// <param name="token">The cancellation token</param>
        /// <returns>The NAV object where an occurrence of the keyword is found</returns>
        public static IEnumerable<NAVObject> FindWhere(Models.File[] files, string keyword, IProgress<KeyValuePair<double, string>> progress, CancellationToken token)
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

                    if (!string.IsNullOrWhiteSpace(lines) && lines.IndexOf("  OBJECT-PROPERTIES", StringComparison.Ordinal) != -1)
                    {
                        foreach (string navObjectLines in Split(lines))
                        {
                            if (navObjectLines.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) != -1) // The file contains the keyword
                            {
                                using (StringReader stringReader = new StringReader(navObjectLines))
                                {
                                    string[] startingLineWords = new StringReader(navObjectLines).ReadLine()?.Split(' '); // Get the first line of the file

                                    yield return new NAVObject(GetObjectTypeFromString(startingLineWords?[1]), startingLineWords?[2], GetObjectNameFromFirstLine(startingLineWords), file.Path);
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
        private static IEnumerable<NAVObject> GetObjects(Models.File[] files, IProgress<KeyValuePair<double, string>> progress, CancellationToken token)
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

                    if (!string.IsNullOrWhiteSpace(lines) && lines.IndexOf("  OBJECT-PROPERTIES", StringComparison.Ordinal) != -1)
                    {
                        foreach (string navObjectLines in Split(lines))
                        {
                            using (StringReader stringReader = new StringReader(navObjectLines))
                            {
                                string[] startingLineWords = new StringReader(navObjectLines).ReadLine()?.Split(' '); // Get the first line of the file

                                yield return new NAVObject(GetObjectTypeFromString(startingLineWords?[1]), startingLineWords?[2], GetObjectNameFromFirstLine(startingLineWords), file.Path);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Method which composes NAV object name given the words of the first line of a NAV object.
        /// </summary>
        /// <param name="words">The words of the first line of the object</param>
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
        /// Method which parses the NAV object type from the string.
        /// </summary>
        /// <param name="type">The type of the object</param>
        /// <returns>The Enum type of the object</returns>
        private static NAVObjectType GetObjectTypeFromString(string type)
        {
            return (NAVObjectType)Enum.Parse(typeof(NAVObjectType), type, true);
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
        private static string SplitAndStore(string filePath, string destinationFolder, IProgress<KeyValuePair<bool, string>> progress, CancellationToken token)
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

            progress.Report(new KeyValuePair<bool, string>(true, "Splitting file..."));

            if (!string.IsNullOrWhiteSpace(lines) && lines.IndexOf("  OBJECT-PROPERTIES", StringComparison.Ordinal) != -1)
            {
                Parallel.ForEach(Split(lines), navObjectLines =>
                {
                    if (token.IsCancellationRequested)
                    {
                        token.ThrowIfCancellationRequested();
                    }

                    string firstLine = navObjectLines.Substring(0, navObjectLines.IndexOf(Environment.NewLine, StringComparison.Ordinal));
                    string fileName = string.Join("_", firstLine.Trim().Split(InvalidFileNameChars)).Substring(7);

                    string destFolder = Path.Combine(extractionFolderPath, firstLine.Split(' ')[1]);
                    if (!Directory.Exists(destFolder))
                    {
                        Directory.CreateDirectory(destFolder);
                    }

                    using (FileStream stream = new FileStream(Path.Combine(destFolder, fileName + ".txt"), FileMode.Create, FileAccess.Write))
                    using (StreamWriter writer = new StreamWriter(stream, Encoding.GetEncoding(Properties.Settings.Default.DefaultEncoding)))
                    {
                        writer.Write(navObjectLines);
                    }
                });
            }

            return extractionFolderPath;
        }

        /// <summary>
        /// Method which return an array containing splitted NAV objects.
        /// </summary>
        /// <param name="lines">The string containing multiple objects</param>
        /// <returns>The array containing splitted objects</returns>
        private static IEnumerable<string> Split(string lines)
        {
            string[] splittedParts = Regex.Split(lines, ObjectSplitterPattern, RegexOptions.Compiled).Skip(1).ToArray();

            for (int i = 0; i < splittedParts.Length; i++)
            {
                yield return (splittedParts[i++] + splittedParts[i]);
            }
        }

        /// <summary>
        /// Method which searches for NAV object IDs in passed string in order to compose
        /// object type filters.
        /// </summary>
        /// <param name="lines">The string containing object IDs</param>
        /// <returns>The filters</returns>
        public static Filters GetFiltersFromString(string lines)
        {
            List<NAVObject> navObjects = new List<NAVObject>();

            using (StringReader reader = new StringReader(lines))
            {
                if (reader.ReadLine().StartsWith("Type"))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        string[] objectDetails = line.Split('\t');

                        navObjects.Add(new NAVObject(GetObjectTypeFromString(objectDetails[0]), objectDetails[1]));
                    }
                }
                else
                {
                    throw new ArgumentException("The clipboard doesn't contain any NAV object data.");
                }
            }

            return GetFiltersFromNAVObjects(navObjects);
        }

        /// <summary>
        /// Method which composes object type filters from passed NAV object files.
        /// </summary>
        /// <param name="files">The collection of files to read</param>
        /// <param name="progress">The progress of the operation</param>
        /// <param name="token">The cancellation token</param>
        /// <returns>The filters</returns>
        public static Filters GetFiltersFromFiles(Models.File[] files, IProgress<KeyValuePair<double, string>> progress, CancellationToken token)
        {
            List<NAVObject> navObjects = new List<NAVObject>();

            foreach (NAVObject navObject in GetObjects(files, progress, token))
            {
                navObjects.Add(navObject);
            }

            return GetFiltersFromNAVObjects(navObjects);
        }

        /// <summary>
        /// Method which composes object type filters from passed NAV object files.
        /// </summary>
        /// <param name="navObjects">The collection of NAV objects</param>
        /// <returns>The filters</returns>
        public static Filters GetFiltersFromNAVObjects(List<NAVObject> navObjects)
        {
            Filters filters = new Filters
            {
                Table = string.Join("|", navObjects.Where(navObject => navObject.Type.Equals(NAVObjectType.Table)).Select(navObject => navObject.ID).ToArray()),
                Form = string.Join("|", navObjects.Where(navObject => navObject.Type.Equals(NAVObjectType.Form)).Select(navObject => navObject.ID).ToArray()),
                Report = string.Join("|", navObjects.Where(navObject => navObject.Type.Equals(NAVObjectType.Report)).Select(navObject => navObject.ID).ToArray()),
                Dataport = string.Join("|", navObjects.Where(navObject => navObject.Type.Equals(NAVObjectType.Dataport)).Select(navObject => navObject.ID).ToArray()),
                Codeunit = string.Join("|", navObjects.Where(navObject => navObject.Type.Equals(NAVObjectType.Codeunit)).Select(navObject => navObject.ID).ToArray()),
                XMLport = string.Join("|", navObjects.Where(navObject => navObject.Type.Equals(NAVObjectType.XMLport)).Select(navObject => navObject.ID).ToArray()),
                MenuSuite = string.Join("|", navObjects.Where(navObject => navObject.Type.Equals(NAVObjectType.MenuSuite)).Select(navObject => navObject.ID).ToArray()),
                Query = string.Join("|", navObjects.Where(navObject => navObject.Type.Equals(NAVObjectType.Query)).Select(navObject => navObject.ID).ToArray()),
                Page = string.Join("|", navObjects.Where(navObject => navObject.Type.Equals(NAVObjectType.Page)).Select(navObject => navObject.ID).ToArray())
            };

            return filters;
        }

        /// <summary>
        /// Method which checks if the version list value is valid.
        /// </summary>
        /// <param name="versionList">The version list</param>
        /// <param name="maxLength">The maximum length of the version list</param>
        /// <returns>The validity of the version list</returns>
        public static bool IsVersionListValid(string versionList, int maxLength)
        {
            return !string.IsNullOrWhiteSpace(versionList) && versionList.Length <= maxLength;
        }
    }
}