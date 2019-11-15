using SQLite;
using System;

namespace PrecipitationDataHandling
{
    public class DataPoint
    {
        #region Constructors

        public DataPoint()
        {
        }

        public DataPoint(DataPoint data)
        {
            this.Xref = data.Xref;
            this.Xref = data.Yref;
            this.Date = data.Date;
            this.Value = data.Value;
        }

        public DataPoint(int xRef, int yRef, DateTime date, int value)
        {
            this.Xref = xRef;
            this.Yref = yRef;
            this.Date = date;
            this.Value = value;
        }

        #endregion Constructors

        #region Properties

        [NotNull]
        public DateTime Date { get; set; }

        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }

        [NotNull]
        public int Value { get; set; }

        [NotNull]
        public int Xref { get; set; }

        [NotNull]
        public int Yref { get; set; }

        #endregion Properties
    }
}