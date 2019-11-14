using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace PrecipitationDataHandling.Database
{
    internal static class DbContext
    {
        private static SQLiteConnection db = new SQLiteConnection("PrecipitationDB.db");

        public static void InitialiseDataBase()
        {
            _ = db.CreateTable<DataPoint>();
        }

        public static void DeleteDataPoint(DataPoint data)
        {
            if (data.ID == 0)
                throw new Exception("Entry has no ID!");

            _ = db.Delete(data);
        }
        public static int InsertDataPoint(DataPoint data)
        {
            DataPoint adding = new DataPoint(data);

            return db.Insert(adding);
        }

        public static void TruncateDataPointsTable()
        {
            db.DeleteAll<DataPoint>();
        }

       
        public static int BulkInsertDataPoints(List<DataPoint> dataPoints)
        {
            return db.InsertAll(dataPoints, typeof(DataPoint));
        }
    }
}
