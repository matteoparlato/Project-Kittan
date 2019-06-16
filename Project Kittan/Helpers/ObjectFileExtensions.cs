using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Project_Kittan.Helpers
{
    /// <summary>
    /// ObjectFileExtensions Class
    /// </summary>
    internal static class ObjectFileExtensions
    {
        private static readonly string ObjectSplitterPattern = @"(OBJECT \w* \d* .*
{)";

        private static readonly char[] InvalidFileNameChars = Path.GetInvalidFileNameChars();

        /// <summary>
        /// Method which splits and stores a text file containing multiple objects into single object text files.
        /// </summary>
        /// <param name="file">The file to split</param>
        internal async static Task<KeyValuePair<int, string>> SplitAndStoreObjects(Models.File file)
        {
            return await SplitAndStoreObjects(file.FilePath);
        }

        /// <summary>
        /// Method which splits and stores a text file containing multiple objects into single object text files.
        /// </summary>
        /// <param name="filePath">The path of the file to split</param>
        internal async static Task<KeyValuePair<int, string>> SplitAndStoreObjects(string filePath)
        {
            string extractionFolderPath = string.Empty;
            string[] objectCode = { };

            string folderName = InvalidFileNameChars.Aggregate("SPLIT_" + DateTime.Now, (current, c) => current.Replace(c.ToString(), string.Empty));

            extractionFolderPath = Path.GetDirectoryName(filePath) + @"\" + folderName;
            if (!Directory.Exists(extractionFolderPath))
            {
                Directory.CreateDirectory(extractionFolderPath);
            }

            using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            using (StreamReader reader = new StreamReader(stream, Encoding.GetEncoding(Properties.Settings.Default.RWEncoding)))
            {
                objectCode = SplitObjects(await reader.ReadToEndAsync());

                Parallel.ForEach(objectCode, codeLines =>
                {
                    WriteSplittedFile(codeLines, extractionFolderPath);
                });
            }

            return new KeyValuePair<int, string>(objectCode.Length, extractionFolderPath);
        }

        /// <summary>
        /// Method which return an array containing splitted objects.
        /// </summary>
        /// <param name="lines">The string containing multiple objects</param>
        /// <returns></returns>
        internal static string[] SplitObjects(string lines)
        {
            string[] splittedParts = Regex.Split(lines, ObjectSplitterPattern).Skip(1).ToArray();

            List<string> singleObjects = new List<string>();
            for (int i = 0; i < splittedParts.Length; i++)
            {
                singleObjects.Add(splittedParts[i++] + splittedParts[i]);
            }

            return singleObjects.ToArray();
        }

        /// <summary>
        /// Method which stores the splitted object in the specified folder.
        /// </summary>
        /// <param name="objectLines">The string containing object code</param>
        /// <param name="folderPath">The path where to save the splitted file</param>
        private static void WriteSplittedFile(string objectLines, string folderPath)
        {
            string[] startingLine = objectLines.Substring(0, objectLines.IndexOf(Environment.NewLine)).Split(' ');

            string fileName = string.Empty;
            for (int i = 1; i < startingLine.Length; i++)
            {
                fileName += " " + startingLine[i];
            }

            fileName = InvalidFileNameChars.Aggregate(fileName.Trim(), (current, c) => current.Replace(c.ToString(), string.Empty));

            switch (startingLine[1])
            {
                case "Table":
                    {
                        if (!Directory.Exists(folderPath + @"\Table"))
                        {
                            Directory.CreateDirectory(folderPath + @"\Table");
                        }
                        folderPath = folderPath + @"\Table\";
                        break;
                    }
                case "Form":
                    {
                        if (!Directory.Exists(folderPath + @"\Form"))
                        {
                            Directory.CreateDirectory(folderPath + @"\Form");
                        }
                        folderPath = folderPath + @"\Form\";
                        break;
                    }
                case "Page":
                    {
                        if (!Directory.Exists(folderPath + @"\Page"))
                        {
                            Directory.CreateDirectory(folderPath + @"\Page");
                        }
                        folderPath = folderPath + @"\Page\";
                        break;
                    }
                case "Report":
                    {
                        if (!Directory.Exists(folderPath + @"\Report"))
                        {
                            Directory.CreateDirectory(folderPath + @"\Report");
                        }
                        folderPath = folderPath + @"\Report\";
                        break;
                    }
                case "Dataport":
                    {
                        if (!Directory.Exists(folderPath + @"\Dataport"))
                        {
                            Directory.CreateDirectory(folderPath + @"\Dataport");
                        }
                        folderPath = folderPath + @"\Dataport\";
                        break;
                    }
                case "XMLport":
                    {
                        if (!Directory.Exists(folderPath + @"\XMLPort"))
                        {
                            Directory.CreateDirectory(folderPath + @"\XMLPort");
                        }
                        folderPath = folderPath + @"\XMLPort\";
                        break;
                    }
                case "Codeunit":
                    {
                        if (!Directory.Exists(folderPath + @"\Codeunit"))
                        {
                            Directory.CreateDirectory(folderPath + @"\Codeunit");
                        }
                        folderPath = folderPath + @"\Codeunit\";
                        break;
                    }
                case "MenuSuite":
                    {
                        if (!Directory.Exists(folderPath + @"\Menusuite"))
                        {
                            Directory.CreateDirectory(folderPath + @"\Menusuite");
                        }
                        folderPath = folderPath + @"\Menusuite\";
                        break;
                    }
                case "Query":
                    {
                        if (!Directory.Exists(folderPath + @"\Query"))
                        {
                            Directory.CreateDirectory(folderPath + @"\Query");
                        }
                        folderPath = folderPath + @"\Query\";
                        break;
                    }
                default:
                    {
                        break;
                    }
            }

            using (FileStream stream = new FileStream(folderPath + fileName + ".txt", FileMode.Create, FileAccess.Write))
            using (StreamWriter writer = new StreamWriter(stream, Encoding.GetEncoding(Properties.Settings.Default.RWEncoding)))
            {
                writer.Write(objectLines);
            }
        }
    }
}
