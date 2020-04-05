using Project_Kittan.Helpers;
using System.Diagnostics;
using System.Windows.Input;

namespace Project_Kittan.ViewModels
{
    /// <summary>
    /// WorkspaceFile class
    /// </summary>
    public class WorkspaceFile : Models.File
    {
        /// <summary>
        /// Constructor which initializes a WorkspaceFile object with passed information.
        /// </summary>
        /// <param name="path">The path of the file</param>
        public WorkspaceFile(string path) : base(path)
        {
            OpenFileCommand = new RelayCommand<object>(OpenFile_Action);
        }

        public ICommand OpenFileCommand { get; set; }

        private void OpenFile_Action(object obj)
        {
#if !APPX
            Process.Start(Path);
#endif
        }
    }
}
