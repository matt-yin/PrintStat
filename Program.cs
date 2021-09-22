﻿using System;
using System.Collections.Generic;
using System.IO;

// 4. Implement the data display

namespace PrintStat
{
    class Program
    {
        static void Main(string[] args)
        {
            string printingLogDir = @"Z:\3. Patients for Printing\4. Printing Log\2021";

            Statistics stat = new Statistics();
            string completeDir = @"Z:\3. Patients for Printing\3. Completed\2021";
            string printingDir = @"Z:\3. Patients for Printing\2. Printing";
            DateTime day = new DateTime(2021, 9, 20);

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
