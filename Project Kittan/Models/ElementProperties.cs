using System;
using System.Collections.Generic;

namespace Project_Kittan.Models
{
    /// <summary>
    /// ElementProperties class
    /// </summary>
    public class ElementProperties : IComparable<ElementProperties>
    {
        public string ID { get; set; } = "";

        public List<ElementProperties> Vars { get; set; } = new List<ElementProperties>();

        public int LineNumber { get; set; }

        public string LinePreview { get; set; } = "";

        /// <summary>
        /// Parameterless constructor of ElementProperties class.
        /// </summary>
        public ElementProperties()
        {
            //
        }

        /// <summary>
        /// Constructor which initializes a ElementProperties object with passed information.
        /// </summary>
        /// <param name="id">The ID of the element</param>
        /// <param name="lineNumber">The line number where the ID is located</param>
        /// <param name="linePreview">The text at that line number</param>
        public ElementProperties(string id, int lineNumber, string linePreview)
        {
            ID = id;
            LineNumber = lineNumber;
            LinePreview = linePreview;
        }

        /// <summary>
        /// Method which compares the current instance with another object of the same type and returns an
        /// integer that indicates whether the current instance precedes, follows, or occurs in the same 
        /// position in the sort order as the other object.
        /// </summary>
        /// <param name="other">An object to compare with this instance</param>
        /// <returns>A value that indicates the relative order of the objects being compared</returns>
        public int CompareTo(ElementProperties other)
        {
            return this.ID.CompareTo(other.ID);
        }
    }
}
