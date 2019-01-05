using System.Collections.Generic;

namespace Project_Kittan.Models
{
    /// <summary>
    /// ObjectElements class
    /// </summary>
    internal class ObjectElements
    {
        public string ID { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string FilePath { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        /// <param name="name"></param>
        public ObjectElements(string id, string type, string name, string filePath)
        {
            ID = id;
            Type = type;
            Name = name;
            FilePath = filePath;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        public ObjectElements(string filePath)
        {
            FilePath = filePath;
        }

        public List<ElementProperties> Controls { get; private set; } = new List<ElementProperties>();

        public List<ElementProperties> Procedures { get; private set; } = new List<ElementProperties>();

        public HashSet<ElementProperties> Conflicts { get; set; } = new HashSet<ElementProperties>();
    }
}
