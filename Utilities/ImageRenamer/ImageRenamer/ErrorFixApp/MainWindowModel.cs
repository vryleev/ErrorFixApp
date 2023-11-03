using System.ComponentModel;
using System.Configuration;
using System.Runtime.CompilerServices;
using ErrorDataLayer;
using ErrorFixApp.Properties;
using ErrorFixApp.Controls;

namespace ErrorFixApp
{
    public class MainWindowModel : INotifyPropertyChanged
    {
        public MainWindowModel()
        {
            AddErrorPanelControlVm = new AddErrorPanelControlModel();
            ViewErrorsControlVm = new ViewErrorsControlModel();
            ExportControlVm = new ExportControlModel();
            SqLiteManager.User = ConfigurationParams.User;
        } 
        
        
        private int _selectedTabIndex;

        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set
            {
                _selectedTabIndex = value;
                if (_selectedTabIndex == 0)
                {
                    AddErrorPanelControlVm.Update();
                }
                if (_selectedTabIndex == 1)
                {
                    ViewErrorsControlVm.Update();
                }
                OnPropertyChanged();
            }
        }

        public string ApplicationName
        {
            get
            {
                if (ConfigurationManager.AppSettings["WorkingType"] == "Local")
                {
                    return
                        $"Logos Error Fix App/ {Resources.User}: {ConfigurationParams.User}/ {Resources.WorkingType}: {ConfigurationParams.WorkingType}";
                }
                else
                {
                    return
                        $"Logos Error Fix App/ {Resources.User}: {ConfigurationParams.User}/ {Resources.WorkingType}: {ConfigurationParams.WorkingType}/ Host: {ConfigurationManager.AppSettings["RemoteUrl"]}";
                }
            }
        }
        
        private AddErrorPanelControlModel _addErrorPanelControlVm;
        public AddErrorPanelControlModel AddErrorPanelControlVm
        {
            get => _addErrorPanelControlVm;
            private set
            {
                _addErrorPanelControlVm = value;
                OnPropertyChanged();
            }
        }
        
        private ViewErrorsControlModel _viewErrorsControlVm;
        public ViewErrorsControlModel ViewErrorsControlVm
        {
            get => _viewErrorsControlVm;
            private set
            {
                _viewErrorsControlVm = value;
                OnPropertyChanged();
            }
        }
        
        
        private ExportControlModel _exportControlVm;
        public ExportControlModel ExportControlVm
        {
            get => _exportControlVm;
            private set
            {
                _exportControlVm = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

       
        public void ClosingDb()
        {
            SqLiteManager.IsCheckQueue = false;
            SqLiteManager.StopTasks();
            
        }

    }
}