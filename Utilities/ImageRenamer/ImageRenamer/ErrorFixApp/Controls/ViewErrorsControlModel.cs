using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Input;
using ErrorDataLayer;
using ErrorFixApp.Properties;

namespace ErrorFixApp.Controls
{
    public class ViewErrorsControlModel: INotifyPropertyChanged
    {
        
        public ViewErrorsControlModel()
        {
            //_sqLiteManager = new SqLiteManager(ConfigurationParams.User);
            _webApiManager = new WebApiManager();
            
            _imageEditControlVm = new ImageEditControlModel();
            _errorEditControlVm.Error = _errorDetail;
          
            _imageEditControlVm.IsVisible = Visibility.Hidden;
            _errorEditControlVm.IsVisible = Visibility.Hidden;
            GetDbList();
          
        }
        
        private bool _errorLoaded;
        public bool ErrorLoaded
        {
            get => _errorLoaded;
            set
            {
                _errorLoaded = value;
                OnPropertyChanged();
            }
        }
        
        private ImageEditControlModel _imageEditControlVm;
       

        public ImageEditControlModel ImageEditControlVm
        {
            get => _imageEditControlVm;
            set
            {
                _imageEditControlVm = value;
                OnPropertyChanged();
            }
        }
        
        
        private ErrorDetailsEditControlModel _errorEditControlVm  = new ErrorDetailsEditControlModel();
        
        public ErrorDetailsEditControlModel ErrorEditControlVm
        {
            get => _errorEditControlVm;
            set
            {
                _errorEditControlVm = value;
                OnPropertyChanged();
            }
        }
        
        //private readonly SqLiteManager _sqLiteManager;
        private readonly WebApiManager _webApiManager;
        
        private ErrorEntity _errorEntity = new ErrorEntity();
        
        private string _selectedDb;
        public string SelectedDb
        {
            get => _selectedDb;
            set
            {
                _selectedDb = value;
                UpdateErrorCount();
                UpdateErrorIdList();
                ErrorComboBoxFocused = true;
                OnPropertyChanged();
            }
        }
        
        
        private bool _errorComboBoxFocused;
        public bool ErrorComboBoxFocused
        {
            get => _errorComboBoxFocused;
            set
            {
                _errorComboBoxFocused = value;
                OnPropertyChanged();
            }
        }
            
            
        private string _selectedError;
        public string SelectedError
        {
            get => _selectedError;
            set
            {
                _selectedError = value;
                if (_selectedError != null && !_selectedError.Contains("БД"))
                {
                    LoadObject();
                }
                OnPropertyChanged();
            }
        }
        
        private int _selectedErrorIndex;
        public int SelectedErrorIndex
        {
            get => _selectedErrorIndex;
            set
            {
                _selectedErrorIndex = value;
                OnPropertyChanged();
            }
        }
        
        
        private string _dbInfo;
        public string DbInfo
        {
            get => _dbInfo;
            set
            {
                _dbInfo = SelectedDb + ":"+ value;
               
                OnPropertyChanged();
            }
        }
        
        private async void UpdateErrorCount()
        {
            if (ConfigurationParams.WorkingType == "Local")
            {
                SqLiteManager.SetBaseName(SelectedDb);
                _imageEditControlVm.IsVisible = Visibility.Hidden;
                _errorEditControlVm.IsVisible = Visibility.Hidden;
                ErrorId = -1;
                DbInfo =
                    $"{Resources.TotalErrors}: {SqLiteManager.GetErrorCount()}, {Resources.MaxId}: {SqLiteManager.GetMaxId()}";
            }
            else
            {
                int errorCount = await _webApiManager.GetErrorCount(SelectedDb);
                //ToDo
                DbInfo = $"{Resources.TotalErrors}: {errorCount}";
            }
           
        }
        
        private ObservableCollection<string> _errorIdList = new ObservableCollection<string>();
        
