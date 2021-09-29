using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Data;
using System.Linq;



namespace PrintStat
{
    public class Statistics
    {
        public DateTime[] Date { get; set; }
        private DataTable jobTable;
        private DataTable caseTable;
        private List<string> messages = new List<string>();
        private Dictionary<string, int> subTotalbyCustomer = new Dictionary<string, int>();
        private int total = 0;
        public CustomerMapping customerDict = new CustomerMapping();

        public Statistics()
        {
            // Initiate the job table
            jobTable = new DataTable();
            jobTable.Columns.Add("Name", typeof(string));
            jobTable.Columns.Add("CreationDate", typeof(DateTime));
            jobTable.Columns.Add("Extension", typeof(string));

            // Initiate the case table
            caseTable = new DataTable();
            caseTable.Columns.Add("ID", typeof(string));
            caseTable.Columns.Add("FullName", typeof(string));
            caseTable.Columns.Add("Customer", typeof(string));
            caseTable.Columns.Add("Size", typeof(int));
            caseTable.Columns.Add("Path", typeof(string));
        }

        // Load the job files into a data table in memory given the specified job directory
        // File name, creation date and extension are retrieved
        // Files without an extension of 3dprint, print, rpproj are discarded
        public void LoadJobFiles(string path, DateTime[] dates)
        {
            jobTable.Clear();

            string[] jobFiles = Directory.GetFiles(path);
            foreach (var jobFile in jobFiles)
            {
                FileInfo info = new FileInfo(jobFile);

                var row = jobTable.NewRow();
                row["Name"] = Path.GetFileName(jobFile);
                row["CreationDate"] = info.CreationTime.Date;
                row["Extension"] = Path.GetExtension(jobFile);
                jobTable.Rows.Add(row);

            }

            // // Filter file extension
            string extensionFilter = "Extension NOT IN ('.3dprint', '.print', '.rpproj')";
            RemoveDataRows(jobTable, extensionFilter);

            string dateFilter = "";
            // Single date mode
            if (dates.Length == 1)
            {
                dateFilter = $"CreationDate <> '{dates[0].ToString("d")}'";
            }
            // Date range mode
            else
            {
                dateFilter = $"CreationDate < '{dates[0].ToString("d")}' OR CreationDate > '{dates[1].ToString("d")}'";
            }
            RemoveDataRows(jobTable, dateFilter);
        }

        private void RemoveDataRows(DataTable table, string filter)
        {
            var rows = table.Select(filter);
            foreach (var row in rows)
            {
                row.Delete();
            }
            table.AcceptChanges();
        }

        public void GetCaseInfo(string[] caseFolders)
        {
            List<string> caseList = new List<string>();
            foreach (var folder in caseFolders)
            {
                caseList.AddRange(Directory.GetDirectories(folder).ToList<string>());
            }
            var allCases = caseList.ToArray();

            foreach (DataRow job in jobTable.Rows)
            {
                List<string> cases = ParseJobName(job["Name"].ToString());

                foreach (var c in cases)
                {
                    string casePath = "";
                    string caseFullName = "";
                    int size = 0;

                    try
                    {
                        casePath = GetCasePath(c, allCases);
                        DirectoryInfo dirInfo = new DirectoryInfo(casePath);
                        caseFullName = dirInfo.Name;
                    }
                    catch (System.Exception e)
                    {
                        messages.Add($"Case {c} from Job {job["Name".ToString()]}:\nUnable to get the path: {e.Message}");
                        continue;
                    }

                    string cust = GetCustomer(caseFullName, customerDict.CustomerDict);

                    try
                    {
                        size = GetCaseSize(casePath);
                    }
                    catch (System.Exception e)
                    {
                        messages.Add($"Case {c} from Job {job["Name".ToString()]}:\nUnable to get the case size: {e.Message}");
                        continue;
                    }

                    DataRow row = caseTable.NewRow();
                    row["ID"] = c;
                    row["FullName"] = caseFullName;
                    row["Customer"] = cust;
                    row["Size"] = size;
                    row["Path"] = casePath;
                    caseTable.Rows.Add(row);
                }
            }

            // Remove duplicate rows
            caseTable = caseTable.AsEnumerable().GroupBy(x => x.Field<string>("ID")).Select(y => y.First()).CopyToDataTable();
        }

