using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Microsoft.WindowsAPICodePack.Dialogs;
using ImageMagick;
using System.Text.RegularExpressions;
using Microsoft.Extensions.FileSystemGlobbing;
using System.Windows.Interop;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using System.IO;

namespace ImageConverter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool isStop = false;

        public MainWindow()
        {
            var extList = new List<string>();
            InitializeComponent();

            foreach (MagickFormat Value in Enum.GetValues(typeof(MagickFormat)))
            {
                string name = Enum.GetName(typeof(MagickFormat), Value).ToLower();
                extList.Add(name);
            }
            
            extList.RemoveAt(0);

            InputImageExtComboBox.ItemsSource = extList;
            InputImageExtComboBox.SelectedIndex = 0;
            OutputImageExtComboBox.ItemsSource = extList;
            OutputImageExtComboBox.SelectedIndex = 0;
        }

        private void folderSelectButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            
            if(dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                targetFolderTextbox.Text = dialog.FileName;
            }

        }

        private async void Convert_Click(object sender, RoutedEventArgs e)
        {
            Matcher matcher = new ();
            matcher.AddInclude( "**/*." + InputImageExtComboBox.SelectedItem.ToString());
            IEnumerable<string> matchingFiles = matcher.GetResultsInFullPath(targetFolderTextbox.Text + "/");
            progressBar.Value = 0;
            progressBar.Minimum = 0;
            progressBar.Maximum = matchingFiles.Count();
            changeEnableStatus( false );

            foreach (var file in matchingFiles)
            {
                var ext = OutputImageExtComboBox.SelectedItem.ToString();
                var isDeleteChecked = isDeleteOriginalCheckBox.IsChecked;
                if(isStop)
                {
                    isStop = false;
                    MessageBox.Show("Interrupted.");
                    break;
                }
                await Task.Run(() => {
                    try
                    {
                        using (var image = new MagickImage(file))
                        {
                            var destination = Path.ChangeExtension(file, ext);
                            image.Write(destination);
                            if (isDeleteChecked == true)
                            {
                                File.Delete(file);
                            }
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                progressBar.Value += 1;
                            });
                        }
                    }catch(Exception e)
                    {
                        MessageBox.Show(e.Message);
                    }
                });
            }

            MessageBox.Show("Finished.");
            progressBar.Value = 0;

            changeEnableStatus(true);
        }

        private void changeEnableStatus(bool status)
        {
            Convert.IsEnabled = status;
            InputImageExtComboBox.IsEnabled = status;
            OutputImageExtComboBox.IsEnabled = status;
            targetFolderTextbox.IsEnabled = status;
            folderSelectButton.IsEnabled = status;
            isDeleteOriginalCheckBox.IsEnabled = status;
            stopButton.IsEnabled = status == true ? false : true ;
        }

        private void stopButton_Click(object sender, RoutedEventArgs e)
        {
            isStop = true;
        }
    }
}
