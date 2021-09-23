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
        public DateTime Date { get; set; }
        private DataTable jobTable;
        private DataTable caseTable;
        private List<string> messages = new List<string>();
        private Dictionary<string, int> subTotalbyCustomer = new Dictionary<string, int>();
        private int total = 0;

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
        public void LoadJobFiles(string path, DateTime day)
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

            // Filter creation date
            string dateFilter = $"CreationDate <> '{day.ToString("d")}'";
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

                    string cust = GetCustomer(caseFullName);

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
            if (messages.Length > 0)
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
            PrintJobTableHeader(Date);
            PrintJobTableRows();

            PrintCaseTableHeader(Date);
            PrintCaseTableRows();

            PrintStatisticsHeader(Date);
            PrintStatistics();

            PrintMessages();
        }

        private void PrintStatisticsHeader(DateTime d)
        {
            System.Console.WriteLine("");
            System.Console.WriteLine("Statistics by Customer on {d.Date}");
            System.Console.WriteLine("========================================");

        }
        public void PrintStatistics()
        {
            foreach (var item in subTotalbyCustomer)
            {
                System.Console.WriteLine(string.Format("{0,-10}{1,-10}", item.Key, item.Value));
            }

            System.Console.WriteLine("----------------------------------------");
            System.Console.WriteLine($"The total count of arches is {total}");
        }

        private void PrintJobTableHeader(DateTime d)
        {
            System.Console.WriteLine("");
            System.Console.WriteLine("Jobs printed on {d.Date}");
            System.Console.WriteLine("========================================");
        }

        private void PrintJobTableRows()
        {
            foreach (DataRow row in jobTable.Rows)
            {
                String name = row["Name"].ToString();
                String extension = row["Extension"].ToString();
                System.Console.WriteLine($"{name}{extension}");
            }
        }

        private void PrintCaseTableHeader(DateTime d)
        {
            System.Console.WriteLine("");
            System.Console.WriteLine("Cases printed on {d.Date}");
            System.Console.WriteLine("========================================");
        }

        public void PrintCaseTableRows()
        {
            System.Console.WriteLine(string.Format("{0,-10}{1,-30}{2,-10}{3,-5}", "Case ID", "Full Name", "Customer", "Size"));

            foreach (DataRow row in caseTable.Rows)
            {
                string id = row["ID"].ToString();
                string fullName = row["FullName"].ToString(); ;
                string customer = row["Customer"].ToString();
                string size = row["Size"].ToString();
                System.Console.WriteLine(string.Format("{0,-10}{1,-30}{2,-10}{3,-5}", id, fullName, customer, size));
            }
        }

        // Extract case numbers from the job filename which match the pattern DD-DDDD
        public List<string> ParseJobName(string job)
        {
            string casePattern = "\\d{2}-\\d{4}";
            Regex rgx = new Regex(casePattern);
            var matches = rgx.Matches(job);
            var list = matches.Cast<Match>().Select(Match => Match.Value).ToList();
            return list;
        }



        public string GetCasePath(string ID, string[] collection)
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

        public string GetCustomer(string name)
        {
            if (!name.Contains('_'))
            {
                return "SureCure";
            }
            else
            {
                string pattern = @"^[^_]*_([a-zA-Z0-9]+)";
                Regex rgx = new Regex(pattern, RegexOptions.IgnoreCase);
                string result = rgx.Match(name).Groups[1].Value;
                return (string.Equals(result, "retainer", StringComparison.OrdinalIgnoreCase)) ? "SureCure" : result.ToUpper();
            }
        }

        public int GetCaseSize(string path)
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