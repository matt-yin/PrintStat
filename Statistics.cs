using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Data;
using System.Linq;

// 1. Implement exception handling for accessing directories and files
// 2. Delete duplicte cases which has multiple appearances in the job files
// 3. Implement the data display

namespace PrintStat
{
    public class Statistics
    {
        private DataTable jobTable;
        private DataTable caseTable;

        public List<CaseInfo> cases = new List<CaseInfo>();

        private CustomerDict customerMap = new CustomerDict();

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

            // Filter file extension
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

        public void GetCaseInfo(string caseFolder)
        {
            var allCases = Directory.GetDirectories(caseFolder);

            foreach (DataRow job in jobTable.Rows)
            {
                System.Console.WriteLine(job["Name"].ToString());
                List<string> cases = ParseJobName(job["Name"].ToString());

                foreach (var c in cases)
                {
                    string caseFullName = GetCaseFullName(c, allCases);
                    string cust = GetCustomer(caseFullName);
                    int size = GetCaseSize(caseFullName, caseFolder);

                    DataRow row = caseTable.NewRow();
                    row["ID"] = c;
                    row["FullName"] = caseFullName;
                    row["Customer"] = cust;
                    row["Size"] = size;
                    caseTable.Rows.Add(row);
                }
            }
        }

        public void PrintCaseTableRows()
        {
            foreach (DataRow row in caseTable.Rows)
            {
                string id = row["ID"].ToString();
                string fullName = row["FullName"].ToString();;
                string customer = row["Customer"].ToString();
                string size = row["Size"].ToString();
                System.Console.WriteLine($"{id}\t{customer}\t{size}");
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

        public void PrintJobTableRows()
        {
            foreach (DataRow row in jobTable.Rows)
            {
                String name = row["Name"].ToString();
                String creationDate = ((DateTime)row["CreationDate"]).ToString("MM/dd/yyyy");
                String extension = row["Extension"].ToString();
                System.Console.WriteLine($"{name}\t{creationDate}\t{extension}");
            }
        }

        public string GetCaseFullName(string ID, string[] collection)
        {
            List<string> result = new List<string>();
            foreach (var c in collection)
            {
                DirectoryInfo dirInfo = new DirectoryInfo(c);
                var caseFolderName = dirInfo.Name;

                if (caseFolderName.StartsWith(ID))
                {
                    result.Add(caseFolderName);
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

        private string GetCustomer(string name)
        {
            if (!name.Contains('_'))
            {
                return "SureCure";
            }
            else
            {
                string pattern = @"^[^_]*_([\S]+)\s";
                Regex rgx = new Regex(pattern, RegexOptions.IgnoreCase);
                return rgx.Match(name).Groups[1].Value;
            }
        }

        // public void GetCaseInfo()
        // {
        //     cases.Clear();

        //     foreach (var job in jobs)
        //     {
        //         var jobCases = ParseJob(job);
        //         cases.AddRange(jobCases);
        //     }
        // }

        // Parse the job name to get the list of cases, and retrieve the case year, case size and customer
        // Case number must be of pattern XX-XXXX and case year 20XX where X denotes a numeric digit
        // private List<CaseInfo> ParseJob(JobInfo job)
        // {
        //     string[] sections = job.Name.Split('-');
        //     string caseString = Regex.Replace(sections[0], @"s", "");
        //     string[] cases = caseString.Split('_');
        //     List<CaseInfo> result = new List<CaseInfo>();

        //     Regex pattern = new Regex(@"^\d{2}-\d{4}");
        //     foreach (var item in cases)
        //     {
        //         // Check if the case number mathces the pattern xx-xxxx
        //         if (pattern.IsMatch(item))
        //         {
        //             // Initialize the case with case number and case year
        //             CaseInfo c = new CaseInfo(item, $"20{item.Substring(0, 2)}");
        //             c.CaseSize = GetCaseSize(c.CaseNumber, caseRootDir);
        //             c.CustomerCode = getCustomerCode(c.CaseFullName);
        //             c.PrintDate = job.CreateDate;
        //             result.Add(c);
        //         }
        //     }

        //     return result;
        // }

        public int GetCaseSize(string caseFullName, string caseRootDir)
        {
            string stlDir = $"{caseRootDir}//{caseFullName}//Treatment//3D Printing Files";
            string[] stlFiles = Directory.GetFiles(stlDir, "*.stl");

            return stlFiles.Length;
        }

        // private string GetCaseFullName(string caseNumber, string caseRootDir)
        // {
        //     string[] casePool = Directory.GetDirectories(caseRootDir);
        //     foreach (var folder in casePool)
        //     {
        //         string folderName = Path.GetDirectoryName(folder);
        //         if (folderName.StartsWith(caseNumber))
        //         {
        //             return folderName;
        //         }
        //     }

        //     return "";
        // }

        // private string getCustomerCode(string caseFullName)
        // {
        //     string[] sections = Regex.Replace(caseFullName, @"s", "").Split('_');

        //     if (sections.Length == 2)
        //     {
        //         return sections[^1];
        //     }

        //     return "";

        // }

        // private string GetCustomerName(string customerCode)
        // {
        //     return customerMap.GetName(customerCode);
        // }

        // public void CreateDataTable()
        // {
        //     dt = new DataTable();
        //     dt.Columns.Add("Date", typeof(DateTime));
        //     dt.Columns.Add("Size", typeof(int));
        //     dt.Columns.Add("Customer", typeof(string));

        //     var row = dt.NewRow();
        //     row["Date"] = new DateTime(2021,9,1);
        //     row["Size"] = 32;
        //     row["Customer"] = "Uniform Teeth";
        //     dt.Rows.Add(row);

        //     row = dt.NewRow();
        //     row["Date"] = new DateTime(2021,9,4);
        //     row["Size"] = 18;
        //     row["Customer"] = "Uniform Teeth";
        //     dt.Rows.Add(row);

        //     row = dt.NewRow();
        //     row["Date"] = new DateTime(2021,9,4);
        //     row["Size"] = 44;
        //     row["Customer"] = "SureCure";
        //     dt.Rows.Add(row);

        //     // foreach (DataRow item in dt.Rows)
        //     // {
        //     //     System.Console.WriteLine($"{item["Date"]}, {item["Size"]}, {item["Customer"]}");
        //     // }

        //     // Sort the rows based on the value in Date in descendent order
        //     DataView dv = dt.DefaultView;
        //     dv.Sort = "Date desc";
        //     DataTable result = dv.ToTable();



        //     // Select distinct values in Date
        //     DateTime[] dateArray = dt.DefaultView.ToTable(true, "Date").AsEnumerable().Select(r=>r.Field<DateTime>("Date")).ToArray();
        //     foreach (var item in dateArray)
        //     {
        //         System.Console.WriteLine(item);
        //     }

        //     DateTime d = new DataTime(2021,9,4);
        //     DataRows[] res = dt.Select($"Date = {d}").ToArray();

        //     foreach (DataRow item in res)
        //     {
        //         Console.WriteLine($"{item["Date"]}, {item["Size"]}, {item["Customer"]}");
        //     }

        // }

        // public void SortTest()
        // {

        // }

        // public void SortByDate()
        // {
        //     foreach (var item in cases)
        //     {
        //         if (dateMap.ContainsKey(item.PrintDate))
        //         {
        //             dateMap[item.PrintDate].Add(item);
        //         }

        //         dateMap.Add(item.PrintDate, new List<CaseInfo>());
        //     }
        // }

    }
}