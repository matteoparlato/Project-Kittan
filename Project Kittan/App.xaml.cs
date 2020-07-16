using Microsoft.AppCenter;
using Microsoft.AppCenter.Crashes;
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

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            AppCenter.Start("ef0a6c74-973c-4a77-93ab-4fc7d7d2ee95", typeof(Crashes));
        }
    }
}
