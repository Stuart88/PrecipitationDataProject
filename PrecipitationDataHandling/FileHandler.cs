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
        /// Bypass processing. Add note in FailedLines list
        /// </summary>
        Bypass
    }

    public class FileHandler
    {
        #region Fields

        public PrecipiationFileData FileData = new PrecipiationFileData();

        private int CurrentLineNumber = 0;
        private List<ErrorLine> ErrorLines = new List<ErrorLine>();
        private string FilePath;

        #endregion Fields

        #region Constructors

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

        #endregion Constructors

        #region Enums

        private enum ProcessingLineNumber
        {
            TitleLine,
            LineTwo,
            CRU,
            LongLatGrid,
            BoxesYearsMulti,
            GridRefLines
        }

        #endregion Enums

        #region Properties

        public int DataCount { get { return FileData.DataPoints.Count; } }
        public int ErrorCount { get { return ErrorLines.Count; } }

        /// <summary>
        /// Action to take when program encounters an issue in one of the precipitation data lines
        /// </summary>
        public ErrorHandlingEnum ErrorHandling { get; set; }

        public bool FileExists { get { return File.Exists(FilePath); } }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Attempts to create data points from input file
        /// </summary>
        /// <returns></returns>
        public (bool ok, string resultMessage) CreateDataPoints()
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
            catch (FileHandlerException e_fh)
            {
                return (false, string.Format("Error parsing data, please check Line {0}\n\nMessage:\n{1}", CurrentLineNumber, e_fh.Message));
            }
            catch (Exception e)
            {
                return (false, string.Format("Error parsing data, please check Line {0}\n\nMessage:\n{1}", CurrentLineNumber, e.Message));
            }
        }

        /// <summary>
        /// Returns single string of all error data, formatted for console output
        /// </summary>
        /// <returns></returns>
        public string GetErrorLinesData()
        {
            string returnString = "";

            foreach (var e in ErrorLines)
            {
                returnString += string.Format("Line: {0}\nReason: {1}\nRaw Data: {2}\n\n", e.LineNumber, e.Reason, e.Line);
            }

            return returnString;
        }

        /// <summary>
        /// Read top lines of file to find title and other data.
        /// </summary>
        /// <returns></returns>
        public (bool ok, string message) ParseBasicFileData()
        {
            try
            {
                CurrentLineNumber = 0;
                _ = PreliminaryProcess(File.ReadAllLines(FilePath));

                return (true, "");
            }
            catch (FileHandlerException e_fh)
            {
                return (false, string.Format("Error parsing file info, please check Line {0}\n\nMessage:\n{1}", CurrentLineNumber, e_fh.Message));
            }
            catch (Exception e)
            {
                return (false, string.Format("Error parsing file info, please check Line {0}\n\nMessage:\n {1}", CurrentLineNumber, e.Message));
            }
        }

        public async Task<(int totalSaved, int missed, bool ok, string message)> SaveData()
        {
            try
            {
                DbContext.TruncateDataPointsTable();    // Do this to avoid re-saving all the data each time SaveData() is called.
                                                        //This is obviously unrealistic, but it's good enough for what this application is supposed to do.
                int savedAmount = await DbContext.BulkInsertDataPoints(FileData.DataPoints);

                return (savedAmount, FileData.DataPoints.Count - savedAmount, true, "");
            }
            catch (Exception e)
            {
                return (0, 0, false, e.Message);
            }
        }

        public void SetInputFilePath(string filePath)
        {
            FilePath = filePath;
        }

       

        /// <summary>
        /// Returns extracted grid ref data
        /// </summary>
        /// <returns></returns>
        private List<DataPoint> GetDataPoints()
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
                        ProcessLine_LongLatGridLine(line);
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
                    throw new FileHandlerException(string.Format("\nError on line {0}\nReason: {1}\nRaw Data:\n\"{2}\"\n\nProgram terminated\n\nUse the 'Bypass errors' setting to skip data lines that cannot be parsed \n\n", CurrentLineNumber, message, line));
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
            string[] entries = SplitLine(line);
            string[] required = new string[4] { "Boxes=", "Years=", "Multi=", "Missing=" };

            if (entries.Length == 4 && entries.All(x => required.Any(r => x.StartsWith(r)))) //all entries must start with one of the required labels
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
        private void ProcessLine_LongLatGridLine(string line)
        {
            string[] entries = SplitLine(line);
            string[] required = new string[3] { "Long=", "Lati=", "Grid X,Y=" };

            if (entries.Length == 3 && entries.All(x => required.Any(r => x.StartsWith(r)))) //all entries must start with one of the required labels
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
        /// <returns></returns>
        private string[] SplitLine(string line)
        {
            return Regex.Matches(line, @"(?<=\[).*?(?=\])").ToArray();
        }

        #endregion Methods
    }

    internal class ErrorLine
    {
        #region Constructors

        public ErrorLine(int lineNum, string line, string reason = "")
        {
            LineNumber = lineNum;
            Line = line;
            Reason = reason;
        }

        #endregion Constructors

        #region Properties

        public string Line { get; set; }
        public int LineNumber { get; set; }
        public string Reason { get; set; }

        #endregion Properties
    }

    [Serializable]
    public class FileHandlerException : Exception
    {
        #region Constructors

        public FileHandlerException()
        {
        }

        public FileHandlerException(string message) : base(message)
        {
        }

        public FileHandlerException(string message, Exception inner) : base(message, inner)
        {
        }

        protected FileHandlerException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

        #endregion Constructors
    }
}