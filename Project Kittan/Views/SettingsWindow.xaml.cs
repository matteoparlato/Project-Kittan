using System.Diagnostics;
using System.Windows;

namespace Project_Kittan
{
    /// <summary>
    /// SettingsWindow class
    /// </summary>
    public partial class SettingsWindow : Window
    {
        /// <summary>
        /// Parameterless constructor of SettingsWindow class.
        /// </summary>
        public SettingsWindow()
        {
            InitializeComponent();

            if (Properties.Settings.Default.DefaultEncoding == 0)
            {
                AutomaticEncodingRadioButton.IsChecked = AutomaticEncodingOptionsStackPanel.IsEnabled = true;
                CustomEncodingRadioButton.IsChecked = CustomEncodingOptionsStackPanel.IsEnabled = false;
            }
            else
            {
                AutomaticEncodingRadioButton.IsChecked = AutomaticEncodingOptionsStackPanel.IsEnabled = false;
                CustomEncodingRadioButton.IsChecked = CustomEncodingOptionsStackPanel.IsEnabled = true;
            }

            if (string.IsNullOrWhiteSpace(Properties.Settings.Default.DefaultLocale))
            {
                SystemLocaleRadioButton.IsChecked = SystemLocaleOptionsStackPanel.IsEnabled = true;
                CustomLocaleRadioButton.IsChecked = CustomLocaleOptionsStackPanel.IsEnabled = false;
            }
            else
            {
                SystemLocaleRadioButton.IsChecked = SystemLocaleOptionsStackPanel.IsEnabled = false;
                CustomLocaleRadioButton.IsChecked = CustomLocaleOptionsStackPanel.IsEnabled = true;
            }
        }

        private void AutomaticEncodingRadioButton_Click(object sender, RoutedEventArgs e)
        {
            AutomaticEncodingRadioButton.IsChecked = AutomaticEncodingOptionsStackPanel.IsEnabled = true;
            CustomEncodingRadioButton.IsChecked = CustomEncodingOptionsStackPanel.IsEnabled = false;
        }

        private void CustomEncodingRadioButton_Click(object sender, RoutedEventArgs e)
        {
            AutomaticEncodingRadioButton.IsChecked = AutomaticEncodingOptionsStackPanel.IsEnabled = false;
            CustomEncodingRadioButton.IsChecked = CustomEncodingOptionsStackPanel.IsEnabled = true;
        }

        private void SystemLocaleRadioButton_Click(object sender, RoutedEventArgs e)
        {
            SystemLocaleRadioButton.IsChecked = SystemLocaleOptionsStackPanel.IsEnabled = true;
            CustomLocaleRadioButton.IsChecked = CustomLocaleOptionsStackPanel.IsEnabled = false;
        }

        private void RadioButton_Click(object sender, RoutedEventArgs e)
        {
            SystemLocaleRadioButton.IsChecked = SystemLocaleOptionsStackPanel.IsEnabled = false;
            CustomLocaleRadioButton.IsChecked = CustomLocaleOptionsStackPanel.IsEnabled = true;
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}