        public void Sort()
        {
            // Get distint values of customers
            var customerList = (from r in caseTable.AsEnumerable() select r["Customer"]).Distinct();
            var customerArray = customerList.ToArray();
            foreach (var cust in customerList)
            {
                string customer = cust.ToString();
                var subTotal = caseTable.AsEnumerable().Where(y => y.Field<string>("Customer") == customer).Sum(x => x.Field<int>("Size"));
                subTotalbyCustomer.Add(customer, subTotal);
            }

            total = caseTable.AsEnumerable().Sum(x => x.Field<int>("Size"));
        }

        private void PrintMessages()
        {
            if (messages.Count > 0)
            {
                System.Console.WriteLine("Note");
                System.Console.WriteLine("========================================");

                foreach (var msg in messages)
                {
                    System.Console.WriteLine($"* {msg}");
                }
            }
        }

        public void Print()
        {
            PrintJobTableHeader();
            PrintJobTableRows();

            PrintCaseTableHeader();
            PrintCaseTableRows();

            PrintStatisticsHeader();
            PrintStatistics();

            PrintMessages();
            System.Console.WriteLine("\n");

        }

        private void PrintStatisticsHeader()
        {
            System.Console.WriteLine("");
            System.Console.WriteLine($"Sorted by Customer");
            System.Console.WriteLine("========================================");

        }
        public void PrintStatistics()
        {
            foreach (var item in subTotalbyCustomer)
            {
                System.Console.WriteLine(string.Format("{0,-25}{1,-10}", item.Key, item.Value));
            }

            System.Console.WriteLine("----------------------------------------");
            System.Console.WriteLine($"The total count of arches is {total}");
        }

        private void PrintJobTableHeader()
        {
            System.Console.WriteLine("");
            System.Console.WriteLine($"Job List");
            System.Console.WriteLine("========================================");
        }

        private void PrintJobTableRows()
        {
            System.Console.WriteLine(string.Format("{0,-15}{1}", "Printed On", "Job Name"));

            foreach (DataRow row in jobTable.Rows)
            {
                string dt = ((DateTime)row["CreationDate"]).Date.ToString("d");
                System.Console.WriteLine(string.Format("{0,-15}{1}", dt, row["Name"]));
            }
        }

        private void PrintCaseTableHeader()
        {
            System.Console.WriteLine("");
            System.Console.WriteLine($"Case List");
            System.Console.WriteLine("========================================");
        }

        private void PrintCaseTableRows()
        {
            System.Console.WriteLine(string.Format("{0,-60}{1,-25}{2,-10}", "Case ID and Patient Name", "Customer", "Size"));

            foreach (DataRow row in caseTable.Rows)
            {
                string fullName = row["FullName"].ToString(); ;
                string customer = row["Customer"].ToString();
                string size = row["Size"].ToString();
                System.Console.WriteLine(string.Format("{0,-60}{1,-25}{2,-10}", fullName, customer, size));
            }
        }

        // Extract case numbers from the job filename which match the pattern DD-DDDD
        private List<string> ParseJobName(string job)
        {
            string casePattern = "\\d{2}-\\d{4}";
            Regex rgx = new Regex(casePattern);
            var matches = rgx.Matches(job);
            var list = matches.Cast<Match>().Select(Match => Match.Value).ToList();
            return list;
        }



        private string GetCasePath(string ID, string[] collection)
        {
            List<string> result = new List<string>();
            foreach (var c in collection)
            {
                DirectoryInfo dirInfo = new DirectoryInfo(c);
                var caseFolderName = dirInfo.Name;

                if (caseFolderName.StartsWith(ID))
                {
                    result.Add(c);
                }
            }

            if (result.Count == 1)
            {
                return result[0];
            }
            else if (result.Count == 0)
            {
                throw new DirectoryNotFoundException($"Case {ID}: directory NOT found!");
            }
            else
            {
                throw new ArgumentOutOfRangeException($"Case {ID}: multiple diretories found!");
            }
        }

        public string GetCustomer(string name, Dictionary<string, string> dict)
        {
            string pattern = @"_[a-zA-Z0-9]+";
            Regex rgx = new Regex(pattern, RegexOptions.IgnoreCase);
            //string result = rgx.Match(name).Groups[1].Value;
            foreach (Match match in rgx.Matches(name))
            {
                if (dict.ContainsKey(match.Value.Substring(1)))
                {
                    return dict[match.Value.Substring(1)];
                }
            }
            return "SureCure";
        }

        private int GetCaseSize(string path)
        {
            string stlDir = $"{path}//Treatment//3D Printing Files";
            string[] stlFiles = Directory.GetFiles(stlDir, "*.stl");

            if (stlFiles.Length == 0)
            {
                throw new FileNotFoundException("No Stl files found for case {caseFullName}");
            }

            return stlFiles.Length;
        }

    }
}