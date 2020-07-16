using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Project_Kittan.Helpers
{
    public static class FileExtensions
    {
        public static List<string> GetFilesFromDirectory(string path)
        {
            return Directory.GetFiles(path, "*.txt", SearchOption.AllDirectories).Where(item => !(item.Contains(@"\.hg") || item.Contains(@"\.git"))).ToList();
        }
    }
}
