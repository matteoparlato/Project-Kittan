using System;

namespace Project_Kittan.Models
{
    /// <summary>
    /// ControlProperties class
    /// </summary>
    internal class ControlProperties : IComparable<ControlProperties>
    {
        public string ID { get; set; } = string.Empty;

        public string LineNumber { get; set; } = string.Empty;

        public string LinePreview { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(ControlProperties other)
        {
            return this.ID.CompareTo(other.ID);
        }
    }
}
