using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Threading;

namespace PrintStat
{
    class Program
    {
        static void Main(string[] args)
        {
            string patientDriveLabel = "Patients";
            string patientDriveName = "";
            try
            {
                patientDriveName = GetDriveNamebyLabel(patientDriveLabel).Substring(0, 1).ToUpper();
            }
            catch (DriveNotFoundException e)
            {
                System.Console.WriteLine(e.Message);
                return;
            }

            System.Console.WriteLine("========================================");
            System.Console.WriteLine("Welcome to 3DPStats v1.1.0");

            // Loop for getting manual input
            // while (true)
            // {
            //     System.Console.WriteLine("========================================");
            //     System.Console.WriteLine("Enter a single date (YYYYMMDD)\nor press X to exit:");
            //     string userInput = Console.ReadLine();

            //     if (String.Equals(userInput, "X", StringComparison.OrdinalIgnoreCase))
            //     {
            //         return;
            //     }

            //     if (DateTime.TryParseExact(userInput, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
            //     {
            //         Run(date, patientDriveName);
            //     }
            //     else
            //     {
            //         System.Console.WriteLine("Invalid input! Please try again or press X to exit");
            //     }
            // }

            // Automatic running mode


            bool hasRun = false;
            while (true)
            {
                DateTime now = DateTime.Now;
                // if (now.Hour != 14)
                // {
                //     Thread.Sleep(5000);
                //     continue;
                // }

                if (hasRun == false)
                {
                    Run(now, patientDriveName);
                    hasRun = true;
                    continue;
                }

                System.Console.WriteLine("Program sleeping...");
                Thread.Sleep(5000);
            }

        }

        static void Run(DateTime date, string letter)
        {
            // Directory for job files
            string printingLog2021Dir = $"{letter}:\\3. Patients for Printing\\4. Printing Log\\2021";
            string printingLog2022Dir = $"{letter}:\\3. Patients for Printing\\4. Printing Log\\2022";

            // Directory for case folders
            string printingDir = $"{letter}:\\3. Patients for Printing\\2. Printing";
            string complete2020Dir = $"{letter}:\\3. Patients for Printing\\3. Completed\\2020";
            string complete2021Dir = $"{letter}:\\3. Patients for Printing\\3. Completed\\2021";
            string complete2022Dir = $"{letter}:\\3. Patients for Printing\\3. Completed\\2022";


            Statistics stat = new Statistics() { Date = date };

            try
            {
                string[] jobDirectories = { printingLog2021Dir, printingLog2022Dir };
                stat.LoadJobFiles(jobDirectories);
            }
            catch (System.Exception e)
            {
                System.Console.WriteLine($"Loading job files failed: {e.Message}");
                return;
            }

            try
            {
                string[] searchDirectories = { printingDir, complete2020Dir, complete2021Dir, complete2022Dir };
                stat.GetCaseInfo(searchDirectories);
            }
            catch (System.Exception e)
            {
                System.Console.WriteLine($"Getting case information failed: {e.Message}");
                return;
            }

            //stat.Sort();
            // stat.Print();
            stat.Export();
        }

        private static string GetDriveNamebyLabel(string label)
        {
            var allDrives = DriveInfo.GetDrives();
            foreach (var drive in allDrives)
            {
                if (String.Equals(drive.VolumeLabel, label, StringComparison.InvariantCultureIgnoreCase))
                {
                    return drive.Name;
                }
            }

            throw new DriveNotFoundException($"Unable to locate {label.ToUpper()} drive!");
        }
    }
}
