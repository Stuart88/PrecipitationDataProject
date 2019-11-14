using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrecipitationDataHandling
{
    public class PrecipiationFileData
    {
        public string FileTitle { get; set; }
        public (float Min, float Max) LongRange { get; set; }
        public (float Min, float Max) LatRange { get; set; }
        public (int X, int Y) Grid { get; set; }
        public int Boxes { get; set; }
        public (int Min, int Max) Years { get; set; }
        public float Multi { get; set; }
        public int Missing { get; set; }
        public List<DataPoint> DataPoints { get; set; } = new List<DataPoint>();
    }
}
