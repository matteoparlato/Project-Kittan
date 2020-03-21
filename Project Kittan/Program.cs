using System;
using System.IO;
using System.Windows;

namespace Project_Kittan
{
    class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            switch (args.Length)
            {
                case 0:
                    {
                        App.Main();
                        break;
                    }
                case 1:
                    {
                        if (Path.GetFileName(args[0]).EndsWith(".txt"))
                        {
                            new SplitDialog(args[0]).ShowDialog();
                        }

                        break;
                    }
                default:
                    {
                        MessageBox.Show("Project Kittan accepts only one parameter:\n[filePath] : the path of the file to split");
                        App.Main();
                        break;
                    }
            }
        }
    }
}
