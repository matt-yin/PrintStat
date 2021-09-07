using System;

namespace PrintStat
{
    public class CaseInfo
    {
        public string CaseNumber { get; set; }
        public string CaseFullName { get; set; }
        public string CaseYear { get; set; }
        public int CaseSize { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
        public DateTime PrintDate { get; set; }

        public CaseInfo(string number, string year)
        {
            CaseNumber = number;
            CaseYear = year;
        }
    }



}