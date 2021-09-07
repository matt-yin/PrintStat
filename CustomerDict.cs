using System;
using System.Collections.Generic;

namespace PrintStat
{
    public class CustomerDict
    {
        private Dictionary<string, string> customer = new Dictionary<string, string>();

        public CustomerDict()
        {
            InitializeCustomers();
        }

        public string GetName(string code)
        {
            return customer[code];
        }

        private void InitializeCustomers()
        {
            customer.Add("UT","Uniform Teeth");
            customer.Add("ACO", "AlignerCo");
            customer.Add("SDL", "Summum Dental Lab");
            customer.Add("ZM", "Zoom");
        }
    }
}