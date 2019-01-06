using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace Project_Kittan.Helpers
{
    internal static class FilesExtensions
    {
        /// <summary>
        /// Method which finds all *.txt files in specified folder and subfolders.
        /// </summary>
        /// <param name="Path">The path of the directory</param>
        /// <returns>The list containing all the paths of the text files found in working directory</returns>
        internal static HashSet<Models.File> FindFiles(string Path)
        {
            List<Models.File> files = new List<Models.File>();
            try
            {
                foreach (string f in Directory.GetFiles(Path).Where(i => i.EndsWith(".txt")))
                {
                    files.Add(new Models.File(f));
                }
                foreach (string d in Directory.GetDirectories(Path).Where(i => !i.EndsWith(".hg") && !i.EndsWith(".git")))
                {
                    files.AddRange(FindFiles(d));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Project Kittan", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return new HashSet<Models.File>(files);
        }
    }
}
