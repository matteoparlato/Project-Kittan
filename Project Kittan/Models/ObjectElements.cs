using System.Collections.Generic;

namespace Project_Kittan.Models
{
    /// <summary>
    /// ObjectElements class
    /// </summary>
    internal class ObjectElements
    {
        public string FilePath { get; set; } = string.Empty;

        public List<ElementProperties> Controls { get; private set; } = new List<ElementProperties>();

        public List<ElementProperties> Procedures { get; private set; } = new List<ElementProperties>();

        public HashSet<ElementProperties> Conflicts { get; set; } = new HashSet<ElementProperties>();
    }
}