        public ObservableCollection<string> ErrorIdList
        {
            get => _errorIdList;
            private set
            {
                _errorIdList = value;
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
        
        private int _errorId = -1;
        
        public int ErrorId
        {
            get => _errorId;
            set
            {
                _errorId = value;
                OnPropertyChanged();
            }
        }
        
        private ErrorDetail _errorDetail = new ErrorDetail();

        private ErrorDetail Error => _errorDetail;


        private ICommand _loadCommand;

        public ICommand LoadCommand
        {
            get
            {
                return _loadCommand ?? (_loadCommand = new RelayCommand<object>(
                    param => this.LoadObject(),
                    param => true
                ));
            }
        }
        
        
        private async void LoadObject()
        {
            _errorEditControlVm.IsVisible = Visibility.Hidden;
            _imageEditControlVm.IsVisible = Visibility.Hidden;
            ErrorLoaded = false;
            ErrorId = 0;
            
            if (Int32.TryParse(SelectedError.Split('-').First(), out var res))
            {
                ErrorId = res;
            }
            if (ErrorId <= 0)
            {
                MessageBox.Show(Resources.IdMessage);
            }
            
            if (SelectedDb != String.Empty && ErrorId > 0)
            {
                Error.ImageM?.Dispose();
                Error.ImageV?.Dispose();
                Error.BImageM?.StreamSource?.Dispose();
                Error.BImageV?.StreamSource?.Dispose();
                

                if (ConfigurationManager.AppSettings.Get("WorkingType") == "Local")
                {
                    _errorEntity = SqLiteManager.LoadError(ErrorId);
                }
                else
                {
                    _errorEntity = await _webApiManager.GetError(ErrorId, SelectedDb);
                }

                if (Error.UpdateErrorDetails(_errorEntity))
                {
                    _imageEditControlVm.BImageV = Error.BImageV;
                    _imageEditControlVm.BImageM = Error.BImageM;
                    _imageEditControlVm.ImageM = Error.ImageM;
                    _imageEditControlVm.ImageV = Error.ImageV;
                    ErrorLoaded = true;

                    if (Directory.Exists(ConfigurationParams.TrainerPath))
                    {
                        File.WriteAllText(ConfigurationParams.PositionFilePathSetup, Error.Position, Encoding.ASCII);
                    }

                    if (Directory.Exists(ConfigurationParams.SceneGeneratorPath))
                    {
                        File.WriteAllText(ConfigurationParams.PositionFilePathSgSetup, Error.Position, Encoding.ASCII);
                    }
                    
                    _errorEditControlVm.IsVisible = Visibility.Visible;
                    _imageEditControlVm.IsVisible = Visibility.Visible;
                }
                else
                {
                    ErrorLoaded = false;
                    MessageBox.Show("Ошибки не существует");
                }


            }
        }
        
         private ICommand _updateCommand;

        public ICommand UpdateCommand
        {
            get
            {
                return _updateCommand ?? (_updateCommand = new RelayCommand<object>(
                    param => this.UpdateObject(),
                    param => true
                ));
            }
        }
        
        
        private void UpdateObject()
        {
            if (ErrorId <= 0)
            {
                MessageBox.Show(Resources.IdMessage);
            }

            if (SelectedDb != String.Empty && ErrorId > 0)
            {
                if (ConfigurationManager.AppSettings.Get("WorkingType") == "Local")
                {
                    Error.ImageM = _imageEditControlVm.ImageM;
                    Error.ImageV = _imageEditControlVm.ImageV;
                    Error.UpdateErrorEntity(_errorEntity);
                    SqLiteManager.UpdateErrorInDb(_errorEntity);
                }
                else
                {
                    //ToDo _errorEntity = await _webApiManager.Update(_errorEntity, SelectedDb);
                }
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
        
        private async void UpdateErrorIdList()
        {
            List<string> errorList = new List<string>();
            if (ConfigurationParams.WorkingType == "Local")
            {
                var errorEntityList = SqLiteManager.LoadErrors();
                foreach (var ee in errorEntityList)
                {
                    errorList.Add($"{ee.Id}-{ee.RouteName}-{ee.Comment}");
                }
            }
            else
            {
                //errorList = await _webApiManager.GetAvailableDb();
            }

            if (errorList.Count == 0)
            {
                errorList.Add(Resources.NoDb);
            }
            ErrorIdList.Clear();
            ErrorIdList = new ObservableCollection<string>(errorList);
            SelectedError = ErrorIdList.First();
        }

        public void Update()
        {
            GetDbList();
            _imageEditControlVm.IsVisible = Visibility.Hidden;
            _errorEditControlVm.IsVisible = Visibility.Hidden;
            ErrorId = -1;
        }
        
        
       
        private async void DeleteObject()
        {
            if (Error.Id > 0)
            {
                MessageBoxResult dialogResult = MessageBox.Show(Resources.DeleteMessage, Resources.DeleteCaption,
                    MessageBoxButton.YesNo);
                if (dialogResult == MessageBoxResult.Yes)
                {
                    if (ConfigurationParams.WorkingType == "Local")
                    {
                        SqLiteManager.DeleteErrorFromDb(Error.Id);
                    }
                    else
                    {
                        await _webApiManager.DeleteFromDb(Error.Id, SelectedDb);
                    }

                    Error.Id = 0;
                    ErrorId = -1;
                    _errorEditControlVm.IsVisible = Visibility.Hidden;
                    _imageEditControlVm.IsVisible = Visibility.Hidden;
                    UpdateErrorCount();

                }

            }
            else
            {
                MessageBox.Show(Resources.DeleteNotPossible, Resources.DeleteCaption, MessageBoxButton.OK);
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