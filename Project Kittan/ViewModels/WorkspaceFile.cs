using Project_Kittan.Models;
using System;
using System.Windows.Input;

namespace Project_Kittan.ViewModels
{
    public class WorkspaceFile : Models.File
    {
        public WorkspaceFile(string path) : base(path)
        {
            OpenFile = new DelegateCommand(new Action<object>(OpenFile_Action));
        }

        public ICommand OpenFile { get; set; }

        public void OpenFile_Action(object obj)
        {
#if !APPX
            Process.Start(Path);
#endif
        }
    }
}
