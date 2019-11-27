using Microsoft.Win32;
using System;
using System.IO;
using System.Reflection;
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

                        if (args[0].Equals("/enableSplitFileShellIntegration"))
                        {
                            try
                            {
                                RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE", true).OpenSubKey("Classes", true).CreateSubKey("txtfile", true).CreateSubKey("shell", true).CreateSubKey("Split with Project Kittan", true).CreateSubKey("command");
                                registryKey.SetValue("", Assembly.GetEntryAssembly().Location + " %1");
                            }
                            catch (Exception)
                            {
                                //
                            }
                        }

                        if (args[0].Equals("/disableSplitFileShellIntegration"))
                        {
                            try
                            {
                                Registry.CurrentUser.OpenSubKey("SOFTWARE", true).OpenSubKey("Classes", true).CreateSubKey("txtfile", true).CreateSubKey("shell", true).DeleteSubKeyTree("Split with Project Kittan");
                            }
                            catch (Exception)
                            {
                                //
                            }
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
