using System;
using System.Collections.Generic;
using System.IO;

namespace PrintStat
{
    class Program
    {
        static void Main(string[] args)
        {
            // Get the job file names and store in a list

            // for every job in the list, get the create date and parse the case number


            //string PrintingLogDir = @"Z:\3. Patients for Printing\4. Printing Log\2021";

            Statistics stat = new Statistics();
            string path = @"E:\MattWorkspace\Test";
            stat.LoadJobFiles(path);
            stat.PrintJobTableRows();
            //stat.GetCaseInfo();
            //stat.CreateDataTable();

        }
    }
}
