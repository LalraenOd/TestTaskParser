using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TestTaskProCarsClient.ViewModels
{
    class MainViewModel : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
