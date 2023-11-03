using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DocumentFormat.OpenXml.Packaging;
using ErrorDataLayer;
using ErrorFixApp.Properties;
using ImageToXlsx;

namespace ErrorFixApp.Controls
{
    public class ExportControlModel: INotifyPropertyChanged
    {
        public ExportControlModel()
        {
            //_sqLiteManager = new SqLiteManager(ConfigurationParams.User);
            _webApiManager = new WebApiManager();
            ExportPath = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\Export";
            if (!Directory.Exists(ExportPath))
            {
                Directory.CreateDirectory(ExportPath);
            }
        }
        
       
        
        //private readonly SqLiteManager _sqLiteManager;
        private readonly WebApiManager _webApiManager;

        private string _exportPath;
        public string ExportPath
        {
            get => _exportPath;
            set
            {
                _exportPath = value;
              
                OnPropertyChanged();
            }
        }

        private ObservableCollection<string> _messageCollection = new ObservableCollection<string>();

        public ObservableCollection<string> MessageCollection
        {
            get => _messageCollection;
            set
            {
                _messageCollection = value;
              
                OnPropertyChanged();
            }
        }

        private readonly List<string> _dbListToExport = new List<string>();
        private int _totalErrorsCount = 0;
        
        private List<ErrorEntity> _errorListToExport = new List<ErrorEntity>();
        
       
        
       
        private void FillDbToExport(SelectedDatesCollection dates)
        {
            _dbListToExport.Clear();
            List<string> dateList = new List<string>();

            foreach (var d in dates)
            {
                dateList.Add($"{d:yy-MM-dd}");
            }
            List<string> availableDb = SqLiteManager.GetAvailableDb();
            foreach (var db in availableDb)
            {
                string dbDate = "";
                var dbParts = db.Split('-');
                if (dbParts.Length > 3)
                {
                    dbDate = dbParts[0] + "-" + dbParts[1] + "-" + dbParts[2];
                }
                else
                {
                    dbParts = db.Split('_');
                    if (dbParts.Length > 3)
                    {
                        dbDate = dbParts[2] + "-" + dbParts[1] + "-" + dbParts[0];
                    }
                }

                string findRes = dateList.Find(x => x.Contains(dbDate));
                if (findRes != null)
                {
                    _dbListToExport.Add(db);
                }
                
            }
        }

         
        private ICommand _exportCommand;

        public ICommand ExportCommand
        {
            get
            {
                return _exportCommand ?? (_exportCommand = new RelayCommand<object>(
                    param => this.ExportObject(param as SelectedDatesCollection),
                    param => true
                ));
            }
        }

        private void ExportObject(SelectedDatesCollection dates)
        {
            _totalErrorsCount = 0;
            MessageCollection.Clear();
            MessageCollection.Add("Доступные БД:");
            FillDbToExport(dates);
            int i = 0;
            foreach (var db in _dbListToExport)
            {
                var errorCount = SqLiteManager.GetErrorCount(db);
                MessageCollection.Add($"{i}. БД: {db}, Кол-во записей: {errorCount}");
                i++;
            }

            ExportToXlsxFileTask();
        }

        private ICommand _checkCommand;

        public ICommand CheckCommand
        {
            get
            {
                return _checkCommand ?? (_checkCommand = new RelayCommand<object>(
                    param => this.CheckObject(param as SelectedDatesCollection),
                    param => true
                ));
            }
        }
        
        private void CheckObject(SelectedDatesCollection dates)
        {
            //ToDo for web api
            MessageCollection.Clear();
            MessageCollection.Add("Доступные БД:");
            FillDbToExport(dates);
            int i = 0;
            foreach (var db in _dbListToExport)
            {
               var errorCount = SqLiteManager.GetErrorCount(db);
               var exportDate = SqLiteManager.GetLastExportDate(db);
               MessageCollection.Add($"{i}. БД: {db}, Кол-во записей: {errorCount}, Дата экспорта: {exportDate}");
               i++;
            }
            //ToDo for web api
            
           
        }
        
        private void ExportToXlsxFileTask()
        {
            //Task.Factory.StartNew(ExportToXlsxFile);
            ExportToXlsxFile();
        }

        private void ExportToXlsxFile()
        {
            try
            {
                if (_dbListToExport.Count > 0)
                {
                    foreach (var db in _dbListToExport)
                    {
                        List<ErrorEntity> errors = new List<ErrorEntity>();
                        if (ConfigurationManager.AppSettings.Get("WorkingType") == "Local")
                        {
                            errors = SqLiteManager.LoadErrors(db);
                            SqLiteManager.AddExportDateToDb(db);
                        }
                        //else
                        //{
                            //errors = await _webApiManager.GetAllErrors(db);
                        //}

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

                        var databaseDate = Path.GetFileNameWithoutExtension(db);
                     
                        foreach (var errorType in errorTypeList)
                        {
                            foreach (var route in routeList)
                            {
                                var dirToExport = $"{ExportPath}\\{route}";
                                if (CheckDirectoryToExport(dirToExport))
                                {
                                    MessageCollection.Add($"Создана директория: {route}");
                                }
                                
                                var exportDbPath =
                                    $"{dirToExport}\\{route}_{errorType}_{databaseDate}.xlsx";

                                ExportToXlsSeparatedRoutes(route, errors, errorType, exportDbPath);
                                
                            }
                        }
                        errors.Clear();
                        MessageCollection.Add($"Всего добавлено: {_totalErrorsCount} ошибок");
                        
                        
                    }
                    string argument = $"/select, \"{ExportPath}\\";

                    System.Diagnostics.Process.Start("explorer.exe", argument);
                    
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private bool CheckDirectoryToExport(string dirToExport)
        {
            try
            {
                if (!Directory.Exists(dirToExport))
                {
                    Directory.CreateDirectory(dirToExport);
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return false;
        }

        private void ExportToXlsSeparatedRoutes(string route, List<ErrorEntity> errors, string errorType,
            string xlsToExport)
        {
            if (errors.Count > 0)
            {
                var routeErrors = errors.Where(e => e.RouteName == route && e.ErrorType == errorType).ToList();

                if (routeErrors.Count > 0)
                {
                    if (File.Exists(xlsToExport))
                    {
                        File.Delete(xlsToExport);
                    }
                    MessageCollection.Add($"Создан файл:{Path.GetFileNameWithoutExtension(xlsToExport)}");
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

                if (i == 1)
                {
                    MessageCollection.Add($"{route} - {Resources.AddedErrors} {i} из {routeErrors.Count}");
                }
                else
                {
                    MessageCollection[MessageCollection.Count - 1] =
                        $"{route} - {Resources.AddedErrors} {i} из {routeErrors.Count}";
                }

                _totalErrorsCount++;
                i++;
            }
            worksheetPart.Worksheet.Save();
            
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}