using Project_Kittan.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Media;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Project_Kittan.Views
{
    /// <summary>
    /// SplitDialog class
    /// </summary>
    public partial class SplitDialog : Window, INotifyPropertyChanged
	{
        private string _filePath;

        private Task _runningTask;

        private CancellationToken _token;

        private CancellationTokenSource _tokenSource;

        private string _progressText;
        public string ProgressText
        {
            get => _progressText;
            set => Set(ref _progressText, value);
        }

        private bool _progressStatus;
        public bool ProgressStatus
        {
            get => _progressStatus;
            set => Set(ref _progressStatus, value);
        }

        private bool _canClose;
        public bool CanClose
        {
            get => !_canClose;
            set => Set(ref _canClose, value);
        }

        /// <summary>
        /// Constructor which initializes a SplitDialog Window with passed information.
        /// </summary>
        /// <param name="filePath">The path of the file to split</param>
		public SplitDialog(string filePath)
		{
			InitializeComponent();
            DataContext = this;

            _filePath = filePath;
		}

        /// <summary>
        /// Method invoked the window is laid out, rendered, and ready for interaction.
        /// Starts file splitting.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            IProgress<KeyValuePair<bool, string>> progress = new Progress<KeyValuePair<bool, string>>(status =>
            {
                ProgressStatus = status.Key;
                ProgressText = status.Value;
            });

            _tokenSource = new CancellationTokenSource();
            _token = _tokenSource.Token;

            _runningTask = Task.Run(() =>
            {
                _filePath = NAVObjectExtensions.SplitAndStore(_filePath, progress, _token);

                progress.Report(new KeyValuePair<bool, string>(false, string.Format("Files extracted in: {0}", _filePath)));
                SystemSounds.Asterisk.Play();

                CanClose = true;
            });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
#if !APPX
            Process.Start(_filePath);
#endif
            Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if ((_token != null) && (_runningTask != null) && (_tokenSource != null))
            {
                if ((_token.IsCancellationRequested == false) && (_runningTask.Status != TaskStatus.Canceled))
                {
                    _tokenSource.Cancel();
                    CanClose = true;
                }
            }

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

        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
