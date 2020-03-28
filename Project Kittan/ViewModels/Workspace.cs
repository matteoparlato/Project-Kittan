using Project_Kittan.Helpers;
using Project_Kittan.Models;
using Project_Kittan.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Project_Kittan.ViewModels
{
    public class Workspace : BindableBase
    {
        public ObservableCollection<WorkspaceFile> WorkspaceFiles { get; set; }

        public ObservableCollection<NAVObject> SearchResult { get; set; }

        private Filters _fileFilters;
        public Filters FileFilters
        {
            get => _fileFilters;
            set => SetProperty(ref _fileFilters, value);
        }

        private WorkspaceFile _selectedWorkspaceFile;
        public WorkspaceFile SelectedWorkspaceFile
        {
            get => _selectedWorkspaceFile;
            set => SetProperty(ref _selectedWorkspaceFile, value);
        }

        private string _path;
        public string Path
        {
            get => _path;
            set => SetProperty(ref _path, value);
        }

        private bool _ready;
        public bool Ready
        {
            get => _ready;
            set => SetProperty(ref _ready, value);
        }
        public ICommand RemoveFile { get; set; }

        private double _progressValue;
        public double ProgressValue
        {
            get => _progressValue;
            set => SetProperty(ref _progressValue, value);
        }

        private string _progressText;
        public string ProgressText
        {
            get => _progressText;
            set => SetProperty(ref _progressText, value);
        }

        private bool _backgroundActivity;
        public bool BackgroundActivity
        {
            get => _backgroundActivity;
            set => SetProperty(ref _backgroundActivity, value);
        }

        private bool _cancelableBackgroundActivity;
        public bool CancelableBackgroundActivity
        {
            get => _cancelableBackgroundActivity;
            set => SetProperty(ref _cancelableBackgroundActivity, value);
        }

        public ICommand AddFilesFromExecutableFolder { get; set; }
        public ICommand BrowseWorkspaceFolder { get; set; }
        public ICommand ClearWorkspace { get; set; }
        public ICommand OpenFile { get; set; }
        public ICommand OpenFileLocation { get; set; }
        public ICommand DropFile { get; set; }
        public ICommand SearchOccurences { get; set; }
        public ICommand AddTag { get; set; }
        public ICommand RemoveTag { get; set; }
        public ICommand ThrowCancellation { get; set; }
        public ICommand OpenSettings { get; set; }
        public ICommand GetFiltersFromFiles { get; set; }
        public ICommand GetFiltersFromClipboard { get; set; }
        public ICommand GetFiltersFromOccurences { get; set; }

    public Workspace()
        {
            RemoveFile = new DelegateCommand(new Action<object>(RemoveFile_Action), new Predicate<object>(Command_CanExecute));
            AddFilesFromExecutableFolder = new DelegateCommand(new Action<object>(AddFilesFromExecutableFolder_Action), new Predicate<object>(Command_CanExecute));
            BrowseWorkspaceFolder = new DelegateCommand(new Action<object>(BrowseWorkspaceFolder_Action), new Predicate<object>(Command_CanExecute));
            ClearWorkspace = new DelegateCommand(new Action<object>(ClearWorkspace_Action), new Predicate<object>(Command_CanExecute));
            OpenFile = new DelegateCommand(new Action<object>(OpenFile_Action), new Predicate<object>(Command_CanExecute));
            OpenFileLocation = new DelegateCommand(new Action<object>(OpenFileLocation_Action), new Predicate<object>(Command_CanExecute));
            DropFile = new DelegateCommand(new Action<object>(DropFile_Action), new Predicate<object>(Command_CanExecute));
            RemoveTag = new DelegateCommand(new Action<object>(RemoveTag_Action), new Predicate<object>(Command_CanExecute));
            SearchOccurences = new DelegateCommand(new Action<object>(SearchOccurences_Action), new Predicate<object>(Command_CanExecute));
            AddTag = new DelegateCommand(new Action<object>(AddTag_Action), new Predicate<object>(Command_CanExecute));
            ThrowCancellation = new DelegateCommand(new Action<object>(Command_ThrowCancellation));
            OpenSettings = new DelegateCommand(new Action<object>(OpenSettings_Action), new Predicate<object>(Command_CanExecute));
            GetFiltersFromFiles = new DelegateCommand(new Action<object>(GetFiltersFromFiles_Action), new Predicate<object>(Command_CanExecute));
            GetFiltersFromClipboard = new DelegateCommand(new Action<object>(GetFiltersFromClipboard_Action), new Predicate<object>(Command_CanExecute));
            GetFiltersFromOccurences = new DelegateCommand(new Action<object>(GetFiltersFromOccurences_Action), new Predicate<object>(Command_CanExecute));

            WorkspaceFiles = new ObservableCollection<WorkspaceFile>();
            SearchResult = new ObservableCollection<NAVObject>();
            FileFilters = new Filters();

            WorkspaceFiles.CollectionChanged += Files_CollectionChanged;
        }

        private void GetFiltersFromClipboard_Action(object obj)
        {
            string clipboardText = Clipboard.GetText();
            if (!string.IsNullOrWhiteSpace(clipboardText))
            {
                try
                {
                    FileFilters = NAVObjectExtensions.GetFiltersFromClipboard(clipboardText);
                    ProgressText = "Succesfully obtained filters from clipboard";
                }
                catch(ArgumentException ex)
                {
                    MessageBox.Show(ex.Message, Properties.Resources.AppName, MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void GetFiltersFromOccurences_Action(object obj)
        {
            if (!(SearchResult.Count == 0))
            {
                try
                {
                    FileFilters = NAVObjectExtensions.GetFilters(SearchResult.ToList());
                    ProgressText = "Succesfully obtained filters from occurences";
                }
                catch (ArgumentException ex)
                {
                    MessageBox.Show(ex.Message, Properties.Resources.AppName, MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void Files_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
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

                progress.Report(new KeyValuePair<double, string>(100, string.Format("Succesfully obtained filters from loaded files", SearchResult.Count, WorkspaceFiles.Count)));
                BackgroundActivity = CancelableBackgroundActivity = false;
            }, _token);
        }

        public void RemoveFile_Action(object obj)
        {
            WorkspaceFiles.Remove(SelectedWorkspaceFile);
            // TODO: Filters.Remove(SelectedWorkspaceFile);
        }

        public void OpenFile_Action(object obj)
        {
#if !APPX
            Process.Start(SelectedWorkspaceFile.Path);
#endif
        }

        public void OpenFileLocation_Action(object obj)
        {
#if !APPX
            Process.Start("explorer.exe", "/select, " + SelectedWorkspaceFile.Path);
#endif
        }

        internal void AddFilesFromDrop(string[] files)
        {
            files = files.Where(i => i.EndsWith(".txt")).ToArray();
            foreach (string file in files)
            {
                WorkspaceFiles.Add(new WorkspaceFile(file));
            }
        }

        /// <summary>
        /// Method invoked when the user clicks on Use current folder button.
        /// Set current Project Kittan.exe directory as working directory.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddFilesFromExecutableFolder_Action(object obj)
        {
            string executablePath = Process.GetCurrentProcess().MainModule.FileName;
            executablePath = executablePath.Substring(0, executablePath.Length - System.IO.Path.GetFileName(executablePath).Length);

            OpenFolder(executablePath);
        }

        private void OpenFolder(string path)
        {
            WorkspaceFiles.Clear();
            foreach(string filePath in GetFilesFromDirectory(path))
            {
                WorkspaceFiles.Add(new WorkspaceFile(filePath));
            }

            if (WorkspaceFiles.Count == 0)
            {
                if (MessageBox.Show("No file found. Do you want to select another folder?", Properties.Resources.AppName, MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                {
                    return;
                }
                else
                {
                    BrowseWorkspaceFolder_Action(null);
                }
            }

            Path = path;
        }

        /// <summary>
        /// Method invoked when the user clicks on Browse Folder button.
        /// Opens OpenFolderDialog for choosing the working directory.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BrowseWorkspaceFolder_Action(object obj)
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

        /// <summary>
        /// Method invoked when the user clicks on Clear textblock.
        /// Clear Files collection and FoundFilesListBox.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClearWorkspace_Action(object obj)
        {
            WorkspaceFiles.Clear();
        }

        /// <summary>
        /// Method invoked when the user drag and drop a file on left pane.
        /// Add dropped *.txt files to Files collection.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DropFile_Action(object obj)
        {
            DragEventArgs args = (DragEventArgs)obj;
            if (args.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])args.Data.GetData(DataFormats.FileDrop);
                files = files.Where(i => i.EndsWith(".txt")).ToArray();
                foreach (string file in files)
                {
                    WorkspaceFiles.Add(new WorkspaceFile(file));
                }
            }
        }

        private Task _runningTask;
        private CancellationToken _token;
        private CancellationTokenSource _tokenSource;

        public bool Command_CanExecute(object obj)
        {
            if (_runningTask == null) return true;
            
            return !(_runningTask.Status == TaskStatus.Running);
        }

        public void Command_ThrowCancellation(object obj)
        {
            if ((_token != null) && (_runningTask != null) && (_tokenSource != null))
            {
                if ((_token.IsCancellationRequested == false) && (_runningTask.Status != TaskStatus.Canceled))
                {
                    BackgroundActivity = false;
                    _tokenSource.Cancel();
                }
            }
        }

        /// <summary>
        /// Method invoked when the user clicks on Search for occurrences button.
        /// Start occurrences search on all text files in working directory.
        /// </summary>
        private void SearchOccurences_Action(object obj)
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

        /// <summary>
        /// Method invoked when the user clicks on Update OBJECT-PROPERTIES button.
        /// Start OBJECT-PROPERTIES update on all text files in working directory.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddTag_Action(object obj)
        {
            var parameters = (object[])obj;
            string tag = (string)parameters[1];
            
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

        /// <summary>
        /// Method invoked when the user clicks on Update Remove tag button.
        /// Start tag removal from all text files in working directory.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RemoveTag_Action(object obj)
        {
            var parameters = (object[])obj;
            string tag = (string)parameters[1];

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

        private void OpenSettings_Action(object obj)
        {
            new SettingsWindow().ShowDialog();
        }

        /// <summary>
        /// Method which finds all *.txt files in the specified folder and relative subfolders.
        /// </summary>
        /// <param name="Path">The path of the directory</param>
        /// <returns>The list containing all the paths of the text files found in working directory</returns>
        private List<string> GetFilesFromDirectory(string Path)
        {
            List<string> files = new List<string>();
            try
            {
                foreach (string f in Directory.GetFiles(Path).Where(i => i.EndsWith(".txt")))
                {
                    files.Add(f);
                }
                foreach (string d in Directory.GetDirectories(Path).Where(i => !i.EndsWith(".hg") && !i.EndsWith(".git")))
                {
                    files.AddRange(GetFilesFromDirectory(d));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Project Kittan", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return files;
        }
    }
}
