using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using ErrorFixApp.Properties;

namespace ErrorFixApp.Controls
{
    public class ErrorDetailsEditControlModel: INotifyPropertyChanged
    {
        public ErrorDetailsEditControlModel()
        {
            GetComboBoxLists();
            var errorPatterns = ConfigurationManager.AppSettings.Get("PatternErrors").Split(',').ToList();
            ErrorPatterns = new ObservableCollection<MenuItemObject>();
            foreach (var ep in errorPatterns)
            {
                ErrorPatterns.Add(new MenuItemObject {Command = new RelayCommand<object>(MenuClicked), Content = ep});
            }
            
            
        }

        private ErrorDetail _errorDetail = new ErrorDetail();
        
        private void MenuClicked(object o)
        {
            
            string error = o as string;
            if (error != null)
            {
                Error.Comment = error;
            }
        }

       
        
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
        
        private ObservableCollection<MenuItemObject> _errorPatterns;
        
        public ObservableCollection<MenuItemObject> ErrorPatterns
        {
            get => _errorPatterns;
            set
            {
                _errorPatterns = value;
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