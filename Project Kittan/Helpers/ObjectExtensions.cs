﻿using Project_Kittan.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Project_Kittan.Helpers
{
    /// <summary>
    /// ObjectExtensions class
    /// </summary>
    internal static class ObjectExtensions
    {
        public static ObservableCollection<ObjectElements> Conflicts { get; private set; } = new ObservableCollection<ObjectElements>();

        #region Conflicts

        /// <summary>
        /// Method which checks NAV objects in working directory for possible conflicts in
        /// CONTROLs, FIELDs, PROCEDUREs, local and global VARs.
        /// </summary>
        /// <param name="files">The files to check for conflicts</param>
        /// <returns></returns>
        internal async static Task ConflictsFinder(Models.File[] files)
        {
            double progressStep = (double)100 / files.Length;

            Conflicts.Clear();

            for (int i = 0; i < files.Length; i++)
            {
                MainWindow.Current.StatusTextBlock.Text = "Searching conflicts in " + files[i].FileName + "..."; // Update status

                using (FileStream stream = new FileStream(files[i].FilePath, FileMode.Open, FileAccess.Read))
                using (StreamReader reader = new StreamReader(stream, Encoding.GetEncoding(1252))) // Encoding 1252 is the same used by NAV (Windows-1252)
                {
                    string lines = await reader.ReadToEndAsync();
                    int lineNumber = 0;

                    if (GetFileType(lines, "  OBJECT-PROPERTIES") > 1) // The file contains multiple objects
                    {
                        var objects = lines.Split(new string[] { "OBJECT " }, StringSplitOptions.None);

                        for (int j = 1; j < objects.Length; j++) { Find(objects[j], files[i].FileName, ref lineNumber); }
                    }
                    else { Find(lines, files[i].FileName, ref lineNumber); } // The file contains one object
                }

                MainWindow.Current.StatusProgressBar.Value += progressStep; // Update status
            }

            MainWindow.Current.StatusTextBlock.Text = Conflicts.Count != 0 ? "Found " + Conflicts.Count + " possible conflicts" : "No conflicts found"; // Update status
        }

        /// <summary>
        /// Method which perform the check of possible ID conflicts on passed text.
        /// </summary>
        /// <param name="text">The text of the object to check</param>
        /// <param name="fileName">The name of the file to check</param>
        /// <param name="lineNumber"></param>
        private static void Find(string text, string fileName, ref int lineNumber)
        {
            var lines = text.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

            ObjectElements obj = new ObjectElements { FilePath = fileName + " - " + lines[0] };

            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Equals("  CONTROLS") || lines[i].Equals("  FIELDS")) // CONTROLs and FIELDs ID detection
                {
                    while (!lines[i].Equals("  }")) // End of CONTROLs and FIELDs object part
                    {
                        if (lines[i].StartsWith("    {")) { obj.Controls.Add(GetControlID(lines[i], lineNumber)); }

                        i++;
                        lineNumber++;
                    }
                }
                if (lines[i].Equals("  CODE")) // PROCEDUREs ID detection
                {
                    if (lines[i + 2].Equals("    VAR")) // Global VARs ID detection
                    {
                        i += 3;
                        lineNumber += 3;
                        ElementProperties globals = new ElementProperties();
                        while (!(lines[i].StartsWith("    PROCEDURE") || lines[i].StartsWith("    LOCAL") || lines[i].Equals("  }") || lines[i].Trim().Equals("")))
                        {
                            globals.Vars.Add(GetVarID(lines[i], lineNumber));

                            i++;
                            lineNumber++;
                        }
                        obj.Procedures.Add(globals);
                    }
                    while (!lines[i].Equals("  }")) // End of PROCEDUREs object part
                    {
                        if (lines[i].StartsWith("    PROCEDURE") || lines[i].StartsWith("    LOCAL"))
                        {
                            ElementProperties procedure = GetProcedureID(lines[i], lineNumber);

                            i++;
                            lineNumber++;

                            if (lines[i].StartsWith("    VAR"))
                            {
                                i++;
                                lineNumber++;

                                while (!lines[i].Equals("    BEGIN"))
                                {
                                    procedure.Vars.Add(GetVarID(lines[i], lineNumber)); // Local VARs ID detection

                                    i++;
                                    lineNumber++;
                                }

                                obj.Procedures.Add(procedure);
                            }
                        }

                        i++;
                        lineNumber++;
                    }
                }

                lineNumber++;
            }

            foreach (ElementProperties control in obj.Controls) { Search(control, obj.Controls, ref obj); }

            foreach (ElementProperties procedure in obj.Procedures)
            {
                Search(procedure, obj.Procedures, ref obj);
                foreach (ElementProperties @var in procedure.Vars) { Search(@var, procedure.Vars, ref obj); }
            }

            if (obj.Conflicts.Count > 0) { Conflicts.Add(obj); }
        }

        /// <summary>
        /// Method which searches the given ControlProperties object in the passed collection of ElementProperties.
        /// Any occurrence is added to the Conflicts collection of passed ObjectElements.
        /// </summary>
        /// <param name="control"></param>
        /// <param name="collection"></param>
        /// <param name="obj"></param>
        private static void Search(ElementProperties control, List<ElementProperties> collection, ref ObjectElements obj)
        {
            foreach (ElementProperties other in collection)
            {
                if (control.ID.Equals(other.ID) && !control.LineNumber.Equals(other.LineNumber)) { obj.Conflicts.Add(other); }
            }
        }

        /// <summary>
        /// Method which returns the ID of the VAR from passed string.
        /// </summary>
        /// <param name="line">The line where the ID is</param>
        /// <param name="lineNumber"></param>
        /// <returns>The ID of the VAR</returns>
        private static ElementProperties GetVarID(string line, int lineNumber)
        {
            var lineParts = line.Split('@');
            if(line.Length >= 2)
            {
                lineParts = lineParts[1].Split(':');
                if(lineParts.Length >= 1) return new ElementProperties(lineParts[0], lineNumber, line);
            }
            return new ElementProperties();            
        }

        /// <summary>
        /// Method which returns the ID of the CONTROL/FIELD from passed string.
        /// </summary>
        /// <param name="line">The line where the ID is</param>
        /// <param name="lineNumber"></param>
        /// <returns>Tehe ID of the CONTROL/FIELD</returns>
        private static ElementProperties GetControlID(string line, int lineNumber)
        {
            var lineParts = line.Split(';');
            if (lineParts.Length > 0)
            {
                string id = lineParts[0].Replace('{', ' ').Trim();
                if (id.Length > 0 && !id.Contains("ID=")) return new ElementProperties(id, lineNumber, line);
            }
            return new ElementProperties();
        }

        /// <summary>
        /// Method which returns the ID of the PROCEDURE from passed string.
        /// </summary>
        /// <param name="line">The line where the ID is</param>
        /// <param name="lineNumber"></param>
        /// <returns>The ID of the PROCEDURE</returns>
        private static ElementProperties GetProcedureID(string line, int lineNumber)
        {
            var lineParts = line.Split('@');
            if(lineParts.Length >= 2)
            {
                lineParts = line.Split('(');
                if(lineParts.Length > 0) return new ElementProperties(lineParts[0], lineNumber, line);
            }
            return new ElementProperties();
        }

        /// <summary>
        /// Method which return the type of file (if count is higher than one the file
        /// contains more than one object otherewise only one).
        /// </summary>
        /// <param name="text">The text of the file</param>
        /// <param name="pattern">The text to search for occurency count</param>
        /// <returns>The type of file</returns>
        private static int GetFileType(string text, string pattern)
        {
            int count = 0, i = 0;

            while ((i = text.IndexOf(pattern, i)) != -1)
            {
                i += pattern.Length;
                count++;

                if (count > 1) return count;
            }
            return count;
        }

        #endregion

        #region Properties updater

        /// <summary>
        /// Method which updates OBJECT-PROPERTIES with passed information.
        /// </summary>
        /// <param name="files">The collection of files to update</param>
        /// <param name="avoidUpdateDateTime"></param>
        /// <param name="version">The version tag to add</param>
        /// <param name="navVersion">The version of NAV used</param>
        internal async static Task UpdateObjects(Models.File[] files, bool avoidUpdateDateTime, string version, int navVersion)
        {
            double step = (double)100 / files.Length;

            foreach (Models.File file in files)
            {
                MainWindow.Current.StatusTextBlock.Text = "Updating " + file.FileName + " properties..."; // Update status

                StringBuilder builder = new StringBuilder();

                using (FileStream stream = new FileStream(file.FilePath, FileMode.Open, FileAccess.Read))
                using (StreamReader reader = new StreamReader(stream, Encoding.GetEncoding(1252))) // Encoding 1252 is the same used by NAV (Windows-1252)
                {
                    string line;

                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        if (line.Equals("  OBJECT-PROPERTIES"))
                        {
                            builder.AppendLine(line); // Add "  OBJECT-PROPERTIES"
                            builder.AppendLine(await reader.ReadLineAsync()); // Add "  {"

                            line = await reader.ReadLineAsync();
                            if (line.Contains("Date") && !avoidUpdateDateTime)
                            {
                                builder.AppendLine(string.Format("    Date={0:dd.MM.yy};", DateTime.Today));
                            }
                            else
                            {
                                builder.AppendLine(line);
                            }

                            line = await reader.ReadLineAsync();
                            if (line.Contains("Time") && !avoidUpdateDateTime)
                            {
                                builder.AppendLine(string.Format("    Time={0:HH:mm:ss};", DateTime.Now)); // Add "Time..."
                            }
                            else
                            {
                                builder.AppendLine(line);
                            }

                            line = await reader.ReadLineAsync();
                            if (line.Contains("Modified"))
                            {
                                builder.AppendLine(line); // Add "Modified..."
                                line = await reader.ReadLineAsync();
                            }

                            if (line.Contains("Version List"))
                            {
                                // Check if the user defined tag is already in the "Version List"
                                if (!string.IsNullOrWhiteSpace(version) && line.IndexOf(version, StringComparison.OrdinalIgnoreCase) == -1)
                                {
                                    line = line.Replace(";", ",");
                                    line = line + version + ";";
                                }

                                string temp = line.Substring(17);
                                temp = temp.Substring(0, temp.Length - 1); // Remove "    Version List=...;"

                                bool avoidInsert = false;

                                switch (navVersion)
                                {
                                    case 0: // NAV 2013 and below
                                        {
                                            if (temp.Length > 80)
                                            {
                                                MainWindow.Current.StatusProgressBar.IsIndeterminate = true; // Update status

                                                RequestDialog dialog = new RequestDialog(temp, 80);
                                                if (dialog.ShowDialog() == true)
                                                {
                                                    builder.AppendLine("    Version List=" + dialog.VersionList + ";");
                                                    avoidInsert = true;
                                                }
                                            }
                                            break;
                                        }
                                    default: // NAV 2015 and above
                                        {
                                            if (temp.Length > 250)
                                            {
                                                MainWindow.Current.StatusProgressBar.IsIndeterminate = true; // Update status

                                                RequestDialog dialog = new RequestDialog(temp, 250);
                                                if (dialog.ShowDialog() == true)
                                                {
                                                    builder.AppendLine("    Version List=" + dialog.VersionList + ";");
                                                    avoidInsert = true;
                                                }
                                            }
                                            break;
                                        }
                                }
                                MainWindow.Current.StatusProgressBar.IsIndeterminate = false; // Update status

                                if (!avoidInsert) { builder.AppendLine(line); }
                            }
                        }
                        else
                        {
                            builder.AppendLine(line);
                        }
                    }

                    MainWindow.Current.StatusProgressBar.Value += step; // Update status
                }

                using (FileStream stream = new FileStream(file.FilePath, FileMode.Truncate, FileAccess.Write))
                using (StreamWriter writer = new StreamWriter(stream, Encoding.GetEncoding(1252)))
                {
                    MainWindow.Current.StatusTextBlock.Text = "Saving " + file.FileName + " changes..."; // Update status
                    await writer.WriteAsync(builder.ToString());
                }
            }
        }

        #endregion
    }
}