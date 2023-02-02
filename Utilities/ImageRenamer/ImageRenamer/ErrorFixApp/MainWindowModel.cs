using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using DocumentFormat.OpenXml.Packaging;
using ImageToXlsx;
using Microsoft.Win32;

namespace ErrorFixApp
{
    public class MainWindowModel: INotifyPropertyChanged
    {
        public MainWindowModel()
        {
            _sqLiteManager = new SQLiteManager();
          
        }

        private SQLiteManager _sqLiteManager;

        
        //private static string _trainerPath = "F:/Trainer3DMoscow";//ConfigurationSettings.AppSettings.Get("TrainerPath");
        //private static string _fileNamePos = "vehiclePosLog";     //ConfigurationSettings.AppSettings.Get("FileNamePos");

        private static string _trainerPath = ConfigurationManager.AppSettings.Get("TrainerPath");
        private static string _fileNamePos = ConfigurationManager.AppSettings.Get("FileNamePos");

        
        private string _positionFilePath = $"{_trainerPath}/{_fileNamePos}";
        private string _positionFilePathSetup = $"{_trainerPath}/{_fileNamePos}_setup"; 
        
        private string _databaseToView = String.Empty;
        private string _databaseToShow = String.Empty;
        
        private string _xlsToExport = String.Empty;
        private string _xlsToView = String.Empty;

        private int _errorId = -1;
        
        private ErrorDetail _errorDetail = new ErrorDetail();

        private Visibility _addButtonVisibility = Visibility.Hidden;
        private Visibility _screenShotButtonVisibility = Visibility.Visible;
     
        private WindowState _wState = WindowState.Normal;
        
        public string DatabaseToView
        {
            get { return _databaseToView; }
            set
            {
                _databaseToView = value;
                OnPropertyChanged("DatabaseToView");
            }
        }
        
        public string DatabaseToShow
        {
            get { return _databaseToShow; }
            set
            {
                _databaseToShow = value;
                OnPropertyChanged("DatabaseToShow");
            }
        }
        
        public string XlsToExport
        {
            get { return _xlsToExport; }
            set
            {
                _xlsToExport = value;
                OnPropertyChanged("XlsToExport");
            }
        }
        
        public string XlsToView
        {
            get { return _xlsToView; }
            set
            {
                _xlsToView = value;
                OnPropertyChanged("XlsToView");
            }
        }
        
        public int ErrorId
        {
            get => _errorId;
            set
            {
                _errorId = value;
                OnPropertyChanged("ErrorId");
            }
        }
        
        public Visibility AddButtonVisibility
        {
            get { return _addButtonVisibility; }
            set
            {
                _addButtonVisibility = value;
                OnPropertyChanged("AddButtonVisibility");
            }
        }
        
        public Visibility ScreenShotButtonVisibility
        {
            get { return _screenShotButtonVisibility; }
            set
            {
                _screenShotButtonVisibility = value;
                OnPropertyChanged("ScreenShotButtonVisibility");
            }
        }
        
        public ErrorDetail Error
        {
            get { return _errorDetail; }
            set
            {
                _errorDetail = value;
                OnPropertyChanged("Error");
            }
        }
        public WindowState WState
        {
            get { return _wState; }
            set
            {
                _wState = value;
                OnPropertyChanged("WState");
            }
        }
        
       
        
 


        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
        
        
        private ICommand _fixCommand;

        public ICommand FixCommand
        {
            get
            {
                if (_fixCommand == null)
                {
                    _fixCommand = new RelayCommand(
                        param => this.FixObject(), 
                        param => this.CanLoad()
                    );
                }
                return _fixCommand;
            }
        }
        
        private ICommand _loadCommand;

        public ICommand LoadCommand
        {
            get
            {
                if (_loadCommand == null)
                {
                    _loadCommand = new RelayCommand(
                        param => this.LoadObject(), 
                        param => this.CanLoad()
                    );
                }
                return _loadCommand;
            }
        }
        
        private ICommand _cancelCommand;

        public ICommand CancelCommand
        {
            get
            {
                if (_cancelCommand == null)
                {
                    _cancelCommand = new RelayCommand(
                        param => this.CancelObject(), 
                        param => this.CanLoad()
                    );
                }
                return _cancelCommand;
            }
        }
        
