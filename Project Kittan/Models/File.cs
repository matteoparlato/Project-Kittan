using System.IO;

namespace Project_Kittan.Models
{
    /// <summary>
    /// File class
    /// </summary>
    internal class File
    {
        public string FileName { get; private set; } = string.Empty;

        public string _filePath;
        public string FilePath
        {
            get { return _filePath; }
            set
            {
                _filePath = value;
                FileName = Path.GetFileName(value);
            }
        }

        /// <summary>
        /// Constructor which initializes a File object with passed information.
        /// </summary>
        /// <param name="filePath">The path of the file</param>
        public File(string filePath)
        {
            FilePath = filePath;
        }
    }
}
