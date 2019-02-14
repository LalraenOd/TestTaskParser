using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TestTaskProCarsClient.Models
{
    class PartsDB : INotifyPropertyChanged
    {
        private int partId;

        public int PartId
        {
            get { return PartId; }
            set { PartId = value; }
        }

        private string url;

        public string Url
        {
            get { return url; }
            set { url = value; }
        }

        private string atrNumber;

        public string ArtNumber
        {
            get { return atrNumber; }
            set { atrNumber = value; }
        }

        private string brandName;

        public string BrandName
        {
            get { return brandName; }
            set { brandName = value; }
        }

        private string partName;

        public string PartName
        {
            get { return partName; }
            set { partName = value; }
        }

        private string specs;

        public string Specs
        {
            get { return specs; }
            set { specs = value; }
        }

        private int[] linkedParts;

        public int[] LinkedParts
        {
            get { return linkedParts; }
            set { linkedParts = value; }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class LinkedPartsDb : INotifyPropertyChanged
    {
        private int id;

        public int Id
        {
            get { return id; }
            set { id = value; }
        }

        private string linkedParts;

        public string LinkedParts
        {
            get { return  linkedParts; }
            set {  linkedParts = value; }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
