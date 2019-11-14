using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace PrecipitationDataHandling
{
    public class DataPoint
    {
        public DataPoint() { }
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
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        [NotNull]
        public int Xref { get; set; }
        [NotNull]
        public int Yref { get; set; }
        [NotNull]
        public DateTime Date { get; set; }
        [NotNull]
        public int Value { get; set; }
    }
}
