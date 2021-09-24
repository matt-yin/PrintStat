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
            System.Console.WriteLine("Welcome to 3DPStats v1.0.0");

            DateTime date;
            while (true)
            {
                System.Console.WriteLine("========================================");

                System.Console.WriteLine("Enter a date (YYYYMMDD) or press X to exit:");
                string userInput = Console.ReadLine();

                if (String.Equals(userInput, "X", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                if (DateTime.TryParseExact(userInput, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                {
                    System.Console.WriteLine("Enter the letter for the Patients drive:");
                    var key = Console.ReadKey();
                    System.Console.WriteLine("\n");

                    if (key.Key == ConsoleKey.Escape)
                    {
                        return;
                    }
                    else
                    {
                        Run(date, key.Key.ToString());
                    }
                }
                else
                {
                    System.Console.WriteLine("Invalid input! Please try again or press X to exit");
                }
            }
        }

        static void Run(DateTime date, string letter)
        {
            string printingLogDir = $"{letter}:\\3. Patients for Printing\\4. Printing Log\\2021";
            string completeDir = $"{letter}:\\3. Patients for Printing\\3. Completed\\2021";
            string printingDir = $"{letter}:\\3. Patients for Printing\\2. Printing";

            Statistics stat = new Statistics();
            stat.Date = date;

            try
            {
                stat.LoadJobFiles(printingLogDir, date);
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
    }
}
