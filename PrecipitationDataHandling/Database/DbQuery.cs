using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PrecipitationDataHandling.Database;
using SQLite;

namespace PrecipitationDataHandling.Database
{
    public static class DbQuery
    {
        private static SQLiteConnection db = new SQLiteConnection("PrecipitationDB.db");

        public static List<DataPoint> GetDataPoints()
        {
            return db.Table<DataPoint>().ToList();
        }
        public static List<DataPoint> GetDataPoints(DateTime startDate, DateTime endDate)
        {
            return db.Table<DataPoint>().Where(p => p.Date >= startDate && p.Date <= endDate).ToList();
        }

    }
}
