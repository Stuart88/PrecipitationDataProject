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
        private static SQLiteAsyncConnection db = new SQLiteAsyncConnection("PrecipitationDB.db");

        public static async Task<bool> HasData()
        {
            return await db.Table<DataPoint>().CountAsync() > 0;
        }
        public static async Task<List<DataPoint>> GetDataPoints()
        {
            return await db.Table<DataPoint>().ToListAsync();
        }
        public static async Task<List<DataPoint>> GetDataPoints(DateTime startDate, DateTime endDate)
        {
            return await db.Table<DataPoint>().Where(p => p.Date >= startDate && p.Date <= endDate).ToListAsync();
        }

    }
}
