﻿using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace PrecipitationDataHandling
{
    public static class Functions
    {
        #region Methods

        /// <summary>
        /// <para>Extracts and parses numerical data into Tuple(float,float)</para>
        /// <para>e.g. "Long=-180.00, 180.00"</para>
        /// <para>becomes (-180.0, 180.0)</para>
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="removeText"></param>
        /// <returns></returns>
        public static (float, float) ParseEntry_Float(string entry, string removeText)
        {
            return entry.Replace(removeText, "")
                .Split(',')
                .Where(s => !string.IsNullOrEmpty(s))
                .Select(val => float.Parse(val))
                .ToArray()
                .ToTuple();
        }

        /// <summary>
        /// <para>Extracts and parses numerical data into Tuple(int,int)</para>
        /// <para>e.g. "Grid X,Y= 720, 360"</para>
        /// <para>becomes (720, 360)</para>
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="removeText"></param>
        /// <returns></returns>
        public static (int, int) ParseEntry_Int(string entry, string removeText)
        {
            if (removeText == "Years=")         //all entries have numbers separated by comma, apart from 'Years' entry which is of the form "1999-2000"
                entry = entry.Replace('-', ',');

            return entry.Replace(removeText, "")
                .Split(',')
                .Where(s => !string.IsNullOrEmpty(s))
                .Select(val => int.Parse(val))
                .ToArray()
                .ToTuple();
        }

        /// <summary>
        /// Takes a given line of precipitation data values and converts them into an array of ints
        /// </summary>
        /// <param name="line">e.g. 3020 2820 3040 2880 1740 1360  980  990 1410 1770 2580 2630</param>
        /// <returns>int[] {3020, 2820, 3040, 2880, 1740, 1360,  980,  990, 1410, 1770, 2580, 2630}</returns>
        public static List<int> SplitLineIntoValues(string line)
        {
            return line.Replace("\t", " ").Replace("\n", " ") // replace tab spacing and newlines with whitespace
                .Split(' ')
                .Where(s => !string.IsNullOrEmpty(s))
                .Select(s => int.Parse(s))
                .ToList();
        }

        public static string[] ToArray(this MatchCollection matches)
        {
            string[] result = new string[matches.Count];

            for (int i = 0; i < matches.Count; i++)
            {
                result[i] = matches[i].Value;
            }

            return result;
        }

        public static string ToOutputString(this (float, float) tuple)
        {
            return string.Format("[{0}, {1}]", tuple.Item1, tuple.Item2);
        }

        public static string ToOutputString(this (int, int) tuple)
        {
            return string.Format("[{0}, {1}]", tuple.Item1, tuple.Item2);
        }

        public static (float, float) ToTuple(this float[] data)
        {
            if (data.Length != 2)
                throw new FileHandlerException("ToTuple() Error - Input data does not contain enough entries");
            
            return (data[0], data[1]);
        }

        public static (int, int) ToTuple(this int[] data)
        {
            if (data.Length != 2)
                throw new FileHandlerException("ToTuple() Error - Input data does not contain enough entries");

            return (data[0], data[1]);
        }

        #endregion Methods
    }
}