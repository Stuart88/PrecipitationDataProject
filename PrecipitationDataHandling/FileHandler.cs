using PrecipitationDataHandling.Database;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PrecipitationDataHandling
{
    public enum ErrorHandlingEnum
    {
        /// <summary>
        /// Throw exception, terminate program.
        /// </summary>
        FailOnError,

        /// <summary>
        /// Bypass processing. Add note in FailedLines
        /// </summary>
        Bypass
    }

    public class FileHandler
    {
        #region Private Fields

        private int CurrentLineNumber = 0;
        private List<ErrorLine> ErrorLines = new List<ErrorLine>();
        public PrecipiationFileData FileData = new PrecipiationFileData();
        private string FilePath;

        #endregion Private Fields

        #region Public Constructors
        public FileHandler(string filePath, ErrorHandlingEnum errorHandling = ErrorHandlingEnum.Bypass)
        {
            FilePath = filePath;
            ErrorHandling = errorHandling;
            DbContext.InitialiseDataBase();
        }
        public FileHandler(ErrorHandlingEnum errorHandling = ErrorHandlingEnum.Bypass)
        {
            ErrorHandling = errorHandling;
            DbContext.InitialiseDataBase();
        }
        public int ErrorCount {get{ return ErrorLines.Count; } }

        public string GetErrorLinesData()
        {
            string returnString = "";

            foreach(var e in ErrorLines)
            {
                returnString += string.Format("Line: {0}\nReason: {1}\nRaw Data: {2}\n\n", e.LineNumber, e.Reason, e.Line);
            }

            return returnString;
        }

        #endregion Public Constructors

        public void  SetInputFilePath(string filePath)
        {
            FilePath = filePath;
        }

        /// <summary>
        /// Saves precipitation data to Excel file
        /// </summary>
        /// <param name="outputFilename">Desired file name. Does NOT require file extension</param>
        /// <returns>Saved file path</returns>
        public string ToExcelSheet(string outputFilename)
        {
            List<DataPointExport> dataPoints_excel = FileData.DataPoints.Take(1000).Select(s => new DataPointExport(s)).ToList();

            return Exporter.ToExcelFile(Exporter.ConvertToDataTable(dataPoints_excel), outputFilename);
        }

        #region Private Enums

        private enum ProcessingLineNumber
        {
            TitleLine,
            LineTwo,
            CRU,
            LongLatGrid,
            BoxesYearsMulti,
            GridRefLines
        }

        #endregion Private Enums

        #region Public Properties

        /// <summary>
        /// Action to take when program encounters an issue in one of the precipitation data lines
        /// </summary>
        public ErrorHandlingEnum ErrorHandling { get; set; }

        public bool FileExists { get { return File.Exists(FilePath); } }

        #endregion Public Properties

        #region Public Methods
        /// <summary>
        /// Read top lines of file to find title and other data.
        /// </summary>
        /// <returns></returns>
        public (bool ok, string message) ParseBasicFileData()
        {
            try
            {
                _ = PreliminaryProcess(File.ReadAllLines(FilePath));

                return (true, "");
            }
            catch (Exception e)
            {
                return (false, e.Message);
            }
        }


        public (bool ok, string message) CreateDataPoints()
        {
            if (!FileExists)
                return (false, "File not found!");

            FileData.DataPoints.Clear();
            ErrorLines.Clear();

            try
            {
                string[] file = File.ReadAllLines(FilePath);

                CurrentLineNumber = 0;
                List<(string line, bool isGridRedHeader)> gridRefLines = PreliminaryProcess(file);

                int xRef = 0;
                int yRef = 0;
                int currentYear = FileData.Years.Min;

                foreach (var g in gridRefLines)
                {
                    CurrentLineNumber++;

                    if (g.isGridRedHeader)
                    {
                        (xRef, yRef) = Functions.ParseEntry_Int(g.line, "Grid-ref=");
                        //reset back to min year.
                        currentYear = FileData.Years.Min;
                        continue;
                    }
                    else
                    {
                        FileData.DataPoints.AddRange(ProcessGridRefValues(g.line, xRef, yRef, currentYear));
                        currentYear++;
                    }
                }

                return (true, "");
            }
            catch (Exception e)
            {
                return (false, e.Message);
            }
        }

        public List<DataPoint> GetDataPoints()
        {
            if (FileData.DataPoints.Count > 0)
            {
                return FileData.DataPoints;
            }
            else
            {
                throw new FileHandlerException("No data! First run CreateDataPoints() to process file.");
            }
        }

        public async Task<(int saved, int missed, bool ok, string message)> SaveData()
        {

            try
            {
                DbContext.TruncateDataPointsTable();    // Do this to avoid re-saving all the data each time SaveData() is called. 
                                                        //This is obviously unrealistic, but it's good enough for what this application is supposed to do.
                int savedAmount = await DbContext.BulkInsertDataPoints(FileData.DataPoints);

                return (savedAmount, FileData.DataPoints.Count - savedAmount, true, "");
            }
            catch(Exception e)
            {
                return (0, 0, false, e.Message);
            }
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Processes all lines of input file. Extracts header details ands stores as PrecipiationFileData properties,
        /// then returns list of remaining "Grid-ref" lines for further processing.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        private List<(string line, bool isGridRedHeader)> PreliminaryProcess(string[] file, bool headerLinesOnly = false)
        {
            ProcessingLineNumber currentLine = ProcessingLineNumber.TitleLine;
            List<(string line, bool isGridRedHeader)> returnList = new List<(string line, bool isGridRedHeader)>();

            foreach (string line in file)    // Process by line.
            {
               

                switch (currentLine)
                {
                    case ProcessingLineNumber.TitleLine:
                        CurrentLineNumber++;
                        FileData.FileTitle = line;
                        currentLine = ProcessingLineNumber.LineTwo;
                        break;

                    case ProcessingLineNumber.LineTwo:
                        CurrentLineNumber++;
                        currentLine = ProcessingLineNumber.CRU;
                        break;

                    case ProcessingLineNumber.CRU:
                        CurrentLineNumber++;
                        currentLine = ProcessingLineNumber.LongLatGrid;
                        break;

                    case ProcessingLineNumber.LongLatGrid:
                        CurrentLineNumber++;
                        ProcessLine_LangLatGridLine(line);
                        currentLine = ProcessingLineNumber.BoxesYearsMulti;
                        break;

                    case ProcessingLineNumber.BoxesYearsMulti:
                        CurrentLineNumber++;
                        ProcessLine_BoxesYearsMulti(line);
                        currentLine = ProcessingLineNumber.GridRefLines;
                        break;

                    case ProcessingLineNumber.GridRefLines:
                        if (headerLinesOnly)//terminate early
                        {
                            return returnList;
                        }
                        else
                        {
                            returnList.Add((line, line.Contains("Grid-ref=")));
                        }
                        break;
                }
            }

            return returnList;
        }

        private void ProcessGridRefLineError(string line, string message)
        {
            switch (this.ErrorHandling)
            {
                case ErrorHandlingEnum.FailOnError:
                    throw new FileHandlerException(string.Format("\n\nError on line {0}:\nReason: {1}\nRaw Data:{2}\nProgram terminated\n\n", CurrentLineNumber, message, line));
                case ErrorHandlingEnum.Bypass:
                    ErrorLines.Add(new ErrorLine(this.CurrentLineNumber, line, message));
                    break;
            }
        }

        /// <summary>
        /// Processes GridRef data value line. Splits up entries into List, cycles over list and adds each value as complete DataPoint entry
        /// using given xRef, yRef and currentYear
        /// </summary>
        /// <param name="xRef"></param>
        /// <param name="yRef"></param>
        /// <param name="gridRefValues"></param>
        /// <param name="currentYear"></param>
        /// <returns></returns>
        private IEnumerable<DataPoint> ProcessGridRefValues(string rawLine, int xRef, int yRef, int currentYear)
        {
            List<DataPoint> points = new List<DataPoint>();

            //Check line looks workable
            if (rawLine.Any(c => char.IsLetter(c)))
            {
                ProcessGridRefLineError(rawLine, "Line contains data that cannot be handled");

                //Will continue if ErrorHandling is set to bypass. End here (returns empty list)
                return points;
            }

            //split into list of ints, and process if possible
            List<int> gridRefValues = Functions.SplitLineIntoValues(rawLine);
            if (gridRefValues.Count < 12)
            {
                ProcessGridRefLineError(rawLine, "Not enough data points in line (12 required)");
            }
            else
            {
                //loop over months and process. Add value for each data point from Jan - Dec
                for (int i = 1; i <= 12; i++)
                {
                    points.Add(new DataPoint(xRef, yRef, new DateTime(currentYear, i, 1), gridRefValues[i - 1]));
                }
            }

            return points;
        }

        /// <summary>
        /// Parses and stores data from 'Boxes / Years / Multi' line of file
        /// </summary>
        /// <param name="line"></param>
        private void ProcessLine_BoxesYearsMulti(string line)
        {
            if (SplitLine(line, out string[] entries))
            {
                FileData.Boxes = int.Parse(entries[0].Replace("Boxes=", ""));
                FileData.Years = Functions.ParseEntry_Int(entries[1], "Years=");
                FileData.Multi = float.Parse(entries[2].Replace("Multi=", ""));
                FileData.Missing = int.Parse(entries[3].Replace("Missing=", ""));
            }
            else
            {
                throw new FileHandlerException("Error splitting line: " + line);
            }
        }

        /// <summary>
        /// Parses and stores data from 'Long / Lat / GridXY' line of file
        /// </summary>
        /// <param name="line"></param>
        private void ProcessLine_LangLatGridLine(string line)
        {
            //string[] entries = SplitLine(line); // result e.g. --->  { "Long=-180.00, 180.00", "Lati= -90.00,  90.00", "Grid X,Y= 720, 360" }

            if (SplitLine(line, out string[] entries))
            {
                FileData.LongRange = Functions.ParseEntry_Float(entries[0], "Long=");
                FileData.LatRange = Functions.ParseEntry_Float(entries[1], "Lati=");
                FileData.Grid = Functions.ParseEntry_Int(entries[2], "Grid X,Y=");
            }
            else
            {
                throw new FileHandlerException("Error splitting line: " + line);
            }
        }

        /// <summary>
        /// <para>Converts single line of raw data into separate array entries for further processing.</para>
        /// <para>e.g. "[Long=-180.00, 180.00] [Lati= -90.00,  90.00] [Grid X,Y= 720, 360]" </para>
        /// <para>becomes { "Long=-180.00, 180.00", "Lati= -90.00,  90.00", "Grid X,Y= 720, 360" }</para>
        /// </summary>
        /// <param name="line"></param>
        /// <param name="entries"></param>
        /// <returns></returns>
        private bool SplitLine(string line, out string[] entries)
        {
            try
            {
                entries = Regex.Split(string.Format("] {0} [", line), @"\] \[", RegexOptions.IgnoreCase)
                .Where(x => !string.IsNullOrEmpty(x))   //Remove empty values left by Regex split
                .ToArray();

                return true;
            }
            catch (Exception e)
            {
                entries = new string[] { };
                return false;
            }
        }

        #endregion Private Methods

        #region Public Classes

        [Serializable]
        public class FileHandlerException : Exception
        {
            #region Public Constructors

            public FileHandlerException()
            {
            }

            public FileHandlerException(string message) : base(message)
            {
            }

            public FileHandlerException(string message, Exception inner) : base(message, inner)
            {
            }

            #endregion Public Constructors

            #region Protected Constructors

            protected FileHandlerException(
              System.Runtime.Serialization.SerializationInfo info,
              System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

            #endregion Protected Constructors
        }

        #endregion Public Classes
    }

    internal class ErrorLine
    {
        #region Public Constructors

        public ErrorLine(int lineNum, string line, string reason = "")
        {
            LineNumber = lineNum;
            Line = line;
            Reason = reason;
        }

        #endregion Public Constructors

        #region Public Properties

        public string Line { get; set; }
        public int LineNumber { get; set; }
        public string Reason { get; set; }

        #endregion Public Properties
    }
}