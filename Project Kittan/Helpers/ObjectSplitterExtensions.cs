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
    /// ObjectSplitterExtensions class
    /// </summary>
    public static class ObjectSplitterExtensions
    {
        internal const string ObjectSplitterPattern = @"(OBJECT \w* \d* .*
{)";

        internal static readonly char[] InvalidFileNameChars = Path.GetInvalidFileNameChars();

        /// <summary>
        /// Method which splits and stores a text file containing multiple objects into single object text files.
        /// </summary>
        /// <param name="filePath">The path of the file to split</param>
        /// <param name="encoding">The encoding to use when reading/writing files</param>
        /// <returns></returns>
        public async static Task<KeyValuePair<int, string>> SplitAndStore(string filePath, int encoding)
        {
            return await SplitAndStore(filePath, Path.GetDirectoryName(filePath), encoding);
        }

        /// <summary>
        /// Method which splits and stores a text file containing multiple objects into single object text files.
        /// </summary>
        /// <param name="filePath">The path of the file to split</param>
        /// <param name="destinationFolder">The destination where to store splitted files</param>
        /// <param name="encoding">The encoding to use when reading/writing files</param>
        public async static Task<KeyValuePair<int, string>> SplitAndStore(string filePath, string destinationFolder, int encoding)
        {
            string folderName = Path.GetFileNameWithoutExtension(filePath) + "_" + string.Format("{0:yyyyMMdd}", DateTime.Now);

            string extractionFolderPath = Path.Combine(Path.Combine(destinationFolder, folderName));
            if (!Directory.Exists(extractionFolderPath))
            {
                Directory.CreateDirectory(extractionFolderPath);
            }

            using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            using (BufferedStream buffer = new BufferedStream(stream))
            using (StreamReader reader = new StreamReader(buffer, Encoding.GetEncoding(encoding)))
            {
                Parallel.ForEach(Split(await reader.ReadToEndAsync()), codeLines =>
                {
                    WriteFile(codeLines, extractionFolderPath, encoding);
                });
            }

            // TODO: Implement count -> replace 0
            return new KeyValuePair<int, string>(0, extractionFolderPath);
        }

        /// <summary>
        /// Method which return an array containing splitted objects.
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

        /// <summary>
        /// Method which stores splitted objects in the specified folder.
        /// </summary>
        /// <param name="objectLines">The string containing object code</param>
        /// <param name="folderPath">The path where to save splitted files</param>
        private static void WriteFile(string objectLines, string folderPath, int encoding)
        {
            string firstLine = objectLines.Substring(0, objectLines.IndexOf(Environment.NewLine));
            string fileName = string.Join("_", firstLine.Trim().Split(InvalidFileNameChars)).Substring(7);

            folderPath = Path.Combine(folderPath, firstLine.Split(' ')[1]);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            using (FileStream stream = new FileStream(Path.Combine(folderPath, fileName + ".txt"), FileMode.Create, FileAccess.Write))
            using (StreamWriter writer = new StreamWriter(stream, Encoding.GetEncoding(encoding)))
            {
                writer.Write(objectLines);
            }
        }
    }
}
