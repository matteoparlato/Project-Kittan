﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Project_Kittan.Helpers;
using Project_Kittan.ViewModels;

namespace Project_Kittan
{
    /// <summary>
    /// MainWindow class
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow Current;

        /// <summary>
        /// Parameterless constructor of MainWindow class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            Current = this;

            Title = "Project Kittan - " + Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        /// <summary>
        /// Method invoked the window is laid out, rendered, and ready for interaction.
        /// Verify if there are new updates available for Project Kittan.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await UpdateExtensions.Check();
        }

        /// <summary>
        /// Method invoked when the user clicks on Update Remove tag button.
        /// Start tag removal from all text files in working directory.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Button_Click_5(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(TagTextTextBox.Text))
            {
                StatusProgressBar.Value = 0;
                ResponsiveStatusProgressBar.IsIndeterminate = true;

                //await ObjectExtensions.RemoveTag(Files.ToArray(), TagTextTextBox.Text, (bool)CaseSensitiveCheckBox.IsChecked);

                StatusTextBlock.Text = "Done";
                StatusProgressBar.IsIndeterminate = false;
                ResponsiveStatusProgressBar.IsIndeterminate = false;
            }
        }

        /// <summary>
        /// Method invoked when the user clicks on About menu item.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/matteoparlato/Project-Kittan");
        }

        /// <summary>
        /// Method invoked when the user drag and drop a file on left pane.
        /// Add dropped *.txt files to Files collection.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StackPanel_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                ((Workspace)this.DataContext).AddFilesFromDrop(files);
            }
        }

        /// <summary>
        /// Method invoked when the user Copy value from ConflictsListView contex menu.
        /// Copy the right-clicked value to the clipboard.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            string clipboard = item.CommandParameter.ToString().Trim();

            Clipboard.SetText(clipboard);
            StatusTextBlock.Text = clipboard + " copied to clipboard";
        }

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            new SettingsWindow().ShowDialog();
        }
    }
}
