using Project_Kittan.Helpers;
using Project_Kittan.Models;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;

namespace Project_Kittan.ViewModels
{
    public class Workspace : BindableBase
    {
        public ObservableCollection<WorkspaceFile> WorkspaceFiles { get; set; }

        public ObservableCollection<NAVObject> Result { get; set; }

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

        private ICommand _removeFile;
        public ICommand RemoveFile
        {
            get { return _removeFile; }
            set { _removeFile = value; }
        }

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

        private ICommand _addFilesFromExecutableFolder;
        public ICommand AddFilesFromExecutableFolder
        {
            get { return _addFilesFromExecutableFolder; }
            set { _addFilesFromExecutableFolder = value; }
        }

        private ICommand _browseWorkspaceFolder;
        public ICommand BrowseWorkspaceFolder
        {
            get { return _browseWorkspaceFolder; }
            set { _browseWorkspaceFolder = value; }
        }

        private ICommand _clearWorkspace;
        public ICommand ClearWorkspace
        {
            get { return _clearWorkspace; }
            set { _clearWorkspace = value; }
        }

        private ICommand _openFile;
        public ICommand OpenFile
        {
            get { return _openFile; }
            set { _openFile = value; }
        }

        private ICommand _openFileLocation;
        public ICommand OpenFileLocation
        {
            get { return _openFileLocation; }
            set { _openFileLocation = value; }
        }

        private ICommand _dropFile;
        public ICommand DropFile
        {
            get { return _dropFile; }
            set { _dropFile = value; }
        }

        private ICommand _searchOccurences;
        public ICommand SearchOccurences
        {
            get { return _searchOccurences; }
            set { _searchOccurences = value; }
        }

        private ICommand _addTag;
        public ICommand AddTag
        {
            get { return _addTag; }
            set { _addTag = value; }
        }

        private ICommand _removeTag;
        public ICommand RemoveTag
        {
            get { return _removeTag; }
            set { _removeTag = value; }
        }

        private ICommand _copyValue;
        public ICommand CopyValue
        {
            get { return _copyValue; }
            set { _copyValue = value; }
        }

        private ICommand _throwCancellation;
        public ICommand ThrowCancellation
        {
            get { return _throwCancellation; }
            set { _throwCancellation = value; }
        }

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
            CopyValue = new DelegateCommand(new Action<object>(CopyValue_Action), new Predicate<object>(Command_CanExecute));
            ThrowCancellation = new DelegateCommand(new Action<object>(Command_ThrowCancellation));

            WorkspaceFiles = new ObservableCollection<WorkspaceFile>();
            Result = new ObservableCollection<NAVObject>();
            WorkspaceFiles.CollectionChanged += Files_CollectionChanged;
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
                Path = string.Empty;
            }
        }

        public void RemoveFile_Action(object obj)
        {
            WorkspaceFiles.Remove(SelectedWorkspaceFile);
        }

        public void OpenFile_Action(object obj)
        {
            Process.Start(SelectedWorkspaceFile.Path);
        }

        public void OpenFileLocation_Action(object obj)
        {
            Process.Start("explorer.exe", "/select, " + SelectedWorkspaceFile.Path);
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
            foreach(string filePath in Directory.GetFiles(path, "*.txt", SearchOption.AllDirectories))
            {
                WorkspaceFiles.Add(new WorkspaceFile(filePath));
            }

            if (WorkspaceFiles.Count == 0)
            {
                if (MessageBox.Show("No file found. Do you want to select another folder?", "Project Kittan", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
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
            string keyword = (string)obj;

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                IProgress<KeyValuePair<double, string>> progress = new Progress<KeyValuePair<double, string>>(status =>
                {
                    ProgressValue = status.Key;
                    ProgressText = status.Value;
                });

                _tokenSource = new CancellationTokenSource();
                _token = _tokenSource.Token;

                Result.Clear();
                _runningTask = Task.Run(() =>
                {
                    BackgroundActivity = CancelableBackgroundActivity = true;
                    foreach (NAVObject navObject in ObjectExtensions.FindWhere(WorkspaceFiles.ToArray(), keyword, progress, _token))
                    {
                        Application.Current.Dispatcher.Invoke(delegate
                        {
                            Result.Add(navObject);
                        });
                    }

                    progress.Report(new KeyValuePair<double, string>(100, string.Format("{0} found  in {1} files of {2}", keyword, Result.Count, WorkspaceFiles.Count)));
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
            
            if (!string.IsNullOrWhiteSpace(tag))
            {
                IProgress<KeyValuePair<double, string>> progress = new Progress<KeyValuePair<double, string>>(status =>
                {
                    ProgressValue = status.Key;
                    ProgressText = status.Value;
                });

                Result.Clear();
                _runningTask = Task.Run(() =>
                {
                    BackgroundActivity = true;
                    ObjectExtensions.AddTag(WorkspaceFiles.ToArray(), (int)parameters[0], (string)parameters[1], (bool)parameters[2], progress);

                    progress.Report(new KeyValuePair<double, string>(0, string.Format("Tag {0} added to the version list of {1} files", (string)parameters[1], WorkspaceFiles.Count)));
                    BackgroundActivity = false;
                });
            }
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

                Result.Clear();
                _runningTask = Task.Run(() =>
                {
                    BackgroundActivity = true;
                    ObjectExtensions.RemoveTag(WorkspaceFiles.ToArray(), tag, (bool)parameters[0], progress);

                    progress.Report(new KeyValuePair<double, string>(100, string.Format("Tag {0} removed from the version list of {1} files", (string)parameters[1], WorkspaceFiles.Count)));
                    BackgroundActivity = false;
                });
            }
        }


        /// <summary>
        /// Method invoked when the user Copy value from ConflictsListView contex menu.
        /// Copy the right-clicked value to the clipboard.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CopyValue_Action(object obj)
        {
            Clipboard.SetText(((string)obj).Trim());
        }
    }
}
