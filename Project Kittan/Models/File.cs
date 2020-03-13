using System;

namespace Project_Kittan.Models
{
    /// <summary>
    /// File class
    /// </summary>
    public class File : BindableBase
    {
        private string _name;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private string _path;
        public string Path
        {
            get => _path;
            set => SetProperty(ref _path, value);
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
