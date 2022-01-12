using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;

// 1. Refactor the Count class

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
            System.Console.WriteLine("Welcome to 3DPStats v1.0.1");

            DateTime singleDate;
            DateTime startDate;
            DateTime endDate;

            while (true)
            {
                System.Console.WriteLine("========================================");
                System.Console.WriteLine("Enter a single date (YYYYMMDD) or a date range (YYYYMMDD-YYYYMMDD)\nor press X to exit:");
                string userInput = Console.ReadLine();

                if (String.Equals(userInput, "X", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                if (!userInput.Contains('-'))
                {
                    if (DateTime.TryParseExact(userInput, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out singleDate))
                    {
                        DateTime[] dateArray = { singleDate };
                        Run(dateArray, patientDriveName);
                    }
                    else
                    {
                        System.Console.WriteLine("Invalid input! Please try again or press X to exit");
                    }
                }
                else
                {
                    var sections = userInput.Split('-');
                    if (DateTime.TryParseExact(sections[0], "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out startDate)
                        && DateTime.TryParseExact(sections[1], "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out endDate))
                    {
                        DateTime[] dateArray = { startDate, endDate };

                        Run(dateArray, patientDriveName);
                    }
                    else
                    {
                        System.Console.WriteLine("Invalid input! Please try again or press X to exit");

                    }
                }
            }
        }

        static void Run(DateTime[] dates, string letter)
        {
            string printingLogDir = $"{letter}:\\3. Patients for Printing\\4. Printing Log\\2022";
            string completeDir = $"{letter}:\\3. Patients for Printing\\3. Completed\\2022";
            string printingDir = $"{letter}:\\3. Patients for Printing\\2. Printing";
            

            Statistics stat = new Statistics();
            stat.Date = dates;

            try
            {
                stat.LoadJobFiles(printingLogDir, dates);
            }
            catch (System.Exception e)
            {
                System.Console.WriteLine($"Loading job files failed: {e.Message}");
                return;
            }

            try
            {
                string[] searchDirectories = { completeDir, printingDir };
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
