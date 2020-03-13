using Project_Kittan.Helpers;
using Project_Kittan.Models;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows;
using System.Threading.Tasks;
using System.IO;

namespace Project_Kittan.ViewModels
{
    public class Workspace : BindableBase
    {
        public ObservableCollection<Models.File> Files { get; set; }

        public Models.File File { get; set; }


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

        private ICommand _deleteFileCommand;
        public ICommand DeleteFileCommand
        {
            get { return _deleteFileCommand; }
            set { _deleteFileCommand = value; }
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

        public Workspace()
        {
            DeleteFileCommand = new DelegateCommand(new Action<object>(RemoveSelectedFile_Action));
            AddFilesFromExecutableFolder = new DelegateCommand(new Action<object>(AddFilesFromExecutableFolder_Action));
            BrowseWorkspaceFolder = new DelegateCommand(new Action<object>(BrowseWorkspaceFolder_Action));
            ClearWorkspace = new DelegateCommand(new Action<object>(ClearWorkspace_Action));

            Files = new ObservableCollection<Models.File>();
        }

        public void RemoveSelectedFile_Action(object obj)
        {
            Files.Remove((Models.File)obj);
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
                Files.Add(new Models.File(filePath));
            }

            if (Files.Count == 0)
            {
                _ready = false;

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
            Ready = true;
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
            Files.Clear();
            Ready = false;
            Path = string.Empty;
        }
    }
}
