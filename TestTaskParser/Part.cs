using System.Collections.Generic;

namespace TestTaskProCars
{
    class Part
    {
        public string Url { get; set; }
        public string PartBrand { get; set; }
        public string ArtNumber { get; set; }
        public string PartName { get; set; }
        public List<string> Specs { get; set; } = new List<string>();
        //public int[] LinkedParts { get; set; }

        public override string ToString()
        {
            return "\nPart URL: " + Url +
                   "\nPart Brand: " + PartBrand +
                   "\nArtNumber: " + ArtNumber +
                   "\nPartName: " + PartName +
                   "\nSpecs:\n " +  string.Join("\n", Specs);//+
                   //"LinkedParts" + LinkedParts.ToString();
        }
    }
}
