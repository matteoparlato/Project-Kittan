using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows;

namespace Project_Kittan
{
    class Program
    {
        private static Dictionary<string, Assembly> loadedLibs = new Dictionary<string, Assembly>();

        [STAThread]
        public static void Main(string[] args)
        {
            AppDomain.CurrentDomain.AssemblyResolve += Resolve;

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
                                RegistryKey registryKey = Registry.ClassesRoot.OpenSubKey("txtfile", true).CreateSubKey("shell", true).CreateSubKey("Split with Project Kittan", true).CreateSubKey("command");
                                registryKey.SetValue("", Assembly.GetEntryAssembly().Location + " %1");
                                registryKey.Close();
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
                                Registry.ClassesRoot.OpenSubKey("txtfile", true).CreateSubKey("shell", true).DeleteSubKeyTree("Split with Project Kittan");
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
                        break;
                    }
            }
        }

        private static Assembly Resolve(object sender, ResolveEventArgs e)
        {
            string libName = new AssemblyName(e.Name).Name;

            if (loadedLibs.ContainsKey(libName)) return loadedLibs[libName];

            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Project_Kittan." + libName + ".dll"))
            {
                try
                {
                    byte[] buffer = new BinaryReader(stream).ReadBytes((int)stream.Length);
                    Assembly assembly = Assembly.Load(buffer);
                    loadedLibs[libName] = assembly;
                    return assembly;
                }
                catch (Exception) { }
            }
            return null;

            // Blocked by Windows Defender for possible Trojan:Win32/Cloxer.D!cl

            ////var thisAssembly = Assembly.GetExecutingAssembly();

            ////var assemblyName = new AssemblyName(e.Name);
            ////var dllName = assemblyName.Name + ".dll";

            ////var resources = thisAssembly.GetManifestResourceNames().Where(s => s.EndsWith(dllName));
            ////if (resources.Any())
            ////{
            ////    var resourceName = resources.First();
            ////    using (var stream = thisAssembly.GetManifestResourceStream(resourceName))
            ////    {
            ////        if (stream == null) return null;
            ////        var block = new byte[stream.Length];
            ////        try
            ////        {
            ////            stream.Read(block, 0, block.Length);
            ////            return Assembly.Load(block);
            ////        }
            ////        catch (IOException)
            ////        {
            ////            return null;
            ////        }
            ////        catch (BadImageFormatException)
            ////        {
            ////            return null;
            ////        }
            ////    }
            ////}
            ////return null;
        }
    }
}
