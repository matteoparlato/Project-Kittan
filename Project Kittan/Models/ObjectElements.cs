using System.Collections.Generic;

namespace Project_Kittan.Models
{
    /// <summary>
    /// ObjectElements class
    /// </summary>
    internal class ObjectElements
    {
        public string FilePath { get; set; } = string.Empty;

        public List<ControlProperties> Controls { get; set; } = new List<ControlProperties>();

        public List<ControlProperties> Procedures { get; set; } = new List<ControlProperties>();

        public List<ControlProperties> Conflicts { get; set; } = new List<ControlProperties>();
    }
}
