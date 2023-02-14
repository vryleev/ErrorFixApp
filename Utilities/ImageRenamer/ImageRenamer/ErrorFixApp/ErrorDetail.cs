using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media.Imaging;
using ErrorFixApp.Properties;

namespace ErrorFixApp
{
    public class ErrorDetail: INotifyPropertyChanged
    {
        private string _comment = Resources.AddComment;
        private string _position = Resources.PositionNotSet;
        private string _user = String.Empty;
        private string _timestamp = Resources.TimeStampNotSet;
        private string _routeName = Resources.SetupRoute;
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
        
        
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
        
        
    }
}