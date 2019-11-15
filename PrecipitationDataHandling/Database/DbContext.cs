using SQLite;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PrecipitationDataHandling.Database
{
    internal static class DbContext
    {
        #region Fields

        private static SQLiteAsyncConnection db = new SQLiteAsyncConnection("PrecipitationDB.db");

        #endregion Fields

        #region Methods

        public static async Task<int> BulkInsertDataPoints(List<DataPoint> dataPoints)
        {
            return await db.InsertAllAsync(dataPoints, typeof(DataPoint));
        }

        public static void DeleteDataPoint(DataPoint data)
        {
            if (data.ID == 0)
                throw new Exception("Entry has no ID!");

            _ = db.DeleteAsync(data);
        }

        public static void InitialiseDataBase()
        {
            _ = db.CreateTableAsync<DataPoint>();
        }

        public static async Task<int> InsertDataPoint(DataPoint data)
        {
            DataPoint adding = new DataPoint(data); // ensure new data point (no ID)

            return await db.InsertAsync(adding);
        }

        public static void TruncateDataPointsTable()
        {
            _ = db.DeleteAllAsync<DataPoint>();
        }

        #endregion Methods
    }
}