        private ICommand _saveCommand;
        public ICommand SaveCommand
        {
            get
            {
                if (_saveCommand == null)
                {
                    _saveCommand = new RelayCommand(
                        param => this.SaveObject(), 
                        param => this.CanSave()
                    );
                }
                return _saveCommand;
            }
        }
        
        
        private ICommand _changeDatabaseCommand;
        public ICommand ChangeDatabaseCommand
        {
            get
            {
                if (_changeDatabaseCommand == null)
                {
                    _changeDatabaseCommand = new RelayCommand(
                        param => this.ChangeDBObject(), 
                        param => this.CanSave()
                    );
                }
                return _changeDatabaseCommand;
            }
        }
        
        private ICommand _selectXlsFileCommand;
        public ICommand SelectXlsFileCommand
        {
            get
            {
                if (_selectXlsFileCommand == null)
                {
                    _selectXlsFileCommand = new RelayCommand(
                        param => this.ExportToXlsxFile(), 
                        param => this.CanSave()
                    );
                }
                return _selectXlsFileCommand;
            }
        }

        private bool CanLoad()
        {
            return true;
        }
        
        private bool CanSave()
        {
           
            return true;
        }

        private void FixObject()
        {
            if (Error.RouteName.Contains("Задайте маршрут"))
            {
                MessageBox.Show("Необходимо задать маршрут");
                return;
            }

            Error.Id = -1;
            Error.Comment = "Добавь комментарий";
            WState = WindowState.Minimized;
            Error.ImageVisibility = Visibility.Visible;
            Error.TimeStamp = DateTime.Now.ToString("ddMMyyyy-hhmmss", CultureInfo.InvariantCulture);
            ScreenShotButtonVisibility = Visibility.Collapsed;

            if (File.Exists(_positionFilePath))
            {
                Error.Position = File.ReadAllText(_positionFilePath, Encoding.UTF8);
            }

            String filename1 = "ScreenCapture1-" + DateTime.Now.ToString("ddMMyyyy-hhmmss") + ".png";
            String filename2 = "ScreenCapture2-" + DateTime.Now.ToString("ddMMyyyy-hhmmss") + ".png";

            int screenLeft = (int) SystemParameters.VirtualScreenLeft ;
            int screenTop = (int) SystemParameters.VirtualScreenTop;
            int screenWidth = (int)SystemParameters.VirtualScreenWidth;
            int screenHeight = (int) SystemParameters.VirtualScreenHeight;

            //MessageBox.Show($"Height = {screenHeight} Width = {screenWidth}");

            Bitmap bitmapScreen1 = new Bitmap(screenWidth/2, screenHeight);
            Bitmap bitmapScreen2 = new Bitmap(screenWidth/2, screenHeight);

            Graphics g1 = Graphics.FromImage(bitmapScreen1); 
            Graphics g2 = Graphics.FromImage(bitmapScreen2);    

            g1.CopyFromScreen(screenLeft, screenTop, 0, 0, bitmapScreen1.Size);
            Error.BImageV = ImageUtils.BitmapToImageSource(bitmapScreen1);
            Error.ImageV = bitmapScreen1;

            Bitmap testV = ImageUtils.ResizeImage(Error.ImageV, 1024);
            Error.ImageV = testV;
            //testV.Save("D:\\temp\\" + filename1);
            

            g2.CopyFromScreen(screenWidth/2, 0, 0, 0, bitmapScreen2.Size);
            Error.BImageM = ImageUtils.BitmapToImageSource(bitmapScreen2);
            Error.ImageM = bitmapScreen2;
            
            Bitmap testM = ImageUtils.ResizeImage(Error.ImageM, 1024);
            Error.ImageM = testM;
            
            WState = WindowState.Maximized;
            AddButtonVisibility = Visibility.Visible;


        }
        
