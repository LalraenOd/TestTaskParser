using System;
using System.Collections.Generic;

namespace TestTaskParser
{
    public class Part
    {
        public string PartUrl { get; set; }
        public string PartBrand { get; set; }
        public string PartArtNumber { get; set; }
        public string PartName { get; set; }
        public List<string> PartSpecs { get; set; } = new List<string>();
        //public int[] LinkedParts { get; set; }

        public override string ToString()
        {
            return "\nPart URL: " + PartUrl +
                   "\nPart Brand: " + PartBrand +
                   "\nArtNumber: " + PartArtNumber +
                   "\nPartName: " + PartName +
                   "\nSpecs:\n " +  string.Join("\n", PartSpecs.ToArray());//+
                   //"LinkedParts" + LinkedParts.ToString();
        }
    }
}
