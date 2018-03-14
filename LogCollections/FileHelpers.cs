using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LogCollections
{
    internal static class FileHelpers
    {
        internal static IEnumerable<string> GetAllFileNamesForLog(string folder, string logName)
        {
            return Directory.EnumerateFiles(folder, $"{logName}-*");
        }

        internal static int GetCurrentLogNumber(string folder, string logName, long maxFileSize)
        {
            var fileNames = GetAllFileNamesForLog(folder, logName);
            int max = 0;
            foreach (string fileName in fileNames)
            {
                max = Math.Max(GetNumber(fileName), max);
            }

            return max;
        }

        internal static int GetNumber(string fileName)
        {
            string nb = fileName.Split('-').LastOrDefault();
            if (int.TryParse(nb, out int current))
            {
                return current;
            }

            return -1;
        }

        internal static string GetFileName(string folder, string logName, int number = -1)
        {
            if (number >= 0)
                return Path.Combine(folder, $"{logName}-{number}");
            else
                return Path.Combine(folder, $"{logName}-current");
        }        
    }
}
