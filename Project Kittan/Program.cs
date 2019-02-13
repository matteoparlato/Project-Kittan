using Project_Kittan.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Project_Kittan
{
    class Program
    {
        [DllImport("kernel32.dll")]
        static extern bool FreeConsole();

        private static Dictionary<string, Assembly> loadedLibs = new Dictionary<string, Assembly>();

        [STAThread]
        public static void Main(string[] args)
        {
            AppDomain.CurrentDomain.AssemblyResolve += Resolve;

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Project Kittan");
            Console.ResetColor();
            Console.WriteLine(" - version " + Assembly.GetExecutingAssembly().GetName().Version.ToString() + "\n");

            switch (args.Length)
            {
                case 0:
                    {
                        FreeConsole();
                        App.Main();
                        break;
                    }
                case 1:
                    {
                        foreach (string arg in args)
                        {
                            ObjectExtensions.SplitFile(arg);
                        }

                        Console.Write("\nPress any key to continue");
                        while (Console.KeyAvailable)
                            Console.ReadKey(true);
                        Console.ReadKey();
                        break;
                    }
                default:
                    {
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.BackgroundColor = ConsoleColor.Yellow;
                        Console.Write("Attention");
                        Console.ResetColor();
                        Console.WriteLine("\nWrong number of parameters.");
                        Console.Write("\nPress any key to continue");
                        while (Console.KeyAvailable)
                            Console.ReadKey(true);
                        Console.ReadKey();
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
