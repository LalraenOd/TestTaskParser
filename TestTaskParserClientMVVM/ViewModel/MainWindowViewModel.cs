using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using TestTaskParser;

namespace TestTaskParserClientMVVM.ViewModel
{
    public class MainWindowViewModel 
    {
        public MainWindowViewModel()
        {
            PartsList = Model.Model.GetAllDataFromDB();
        }

        public List<Part> PartsList { get; set; }
    }
}
