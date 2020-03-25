using System.Windows;

namespace Project_Kittan
{
    /// <summary>
    /// Logica di interazione per App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static Window CurrentMainWindow
        {
            get => Current.MainWindow;
        }

        public static string AppName
        {
            get => Project_Kittan.Properties.Resources.AppName;
        }
    }
}
