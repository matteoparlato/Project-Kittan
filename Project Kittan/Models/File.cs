using Project_Kittan.Helpers;
using System.IO;

namespace Project_Kittan.Models
{
    /// <summary>
    /// File class
    /// </summary>
    public class File : Observable
    {
        private string _name;
        public string Name
        {
            get => _name;
            set => Set(ref _name, value);
        }

        private string _path;
        public string Path
        {
            get => _path;
            set => Set(ref _path, value);
        }

        public bool IsReadOnly
        {
            get => new FileInfo(Path).IsReadOnly;
        }

        /// <summary>
        /// Constructor which initializes a File object with passed information.
        /// </summary>
        /// <param name="path">The path of the file</param>
        public File(string path)
        {
            Name = System.IO.Path.GetFileNameWithoutExtension(path);
            Path = path;
        }
    }
}
