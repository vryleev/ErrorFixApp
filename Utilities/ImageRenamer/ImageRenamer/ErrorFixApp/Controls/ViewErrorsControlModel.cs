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
            _sqLiteManager = new SqLiteManager(ConfigurationParams.User);
            _webApiManager = new WebApiManager();
            
            _imageEditControlVm = new ImageEditControlModel();
            _errorEditControlVm.Error = _errorDetail;
          
            _imageEditControlVm.IsVisible = Visibility.Hidden;
            _errorEditControlVm.IsVisible = Visibility.Hidden;
            GetDbList();
          
        }
        
        private bool _errorLoaded = false;
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
        
        private readonly SqLiteManager _sqLiteManager;
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
                _sqLiteManager.SetBaseName(SelectedDb);
                _imageEditControlVm.IsVisible = Visibility.Hidden;
                _errorEditControlVm.IsVisible = Visibility.Hidden;
                ErrorId = -1;
                DbInfo =
                    $"{Resources.TotalErrors}: {_sqLiteManager.GetErrorCount()}, {Resources.MaxId}: {_sqLiteManager.GetMaxId()}";
            }
            else
            {
                int errorCount = await _webApiManager.GetErrorCount(SelectedDb);
                //ToDo
                DbInfo = $"{Resources.TotalErrors}: {errorCount}";
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

        private ErrorDetail Error
        {
            get => _errorDetail;
            set
            {
                _errorDetail = value;
                OnPropertyChanged();
            }
        }
        
        
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
                    _errorEntity = _sqLiteManager.LoadError(ErrorId);
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
        
        
        private async void UpdateObject()
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
                    _sqLiteManager.UpdateErrorInDb(_errorEntity);
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

        public void Update()
        {
            GetDbList();
            _imageEditControlVm.IsVisible = Visibility.Hidden;
            _errorEditControlVm.IsVisible = Visibility.Hidden;
            ErrorId = -1;
        }
        
        
        private ICommand _deleteCommand;

        public ICommand DeleteCommand
        {
            get
            {
                return _deleteCommand ?? (_deleteCommand = new RelayCommand<object>(
                    param => this.DeleteObject(),
                    param => true
                ));
            }
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
                        _sqLiteManager.DeleteErrorFromDb(Error.Id);
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