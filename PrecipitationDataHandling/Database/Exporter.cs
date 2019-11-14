using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrecipitationDataHandling.Database
{
    internal static class Exporter
    {
        public static DataTable ConvertToDataTable<T>(IList<T> data)

        {
            try
            {
                PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(T));

                DataTable table = new DataTable();

                foreach (PropertyDescriptor prop in properties)

                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);

                foreach (T item in data)

                {
                    DataRow row = table.NewRow();

                    foreach (PropertyDescriptor prop in properties)

                        row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;

                    table.Rows.Add(row);
                }

                return table;
            }
            catch (Exception e)
            {
                return new DataTable();
            }
        }

        public static string ToExcelFile(this DataTable dt, string filename)
        {
            XLWorkbook wb = new XLWorkbook();

            wb.Worksheets.Add(dt, "Sheet 1");

            filename = filename + ".xlsx";

            string savePath = Path.Combine(Environment.CurrentDirectory, filename);

            wb.SaveAs(savePath);

            //var byteArray = File.ReadAllBytes(filename);
            //MemoryStream m = new MemoryStream(byteArray);

            return savePath;
        }
    }

    public class DataPointExport
    {
        public DataPointExport(DataPoint data)
        {
            ID = data.ID.ToString();
            XRef = data.Xref.ToString();
            YRef = data.Yref.ToString();
            Date = data.Date.ToString("dd MMM yyyy");
            Value = data.Value.ToString();
        }
        public string ID { get; set; }
        public string XRef { get; set; }
        public string YRef { get; set; }
        public string Date { get; set; }
        public string Value { get; set; }
    }
}
