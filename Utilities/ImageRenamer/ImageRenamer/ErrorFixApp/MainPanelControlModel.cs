using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ErrorFixApp
{
    public class MainPanelControlModel : INotifyPropertyChanged
    {
        private string _xlsToView = "Test";
        
        public string TestContent
        {
            get => _xlsToView;
            set
            {
                _xlsToView = value;
                OnPropertyChanged();
            }
        }
        
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}