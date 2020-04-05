using Project_Kittan.Helpers;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace Project_Kittan.Views
{
    /// <summary>
    /// RequestDialog class
    /// </summary>
    public partial class RequestDialog : Window, INotifyPropertyChanged
    {
        private string _versionList;
        public string VersionList
        {
            get => _versionList;
            set => Set(ref _versionList, value);
        }

        public int MaxLength { get; set; }

        private int _availableChars;
        public int AvailableChars
        {
            get => _availableChars;
            set => Set(ref _availableChars, value);
        }

        private bool _canSave;
        public bool CanSave
        {
            get => _canSave;
            set => Set(ref _canSave, value);
        }

        private bool CanClose;

        public RequestDialog(string versionList, int maxLength)
        {
            InitializeComponent();
            DataContext = this;

            VersionList = versionList;
            MaxLength = maxLength;
            AvailableChars = maxLength - versionList.Length;
        }

        private void TextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            AvailableChars = MaxLength - VersionList.Length;
            CanSave = NAVObjectExtensions.IsVersionListValid(VersionList, MaxLength);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            CanClose = true;
            Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            CanClose = true;
            Close();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = !CanClose;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void Set<T>(ref T storage, T value, [CallerMemberName]string propertyName = null)
        {
            if (Equals(storage, value))
            {
                return;
            }

            storage = value;
            OnPropertyChanged(propertyName);
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
