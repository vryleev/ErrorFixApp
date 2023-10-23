using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Runtime.CompilerServices;
using ErrorDataLayer;
using ErrorFixApp.Properties;

namespace ErrorFixApp.Controls
{
    public class ViewErrorsControlModel: INotifyPropertyChanged
    {
        
        public ViewErrorsControlModel()
        {
            _sqLiteManager = new SqLiteManager();
            _webApiManager = new WebApiManager();
            GetDbList();
          
        }
        
        private readonly SqLiteManager _sqLiteManager;
        private readonly WebApiManager _webApiManager;
        
        private string _selectedDb;
        public string SelectedDb
        {
            get => _selectedDb;
            set
            {
                _selectedDb = value;
                OnPropertyChanged();
            }
        }
        
        private ObservableCollection<string> _dbList = new ObservableCollection<string>();
        
        public ObservableCollection<string> DbList
        {
            get => _dbList;
            private set
            {
                _dbList = value;
                OnPropertyChanged();
            }
        }
        
        public async void GetDbList()
        {
            List<string> dbList;
            if (ConfigurationParams.WorkingType == "Local")
            {
                dbList = _sqLiteManager.GetAvailableDb();
            }
            else
            {
                dbList = await _webApiManager.GetAvailableDb();
            }

            if (dbList.Count == 0)
            {
                dbList.Add(Resources.NoDb);
            }

            DbList = new ObservableCollection<string>(dbList);
            SelectedDb = DbList.First();
        }
        
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}