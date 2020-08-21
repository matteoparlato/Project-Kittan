using System.Windows;

namespace Project_Kittan.Views
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

        private void CloseCommandBinding_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            Close();
        }
    }
}
