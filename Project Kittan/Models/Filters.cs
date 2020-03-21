namespace Project_Kittan.Models
{
    public class Filters : BindableBase
    {
        private string _table;
        public string Table
        {
            get => _table;
            set => SetProperty(ref _table, value);
        }

        private string _page;
        public string Page
        {
            get => _page;
            set => SetProperty(ref _page, value);
        }

        private string _form;
        public string Form
        {
            get => _form;
            set => SetProperty(ref _form, value);
        }

        private string _report;
        public string Report
        {
            get => _report;
            set => SetProperty(ref _report, value);
        }

        private string _codeunit;
        public string Codeunit
        {
            get => _codeunit;
            set => SetProperty(ref _codeunit, value);
        }

        private string _query;
        public string Query
        {
            get => _query;
            set => SetProperty(ref _query, value);
        }

        private string _XMLport;
        public string XMLport
        {
            get => _XMLport;
            set => SetProperty(ref _XMLport, value);
        }

        private string _dataport;
        public string Dataport
        {
            get => _dataport;
            set => SetProperty(ref _dataport, value);
        }

        private string _menuSuite;
        public string MenuSuite
        {
            get => _menuSuite;
            set => SetProperty(ref _menuSuite, value);
        }

        public Filters()
        {
            Table = "";
            Page = "";
            Form = "";
            Report = "";
            Codeunit = "";
            Query = "";
            XMLport = "";
            Dataport = "";
            MenuSuite = "";
        }
    }
}
