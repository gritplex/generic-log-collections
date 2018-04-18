using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.IO;
using LogCollections;

namespace LogViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            ;
            if (e.Data.GetDataPresent("FileName"))
            {
                string file = (e.Data.GetData("FileName") as string[]).FirstOrDefault();
                var entries = new List<LogEntryView>();
                if (File.Exists(file))
                {
                    var reader = new FileStorage(file, true);
                    foreach (var entry in reader)
                    {
                        entries.Add(entry);
                    }                    
                }
                grid.ItemsSource = entries;
            }
        }
    }
}
