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
using System.Windows.Shapes;
using PrecipitationDataHandling;

namespace PrecipitationDataApp_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string SelectedFilePath { get; set; }
        public FileHandler FileHandler { get; set; } = new FileHandler();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void FileSelectBtn_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog fileDialog = new Microsoft.Win32.OpenFileDialog();

            fileDialog.DefaultExt = ".doc";
            fileDialog.Filter = "precipitation (*.pre;)|*.pre;";

            bool? result = fileDialog.ShowDialog();

            if (result == true)
            {
                SelectedFilePath = fileDialog.FileName;
                FileNameText.Text = SelectedFilePath;
                ConsoleLog("Selected file:\n" + SelectedFilePath);

                FileHandler.SetInputFilePath(SelectedFilePath);

                var prelimaryCheckResult = FileHandler.ParseBasicFileData();
                if (prelimaryCheckResult.ok)
                {
                    TitleText.Text = FileHandler.FileData.FileTitle;
                    LongText.Text = FileHandler.FileData.LongRange.ToOutputString();
                    LatText.Text = FileHandler.FileData.LatRange.ToOutputString();
                    GridXyText.Text = FileHandler.FileData.Grid.ToOutputString();
                    BoxesText.Text = FileHandler.FileData.Boxes.ToString();
                    YearsText.Text = FileHandler.FileData.Years.ToOutputString();
                    MultiText.Text = FileHandler.FileData.Multi.ToString();
                    MissingText.Text = FileHandler.FileData.Missing.ToString();

                    FileDataTextGrid.Visibility = Visibility.Visible;
                    ProcessBtn.IsEnabled = true;

                    ConsoleLog("File looks okay! Ready to process.");
                }
                else
                {
                    _ = MessageBox.Show(prelimaryCheckResult.message, "File Error!");
                    ConsoleLog(prelimaryCheckResult.message);
                    FileDataTextGrid.Visibility = Visibility.Hidden;
                    ProcessBtn.Visibility = Visibility.Hidden;
                }
            }
        }

        private void ConsoleLog(string message)
        {
            OutputPanel.Children.Add(new TextBlock { Text = string.Format("{0}\n", message), TextWrapping = TextWrapping.Wrap });

            ConsoleScroller.ScrollToVerticalOffset(OutputPanel.ActualHeight); //Keep scrollviewer scrolled to bottom (where new text appears)
        }

        private void ClearConsoleBtn_Click(object sender, RoutedEventArgs e)
        {
            OutputPanel.Children.Clear();

            ConsoleLog("Output:\n");
        }

        private void ProcessBtn_Click(object sender, RoutedEventArgs e)
        {
            ConsoleLog("\n\nProcessing...");
            var processingResult = FileHandler.CreateDataPoints();

            if (processingResult.ok)
            {
                ConsoleLog("Processing OK!");
                ConsoleLog(string.Format("Processed {0} entries", FileHandler.GetDataPoints().Count));

                if(FileHandler.ErrorCount > 0)
                {
                    ConsoleLog(string.Format("Found {0} errors." , FileHandler.ErrorCount));
                    ConsoleLog(FileHandler.GetErrorLinesData());
                }

                SaveBtn.IsEnabled = true;
            }
            else
            {
                ConsoleLog(processingResult.message);

                _ = MessageBox.Show(processingResult.message, "File Error!");
            }
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            ConsoleLog("\n\nSaving...");

            var saveResult = FileHandler.SaveData();

            if (saveResult.ok)
            {
                //DataViewerBtn.IsEnabled = true;
                ConsoleLog("Save OK! ");
                ConsoleLog(string.Format("Saved {0} entries", saveResult.saved));
            }
            else
            {
                ConsoleLog("Error! ");
                ConsoleLog(saveResult.message);
            }
        }

        private void DataViewerBtn_Click(object sender, RoutedEventArgs e)
        {
            DataViewer dataViewer = new DataViewer();
            dataViewer.Show();
        }
    }
}
