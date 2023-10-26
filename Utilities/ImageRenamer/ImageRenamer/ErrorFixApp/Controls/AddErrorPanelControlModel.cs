using System;
using System.ComponentModel;
using System.Configuration;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using ErrorDataLayer;
using ErrorFixApp.Properties;
using MessageBox = System.Windows.MessageBox;

namespace ErrorFixApp.Controls
{
    public class AddErrorPanelControlModel : INotifyPropertyChanged
    {
        public AddErrorPanelControlModel()
        {
            _sqLiteManager = new SqLiteManager(ConfigurationParams.User);
            _webApiManager = new WebApiManager();
            _imageEditControlVm = new ImageEditControlModel();
            _errorEditControlVm.Error = _errorDetail;
            _imageEditControlVm.IsVisible = Visibility.Hidden;
            _errorEditControlVm.IsVisible = Visibility.Hidden;
            GetDbToSave();
        }
        
        private readonly SqLiteManager _sqLiteManager;
        private readonly WebApiManager _webApiManager;
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


        private readonly ErrorEntity _errorEntity = new ErrorEntity();
        
       

        private string _routeName;

        public string RouteName
        {
            get => _routeName;
            set
            {
                _routeName = value;
                Error.RouteName = value;
                OnPropertyChanged();
            }
        }

