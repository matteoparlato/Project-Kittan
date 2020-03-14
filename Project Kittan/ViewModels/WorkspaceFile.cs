using Project_Kittan.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Project_Kittan.ViewModels
{
    public class WorkspaceFile : Models.File
    {
        public WorkspaceFile(string path) : base(path)
        {
            OpenFile = new DelegateCommand(new Action<object>(OpenFile_Action));
        }

        private ICommand _openFile;
        public ICommand OpenFile
        {
            get { return _openFile; }
            set { _openFile = value; }
        }

        public void OpenFile_Action(object obj)
        {
            Process.Start(Path);
        }
    }
}
