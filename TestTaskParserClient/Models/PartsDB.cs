using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TestTaskParserClient.Models
{
    public class PartsDB : INotifyPropertyChanged
    {
        private int partId;

        public int PartId
        {
            get { return PartId; }
            set { PartId = value; OnPropertyChanged("PartId"); }
        }

        private string brandName;

        public string BrandName
        {
            get { return brandName; }
            set { brandName = value; OnPropertyChanged("BrandName"); }
        }

        private string partName;

        public string PartName
        {
            get { return partName; }
            set { partName = value; OnPropertyChanged("PartName"); }
        }

        private int[] linkedParts;

        public int[] LinkedParts
        {
            get { return linkedParts; }
            set { linkedParts = value; OnPropertyChanged("LinkedParts"); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }

    public class LinkedPartsDb : INotifyPropertyChanged
    {
        private int linkedPartId;

        public int LinkedPartId
        {
            get { return linkedPartId; }
            set { linkedPartId = value; OnPropertyChanged("Id"); }
        }

        private string partName;

        public string PartName
        {
            get { return partName; }
            set { partName = value; OnPropertyChanged("PartName"); }
        }

        private string partNumber;

        public string PartNumber
        {
            get { return partNumber; }
            set { partNumber = value; OnPropertyChanged("partNumber"); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
