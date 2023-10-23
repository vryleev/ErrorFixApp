using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using ErrorFixApp.Properties;

namespace ErrorFixApp.Controls
{
    public class ErrorDetailsEditControlModel: INotifyPropertyChanged
    {
        public ErrorDetailsEditControlModel()
        {
            GetComboBoxLists();
        }

        private ErrorDetail _errorDetail = new ErrorDetail();

       
        
        public ErrorDetail Error
        {
            get => _errorDetail;
            set
            {
                _errorDetail = value;
                OnPropertyChanged();
            }
        }
        
        private Visibility _isVisible;
        
        public Visibility IsVisible
        {
            get => _isVisible;
            set
            {
                _isVisible = value;
                OnPropertyChanged();
            }
        }
        
        private ObservableCollection<string> _errorTypeList;
        
        public ObservableCollection<string> ErrorTypeList
        {
            get => _errorTypeList;
            set
            {
                _errorTypeList = value;
                OnPropertyChanged();
            }
        }
        
        private ObservableCollection<string> _priorityList;
        
        public ObservableCollection<string> PriorityList
        {
            get => _priorityList;
            set
            {
                _priorityList = value;
                OnPropertyChanged();
            }
        }
        
        private void GetComboBoxLists()
        {
            ErrorTypeList = new ObservableCollection<string>(ConfigurationParams.GetErrorTypeList());
            Error.ErrorType = ErrorTypeList.First();
            
            PriorityList = new ObservableCollection<string>(ConfigurationParams.GetPriorityList());
            Error.Priority = PriorityList.First();
        }
        
        
        
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}