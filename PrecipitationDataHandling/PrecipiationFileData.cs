using System.Collections.Generic;

namespace PrecipitationDataHandling
{
    public class PrecipiationFileData
    {
        #region Properties

        public int Boxes { get; set; }
        public List<DataPoint> DataPoints { get; set; } = new List<DataPoint>();
        public string FileTitle { get; set; }
        public (int X, int Y) Grid { get; set; }
        public (float Min, float Max) LatRange { get; set; }
        public (float Min, float Max) LongRange { get; set; }
        public int Missing { get; set; }
        public float Multi { get; set; }
        public (int Min, int Max) Years { get; set; }

        #endregion Properties
    }
}