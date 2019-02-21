using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using TestTaskParser;

namespace TestTaskParserClientMVVM.ViewModel
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private List<Part> partsPartsList;

        public List<Part> PartsList
        {
            get { return partsPartsList = Model.Model.GetAllDataFromDB(); }
            set { partsPartsList = value; OnPropertyChanged("PartsList"); }
        }

        private string filterByNumber;

        public string FilterByNumber
        {
            get { return filterByNumber; }
            set { filterByNumber = value; OnPropertyChanged("FilterByNumber"); }
        }

        private string filterByName;

        public string FilterByName
        {
            get { return filterByName; }
            set { filterByName = value; OnPropertyChanged("FilterByName"); }
        }        

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
            }
        }
    }
}
