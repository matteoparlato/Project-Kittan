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
        /// Constructor which initializes a ObjectElements object with passed information.
        /// </summary>
        /// <param name="id">The ID of the object</param>
        /// <param name="type">The type of the object</param>
        /// <param name="name">The name of the object</param>
        public ObjectElements(string type, string id, string name, string filePath)
        {
            ID = id;
            Type = type;
            Name = name;
            FilePath = filePath;
        }

        /// <summary>
        /// Constructor which initializes a ObjectElements object with passed information.
        /// </summary>
        /// <param name="filePath">The path of the file</param>
        public ObjectElements(string filePath)
        {
            FilePath = filePath;
        }

        public List<ElementProperties> Controls { get; private set; } = new List<ElementProperties>();

        public List<ElementProperties> Procedures { get; private set; } = new List<ElementProperties>();

        public HashSet<ElementProperties> Conflicts { get; set; } = new HashSet<ElementProperties>();
    }
}
