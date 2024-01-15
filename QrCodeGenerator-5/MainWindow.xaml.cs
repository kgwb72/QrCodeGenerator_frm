using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.IO; // For Path
using static System.Net.WebRequestMethods;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Windows.Media;
using iText.Layout;
using System.Windows.Input;
using System.Drawing.Imaging;
using System.Windows.Media.Imaging;
using System.Threading;
using QrCodeGenerator.ViewModels;

namespace QrCodeGenerator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<FileItem> Files { get; set; }
        private MainViewModel _viewModel;
        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new MainViewModel();
            DataContext = _viewModel;

            _viewModel.OnProcessComplete += () => MessageBox.Show("All files processed.");

        }

        private void ListView_DragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;

            // Change the background color of the ListView to indicate a drag is in process
            lvFiles.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(50, 135, 206, 250)); // LightSkyBlue with transparency

            e.Handled = true;
        }

        private void ListView_DragOver(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
            // This ensures the cursor remains a copy cursor, indicating a drop is possible
            e.Effects = DragDropEffects.Copy;
            e.Handled = true;
        }

        private void ListView_DragLeave(object sender, DragEventArgs e)
        {
            lvFiles.Background = System.Windows.Media.Brushes.Transparent;

            e.Handled = true;
        }

        private void ListView_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] droppedFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (string file in droppedFiles)
                {
                    if (Path.GetExtension(file).Equals(".pdf", StringComparison.InvariantCultureIgnoreCase))
                    {
                        _viewModel.Files.Add(new FileItem { FileName = Path.GetFileName(file), FilePath = file });
                    }
                }
            }
            lvFiles.Background = System.Windows.Media.Brushes.Transparent;
        }

        private void LvFiles_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                var listView = sender as ListView;
                var selectedItems = listView.SelectedItems.OfType<FileItem>().ToList();

                if (selectedItems.Count > 0)
                {
                    foreach (var item in selectedItems)
                    {
                        (listView.ItemsSource as ObservableCollection<FileItem>)?.Remove(item);
                    }
                }
            }
        }


    }
}
