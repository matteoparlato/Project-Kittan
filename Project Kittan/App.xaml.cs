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

        public static bool IsAPPX
        {
#if !APPX
            get => false;
#else
            get => true;
#endif
        }

        public static Visibility IsAPPXVisible
        {
#if !APPX
            get => Visibility.Visible;
#else
            get => Visibility.Collapsed;
#endif
        }
    }
}
