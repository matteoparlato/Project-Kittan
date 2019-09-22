using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Project_Kittan.Helpers
{
    internal static class FolderExtensions
    {
        /// <summary>
        /// Method which finds all *.txt files in specified folder and subfolders.
        /// </summary>
        /// <param name="Path">The path of the directory</param>
        /// <returns>The list containing all the paths of the text files found in working directory</returns>
        internal static List<Models.File> GetFiles(string Path)
        {
            List<Models.File> Files = new List<Models.File>();

            Parallel.ForEach(Directory.GetFiles(Path, "*.txt", SearchOption.AllDirectories), file =>
            {
                Files.Add(new Models.File(file));
            });

            return Files;
        }
    }
}
