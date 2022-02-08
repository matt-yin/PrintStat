using System.Data;
using System.Linq;
using System.Collections.Generic;

namespace PrintStat
{
    public static class Analyzer
    {
        public static int GetJobCount(DataTable jobTable)
        {
            return jobTable.Rows.Count;
        }

        public static int GetCaseCount(DataTable caseTable)
        {
            return caseTable.Rows.Count;
        }

        public static int GetArchCount(DataTable caseTable)
        {
            return (int)caseTable.AsEnumerable().Sum(x => x.Field<int>("Size"));
        }

        public static Dictionary<string, int> GetArchCountByCustomer(DataTable caseTable)
        {
            Dictionary<string, int> result = new Dictionary<string, int>();
            var custList = caseTable.AsEnumerable().Select(x => x["Customer"]).Distinct();
            foreach (var cust in custList)
            {
                string customerName = cust.ToString();
                int subTotal = caseTable.AsEnumerable().Where(x => x.Field<string>("Customer") == customerName).Sum(x => x.Field<int>("Size"));
                result.Add(customerName, subTotal);
            }

            return result;
        }

        public static Dictionary<string, int> GetCaseCountByCustomer(DataTable caseTable)
        {
            Dictionary<string, int> result = new Dictionary<string, int>();
            var custList = caseTable.AsEnumerable().Select(x => x["Customer"]).Distinct();
            foreach (var cust in custList)
            {
                string customerName = cust.ToString();
                int subTotal = caseTable.AsEnumerable().Where(x => x.Field<string>("Customer") == customerName).Count();
                result.Add(customerName, subTotal);
            }

            return result;
        }
    }
}