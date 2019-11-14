using PrecipitationDataHandling;
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
using System.Windows.Shapes;
using PrecipitationDataHandling.Database;

namespace PrecipitationDataApp_WPF
{
    /// <summary>
    /// Interaction logic for DataViewer.xaml
    /// </summary>
    public partial class DataViewer : Window
    {
        public List<DataPoint> DataPoints { get; set; }
        public DataViewer()
        {
            DataPoints = DbQuery.GetDataPoints();

            InitializeComponent();

        }
    }
}
