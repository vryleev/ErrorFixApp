using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ErrorFixApp
{
    public class ErrorDetail: INotifyPropertyChanged
    {
        private string _comment = "Добавь комментарий";
        private string _position = "Позиция не задана";
        private string _timestamp = "Время не задано";
        private string _routeName = "Задайте маршрут";
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
                OnPropertyChanged("ImageVisibility");
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
                OnPropertyChanged("BImageV");
            }
        }
        
        public BitmapImage BImageM
        {
            get => _bitmapImageM;
            set
            {
                _bitmapImageM = value;
                OnPropertyChanged("BImageM");
            }
        }
        
        public int Id
        {
            get => _id;
            set
            {
                _id = value;
                OnPropertyChanged("Id");
            }
        }
       
        
        public string Comment
        {
            get => _comment;
            set
            {
                _comment = value;
                OnPropertyChanged("Comment");
            }
        }
        
        public string Position
        {
            get => _position;
            set
            {
                _position = value;
                OnPropertyChanged("Position");
            }
        }
        
        public string RouteName
        {
            get => _routeName;
            set
            {
                _routeName = value;
                OnPropertyChanged("RouteName");
            }
        }
        
        public string TimeStamp
        {
            get => _timestamp;
            set
            {
                _timestamp = value;
                OnPropertyChanged("TimeStamp");
            }
        }
        
        
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
        
        
    }
}