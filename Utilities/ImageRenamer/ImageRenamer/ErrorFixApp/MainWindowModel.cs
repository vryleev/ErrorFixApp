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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using DocumentFormat.OpenXml.Packaging;
using ErrorFixApp.Properties;
using ImageToXlsx;
using Microsoft.Win32;

namespace ErrorFixApp
{
    public class MainWindowModel: INotifyPropertyChanged
    {
        public MainWindowModel()
        {
            _sqLiteManager = new SqLiteManager();
        }

        private readonly SqLiteManager _sqLiteManager;

        private static readonly string TrainerPath        = ConfigurationManager.AppSettings.Get("TrainerPath");
        private static readonly string SceneGeneratorPath = ConfigurationManager.AppSettings.Get("SceneGeneratorPath");
        private static readonly string FileNamePos        = ConfigurationManager.AppSettings.Get("FileNamePos");
        
        private readonly string _positionFilePath        = $"{TrainerPath}/{FileNamePos}";
        private readonly string _positionFilePathSetup   = $"{TrainerPath}/{FileNamePos}_setup"; 
        private readonly string _positionFilePathSgSetup = $"{SceneGeneratorPath}/{FileNamePos}_setup"; 
        
        private string _databaseToView = string.Empty;
        private string _databaseToShow = string.Empty;
        private string _xlsToExport    = string.Empty;
        private string _xlsToView      = string.Empty;

        private int _errorId = -1;
        
        private ErrorDetail _errorDetail               = new ErrorDetail();
        private Visibility _addButtonVisibility        = Visibility.Hidden;
        private Visibility _screenShotButtonVisibility = Visibility.Visible;
     
        private WindowState _wState = WindowState.Normal;

        private string DatabaseToView
        {
            get => _databaseToView;
            set
            {
                _databaseToView = value;
                OnPropertyChanged();
            }
        }
        
        public string DatabaseToShow
        {
            get => _databaseToShow;
            set
            {
                _databaseToShow = value;
                OnPropertyChanged();
            }
        }

        private string XlsToExport
        {
            get => _xlsToExport;
            set
            {
                _xlsToExport = value;
                OnPropertyChanged();
            }
        }
        
        public string XlsToView
        {
            get => _xlsToView;
            set
            {
                _xlsToView = value;
                OnPropertyChanged();
            }
        }
        
        public int ErrorId
        {
            get => _errorId;
            set
            {
                _errorId = value;
                OnPropertyChanged();
            }
        }
        
        public Visibility AddButtonVisibility
        {
            get => _addButtonVisibility;
            set
            {
                _addButtonVisibility = value;
                OnPropertyChanged();
            }
        }
        
        public Visibility ScreenShotButtonVisibility
        {
            get => _screenShotButtonVisibility;
            set
            {
                _screenShotButtonVisibility = value;
                OnPropertyChanged();
            }
        }
        
