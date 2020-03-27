using Project_Kittan.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Media;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Shell;

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
            set => SetProperty(ref _progressText, value);
        }

        private TaskbarItemProgressState _taskbarItemProgressState = TaskbarItemProgressState.Indeterminate;
        public TaskbarItemProgressState TaskbarItemProgressState
        {
            get => _taskbarItemProgressState;
            set => SetProperty(ref _taskbarItemProgressState, value);
        }

        private bool _progressStatus;
        public bool ProgressStatus
        {
            get => _progressStatus;
            set => SetProperty(ref _progressStatus, value);
        }

        private bool _closable;
        public bool Closable
        {
            get => !_closable;
            set => SetProperty(ref _closable, value);
        }

        /// <summary>
        /// Constructor which initializes a SplitDialog Window with passed information.
        /// </summary>
        /// <param name="filePath">The path of the file to split</param>
		public SplitDialog(string filePath)
		{
			InitializeComponent();

            _filePath = filePath;

			WindowStartupLocation = WindowStartupLocation.CenterScreen;

            DataContext = this;
		}

        /// <summary>
        /// Method invoked the window is laid out, rendered, and ready for interaction.
        /// Starts the file splitting.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            UsedEncodingTextBlock.Text = "Encoding " + Encoding.GetEncoding(Properties.Settings.Default.DefaultEncoding).BodyName + " used";
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

                TaskbarItemProgressState = TaskbarItemProgressState.None;

                SystemSounds.Asterisk.Play();

                _closable = true;
            });
        }

        /// <summary>
        /// Method invoked when closing the window.
        /// If _closable = true the window closes, otherwise not.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
            //if ((_token != null) && (_runningTask != null) && (_tokenSource != null))
            //{
            //    if ((_token.IsCancellationRequested == false) && (_runningTask.Status != TaskStatus.Canceled))
            //    {
            //        _tokenSource.Cancel();
            //    }
            //}

            e.Cancel = !_closable;
		}

        /// <summary>
        /// Method invoked when the user clicks on Open Folder button.
        /// Opens the extraction folder in Explorer.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenFolderButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(_filePath);
        }

        /// <summary>
        /// Method invoked when the user clicks on Close button.
        /// Closes the current window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Method invoked when the user presses a keyboard key.
        /// If the user presses Enter close the window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                CloseButton_Click(null, null);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] String propertyName = null)
        {
            if (Equals(storage, value)) return false;
            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
