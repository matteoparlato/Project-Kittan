using Project_Kittan.Helpers;
using System.Windows.Input;

namespace Project_Kittan.ViewModels
{
    public class VersionListRequest : Observable
    {
        public string VersionList { get; set; }

        public int MaxVersionListLenght { get; set; }

        public int AvailableChars { get; set; }

        public ICommand VersionListChanged { get; set; }

        public VersionListRequest()
        {
            VersionListChanged = new RelayCommand<object>(VersionListChanged_Action);
        }

        private void VersionListChanged_Action(object obj)
        {

        }
    }
}
