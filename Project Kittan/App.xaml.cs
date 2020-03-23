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
            get { return Current.MainWindow; }
        }
    }
}
