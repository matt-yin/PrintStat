using System;
using System.Collections.Generic;
using System.IO;

// 1. Refactor the main method with argument
// 2. Compile the solution
// 3. Refactor the Count class

namespace PrintStat
{
    class Program
    {
        // Arguemnt list of the main method is: (DateTime d, string pd) or (string pd)
        // where d specifies the date of interest for the statistics, s specifies the letter of the Patient disk
        static void Main(string[] args)
        {
            string printingLogDir = @"Z:\3. Patients for Printing\4. Printing Log\2021";
            string completeDir = @"Z:\3. Patients for Printing\3. Completed\2021";
            string printingDir = @"Z:\3. Patients for Printing\2. Printing";
            DateTime day = new DateTime(2021, 9, 14);

            Statistics stat = new Statistics();
            stat.Date = day;

            try
            {
                stat.LoadJobFiles(printingLogDir, day);
            }
            catch (System.Exception e)
            {
                System.Console.WriteLine($"Loading job files failed: {e.Message}");
                return;
            }

            try
            {
                string[] searchDirectories = {completeDir, printingDir};
                stat.GetCaseInfo(searchDirectories);
            }
            catch (System.Exception e)
            {
                System.Console.WriteLine($"Getting case information failed: {e.Message}");
                return;
            }

            stat.Sort();
            stat.Print();
        }
    }
}
