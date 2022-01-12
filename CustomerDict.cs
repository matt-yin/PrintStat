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
            
            
            
            customer.Add("S32", "Sequence32");
            customer.Add("SL", "Smile Love");
            customer.Add("USM", "USmile");
            customer.Add("SSS", "Straight Smile");
            customer.Add("OFX", "OrthoFX");
            customer.Add("CF", "Clear Forward");
            customer.Add("BYT", "Byte");
            customer.Add("SHK", "Smile Hawk");
            customer.Add("3DP", "3D Predict");
            customer.Add("GOD", "Global Ortho Design");
        }
    }
}