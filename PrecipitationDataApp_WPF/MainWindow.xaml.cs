using PrecipitationDataHandling;
using PrecipitationDataHandling.Database;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace PrecipitationDataApp_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Constructors

        public MainWindow()
        {
            InitializeComponent();

            Loaded += async (s, e) =>
            {
                ViewDatabaseFileText.Visibility = await DbQuery.HasData()
                    ? Visibility.Visible
                    : Visibility.Hidden;

                PrintHelpString();
            };
        }

        #endregion Constructors

        #region Properties

        public FileHandler FileHandler { get; set; } = new FileHandler(ErrorHandlingEnum.FailOnError);
        public string SelectedFilePath { get; set; }

        #endregion Properties

        #region Methods

        private void ClearConsoleBtn_Click(object sender, RoutedEventArgs e)
        {
            OutputPanel.Children.Clear();

            ConsoleLog("Output:\n");
        }

        private void ConsoleLog(string message)
        {
            OutputPanel.Children.Add(new TextBlock { Text = string.Format("{0}\n", message), TextWrapping = TextWrapping.Wrap });

            ConsoleScroller.ScrollToVerticalOffset(OutputPanel.ActualHeight); //Keep scrollviewer scrolled to bottom (where new text appears)
        }

        private void DataViewerBtn_Click(object sender, RoutedEventArgs e)
        {
            DataViewer dataViewer = new DataViewer();
            dataViewer.Show();
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
                //FileNameText.Text = SelectedFilePath;
                ConsoleLog("Selected file:\n" + SelectedFilePath);

                FileHandler.SetInputFilePath(SelectedFilePath);

                var preliminaryCheck = FileHandler.ParseBasicFileData();

                if (preliminaryCheck.ok)
                {
                    TitleText.Text = FileHandler.FileData.FileTitle;
                    LongText.Text = FileHandler.FileData.LongRange.ToOutputString();
                    LatText.Text = FileHandler.FileData.LatRange.ToOutputString();
                    GridXyText.Text = FileHandler.FileData.Grid.ToOutputString();
                    BoxesText.Text = FileHandler.FileData.Boxes.ToString();
                    YearsText.Text = FileHandler.FileData.Years.ToOutputString();
                    MultiText.Text = FileHandler.FileData.Multi.ToString();
                    MissingText.Text = FileHandler.FileData.Missing.ToString();

                    //FileDataTextGrid.Visibility = Visibility.Visible;
                    ProcessBtn.IsEnabled = true;

                    ConsoleLog("File looks okay! Ready to process.");
                }
                else
                {
                    _ = MessageBox.Show(preliminaryCheck.message, "File Error!");
                    ConsoleLog(preliminaryCheck.message);
                    //FileDataTextGrid.Visibility = Visibility.Hidden;
                    ProcessBtn.Visibility = Visibility.Hidden;
                }
            }
        }

        private void PrintHelpBtn_Click(object sender, RoutedEventArgs e)
        {
            PrintHelpString();
        }

        private void PrintHelpString()
        {
            string helpString = "\nPrecpitation Data File Parser\n\n" +
               "Instructions:\n" +
               "1. Select file\n" +
               "2. Process Data\n" +
               "3. Insert into Database\n" +
               "4. View in Data Viewer\n\n" +
               "Error Handling Options:\n\n" +
               " - Fail on error: Program will terminate with an error message if any data cannot be parsed.\n\n" +
               " - Bypass errors: Skip lines that cannot be parsed. (Errors will be logged)\n";

            ConsoleLog(helpString);
        }

        private void ProcessBtn_Click(object sender, RoutedEventArgs e)
        {
            ConsoleLog("\n\nProcessing...");
            var processingResult = FileHandler.CreateDataPoints();

            if (processingResult.ok)
            {
                ConsoleLog("Processing OK!");
                ConsoleLog(string.Format("Processed {0} entries", FileHandler.DataCount));

                if (FileHandler.ErrorCount > 0)
                {
                    ConsoleLog(string.Format("Found {0} errors. Click \"Print errors in console\" to view.", FileHandler.ErrorCount));
                    ErrorsLinkText.Visibility = Visibility.Visible;
                }

                SaveBtn.IsEnabled = true;
            }
            else
            {
                ConsoleLog(processingResult.resultMessage);

                _ = MessageBox.Show(processingResult.resultMessage, "File Error!");
            }
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            switch (((RadioButton)sender).Tag)
            {
                case "Fail":
                    FileHandler.ErrorHandling = ErrorHandlingEnum.FailOnError;
                    break;

                case "Bypass":
                    FileHandler.ErrorHandling = ErrorHandlingEnum.Bypass;
                    break;
            }
        }

        private async void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            SaveBtn.IsEnabled = false;
            DataViewerBtn.IsEnabled = false;

            ConsoleLog("\n\nSaving...");

            var saveResult = await FileHandler.SaveData();

            if (saveResult.ok)
            {
                //DataViewerBtn.IsEnabled = true;
                ViewDatabaseFileText.Visibility = Visibility.Visible;
                ConsoleLog("Save OK! ");
                ConsoleLog(string.Format("Saved {0} entries", saveResult.totalSaved));
            }
            else
            {
                ConsoleLog("Error! ");
                ConsoleLog(saveResult.message);
            }
            SaveBtn.IsEnabled = true;
            DataViewerBtn.IsEnabled = true;
        }

        private void ShowErrorsBtn_Click(object sender, RoutedEventArgs e)
        {
            ConsoleLog(FileHandler.GetErrorLinesData());
        }

        private void ViewDatabaseFileBtn_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog saveDialog = new Microsoft.Win32.SaveFileDialog();
            saveDialog.FileName = "PrecipitationDB";
            saveDialog.DefaultExt = ".db";
            saveDialog.Filter = "Database fle (.db)|*.db";
            //saveDialog.CheckFileExists = false;
            //saveDialog.CheckFileExists = false;
            //saveDialog.CreatePrompt = true;

            if (saveDialog.ShowDialog() == true)
            {
                File.Copy(System.IO.Path.Combine(Environment.CurrentDirectory, "PrecipitationDB.db"), saveDialog.FileName, true);
            }
        }

        #endregion Methods
    }
}