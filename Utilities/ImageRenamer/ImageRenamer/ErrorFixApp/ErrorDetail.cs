using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media.Imaging;
using ErrorDataLayer;
using ErrorFixApp.Properties;

namespace ErrorFixApp
{
    public class ErrorDetail: INotifyPropertyChanged
    {
        private string _comment = Resources.AddComment;
        private string _position = Resources.PositionNotSet;
        private string _user = String.Empty;
        private string _timestamp = Resources.TimeStampNotSet;
        private string _errorType = "base";
        private string _priority = "Normal";
        private string _routeName = Resources.SetupRoute;
        private string _status = "NotFixed"; //WIP
        private int _id = -1;
        
        private BitmapImage _bitmapImageV = new BitmapImage();
        private BitmapImage _bitmapImageM = new BitmapImage();

        private Visibility _imageVisibility = Visibility.Hidden;
        
        public Visibility ImageVisibility
        {
            get => _imageVisibility;
            set
            {
                _imageVisibility = value;
                OnPropertyChanged();
            }
        }
        
        public Image ImageV { get; set; }

        public Image ImageM { get; set; }

        public BitmapImage BImageV
        {
            get => _bitmapImageV;
            set
            {
                _bitmapImageV = value;
                OnPropertyChanged();
            }
        }
        
        public BitmapImage BImageM
        {
            get => _bitmapImageM;
            set
            {
                _bitmapImageM = value;
                OnPropertyChanged();
            }
        }
        
        public int Id
        {
            get => _id;
            set
            {
                _id = value;
                OnPropertyChanged();
            }
        }
       
        
        public string Comment
        {
            get => _comment;
            set
            {
                _comment = value;
                OnPropertyChanged();
            }
        }
        
        public string Position
        {
            get => _position;
            set
            {
                _position = value;
                OnPropertyChanged();
            }
        }
        
        public string User
        {
            get => _user;
            set
            {
                _user = value;
                OnPropertyChanged();
            }
        }
        
        
        public string RouteName
        {
            get => _routeName;
            set
            {
                _routeName = value;
                OnPropertyChanged();
            }
        }
        
        public string TimeStamp
        {
            get =>  _timestamp;
            set
            {
                _timestamp = value;
                OnPropertyChanged();
            }
        }

        public string ErrorType
        {
            get =>  _errorType;
            set
            {
                _errorType = value;
                OnPropertyChanged();
            }
        }
        
        public string Priority
        {
            get =>  _priority;
            set
            {
                _priority = value;
                OnPropertyChanged();
            }
        }

        public string Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
        
        public void UpdateErrorEntity(ErrorEntity ee)
        {
            ee.Comment = Comment;
            ee.Position = Position;
            ee.RouteName = RouteName;
            ee.Id = Id;
            ee.TimeStamp = TimeStamp;
            ee.User = ConfigurationParams.User;
            ee.ErrorType = ErrorType;
            ee.Priority = Priority;
            ee.Status = Status;

            ee.ImageV = ImageUtils.ImageToByte(ImageV, ImageFormat.Jpeg);
            ee.ImageM = ImageUtils.ImageToByte(ImageM, ImageFormat.Jpeg);
        }
        
        public bool UpdateErrorDetails(ErrorEntity ee)
        {
            bool res = false;
            if (ee != null && ee.Id > 0)
            {
                Position = ee.Position;
                User = ee.User;
                Comment = ee.Comment;
                Id = ee.Id;
                RouteName = ee.RouteName;
                TimeStamp = ee.TimeStamp;
                ImageV = ImageUtils.ByteToImage(ee.ImageV);
                ImageM = ImageUtils.ByteToImage(ee.ImageM);
                BImageV = ImageUtils.BitmapToImageSource(new Bitmap( ImageV));
                BImageM = ImageUtils.BitmapToImageSource(new Bitmap( ImageM));
                ImageVisibility = Visibility.Visible;

                Priority = ee.Priority;
                ErrorType = ee.ErrorType;
                Status = ee.Status;

                res = true;
            }
            else
            {
                Id = 0;
                ImageVisibility = Visibility.Collapsed;
            }

            return res;
        }
        
        
    }
}