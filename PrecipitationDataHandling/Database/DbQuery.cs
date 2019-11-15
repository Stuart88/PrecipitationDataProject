using SQLite;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PrecipitationDataHandling.Database
{
    public static class DbQuery
    {
        #region Fields

        private static SQLiteAsyncConnection db = new SQLiteAsyncConnection("PrecipitationDB.db");

        #endregion Fields

        #region Methods

        public static async Task<List<DataPoint>> GetDataPoints()
        {
            return await db.Table<DataPoint>().ToListAsync();
        }

        public static async Task<List<DataPoint>> GetDataPoints(DateTime startDate, DateTime endDate)
        {
            return await db.Table<DataPoint>()
                .Where(p => p.Date >= startDate && p.Date <= endDate)
                .ToListAsync();
        }

        public static async Task<bool> HasData()
        {
            return await db.Table<DataPoint>().CountAsync() > 0;
        }

        #endregion Methods
    }
}