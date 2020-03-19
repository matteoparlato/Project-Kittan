﻿using Project_Kittan.Helpers;
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

        public ObservableCollection<NAVObject> Filters { get; set; }


        private WorkspaceFile _selectedWorkspaceFile;
        public WorkspaceFile SelectedWorkspaceFile
        {
            get => _selectedWorkspaceFile;
            set => SetProperty(ref _selectedWorkspaceFile, value);
        }

        #region Filters

        private string _workspaceTableFilter;
        public string WorkspaceTableFilter
        {
            get => _workspaceTableFilter;
            set => SetProperty(ref _workspaceTableFilter, value);
        }

        private string _workspacePageFilter;
        public string WorkspacePageFilter
        {
            get => _workspacePageFilter;
            set => SetProperty(ref _workspacePageFilter, value);
        }

        private string _workspaceFormFilter;
        public string WorkspaceFormFilter
        {
            get => _workspaceFormFilter;
            set => SetProperty(ref _workspaceFormFilter, value);
        }

        private string _workspaceReportFilter;
        public string WorkspaceReportFilter
        {
            get => _workspaceReportFilter;
            set => SetProperty(ref _workspaceReportFilter, value);
        }

        private string _workspaceCodeunitFilter;
        public string WorkspaceCodeunitFilter
        {
            get => _workspaceCodeunitFilter;
            set => SetProperty(ref _workspaceCodeunitFilter, value);
        }

        private string _workspaceQueryFilter;
        public string WorkspaceQueryFilter
        {
            get => _workspaceQueryFilter;
            set => SetProperty(ref _workspaceQueryFilter, value);
        }

        private string _workspaceXMLportFilter;
        public string WorkspaceXMLportFilter
        {
            get => _workspaceXMLportFilter;
            set => SetProperty(ref _workspaceXMLportFilter, value);
        }

        private string _workspaceDataportFilter;
        public string WorkspaceDataportFilter
        {
            get => _workspaceDataportFilter;
            set => SetProperty(ref _workspaceDataportFilter, value);
        }

        private string _workspaceMenuSuiteFilter;
        public string WorkspaceMenuSuiteFilter
        {
            get => _workspaceMenuSuiteFilter;
            set => SetProperty(ref _workspaceMenuSuiteFilter, value);
        }

        #endregion Filters

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
            set
            {
                SetProperty(ref _progressText, value);
            }
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

        private ICommand _throwCancellation;
        public ICommand ThrowCancellation
        {
            get { return _throwCancellation; }
            set { _throwCancellation = value; }
        }

        private ICommand _openSettings;
        public ICommand OpenSettings
        {
            get { return _openSettings; }
            set { _openSettings = value; }
        }

        private ICommand _getFiltersFromFiles;
        public ICommand GetFiltersFromFiles
        {
            get { return _getFiltersFromFiles; }
            set { _getFiltersFromFiles = value; }
        }

        private ICommand _getFiltersFromClipboard;
        public ICommand GetFiltersFromClipboard
        {
            get { return _getFiltersFromClipboard; }
            set { _getFiltersFromClipboard = value; }
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
            ThrowCancellation = new DelegateCommand(new Action<object>(Command_ThrowCancellation));
            OpenSettings = new DelegateCommand(new Action<object>(OpenSettings_Action), new Predicate<object>(Command_CanExecute));
            GetFiltersFromFiles = new DelegateCommand(new Action<object>(GetFiltersFromFiles_Action), new Predicate<object>(Command_CanExecute));
            GetFiltersFromClipboard = new DelegateCommand(new Action<object>(GetFiltersFromClipboard_Action), new Predicate<object>(Command_CanExecute));

            WorkspaceFiles = new ObservableCollection<WorkspaceFile>();
            Result = new ObservableCollection<NAVObject>();
            Filters = new ObservableCollection<NAVObject>();

            WorkspaceFiles.CollectionChanged += Files_CollectionChanged;
        }

        private void GetFiltersFromClipboard_Action(object obj)
        {
            string clipboardText = Clipboard.GetText();
            if (!string.IsNullOrWhiteSpace(clipboardText))
            {
                using(StringReader reader = new StringReader(clipboardText))
                {
                    if (reader.ReadLine().StartsWith("Type"))
                    {
                        WorkspaceTableFilter = "";
                        WorkspacePageFilter = "";
                        WorkspaceFormFilter = "";
                        WorkspaceReportFilter = "";
                        WorkspaceCodeunitFilter = "";
                        WorkspaceQueryFilter = "";
                        WorkspaceXMLportFilter = "";
                        WorkspaceDataportFilter = "";
                        WorkspaceMenuSuiteFilter = "";

                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            string[] objectDetails = line.Split('\t');

                            switch (objectDetails[0])
                            {
                                case "1":
                                    {
                                        _workspaceTableFilter += objectDetails[1] + '|';
                                        break;
                                    }
                                case "2":
                                    {
                                        break;
                                    }
                                case "3":
                                    {
                                        _workspaceReportFilter += objectDetails[1] + '|';
                                        break;
                                    }
                                case "4":
                                    {
                                        break;
                                    }
                                case "5":
                                    {
                                        _workspaceCodeunitFilter += objectDetails[1] + '|';
                                        break;
                                    }
                                case "6":
                                    {
                                        _workspaceXMLportFilter += objectDetails[1] + '|';
                                        break;
                                    }
                                case "7":
                                    {
                                        _workspaceMenuSuiteFilter += objectDetails[1] + '|';
                                        break;
                                    }
                                case "8":
                                    {
                                        _workspacePageFilter += objectDetails[1] + '|';
                                        break;
                                    }
                                case "9":
                                    {
                                        _workspaceQueryFilter += objectDetails[1] + '|';
                                        break;
                                    }
                                default:
                                    {
                                        break;
                                    }
                            }
                        }

                        WorkspaceTableFilter = !string.IsNullOrWhiteSpace(_workspaceTableFilter) ? _workspaceTableFilter.Substring(0, _workspaceTableFilter.Length - 1) : "";
                        WorkspacePageFilter = !string.IsNullOrWhiteSpace(_workspacePageFilter) ? _workspacePageFilter.Substring(0, _workspacePageFilter.Length - 1) : "";
                        WorkspaceFormFilter = !string.IsNullOrWhiteSpace(_workspaceFormFilter) ? _workspaceFormFilter.Substring(0, _workspaceFormFilter.Length - 1) : "";
                        WorkspaceReportFilter = !string.IsNullOrWhiteSpace(_workspaceReportFilter) ? _workspaceReportFilter.Substring(0, _workspaceReportFilter.Length - 1) : "";
                        WorkspaceCodeunitFilter = !string.IsNullOrWhiteSpace(_workspaceCodeunitFilter) ? _workspaceCodeunitFilter.Substring(0, _workspaceCodeunitFilter.Length - 1) : "";
                        WorkspaceQueryFilter = !string.IsNullOrWhiteSpace(_workspaceQueryFilter) ? _workspaceQueryFilter.Substring(0, _workspaceQueryFilter.Length - 1) : "";
                        WorkspaceXMLportFilter = !string.IsNullOrWhiteSpace(_workspaceXMLportFilter) ? _workspaceXMLportFilter.Substring(0, _workspaceXMLportFilter.Length - 1) : "";
                        WorkspaceDataportFilter = !string.IsNullOrWhiteSpace(_workspaceDataportFilter) ? _workspaceDataportFilter.Substring(0, _workspaceDataportFilter.Length - 1) : ""; ;
                        WorkspaceMenuSuiteFilter = !string.IsNullOrWhiteSpace(_workspaceMenuSuiteFilter) ? _workspaceMenuSuiteFilter.Substring(0, _workspaceMenuSuiteFilter.Length - 1) : "";

                        ProgressText = "Succesfully obtained filters from clipboard";
                    }
                    else
                    {
                        MessageBox.Show("The clipboard doesn't contain any NAV object data!", Properties.Resources.AppName, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }
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

            Filters.Clear();
            _runningTask = Task.Run(() =>
            {
                BackgroundActivity = CancelableBackgroundActivity = true;
                foreach (NAVObject navObject in ObjectExtensions.PrepareData(WorkspaceFiles.ToArray(),  progress, _token))
                {
                    Application.Current.Dispatcher.Invoke(delegate
                    {
                        Filters.Add(navObject);
                    });
                }

                WorkspaceTableFilter = string.Join("|", Filters.Where(navObject => navObject.Type == "Table").Select(navObject => navObject.ID).ToArray());
                WorkspacePageFilter = string.Join("|", Filters.Where(navObject => navObject.Type == "Page").Select(navObject => navObject.ID).ToArray());
                WorkspaceFormFilter = string.Join("|", Filters.Where(navObject => navObject.Type == "Form").Select(navObject => navObject.ID).ToArray());
                WorkspaceReportFilter = string.Join("|", Filters.Where(navObject => navObject.Type == "Report").Select(navObject => navObject.ID).ToArray());
                WorkspaceCodeunitFilter = string.Join("|", Filters.Where(navObject => navObject.Type == "Codeunit").Select(navObject => navObject.ID).ToArray());
                WorkspaceQueryFilter = string.Join("|", Filters.Where(navObject => navObject.Type == "Query").Select(navObject => navObject.ID).ToArray());
                WorkspaceXMLportFilter = string.Join("|", Filters.Where(navObject => navObject.Type == "XMLport").Select(navObject => navObject.ID).ToArray());
                WorkspaceDataportFilter = string.Join("|", Filters.Where(navObject => navObject.Type == "Dataport").Select(navObject => navObject.ID).ToArray());
                WorkspaceMenuSuiteFilter = string.Join("|", Filters.Where(navObject => navObject.Type == "MenuSuite").Select(navObject => navObject.ID).ToArray());

                progress.Report(new KeyValuePair<double, string>(100, string.Format("Succesfully obtained filters from loaded files", Result.Count, WorkspaceFiles.Count)));
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

                    progress.Report(new KeyValuePair<double, string>(100, string.Format("{0} found in {1} files of {2}", keyword, Result.Count, WorkspaceFiles.Count)));
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

        private void OpenSettings_Action(object obj)
        {
            new SettingsWindow().ShowDialog();
        }
    }
}