        private string _dataBaseToSave;
        public string DataBaseToSave
        {
            get => _dataBaseToSave;
            set
            {
                _dataBaseToSave = value;
                OnPropertyChanged();
            }
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
        
        private WindowState _wState = WindowState.Normal;
        public WindowState WState
        {
            get => _wState;
            set
            {
                _wState = value;
                OnPropertyChanged();
            }
        }
        
        private Visibility _addButtonVisibility = Visibility.Visible;
        
        public Visibility AddButtonVisibility
        {
            get => _addButtonVisibility;
            set
            {
                _addButtonVisibility = value;
                OnPropertyChanged();
            }
        }
        
        private Visibility _saveButtonVisibility = Visibility.Hidden;
        
        public Visibility SaveButtonVisibility
        {
            get => _saveButtonVisibility;
            set
            {
                _saveButtonVisibility = value;
                OnPropertyChanged();
            }
        }
        
        private bool _isSaveButtonEnable;
        
        public bool IsSaveButtonEnable
        {
            get => _isSaveButtonEnable;
            set
            {
                _isSaveButtonEnable = value;
                OnPropertyChanged();
            }
        }
        

        private ICommand _fixCommand;
        public ICommand FixCommand
        {
            get
            {
                return _fixCommand ?? (_fixCommand = new RelayCommand<object>(
                    param => this.FixObject(),
                    param => true
                ));
            }
        }

        private void FixObject()
        {
            if (RouteName.Contains(Resources.SetupRoute))
            {
                MessageBox.Show(Resources.SetupRouteMessage);
                return;
            }

            if (RouteName.Contains(' ') || RouteName.Contains('.'))
            {
                MessageBox.Show(Resources.IncorrectName);
                return;
            }

            Error.Id = -1;
            Error.Comment = Resources.AddComment;
            Error.Position = Resources.PositionNotSet;
            WState = WindowState.Minimized;
            Error.ImageVisibility = Visibility.Visible;
            Error.TimeStamp = DateTime.Now.ToString("ddMMyyyy-hhmmss", CultureInfo.InvariantCulture);
            AddButtonVisibility = Visibility.Collapsed;

            if (File.Exists(ConfigurationParams.PositionFilePath))
            {
                Error.Position = File.ReadAllText(ConfigurationParams.PositionFilePath, Encoding.UTF8);
            }

            float scale = Screen.PrimaryScreen.Bounds.Width / (float)SystemParameters.PrimaryScreenWidth;

            int screenLeft = (int)SystemParameters.VirtualScreenLeft;
            int screenTop = (int)SystemParameters.VirtualScreenTop;
            int screenWidth = (int)(SystemParameters.VirtualScreenWidth * scale);
            int screenHeight = (int)(SystemParameters.VirtualScreenHeight * scale);

            int visualLeft = screenLeft;
            int visualTop = screenTop;
            int visualWidth = screenWidth / 2;
            int visualHeight = screenHeight;

            int mapLeft = screenWidth / 2;
            int mapTop = screenTop;
            int mapWidth = screenWidth / 2;
            int mapHeight = screenHeight;

            string visualRect = ConfigurationManager.AppSettings.Get("VisualRect");
            ConfigurationParams.ParseRectConfiguration(visualRect, screenLeft, screenTop, screenWidth, screenHeight, ref visualLeft,
                ref visualTop, ref visualWidth, ref visualHeight);

            string mapRect = ConfigurationManager.AppSettings.Get("MapRect");
            ConfigurationParams.ParseRectConfiguration(mapRect, screenWidth / 2, screenTop, screenWidth, screenHeight, ref mapLeft,
                ref mapTop, ref mapWidth, ref mapHeight);

            Bitmap bitmapScreen1 = new Bitmap(visualWidth, visualHeight);
            Bitmap bitmapScreen2 = new Bitmap(mapWidth, mapHeight);

            Graphics g1 = Graphics.FromImage(bitmapScreen1);
            Graphics g2 = Graphics.FromImage(bitmapScreen2);

            g1.CopyFromScreen(visualLeft, visualTop, 0, 0, bitmapScreen1.Size);
            _imageEditControlVm.BImageV = ImageUtils.BitmapToImageSource(bitmapScreen1);
            _imageEditControlVm.ImageV = bitmapScreen1;
            
            Bitmap testV = ImageUtils.ResizeImage(_imageEditControlVm.ImageV, 1024);
            _imageEditControlVm.ImageV = testV;
            Error.ImageV =  _imageEditControlVm.ImageV;

            g2.CopyFromScreen(mapLeft, mapTop, 0, 0, bitmapScreen2.Size);
            _imageEditControlVm.BImageM = ImageUtils.BitmapToImageSource(bitmapScreen2);
            _imageEditControlVm.ImageM = bitmapScreen2;
            Error.ImageM = bitmapScreen2;

            Bitmap testM = ImageUtils.ResizeImage(_imageEditControlVm.ImageM, 1024);
            _imageEditControlVm.ImageM = testM;
            Error.ImageM = _imageEditControlVm.ImageM;

            WState = WindowState.Maximized;
            AddButtonVisibility = Visibility.Hidden;
            SaveButtonVisibility = Visibility.Visible;
            _errorEditControlVm.IsVisible = Visibility.Visible;
            _imageEditControlVm.IsVisible = Visibility.Visible;
            
            //IsComboEnabled = true;
        }
        
        private ICommand _saveCommand;

        public ICommand SaveCommand
        {
            get
            {
                return _saveCommand ?? (_saveCommand = new RelayCommand<object>(
                    param => this.SaveObject(),
                    param =>true
                ));
            }
        }
        
        private async void SaveObject()
        {
            if (Error.Comment.Contains(Resources.AddComment))
            {
                MessageBox.Show(Resources.CommentMessage);
            }
            else
            {
                Error.UpdateErrorEntity(_errorEntity);
                if (ConfigurationParams.WorkingType == "Local")
                {
                    _sqLiteManager.AddErrorToDb(_errorEntity);
                }
                else
                {
                    await _webApiManager.AddError(_errorEntity);
                }

                WState = WindowState.Normal;
                IsSaveButtonEnable = false;
                _errorEditControlVm.IsVisible = Visibility.Hidden;
                _imageEditControlVm.IsVisible = Visibility.Hidden;
                AddButtonVisibility = Visibility.Visible;
                SaveButtonVisibility = Visibility.Hidden;
                Error.Comment = Resources.AddComment;
                
            }
        }

        private async void GetDbToSave()
        {
            if (ConfigurationParams.WorkingType == "Local")
            {
                DataBaseToSave = _sqLiteManager.GetDbToAdd();
            }
            else
            {
                DataBaseToSave =  await _webApiManager.GetDbToAdd();
            }
            
        }
        
        private ICommand _cancelCommand;

        public ICommand CancelCommand
        {
            get
            {
                return _cancelCommand ?? (_cancelCommand = new RelayCommand<object>(
                    param => this.CancelObject(),
                    param => true
                ));
            }
        }
        
        public void Update()
        {
           
        }
        
        private void CancelObject()
        {
            WState = WindowState.Normal;
            IsSaveButtonEnable = false;
            _errorEditControlVm.IsVisible = Visibility.Hidden;
            _imageEditControlVm.IsVisible = Visibility.Hidden;
            AddButtonVisibility = Visibility.Visible;
            SaveButtonVisibility = Visibility.Hidden;
            
            Error.Comment = Resources.AddComment;
            Error.ImageM?.Dispose();
            Error.ImageV?.Dispose();
            _imageEditControlVm.BImageM?.StreamSource.Dispose();
            _imageEditControlVm.BImageV?.StreamSource.Dispose();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}