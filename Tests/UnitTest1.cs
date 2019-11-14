using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrecipitationDataHandling;


namespace UnitTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestParseEntry()
        {
            /**
             * Raw data takes the form:
             * [Long=-180.00, 180.00] [Lati= -90.00,  90.00] [Grid X,Y= 720, 360]
             * [Boxes=   67420] [Years=1991-2000] [Multi=    0.1000] [Missing=-999]
             *              */

            var resA = Functions.ParseEntry_Float("Long=-180.00, 180.00", "Long=");
            Assert.AreEqual((-180.0, 180.0), resA);

            var resB = Functions.ParseEntry_Float("Lati= -90.00,  90.00", "Lati=");
            Assert.AreEqual((-90.0, 90.0), resB);

            var resC = Functions.ParseEntry_Int("Grid X,Y= 720, 360", "Grid X,Y=");
            Assert.AreEqual((720, 360), resC);

            int resD = int.Parse("Boxes=   67420".Replace("Boxes=", ""));
            Assert.AreEqual(67420, resD);

            var resE = Functions.ParseEntry_Int("Years=1991-2000", "Years=");
            Assert.AreEqual((1991, 2000), resE);

            float resF = float.Parse("Multi=    0.1000".Replace("Multi=", ""));
            bool floatingPointCompareOkay = Math.Abs(resF - 0.1) < 0.00001;
            Assert.IsTrue(floatingPointCompareOkay);

            int resG = int.Parse("Missing=-999".Replace("Missing=", ""));
            Assert.AreEqual(-999, resG);
        }

        [TestMethod]
        public void TestSplitLineIntoValues()
        {
            string testLine = " 3020 2820 3040 2880 1740 1360  980 \t 990 1410 1770 2580 2630 \n";

            List<int> expected = new List<int>() { 3020, 2820, 3040, 2880, 1740, 1360, 980, 990, 1410, 1770, 2580, 2630 };
            List<int> result = Functions.SplitLineIntoValues(testLine);

            Assert.IsTrue(Enumerable.SequenceEqual(expected, result));
        }

        [TestMethod]
        public void TestFileHandlerCreateData()
        {
            FileHandler handler = new FileHandler(@"G:\C#\JBA Test\jba-software-code-challenge-data-transformation\cru-ts-2-10.1991-2000-cutdown.pre");

            if (handler.FileExists)
            {
                handler.ErrorHandling = ErrorHandlingEnum.Bypass;

                handler.CreateDataPoints();

                var saveresult = handler.SaveData();


                var result = handler.GetDataPoints();
            }
        }

        [TestMethod]
        public void TestFileHandler_DataToExcel()
        {
            FileHandler handler = new FileHandler(@"G:\C#\JBA Test\jba-software-code-challenge-data-transformation\cru-ts-2-10.1991-2000-cutdown.pre");

            if (handler.FileExists)
            {
                handler.ErrorHandling = ErrorHandlingEnum.Bypass;

                handler.CreateDataPoints();

                string savedSpreadsheetLocation = handler.ToExcelSheet("precipitation_data");

                bool fileExists = File.Exists(savedSpreadsheetLocation);

                Assert.IsTrue(fileExists);
            }
        }
    }


}
