using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using DocumentFormat.OpenXml.Packaging;
using ErrorDataLayer;
using ErrorFixApp.Properties;
using ImageToXlsx;
using PhotoEditor;
using MessageBox = System.Windows.MessageBox;


namespace ErrorFixApp
{
    public class MainWindowModel : INotifyPropertyChanged
    {
        public MainWindowModel()
        {
            _sqLiteManager = new SqLiteManager();
            _webApiManager = new WebApiManager();
            GetDbList();
            GetErrorTypeList();
        }

        private async void GetDbList()
        {
            List<string> dbList;
            if (ConfigurationManager.AppSettings["WorkingType"] == "Local")
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

        private async void GetErrorTypeList()
        {
            var errorTypeList = ConfigurationManager.AppSettings.Get("ErrorTypes").Split(',').ToList();
            var priorityList = ConfigurationManager.AppSettings.Get("Priority").Split(',').ToList();

            if (errorTypeList.Count == 0)
            {
                errorTypeList.Add(Resources.NoDb);
            }
          
            ErrorTypeList = new ObservableCollection<string>(errorTypeList);
            SelectedErrorType = ErrorTypeList.First();
            
            PriorityList = new ObservableCollection<string>(priorityList);
            SelectedPriority = PriorityList.First();
        }

        private readonly SqLiteManager _sqLiteManager;
        private readonly WebApiManager _webApiManager;

        private static readonly string TrainerPath = ConfigurationManager.AppSettings.Get("TrainerPath");
        private static readonly string SceneGeneratorPath = ConfigurationManager.AppSettings.Get("SceneGeneratorPath");
        private static readonly string FileNamePos = ConfigurationManager.AppSettings.Get("FileNamePos");

        private readonly string _positionFilePath = $"{TrainerPath}/{FileNamePos}";
        private readonly string _positionFilePathSetup = $"{TrainerPath}/{FileNamePos}_setup";
        private readonly string _positionFilePathSgSetup = $"{SceneGeneratorPath}/{FileNamePos}_setup";

        private string _selectedDbName;
        private string _selectedErrorType;
        private string _selectedPriority;
        private ObservableCollection<string> _dbList = new ObservableCollection<string>();
        private ObservableCollection<string> _errorTypeList = new ObservableCollection<string>();
        private ObservableCollection<string> _priorityList = new ObservableCollection<string>();

        private string _databaseToView = string.Empty;
        private string _xlsToExport = string.Empty;
        private string _xlsToView = string.Empty;

        private RenderTargetBitmap _rtb;

        private int _errorId = -1;

        private ErrorDetail _errorDetail = new ErrorDetail();
        private ErrorEntity _errorEntity = new ErrorEntity();
        private bool _isComboEnabled = true;
        private Visibility _addButtonVisibility = Visibility.Hidden;
        private Visibility _screenShotButtonVisibility = Visibility.Visible;

        private WindowState _wState = WindowState.Normal;

        private string DatabaseToView
        {
            get => _databaseToView;
            set
            {
                _databaseToView = value;
                if (ConfigurationManager.AppSettings["WorkingType"] == "Local")
                {
                    _sqLiteManager.SetBaseName(_databaseToView);
                    XlsToView =
                        $"{Resources.TotalErrors}: {_sqLiteManager.GetErrorCount()}, {Resources.MaxId}: {_sqLiteManager.GetMaxId()}";
                }
                else
                {
                    UpdateErrorCount();
                }

                OnPropertyChanged();
            }
        }

        private async void UpdateErrorCount()
        {
            int errorCount = await _webApiManager.GetErrorCount(SelectedDb);
            XlsToView = $"{Resources.TotalErrors}: {errorCount}";
        }

        public string SelectedDb
        {
            get => _selectedDbName;
            set
            {
                _selectedDbName = value;
                DatabaseToView = _selectedDbName;
                OnPropertyChanged();
            }
        }

        public string SelectedErrorType
        {
            get => _selectedErrorType;
            set
            {
                _selectedErrorType = value;

                OnPropertyChanged();
            }
        }
        
        public string SelectedPriority
        {
            get => _selectedPriority;
            set
            {
                _selectedPriority = value;

                OnPropertyChanged();
            }
        }
        
        

        public ObservableCollection<string> DbList
        {
            get => _dbList;
            private set
            {
                _dbList = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<string> ErrorTypeList
        {
            get => _errorTypeList;
            private set
            {
                _errorTypeList = value;
                OnPropertyChanged();
            }
        }
        
        public ObservableCollection<string> PriorityList
        {
            get => _priorityList;
            private set
            {
                _priorityList = value;
                OnPropertyChanged();
            }
        }

        public string ApplicationName
        {
            get
            {
                if (ConfigurationManager.AppSettings["WorkingType"] == "Local")
                {
                    return
                        $"Logos Error Fix App/ {Resources.User}: {ConfigurationManager.AppSettings["User"]}/ {Resources.WorkingType}: {ConfigurationManager.AppSettings["WorkingType"]}";
                }
                else
                {
                    return
                        $"Logos Error Fix App/ {Resources.User}: {ConfigurationManager.AppSettings["User"]}/ {Resources.WorkingType}: {ConfigurationManager.AppSettings["WorkingType"]}/ Host: {ConfigurationManager.AppSettings["RemoteUrl"]}";
                }
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
        
        public bool IsComboEnabled
        {
            get => _isComboEnabled;
            set
            {
                _isComboEnabled = value;
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

        private void OnPropertyChanged([CallerMemberName] string prop = "")
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

        private ICommand _deleteCommand;

        public ICommand DeleteCommand
        {
            get
            {
                return _deleteCommand ?? (_deleteCommand = new RelayCommand(
                    param => this.DeleteObject(),
                    param => this.CanLoad()
                ));
            }
        }

        private ICommand _closingCommand;

        public ICommand ClosingCommand
        {
            get
            {
                return _closingCommand ?? (_closingCommand = new RelayCommand(
                    param => this.ClosingDb(),
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

        private ICommand _editImageCommand;

        public ICommand EditImageCommand
        {
            get
            {
                return _editImageCommand ?? (_editImageCommand = new RelayCommand(
                    param => this.EditImage(Convert.ToString(param)),
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

        public bool CanLoad()
        {
            return true;
        }

        private bool CanSave()
        {
            return true;
        }

        public void ClosingDb()
        {
            SqLiteManager.IsCheckQueue = false;
        }

        private async void DeleteObject()
        {
            if (Error.Id > 0)
            {
                MessageBoxResult dialogResult = MessageBox.Show(Resources.DeleteMessage, Resources.DeleteCaption,
                    MessageBoxButton.YesNo);
                if (dialogResult == MessageBoxResult.Yes)
                {
                    if (ConfigurationManager.AppSettings.Get("WorkingType") == "Local")
                    {
                        _sqLiteManager.DeleteErrorFromDb(Error.Id);
                    }
                    else
                    {
                        await _webApiManager.DeleteFromDb(Error.Id, SelectedDb);
                    }

                    WState = WindowState.Normal;
                    AddButtonVisibility = Visibility.Hidden;
                    Error.Id = 0;
                    Error.ImageVisibility = Visibility.Hidden;
                    ScreenShotButtonVisibility = Visibility.Visible;
                    Error.Comment = Resources.AddComment;
                    GetErrorTypeList();
                }

            }
            else
            {
                MessageBox.Show(Resources.DeleteNotPossible, Resources.DeleteCaption, MessageBoxButton.OK);
            }
        }

        private void FixObject()
        {
            if (Error.RouteName.Contains(Resources.SetupRoute))
            {
                MessageBox.Show(Resources.SetupRouteMessage);
                return;
            }

            if (Error.RouteName.Contains(' ') || Error.RouteName.Contains('.'))
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
            ScreenShotButtonVisibility = Visibility.Collapsed;

            if (File.Exists(_positionFilePath))
            {
                Error.Position = File.ReadAllText(_positionFilePath, Encoding.UTF8);
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
            ParseRectConfiguration(visualRect, screenLeft, screenTop, screenWidth, screenHeight, ref visualLeft,
                ref visualTop, ref visualWidth, ref visualHeight);

            string mapRect = ConfigurationManager.AppSettings.Get("MapRect");
            ParseRectConfiguration(mapRect, screenWidth / 2, screenTop, screenWidth, screenHeight, ref mapLeft,
                ref mapTop, ref mapWidth, ref mapHeight);

            Bitmap bitmapScreen1 = new Bitmap(visualWidth, visualHeight);
            Bitmap bitmapScreen2 = new Bitmap(mapWidth, mapHeight);

            Graphics g1 = Graphics.FromImage(bitmapScreen1);
            Graphics g2 = Graphics.FromImage(bitmapScreen2);

            g1.CopyFromScreen(visualLeft, visualTop, 0, 0, bitmapScreen1.Size);
            Error.BImageV = ImageUtils.BitmapToImageSource(bitmapScreen1);
            Error.ImageV = bitmapScreen1;

            Bitmap testV = ImageUtils.ResizeImage(Error.ImageV, 1024);
            Error.ImageV = testV;

            g2.CopyFromScreen(mapLeft, mapTop, 0, 0, bitmapScreen2.Size);
            Error.BImageM = ImageUtils.BitmapToImageSource(bitmapScreen2);
            Error.ImageM = bitmapScreen2;

            Bitmap testM = ImageUtils.ResizeImage(Error.ImageM, 1024);
            Error.ImageM = testM;

            WState = WindowState.Maximized;
            AddButtonVisibility = Visibility.Visible;
            IsComboEnabled = true;
        }

        private static void ParseRectConfiguration(string visualRect, int screenLeft, int screenTop, int screenWidth,
            int screenHeight, ref int visualLeft, ref int visualTop, ref int visualWidth, ref int visualHeight)
        {
            if (visualRect.Length > 0)
            {
                string[] visualParams = visualRect.Split(',');
                if (visualParams.Length == 4)
                {
                    try
                    {
                        visualLeft = Int32.Parse(visualParams[0]);
                        visualTop = Int32.Parse(visualParams[1]);
                        visualWidth = Int32.Parse(visualParams[2]);
                        visualHeight = Int32.Parse(visualParams[3]);
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show($"Rect is not corrected {e.Message}");
                        visualLeft = screenLeft;
                        visualTop = screenTop;
                        visualWidth = screenWidth / 2;
                        visualHeight = screenHeight;
                    }
                }
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
                UpdateErrorEntity();
                if (ConfigurationManager.AppSettings.Get("WorkingType") == "Local")
                {
                    _sqLiteManager.AddErrorToDb(_errorEntity);
                    GetDbList();

                    SelectedDb = _sqLiteManager.GetDbToAdd();
                }
                else
                {
                    await _webApiManager.AddError(_errorEntity);
                    SelectedDb = await _webApiManager.GetDbToAdd();
                }

                WState = WindowState.Normal;
                AddButtonVisibility = Visibility.Hidden;
                Error.ImageVisibility = Visibility.Hidden;
                ScreenShotButtonVisibility = Visibility.Visible;
                Error.Comment = Resources.AddComment;
                GetErrorTypeList();
            }
        }

        private void UpdateErrorEntity()
        {
            _errorEntity.Comment = Error.Comment;
            _errorEntity.Position = Error.Position;
            _errorEntity.RouteName = Error.RouteName;
            _errorEntity.Id = Error.Id;
            _errorEntity.TimeStamp = Error.TimeStamp;
            _errorEntity.User = ConfigurationManager.AppSettings["User"];
            _errorEntity.ErrorType = SelectedErrorType;
            _errorEntity.Priority = SelectedPriority;

            _errorEntity.ImageV = ImageUtils.ImageToByte(Error.ImageV, ImageFormat.Jpeg);
            _errorEntity.ImageM = ImageUtils.ImageToByte(Error.ImageM, ImageFormat.Jpeg);
        }

        private async void LoadObject()
        {
            if (ErrorId < 0)
            {
                MessageBox.Show(Resources.IdMessage);
            }

            if (DatabaseToView != String.Empty)
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

                if (UpdateErrorDetails())
                {

                    if (Directory.Exists(TrainerPath))
                    {
                        File.WriteAllText(_positionFilePathSetup, Error.Position, Encoding.ASCII);
                    }

                    if (Directory.Exists(SceneGeneratorPath))
                    {
                        File.WriteAllText(_positionFilePathSgSetup, Error.Position, Encoding.ASCII);
                    }
                }

                IsComboEnabled = false;
            }
        }

        private bool UpdateErrorDetails()
        {
            bool res = false;
            if (_errorEntity != null && _errorEntity.Id > 0)
            {
                Error.Position = _errorEntity.Position;
                Error.User = _errorEntity.User;
                SelectedErrorType = _errorEntity.ErrorType;
                SelectedPriority = _errorEntity.Priority;
                Error.Comment = _errorEntity.Comment;
                Error.Id = _errorEntity.Id;
                Error.RouteName = _errorEntity.RouteName;
                Error.TimeStamp = _errorEntity.TimeStamp;
                Error.ImageV = ImageUtils.ByteToImage(_errorEntity.ImageV);
                Error.ImageM = ImageUtils.ByteToImage(_errorEntity.ImageM);
                Error.BImageV = ImageUtils.BitmapToImageSource(new Bitmap(Error.ImageV));
                Error.BImageM = ImageUtils.BitmapToImageSource(new Bitmap(Error.ImageM));
                Error.ImageVisibility = Visibility.Visible;
                
                res = true;
            }
            else
            {
                Error.Id = 0;
                Error.ImageVisibility = Visibility.Collapsed;
            }

            return res;
        }

        private void EditImage(string pictureType)
        {
            var editorWindow = new MainEditorWindow();

            editorWindow.Closing += EditorWindowOnClosing;

            double layerWidth = Error.ImageV.Width;
            double layerHeight = Error.ImageV.Height;

            GlobalState.NewLayerHeight = layerHeight;
            GlobalState.NewLayerWidth = layerWidth;
            GlobalState.CurrentLayerIndex = 0;
            GlobalState.LayersCount = 1;
            GlobalState.CurrentTool = GlobalState.Instruments.Brush;

            MainEditorWindow.WindowTrigger = 4;
            MainEditorWindow.PictureType = pictureType;
            MainEditorWindow.Picture = pictureType == "Visual"
                ? Error.ImageV.ToStream(ImageFormat.Bmp)
                : Error.ImageM.ToStream(ImageFormat.Bmp);

            MainEditorWindow.EnableBlur(editorWindow);
            MainEditorWindow.ShowMainWindow();
        }

        private void EditorWindowOnClosing(object sender, CancelEventArgs e)
        {
            _rtb = MainEditorWindow.RTB;

            var enc = new PngBitmapEncoder();
            enc.Frames.Add(BitmapFrame.Create(_rtb));

            using (MemoryStream stm = new MemoryStream())
            {
                enc.Save(stm);
                Bitmap editedImage = new Bitmap(stm);
                if (MainEditorWindow.PictureType == "Visual")
                {
                    Error.ImageV = editedImage;
                    Error.BImageV = ImageUtils.BitmapToImageSource(editedImage);
                }
                else
                {
                    Error.ImageM = editedImage;
                    Error.BImageM = ImageUtils.BitmapToImageSource(editedImage);
                }
            }
        }


        private void ExportToXlsxFileTask()
        {
            Task.Factory.StartNew(ExportToXlsxFile);
        }

        private async void ExportToXlsxFile()
        {
            try
            {
                if (DatabaseToView != String.Empty)
                {
                    List<ErrorEntity> errors;
                    if (ConfigurationManager.AppSettings.Get("WorkingType") == "Local")
                    {
                        errors = _sqLiteManager.LoadErrors();
                    }
                    else
                    {
                        errors = await _webApiManager.GetAllErrors(SelectedDb);
                    }

                    List<string> routeList = new List<string>();

                    foreach (var er in errors)
                    {
                        if (!routeList.Contains(er.RouteName))
                        {
                            routeList.Add(er.RouteName);
                        }
                    }



                    List<string> errorTypeList = new List<string>();

                    foreach (var er in errors)
                    {
                        if (!errorTypeList.Contains(er.ErrorType))
                        {
                            errorTypeList.Add(er.ErrorType);
                        }
                    }

                    var databaseDate = Path.GetFileNameWithoutExtension(DatabaseToView);
                    var dirToExport = $"{Directory.GetCurrentDirectory()}\\RouteErrors\\{databaseDate}";
                    CheckDirectoryToExport(dirToExport);


                    foreach (var errorType in errorTypeList)
                    {
                        var errorTypeToSeparateList = ConfigurationManager.AppSettings.Get("ErrorTypesSeparateByRoutes")
                            .Split(',').ToList();

                        if (errorTypeToSeparateList.Contains(errorType))
                        {
                            foreach (var route in routeList)
                            {
                                XlsToExport =
                                    $"{dirToExport}\\{route}_{errorType}_{databaseDate}.xlsx";

                                ExportToXlsSeparatedRoutes(route, errors, errorType, XlsToExport);
                            }
                        }
                        else
                        {
                            XlsToExport =
                                $"{dirToExport}\\{errorType}_{databaseDate}.xlsx";

                            ExportToXlsNotSeparatedRoutes(errorType, errors, XlsToExport);
                        }
                    }

                    errors.Clear();
                    string argument = $"/select, \"{SqLiteManager.BaseDir}\\{SelectedDb}.db3";

                    System.Diagnostics.Process.Start("explorer.exe", argument);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ExportToXlsNotSeparatedRoutes(string errorType, List<ErrorEntity> errors, string xlsToExport)
        {
            if (errors.Count > 0)
            {
                var errorWithType = errors.Where(e => e.ErrorType == errorType).ToList();
                if (errorWithType.Count > 0)
                {
                    SpreadsheetDocument document = ExcelTools.OpenDocument(xlsToExport, "Sheet1",
                        out var workbookPart,
                        out var worksheetPart);
                    AddErrorsToXls(errorWithType, worksheetPart, workbookPart, errorType);
                    ExcelTools.CloseDocument(document, worksheetPart);
                }
            }
        }

        private void CheckDirectoryToExport(string dirToExport)
        {
            try
            {
                if (!Directory.Exists(dirToExport))
                {
                    Directory.CreateDirectory(dirToExport);
                }
                else
                {

                    Directory.Delete(dirToExport,true);
                    Directory.CreateDirectory(dirToExport);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ExportToXlsSeparatedRoutes(string route, List<ErrorEntity> errors, string errorType,
            string xlsToExport)
        {
            if (errors.Count > 0)
            {
                var routeErrors = errors.Where(e => e.RouteName == route && e.ErrorType == errorType).ToList();

                if (routeErrors.Count > 0)
                {

                    SpreadsheetDocument document = ExcelTools.OpenDocument(xlsToExport, "Sheet1",
                        out var workbookPart,
                        out var worksheetPart);
                    AddErrorsToXls(routeErrors, worksheetPart, workbookPart, route);
                    ExcelTools.CloseDocument(document, worksheetPart);
                }
            }

           
        }

    

        private void AddErrorsToXls(List<ErrorEntity> routeErrors, WorksheetPart worksheetPart, WorkbookPart workbookPart, string route)
        {
            uint i = 1;
            foreach (var error in routeErrors)
            {
                using (var imageStream =
                       new MemoryStream(error.ImageV))
                {
                    ExcelTools.AddImage(worksheetPart, imageStream, "", 1, (int)i);
                    imageStream.Close();
                }

                using (var imageStream =
                       new MemoryStream(error.ImageM))
                {
                    ExcelTools.AddImage(worksheetPart, imageStream, "", 2, (int)i);
                    imageStream.Close();
                }

                string[] positionParams = error.Position.Split(' ');

                string positionToXls = error.Position;

                MPoint pos = new MPoint();
                double x = 0.0;
                double y = 0.0;
                
                if (positionParams.Length == 8)
                {
                    //positionToXls = $"{positionParams[3]};{positionParams[4]};{positionParams[5]}";
                    positionToXls = $"{positionParams[3]};{positionParams[4]};20";
                    if (Double.TryParse(positionParams[3], out x) &&
                        Double.TryParse(positionParams[4], out y))
                    {
                        pos = SphericalMercator.FromUnigineToLonLat(x, y);
                    }
                }

                ExcelTools.InsertText(workbookPart, worksheetPart, error.Id.ToString(), "C", i);
                ExcelTools.InsertText(workbookPart, worksheetPart, error.Comment, "D", i);
                ExcelTools.InsertText(workbookPart, worksheetPart, positionToXls, "E", i);
                ExcelTools.InsertText(workbookPart, worksheetPart, error.RouteName, "F", i);
                ExcelTools.InsertText(workbookPart, worksheetPart, error.TimeStamp, "G", i);
                ExcelTools.InsertText(workbookPart, worksheetPart, error.User, "H", i);
                ExcelTools.InsertText(workbookPart, worksheetPart, error.Position, "I", i);
                ExcelTools.InsertText(workbookPart, worksheetPart, $"{pos.Y},{pos.X}", "J", i);
                ExcelTools.InsertText(workbookPart, worksheetPart, error.ErrorType, "K", i);
                ExcelTools.InsertText(workbookPart, worksheetPart, error.Priority, "L", i);

                XlsToView = $"{route} - {Resources.AddedErrors} {i} из {routeErrors.Count}";

                i++;
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
            GetErrorTypeList();
        }
    }
}