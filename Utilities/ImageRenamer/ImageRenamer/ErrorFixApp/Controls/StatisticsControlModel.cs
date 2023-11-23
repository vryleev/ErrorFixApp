using ErrorDataLayer;
using ErrorFixApp.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ErrorFixApp.Controls
{
    public class StatisticsControlModel: INotifyPropertyChanged
    {
        public StatisticsControlModel()
        {
            _webApiManager = new WebApiManager();

            GetDbList();
        }

        private readonly WebApiManager _webApiManager;

        private string _selectedDb;

        public string SelectedDb
        {
            get => _selectedDb;
            set
            {
                _selectedDb = value;
                GetRouteNameList();
                
                OnPropertyChanged();
            }
        }

        private string _selectedRouteName;

        public string SelectedRouteName
        {
            get { return _selectedRouteName; }
            set
            {
                if (_selectedRouteName != value)
                {
                    _selectedRouteName = value;
                    UpdateStatusInfo();
                    OnPropertyChanged(nameof(SelectedRouteName));
                }
                
            }
        }

        private string _routeInfo;

        public string RouteInfo
        {
            get => _routeInfo;
            set
            {
                _routeInfo = SelectedRouteName + ":" + value;
                OnPropertyChanged();
            }
        }

        private ErrorDetail _errorDetail = new ErrorDetail();

        private ErrorDetail Error => _errorDetail;

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

        private ObservableCollection<string> _routeNameList = new ObservableCollection<string>();

        public ObservableCollection<string> RouteNameList
        {
            get => _routeNameList;
            private set
            {
                _routeNameList = value;
                OnPropertyChanged();
            }
        }

        private async void GetDbList()
        {
            List<string> dbList;
            if (ConfigurationParams.WorkingType == "Local")
            {
                dbList = SqLiteManager.GetAvailableDb();
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

        private void GetRouteNameList()
        {
            List<ErrorEntity> list = SqLiteManager.RouteNames(_selectedDb);
            if (list != null)
            {
                RouteNameList.Clear();
                foreach (var error in list)
                {
                    RouteNameList.Add($"{error.RouteName}");
                }
            }
        }

        private void UpdateStatusInfo()
        {
            if (ConfigurationParams.WorkingType == "Local")
            {
                SqLiteManager.SetBaseName(_selectedDb);

                RouteInfo =
                    $"{Resources.FixedErrors}: {SqLiteManager.GetStatusCount(SelectedRouteName)}, {Resources.RouteErrors}: {SqLiteManager.GetRouteErrorsCount(SelectedRouteName)}";
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}

