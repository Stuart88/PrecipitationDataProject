using PrecipitationDataHandling;
using PrecipitationDataHandling.Database;
using System.Collections.Generic;
using System.Windows;

namespace PrecipitationDataApp_WPF
{
    /// <summary>
    /// Interaction logic for DataViewer.xaml
    /// </summary>
    public partial class DataViewer : Window
    {
        #region Constructors

        public DataViewer()
        {
            Loaded += async (s, e) =>
            {
                DataPoints = await DbQuery.GetDataPoints();

                if (DataPoints.Count == 0)
                {
                    LoadingText.Text = "No data! First process and insert data from file.";
                }
                else
                {
                    LoadingText.Visibility = Visibility.Collapsed;
                    DataListView.Visibility = Visibility.Visible;

                    DataListView.ItemsSource = DataPoints;
                }
            };

            InitializeComponent();
        }

        #endregion Constructors

        #region Properties

        public List<DataPoint> DataPoints { get; set; }

        #endregion Properties
    }
}