        public ErrorDetail Error
        {
            get => _errorDetail;
            set
            {
                _errorDetail = value;
                OnPropertyChanged();
            }
        }
        public WindowState WState
        {
            get => _wState;
            set
            {
                _wState = value;
                OnPropertyChanged();
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
        
        
        private ICommand _fixCommand;

        public ICommand FixCommand
        {
            get
            {
                return _fixCommand ?? (_fixCommand = new RelayCommand(
                    param => this.FixObject(),
                    param => this.CanLoad()
                ));
            }
        }
        
        private ICommand _loadCommand;

        public ICommand LoadCommand
        {
            get
            {
                return _loadCommand ?? (_loadCommand = new RelayCommand(
                    param => this.LoadObject(),
                    param => this.CanLoad()
                ));
            }
        }
        
        private ICommand _cancelCommand;

        public ICommand CancelCommand
        {
            get
            {
                return _cancelCommand ?? (_cancelCommand = new RelayCommand(
                    param => this.CancelObject(),
                    param => this.CanLoad()
                ));
            }
        }
        
        private ICommand _saveCommand;
        public ICommand SaveCommand
        {
            get
            {
                return _saveCommand ?? (_saveCommand = new RelayCommand(
                    param => this.SaveObject(),
                    param => this.CanSave()
                ));
            }
        }
        
        private ICommand _changeDatabaseCommand;
        public ICommand ChangeDatabaseCommand
        {
            get
            {
                return _changeDatabaseCommand ?? (_changeDatabaseCommand = new RelayCommand(
                    param => this.ChangeDbObject(),
                    param => this.CanSave()
                ));
            }
        }
        
        private ICommand _selectXlsFileCommand;
        public ICommand SelectXlsFileCommand
        {
            get
            {
                return _selectXlsFileCommand ?? (_selectXlsFileCommand = new RelayCommand(
                    param => this.ExportToXlsxFileTask(),
                    param => this.CanSave()
                ));
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
            if (Error.RouteName.Contains(Resources.SetupRoute))
            {
                MessageBox.Show(Resources.SetupRouteMessage);
                return;
            }

            Error.Id = -1;
            Error.Comment = Resources.AddComment;
            WState = WindowState.Minimized;
            Error.ImageVisibility = Visibility.Visible;
            Error.TimeStamp = DateTime.Now.ToString("ddMMyyyy-hhmmss", CultureInfo.InvariantCulture);
            ScreenShotButtonVisibility = Visibility.Collapsed;

            if (File.Exists(_positionFilePath))
            {
                Error.Position = File.ReadAllText(_positionFilePath, Encoding.UTF8);
            }

            //String filename1 = "ScreenCapture1-" + DateTime.Now.ToString("ddMMyyyy-hhmmss") + ".png";
            //String filename2 = "ScreenCapture2-" + DateTime.Now.ToString("ddMMyyyy-hhmmss") + ".png";

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
            if (Error.Comment.Contains(Resources.AddComment))
            {
                MessageBox.Show(Resources.CommentMessage);
                
            }
            else
            {
                _sqLiteManager.AddErrorToDb(_errorDetail);
                WState = WindowState.Normal;
                AddButtonVisibility = Visibility.Hidden;
                Error.ImageVisibility = Visibility.Hidden;
                ScreenShotButtonVisibility = Visibility.Visible;
                Error.Comment = Resources.AddComment;
            }
            
        }
        
        private void LoadObject()
        {
            if (ErrorId < 0)
            {
                MessageBox.Show(Resources.IdMessage);
            }
            if (DatabaseToView == String.Empty)
            {
                ChangeDbObject();
            }
            if (DatabaseToView != String.Empty)
            {
                Error.ImageM?.Dispose();
                Error.ImageV?.Dispose();
                Error.BImageM?.StreamSource?.Dispose();
                Error.BImageV?.StreamSource?.Dispose();
                _sqLiteManager.LoadError(Error, DatabaseToView, ErrorId);
                Error.ImageVisibility = Visibility.Visible;
                if (Directory.Exists(TrainerPath))
                {
                    File.WriteAllText(_positionFilePathSetup, Error.Position, Encoding.ASCII);
                }
                if (Directory.Exists(SceneGeneratorPath))
                {
                    File.WriteAllText(_positionFilePathSgSetup, Error.Position, Encoding.ASCII);
                }
            }
        }

        private void ChangeDbObject()
        {   
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Multiselect = false,
                Filter = "SQLite files (*.db3)|*.db3",
                InitialDirectory = $"{Directory.GetCurrentDirectory()}\\RouteErrors"
            };

            if (openFileDialog.ShowDialog() != true) return;
            
            DatabaseToView = openFileDialog.FileName;
            DatabaseToShow = Path.GetFileName(DatabaseToView);
            XlsToView = $"{Resources.TotalErrors}: {_sqLiteManager.GetErrorCount(DatabaseToView)}";
        }

        private void ExportToXlsxFileTask()
        {
            Task.Factory.StartNew(ExportToXlsxFile);
        }
        
        private void ExportToXlsxFile()
        {
            if (DatabaseToView != String.Empty)
            {
                XlsToExport =
                    $"{Directory.GetCurrentDirectory()}\\RouteErrors\\{Path.GetFileNameWithoutExtension(DatabaseToView)}.xlsx";
                SpreadsheetDocument document = ExcelTools.OpenDocument(XlsToExport, "Sheet1", out var workbookPart,
                    out var worksheetPart);
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

                    XlsToView = $"{Resources.AddedErrors} {i} из {errors.Count}";

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
            Error.Comment = Resources.AddComment;
            Error.ImageM?.Dispose();
            Error.ImageV?.Dispose();
            Error.BImageM?.StreamSource.Dispose();
            Error.BImageV?.StreamSource.Dispose();
        }
        
    }
}