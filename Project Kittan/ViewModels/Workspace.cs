using Project_Kittan.Helpers;
using Project_Kittan.Models;
using Project_Kittan.Properties;
using Project_Kittan.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Project_Kittan.ViewModels
{
    /// <summary>
    /// Workspace class
    /// </summary>
    public class Workspace : Observable
    {
        private Task _runningTask;
        private CancellationToken _token;
        private CancellationTokenSource _tokenSource;

        public ObservableCollection<WorkspaceFile> WorkspaceFiles { get; set; }
        public ObservableCollection<NAVObject> SearchResult { get; set; }

        private Filters _fileFilters;
        public Filters FileFilters
        {
            get => _fileFilters;
            set => Set(ref _fileFilters, value);
        }

        private WorkspaceFile _selectedWorkspaceFile;
        public WorkspaceFile SelectedWorkspaceFile
        {
            get => _selectedWorkspaceFile;
            set => Set(ref _selectedWorkspaceFile, value);
        }

        private string _path;
        public string Path
        {
            get => _path;
            set => Set(ref _path, value);
        }

        private bool _ready;
        public bool Ready
        {
            get => _ready;
            set => Set(ref _ready, value);
        }

        private double _progressValue;
        public double ProgressValue
        {
            get => _progressValue;
            set => Set(ref _progressValue, value);
        }

        private string _progressText;
        public string ProgressText
        {
            get => _progressText;
            set => Set(ref _progressText, value);
        }

        private bool _backgroundActivity;
        public bool BackgroundActivity
        {
            get => _backgroundActivity;
            set => Set(ref _backgroundActivity, value);
        }

        private bool _cancelableBackgroundActivity;
        public bool CancelableBackgroundActivity
        {
            get => _cancelableBackgroundActivity;
            set => Set(ref _cancelableBackgroundActivity, value);
        }

        public ICommand AddFilesFromExecutableFolderCommand { get; set; }
        public ICommand AddFilesFromFolderCommand { get; set; }
        public ICommand AddTagCommand { get; set; }
        public ICommand ClearWorkspaceCommand { get; set; }
        public ICommand DropFileCommand { get; set; }
        public ICommand GetFiltersFromFilesCommand { get; set; }
        public ICommand GetFiltersFromOccurrencesCommand { get; set; }
        public ICommand GetFiltersFromStringCommand { get; set; }
        public ICommand OpenFileCommand { get; set; }
        public ICommand OpenFileLocationCommand { get; set; }
        public ICommand OpenSettingsCommand { get; set; }
        public ICommand RemoveFileCommand { get; set; }
        public ICommand RemoveTagCommand { get; set; }
        public ICommand SearchOccurrencesCommand { get; set; }
        public ICommand ThrowCancellationCommand { get; set; }

        public Workspace()
        {
            AddFilesFromExecutableFolderCommand = new RelayCommand<object>(AddFilesFromExecutableFolder_Action, Command_CanExecute);
            AddFilesFromFolderCommand = new RelayCommand<object>(AddFilesFromFolder_Action, Command_CanExecute);
            AddTagCommand = new RelayCommand<object>(AddTag_Action, Command_CanExecute);
            ClearWorkspaceCommand = new RelayCommand<object>(ClearWorkspace_Action, Command_CanExecute);
            DropFileCommand = new RelayCommand<object>(DropFile_Action, Command_CanExecute);
            GetFiltersFromFilesCommand = new RelayCommand<object>(GetFiltersFromFiles_Action, Command_CanExecute);
            GetFiltersFromStringCommand = new RelayCommand<object>(GetFiltersFromString_Action, Command_CanExecute);
            GetFiltersFromOccurrencesCommand = new RelayCommand<object>(GetFiltersFromOccurrences_Action, Command_CanExecute);
            OpenFileCommand = new RelayCommand<object>(OpenFile_Action, Command_CanExecute);
            OpenFileLocationCommand = new RelayCommand<object>(OpenFileLocation_Action, Command_CanExecute);
            OpenSettingsCommand = new RelayCommand<object>(OpenSettings_Action, Command_CanExecute);
            RemoveFileCommand = new RelayCommand<object>(RemoveFile_Action, Command_CanExecute);
            RemoveTagCommand = new RelayCommand<object>(RemoveTag_Action, Command_CanExecute);
            SearchOccurrencesCommand = new RelayCommand<object>(SearchOccurrences_Action, Command_CanExecute);
            ThrowCancellationCommand = new RelayCommand<object>(ThrowCancellation_Action);


            WorkspaceFiles = new ObservableCollection<WorkspaceFile>();
            SearchResult = new ObservableCollection<NAVObject>();
            FileFilters = new Filters();

            WorkspaceFiles.CollectionChanged += Files_CollectionChanged;
        }

        public bool Command_CanExecute(object obj)
        {
            if (_runningTask == null)
            {
                return true;
            }

            return _runningTask.Status != TaskStatus.Running;
        }

        private void AddFilesFromExecutableFolder_Action(object obj)
        {
            string executablePath = Process.GetCurrentProcess().MainModule?.FileName;
            executablePath = executablePath?.Substring(0, executablePath.Length - System.IO.Path.GetFileName(executablePath).Length);

            OpenFolder(executablePath);
        }

        private void AddFilesFromFolder_Action(object obj)
        {
            using (System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.SelectedPath))
                {
                    OpenFolder(dialog.SelectedPath);
                }
            }
        }

        private void AddTag_Action(object obj)
        {
            object[] parameters = (object[])obj;
            string tag = (string)parameters[1];

            if (FileExtensions.WorkspaceHasReadOnlyFiles(WorkspaceFiles))
            {
                MessageBox.Show("The workspace contains read-only files. Edit files properties and retry.",
                                Properties.Resources.AppName,
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                return;
            }

            IProgress<KeyValuePair<double, string>> progress = new Progress<KeyValuePair<double, string>>(status =>
            {
                ProgressValue = status.Key;
                ProgressText = status.Value;
            });

            SearchResult.Clear();
            _runningTask = Task.Run(() =>
            {
                BackgroundActivity = true;

                NAVObjectExtensions.AddTag(WorkspaceFiles.ToArray(), (int)parameters[0], (string)parameters[1], (bool)parameters[2], (bool)parameters[3], progress);

                progress.Report(new KeyValuePair<double, string>(0, string.Format("Tag {0} added to the version list of {1} files", (string)parameters[1], WorkspaceFiles.Count)));
                BackgroundActivity = false;
            });
        }

        private void OpenFolder(string path)
        {
            WorkspaceFiles.Clear();
            foreach (string filePath in FileExtensions.GetFilesFromDirectory(path))
            {
                WorkspaceFiles.Add(new WorkspaceFile(filePath));
            }

            if (WorkspaceFiles.Count == 0)
            {
                if (System.Windows.MessageBox.Show("No file found. Do you want to select another folder?", Resources.AppName, MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                {
                    return;
                }

                AddFilesFromFolder_Action(null);
            }

            Path = path;
        }

        private void ClearWorkspace_Action(object obj)
        {
            WorkspaceFiles.Clear();
        }

        public void DropFile_Action(object obj)
        {
            DragEventArgs args = (DragEventArgs)obj;
            if (args.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] paths = (string[])args.Data.GetData(DataFormats.FileDrop);
                foreach (string path in paths)
                {
                    FileAttributes fileAttributes = System.IO.File.GetAttributes(path);
                    if (fileAttributes.HasFlag(FileAttributes.Directory))
                    {
                        foreach (string file in FileExtensions.GetFilesFromDirectory(path))
                        {
                            WorkspaceFiles.Add(new WorkspaceFile(file));
                        }
                    }
                    else
                    {
                        WorkspaceFiles.Add(new WorkspaceFile(path));
                    }
                }
            }
        }

        private void Files_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (WorkspaceFiles.Count > 0)
            {
                Ready = true;
            }
            else
            {
                Ready = false;
                Path = "";
            }
        }

        private void GetFiltersFromFiles_Action(object obj)
        {
            IProgress<KeyValuePair<double, string>> progress = new Progress<KeyValuePair<double, string>>(status =>
            {
                ProgressValue = status.Key;
                ProgressText = status.Value;
            });

            _tokenSource = new CancellationTokenSource();
            _token = _tokenSource.Token;

            _runningTask = Task.Run(() =>
            {
                BackgroundActivity = CancelableBackgroundActivity = true;

                FileFilters = NAVObjectExtensions.GetFiltersFromFiles(WorkspaceFiles.ToArray(), progress, _token);

                progress.Report(new KeyValuePair<double, string>(100, string.Format("Successfully obtained filters from loaded files", SearchResult.Count, WorkspaceFiles.Count)));
                BackgroundActivity = CancelableBackgroundActivity = false;
            }, _token);
        }

        private void GetFiltersFromOccurrences_Action(object obj)
        {
            if (SearchResult.Count != 0)
            {
                try
                {
                    FileFilters = NAVObjectExtensions.GetFiltersFromNAVObjects(SearchResult.ToList());
                    ProgressText = "Successfully obtained filters from concurrences";
                }
                catch (ArgumentException ex)
                {
                    MessageBox.Show(ex.Message, Resources.AppName, MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void GetFiltersFromString_Action(object obj)
        {
            try
            {
                FileFilters = NAVObjectExtensions.GetFiltersFromString(Clipboard.GetText());
                ProgressText = "Successfully obtained filters from clipboard";
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message, Resources.AppName, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void OpenFile_Action(object obj)
        {
#if !APPX
            Process.Start(SelectedWorkspaceFile.Path);
#endif
        }

        private void OpenFileLocation_Action(object obj)
        {
#if !APPX
            Process.Start("explorer.exe", "/select, " + SelectedWorkspaceFile.Path);
#endif
        }

        private void OpenSettings_Action(object obj)
        {
            new SettingsWindow().ShowDialog();
        }

        private void RemoveFile_Action(object obj)
        {
            WorkspaceFiles.Remove(SelectedWorkspaceFile);
        }

        private void RemoveTag_Action(object obj)
        {
            object[] parameters = (object[])obj;
            string tag = (string)parameters[1];

            if (FileExtensions.WorkspaceHasReadOnlyFiles(WorkspaceFiles))
            {
                MessageBox.Show("The workspace contains read-only files. Edit files properties and retry.",
                                Properties.Resources.AppName,
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                return;
            }

            if (!string.IsNullOrWhiteSpace(tag))
            {
                IProgress<KeyValuePair<double, string>> progress = new Progress<KeyValuePair<double, string>>(status =>
                {
                    ProgressValue = status.Key;
                    ProgressText = status.Value;
                });

                SearchResult.Clear();
                _runningTask = Task.Run(() =>
                {
                    BackgroundActivity = true;

                    NAVObjectExtensions.RemoveTag(WorkspaceFiles.ToArray(), tag, (bool)parameters[0], progress);

                    progress.Report(new KeyValuePair<double, string>(100, string.Format("Tag {0} removed from the version list of {1} files", (string)parameters[1], WorkspaceFiles.Count)));
                    BackgroundActivity = false;
                });
            }
        }

        private void SearchOccurrences_Action(object obj)
        {
            string keyword = obj as string;

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                IProgress<KeyValuePair<double, string>> progress = new Progress<KeyValuePair<double, string>>(status =>
                {
                    ProgressValue = status.Key;
                    ProgressText = status.Value;
                });

                _tokenSource = new CancellationTokenSource();
                _token = _tokenSource.Token;

                SearchResult.Clear();
                _runningTask = Task.Run(() =>
                {
                    BackgroundActivity = CancelableBackgroundActivity = true;
                    foreach (NAVObject navObject in NAVObjectExtensions.FindWhere(WorkspaceFiles.ToArray(), keyword, progress, _token))
                    {
                        Application.Current.Dispatcher.Invoke(delegate
                        {
                            SearchResult.Add(navObject);
                        });
                    }

                    progress.Report(new KeyValuePair<double, string>(100, string.Format("{0} found in {1} files of {2}", keyword, SearchResult.Count, WorkspaceFiles.Count)));
                    BackgroundActivity = CancelableBackgroundActivity = false;
                }, _token);
            }
        }

        private void ThrowCancellation_Action(object obj)
        {
            if (_runningTask != null && _tokenSource != null)
            {
                if ((_token.IsCancellationRequested == false) && (_runningTask.Status != TaskStatus.Canceled))
                {
                    BackgroundActivity = false;
                    _tokenSource.Cancel();
                }
            }
        }
    }
}
