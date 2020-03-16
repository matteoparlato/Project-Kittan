namespace Project_Kittan.Models
{
    /// <summary>
    /// NAVObject class
    /// </summary>
    public class NAVObject : File
    {
        private string _objectName;
        public string ObjectName
        {
            get => _objectName;
            set => SetProperty(ref _objectName, value);
        }

        private string _ID;
        public string ID
        {
            get => _ID;
            set => SetProperty(ref _ID, value);
        }

        private string _type;
        public string Type
        {
            get => _type;
            set => SetProperty(ref _type, value);
        }

        /// <summary>
        /// Constructor which initializes a ObjectElements object with passed information.
        /// </summary>
        /// <param name="type">The type of the object</param>
        /// <param name="id">The ID of the object</param>
        /// <param name="objectName">The name of the object</param>
        /// <param name="filePath">The path of the file object</param>
        public NAVObject(string type, string id, string objectName, string filePath) : base(filePath)
        {
            Type = type;
            ID = id;
            ObjectName = objectName;
        }
    }
}
