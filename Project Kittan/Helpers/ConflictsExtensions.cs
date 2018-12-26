using Project_Kittan.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace Project_Kittan.Helpers
{
    internal class ConflictsExtensions
    {
        public static void ProcessLines(string text, string fileName, ref List<ObjectElements> elements, ref int lineNumber)
        {
            var lines = text.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

            ObjectElements element = new ObjectElements();
            element.FilePath = Path.GetFileName(fileName) + " - " + lines[0];

            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Equals("  CONTROLS") || lines[i].Equals("  FIELDS"))
                {
                    while (!lines[i].Equals("  }"))
                    {
                        if (lines[i].Contains("{"))
                        {
                            var lineParts = lines[i].Split(';');
                            if (lineParts.Length > 0)
                            {
                                string id = lineParts[0].Replace('{', ' ').Trim();
                                if (id.Length > 0 && !id.Contains("ID="))
                                {
                                    ControlProperties properties = new ControlProperties();
                                    properties.ID = id;
                                    properties.LineNumber = lineNumber.ToString();
                                    properties.LinePreview = lines[i];
                                    element.Controls.Add(properties);
                                }
                            }
                        }
                        lineNumber++;
                        i++;
                    }
                }
                if (lines[i].Equals("  CODE"))
                {
                    while (!lines[i].Equals("  }"))
                    {
                        ControlProperties properties = new ControlProperties();
                        string deleteMe = lines[i];
                        if (lines[i].StartsWith("    PROCEDURE") || lines[i].StartsWith("    LOCAL"))
                        {
                            var lineParts = lines[i].Split('@');
                            properties.ID = lineParts[1].Split('(')[0];
                            properties.LineNumber = lineNumber.ToString();
                            properties.LinePreview = lines[i];
                            element.Procedures.Add(properties);
                        }
                        //if (line.StartsWith("    VAR"))
                        //{
                        //    while ((line = reader.ReadLine()) != null && !line.Equals("    BEGIN"))
                        //    {
                        //        lineNumber++;

                        //        var lineParts = line.Split(':');
                        //        ControlProperties varProperties = new ControlProperties();
                        //        //varProperties.ID = lineParts[1].Split('(')[0];
                        //        //varProperties.LineNumber = lineNumber.ToString();
                        //        //varProperties.LinePreview = line;
                        //        //element.Procedures.Add(properties);
                        //    }
                        //}

                        lineNumber++;
                        i++;
                    }
                }

                lineNumber++;
            }

            foreach (ControlProperties control in element.Controls)
            {
                Search(control, element.Controls, ref element);
            }

            foreach (ControlProperties control in element.Procedures)
            {
                Search(control, element.Procedures, ref element);
            }

            element.Conflicts.Sort();

            elements.Add(element);
        }

        /// <summary>
        /// Method which searches the given ControlProperties object in the passed collection of ControlObjects.
        /// Any occurrence is added to the Conflicts collection of passed ObjectElements.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="elements"></param>
        /// <param name="objp"></param>
        private static void Search(ControlProperties element, List<ControlProperties> elements, ref ObjectElements objp)
        {
            foreach (ControlProperties elem in elements)
            {
                if (element.ID.Equals(elem.ID) && !element.LineNumber.Equals(elem.LineNumber))
                {
                    objp.Conflicts.Add(elem);
                }
            }
        }
    }
}
