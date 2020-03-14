using Project_Kittan.Helpers;
using Project_Kittan.Models;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows;
using System.IO;
using System.Linq;

namespace Project_Kittan.ViewModels
{
    public class Workspace : BindableBase
    {
        public ObservableCollection<WorkspaceFile> WorkspaceFiles { get; set; }

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

        private ICommand _updateObjectProperties;
        public ICommand UpdateObjectProperties
        {
            get { return _updateObjectProperties; }
            set { _updateObjectProperties = value; }
        }

        public Workspace()
        {
            RemoveFile = new DelegateCommand(new Action<object>(RemoveFile_Action));
            AddFilesFromExecutableFolder = new DelegateCommand(new Action<object>(AddFilesFromExecutableFolder_Action));
            BrowseWorkspaceFolder = new DelegateCommand(new Action<object>(BrowseWorkspaceFolder_Action));
            ClearWorkspace = new DelegateCommand(new Action<object>(ClearWorkspace_Action));
            OpenFile = new DelegateCommand(new Action<object>(OpenFile_Action));
            OpenFileLocation = new DelegateCommand(new Action<object>(OpenFileLocation_Action));
            DropFile = new DelegateCommand(new Action<object>(DropFile_Action));
            SearchOccurences = new DelegateCommand(new Action<object>(SearchOccurences_Action));
            UpdateObjectProperties = new DelegateCommand(new Action<object>(UpdateObjectProperties_Action));

            WorkspaceFiles = new ObservableCollection<WorkspaceFile>();
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

        /// <summary>
        /// Method invoked when the user clicks on Search for occurrences button.
        /// Start occurrences search on all text files in working directory.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void SearchOccurences_Action(object obj)
        {
            string searchPattern = (string)obj;

            if (!string.IsNullOrWhiteSpace(searchPattern))
            {
                //PatternFoundInTextBlock.Text = PatternTextBox.Text + " found in:";
                //StatusProgressBar.Value = 0;
                //ResponsiveStatusProgressBar.IsIndeterminate = true;
                //OutputTabControl.SelectedIndex = 1;

                await ObjectExtensions.FindWhere(WorkspaceFiles.ToArray(), searchPattern);

                //StatusProgressBar.IsIndeterminate = false;
                //ResponsiveStatusProgressBar.IsIndeterminate = false;
            }
        }

        /// <summary>
        /// Method invoked when the user clicks on Update OBJECT-PROPERTIES button.
        /// Start OBJECT-PROPERTIES update on all text files in working directory.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void UpdateObjectProperties_Action(object obj)
        {
            //StatusProgressBar.Value = 0;
            //ResponsiveStatusProgressBar.IsIndeterminate = true;

            var parameters = (object[])obj;

            await ObjectExtensions.UpdateObjects(WorkspaceFiles.ToArray(), (int)parameters[0], (string)parameters[1], (bool)parameters[2]);

            //StatusTextBlock.Text = "Done";
            //StatusProgressBar.IsIndeterminate = false;
            //ResponsiveStatusProgressBar.IsIndeterminate = false;
        }
    }
}
