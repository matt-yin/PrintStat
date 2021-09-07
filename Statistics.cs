using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Data;
using System.Linq;

// Catch exceptions when browsing directories and getting files
// Implement logging function for exception handling
// Initiate and sort the case pool for efficient searching
// Try using EnumerateFiles which may be more efficient
// Refactor the data table and its operations to a new class

namespace PrintStat
{
    public class Statistics
    {
        public List<JobInfo> jobs = new List<JobInfo>();
        public List<CaseInfo> cases = new List<CaseInfo>();
        public SortedDictionary<DateTime, List<CaseInfo>> dateMap = new SortedDictionary<DateTime, List<CaseInfo>>();

        private string caseRootDir = @"Z:\3. Patients for Printing\3. Completed";
        private CustomerDict customerMap = new CustomerDict();

        private DataTable dt;

        // Load the job files in the printing log to the internal job list. Job name and creation date is retrieved.
        public void LoadJobFiles(string path)
        {
            jobs.Clear();

            string[] jobFiles = Directory.GetFiles(path);
            foreach (var jobFile in jobFiles)
            {
                var job = new JobInfo();
                FileInfo info = new FileInfo(jobFile);
                job.CreateDate = info.CreationTime;
                job.Name = Path.GetFileName(jobFile);
                jobs.Add(job);
            }
        }

        public void GetCaseInfo()
        {
            cases.Clear();

            foreach (var job in jobs)
            {
                var jobCases = ParseJob(job);
                cases.AddRange(jobCases);
            }
        }

        // Parse the job name to get the list of cases, and retrieve the case year, case size and customer
        // Case number must be of pattern XX-XXXX and case year 20XX where X denotes a numeric digit
        private List<CaseInfo> ParseJob(JobInfo job)
        {
            string[] sections = job.Name.Split('-');
            string caseString = Regex.Replace(sections[0], @"s",""); 
            string[] cases = caseString.Split('_');
            List<CaseInfo> result = new List<CaseInfo>();

            Regex pattern = new Regex(@"^\d{2}-\d{4}");
            foreach (var item in cases)
            {
                // Check if the case number mathces the pattern xx-xxxx
                if (pattern.IsMatch(item))
                {
                    // Initialize the case with case number and case year
                    CaseInfo c = new CaseInfo(item, $"20{item.Substring(0,2)}");
                    c.CaseSize = GetCaseSize(c.CaseNumber, caseRootDir);
                    c.CustomerCode = getCustomerCode(c.CaseFullName);
                    c.PrintDate = job.CreateDate;
                    result.Add(c);
                }
            }

            return result;
        }

        private int GetCaseSize(string caseNumber, string caseRootDir)
        {
            string fullName = GetCaseFullName(caseNumber, caseRootDir);
            string stlDir = $"{caseRootDir}//{fullName}//Treatment//3D Printing Files";
            string[] stlFiles = Directory.GetFiles(stlDir, "*.stl");

            return stlFiles.Length;
        }

        private string GetCaseFullName(string caseNumber, string caseRootDir)
        {
            string[] casePool = Directory.GetDirectories(caseRootDir);
            foreach (var folder in casePool)
            {
                string folderName = Path.GetDirectoryName(folder);
                if (folderName.StartsWith(caseNumber))
                {
                    return folderName;
                }
            }

            return "";
        }

        private string getCustomerCode(string caseFullName)
        {
            string[] sections = Regex.Replace(caseFullName, @"s","").Split('_');

            if (sections.Length == 2)
            {
                return sections[^1];
            }

            return "";
            
        }

        private string GetCustomerName(string customerCode)
        {
            return customerMap.GetName(customerCode);
        }

        public void CreateDataTable()
        {
            dt = new DataTable();
            dt.Columns.Add("Date", typeof(DateTime));
            dt.Columns.Add("Size", typeof(int));
            dt.Columns.Add("Customer", typeof(string));

            var row = dt.NewRow();
            row["Date"] = new DateTime(2021,9,1);
            row["Size"] = 32;
            row["Customer"] = "Uniform Teeth";
            dt.Rows.Add(row);

            row = dt.NewRow();
            row["Date"] = new DateTime(2021,9,4);
            row["Size"] = 18;
            row["Customer"] = "Uniform Teeth";
            dt.Rows.Add(row);

            row = dt.NewRow();
            row["Date"] = new DateTime(2021,9,4);
            row["Size"] = 44;
            row["Customer"] = "SureCure";
            dt.Rows.Add(row);

            // foreach (DataRow item in dt.Rows)
            // {
            //     System.Console.WriteLine($"{item["Date"]}, {item["Size"]}, {item["Customer"]}");
            // }

            // Sort the rows based on the value in Date in descendent order
            DataView dv = dt.DefaultView;
            dv.Sort = "Date desc";
            DataTable result = dv.ToTable();



            // Select distinct values in Date
            DateTime[] dateArray = dt.DefaultView.ToTable(true, "Date").AsEnumerable().Select(r=>r.Field<DateTime>("Date")).ToArray();
            foreach (var item in dateArray)
            {
                System.Console.WriteLine(item);
            }

            DateTime d = new DataTime(2021,9,4);
            DataRows[] res = dt.Select($"Date = {d}").ToArray();

            foreach (DataRow item in res)
            {
                Console.WriteLine($"{item["Date"]}, {item["Size"]}, {item["Customer"]}");
            }

        }

        public void SortTest()
        {

        }

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