        private void SaveObject()
        {
            if (Error.Comment.Contains("Добавь комментарий"))
            {
                MessageBox.Show("Необходимо заполнить комментарий");
                
            }
            else
            {
                _sqLiteManager.AddErrorToDB(_errorDetail);
                WState = WindowState.Normal;
                AddButtonVisibility = Visibility.Hidden;
                Error.ImageVisibility = Visibility.Hidden;
                ScreenShotButtonVisibility = Visibility.Visible;
                Error.Comment = "Добавь комментарий";
            }
            
        }
        
        
        private void LoadObject()
        {
            if (ErrorId < 0)
            {
                MessageBox.Show("Id должен быть больше -1");
            }
            if (DatabaseToView == String.Empty)
            {
                ChangeDBObject();
            }
            if (DatabaseToView != String.Empty)
            {
                Error.ImageM?.Dispose();
                Error.ImageV?.Dispose();
                Error.BImageM?.StreamSource?.Dispose();
                Error.BImageV?.StreamSource?.Dispose();
                _sqLiteManager.LoadError(Error, DatabaseToView, ErrorId);
                //WState = WindowState.Maximized;
                Error.ImageVisibility = Visibility.Visible;
                if (Directory.Exists(_trainerPath))
                {
                    File.WriteAllText(_positionFilePathSetup, Error.Position, Encoding.ASCII);
                }
            }
        }

        private void ChangeDBObject()
        {
            string databaseName = String.Empty;
            OpenFileDialog pfg = new OpenFileDialog();
            
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = false;
            openFileDialog.Filter = "SQLite files (*.db3)|*.db3";
            openFileDialog.InitialDirectory = $"{Directory.GetCurrentDirectory()}\\RouteErrors";
            if(openFileDialog.ShowDialog() == true)
            {
                DatabaseToView = openFileDialog.FileName;
                DatabaseToShow = Path.GetFileName(DatabaseToView);
            }
        }
        
        private void ExportToXlsxFile()
        {
                if (DatabaseToView != String.Empty)
                {
                    XlsToExport = $"{Directory.GetCurrentDirectory()}\\RouteErrors\\{Path.GetFileNameWithoutExtension(DatabaseToView)}.xlsx";
                    SpreadsheetDocument document = ExcelTools.OpenDocument(XlsToExport, "Sheet1",out var workbookPart, out var worksheetPart);
                    List<ErrorDetail> errors = _sqLiteManager.LoadErrors(DatabaseToView);
                    uint i = 1;
                    foreach (var error in errors)
                    {
                        using (var imageStream =
                               new MemoryStream(ImageUtils.ImageToByte(error.ImageV, ImageFormat.Jpeg)))
                        {
                            ExcelTools.AddImage(worksheetPart, imageStream, "", 1, (int)i);
                            imageStream.Close();
                        }

                        using (var imageStream =
                               new MemoryStream(ImageUtils.ImageToByte(error.ImageM, ImageFormat.Jpeg)))
                        {
                            ExcelTools.AddImage(worksheetPart, imageStream, "", 2, (int)i);
                            imageStream.Close();
                        }

                        ExcelTools.InsertText(workbookPart, worksheetPart, error.Id.ToString(), "C", i);
                        ExcelTools.InsertText(workbookPart, worksheetPart, error.Comment, "D", i);
                        ExcelTools.InsertText(workbookPart, worksheetPart, error.Position, "E", i);
                        ExcelTools.InsertText(workbookPart, worksheetPart, error.RouteName, "F", i);
                        ExcelTools.InsertText(workbookPart, worksheetPart, error.TimeStamp, "G", i);
                        XlsToView = $"Добавлено ошибок {i} из {errors.Count}";
                            i++;
                    }
                    errors.Clear();
                    ExcelTools.CloseDocument(document, worksheetPart);
                }
           
        }
        
        private void CancelObject()
        {
            WState = WindowState.Normal;
            AddButtonVisibility = Visibility.Hidden;
            Error.ImageVisibility = Visibility.Hidden;
            ScreenShotButtonVisibility = Visibility.Visible;
            Error.Comment = "Добавь комментарий";
            Error.ImageM?.Dispose();
            Error.ImageV?.Dispose();
            Error.BImageM?.StreamSource.Dispose();
            Error.BImageV?.StreamSource.Dispose();
        }
        
    }
}