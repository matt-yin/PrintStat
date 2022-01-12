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
            CustomerDict.Add("SL", "Smile Love");
            CustomerDict.Add("USM", "USmile");
            CustomerDict.Add("SSS", "Straight Smile");
            CustomerDict.Add("OFX", "OrthoFX");
            CustomerDict.Add("CF", "Clear Forward");
            CustomerDict.Add("BYT", "Byte");
            CustomerDict.Add("SHK", "Smile Hawk");
            CustomerDict.Add("3DP", "3D Predict");
            CustomerDict.Add("GOD", "Global Ortho Design");
        }
    }
}