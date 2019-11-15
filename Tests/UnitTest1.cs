using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrecipitationDataHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UnitTests
{
    [TestClass]
    public class UnitTest1
    {
        #region Methods

        [TestMethod]
        public void TestFileHandler_CreateData()
        {
            FileHandler handler = new FileHandler(@"G:\C#\JBA Test\jba-software-code-challenge-data-transformation\cru-ts-2-10.1991-2000-cutdown.pre");

            if (handler.FileExists)
            {
                handler.ErrorHandling = ErrorHandlingEnum.Bypass;

                handler.CreateDataPoints();

                var saveresult = handler.CreateDataPoints();

                Assert.IsTrue(saveresult.ok);
                Assert.IsTrue(handler.DataCount > 0);
            }
        }

        [TestMethod]
        public void TestFileHandler_FileInputError()
        {
            FileHandler handler = new FileHandler(@"G:\C#\JBA Test\non-existent-file.pre");

            var result = handler.CreateDataPoints();

            Assert.AreEqual((false, "File not found!"), result);
        }

        [TestMethod]
        public async Task TestFileHandler_SaveData()
        {
            FileHandler handler = new FileHandler(@"G:\C#\JBA Test\jba-software-code-challenge-data-transformation\cru-ts-2-10.1991-2000-cutdown.pre");

            if (handler.FileExists)
            {
                handler.ErrorHandling = ErrorHandlingEnum.Bypass;

                handler.CreateDataPoints();

                var saveresult = await handler.SaveData();

                System.Diagnostics.Debug.WriteLine(handler.GetErrorLinesData());

                Assert.IsTrue(saveresult.ok);
                Assert.IsTrue(saveresult.totalSaved > 0);
                Assert.IsTrue(saveresult.missed == 0);


            }
        }

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


        #endregion Methods
    }
}