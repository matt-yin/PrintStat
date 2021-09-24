using System;
using System.Collections.Generic;

namespace PrintStat
{
    public class CustomerMapping
    {
        public Dictionary<string, string> CustomerDict { get; set; }
        public CustomerMapping()
        {
            CustomerDict = new Dictionary<string, string>();

            CustomerDict.Add("UT", "Uniform Teeth");
            CustomerDict.Add("ACO", "AlignerCO");
            CustomerDict.Add("S32", "Sequence32");
            CustomerDict.Add("ZM", "Zoom");
            CustomerDict.Add("SDL", "Summum Dental Lab");
            CustomerDict.Add("CD", "Candid");
        }
    }
}