using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;

namespace ErrorFixApp.Controls
{
    public class ErrorDetailsEditControlModel: INotifyPropertyChanged
    {
        public ErrorDetailsEditControlModel()
        {
           
            _errorPatternsFromAppSettings = ConfigurationManager.AppSettings.Get("PatternErrors").Split(',').ToList();
            ErrorPatterns = new ObservableCollection<MenuItemObject>();
            GetComboBoxLists();
            // foreach (var ep in _errorPatternsFromAppSettings)
            // {
            //     ErrorPatterns.Add(new MenuItemObject {Command = new RelayCommand<object>(MenuClicked), Content = ep});
            // }
            
            
        }

        private ErrorDetail _errorDetail = new ErrorDetail();

        private readonly List<string> _errorPatternsFromAppSettings;
        
        private void MenuClicked(object o)
        {
            if (o is string error)
            {
                Error.Comment = error;
            }
        }

        private string _selectedErrorType;

        public string SelectedErrorType
        {
            get => _selectedErrorType;
            set
            {
                _selectedErrorType = value;
                if (ErrorPatterns != null)
                {
                    ErrorPatterns.Clear();
                    foreach (var ep in _errorPatternsFromAppSettings)
                    {
                        if (ep.Contains(value))
                        {
                            ErrorPatterns.Add(new MenuItemObject
                                { Command = new RelayCommand<object>(MenuClicked), Content = ep });
                        }
                    }
                }

                OnPropertyChanged();
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
            private set
            {
                _errorPatterns = value;
                OnPropertyChanged();
            }
        }
        
        private ObservableCollection<string> _errorTypeList;
        
        public ObservableCollection<string> ErrorTypeList
        {
            get => _errorTypeList;
            private set
            {
                _errorTypeList = value;
                OnPropertyChanged();
            }
        }
        
        private ObservableCollection<string> _priorityList;
        
        public ObservableCollection<string> PriorityList
        {
            get => _priorityList;
            private set
            {
                _priorityList = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<string> _statusList;

        public ObservableCollection<string> StatusList
        {
            get => _statusList;
            set
            {
                _statusList = value;
                OnPropertyChanged();
            }
        }

        private void GetComboBoxLists()
        {
            ErrorTypeList = new ObservableCollection<string>(ConfigurationParams.GetErrorTypeList());
            Error.ErrorType = ErrorTypeList.First();
            SelectedErrorType = Error.ErrorType;
            
            PriorityList = new ObservableCollection<string>(ConfigurationParams.GetPriorityList());
            Error.Priority = PriorityList.First();

            StatusList = new ObservableCollection<string>(ConfigurationParams.GetStatusList()); //WIP
            Error.Status = StatusList.First();
        }
        
        
        
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}