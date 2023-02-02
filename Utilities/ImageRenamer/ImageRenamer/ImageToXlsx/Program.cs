using System;
using System.Drawing;
using System.IO;
using System.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Drawing.Spreadsheet;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace ImageToXlsx
{
    class Program
    {
        static void Main(string[] args)
        {
            if (!CheckArgs(args, out var routeNumber, out var xlsPath, out var routesDir)) return;

            for (uint i = 1; i< 10; i++)
            {
                ExcelTools.InsertText(@"D:\Temp\Sheet8.xlsx", "Inserted Text", "B", i);
            }
           
            //AddRoutePicturesToXls(routeNumber, xlsPath, routesDir);
        }

       
        private static void AddRoutePicturesToXls(string routeNumber, string xlsPath, string routesDir)
        {
            string dirBackRouteName    = $"{routesDir}\\{routeNumber}\\ОбратныйRenamed";
            string dirForwardRouteName = $"{routesDir}\\{routeNumber}\\ПрямойRenamed";
           

            AddFilesToXls(routeNumber, xlsPath, dirBackRouteName);
            AddFilesToXls(routeNumber, xlsPath, dirForwardRouteName);
            
            
        }

        private static void AddFilesToXls(string routeNumber, string xlsPath, string dirName)
        {
            if (Directory.Exists(dirName))
            {
                var files = Directory.GetFiles(dirName).OrderBy(f => f).ToList();
                if (files.Any())
                {
                    foreach (var file in files)
                    {
                        var fileName = Path.GetFileNameWithoutExtension(file);
                        {
                            var ids = fileName.Split('_');
                            if (ids.Length > 1)
                            {
                                int rowNumber = Int32.Parse(ids[0]);
                                int colNumber = 1;
                                if (ids[1] == "m")
                                {
                                    colNumber = 3;
                                }

                                ExcelTools.AddImage(true, xlsPath, routeNumber.ToString(), file, file, colNumber, rowNumber);
                                Console.WriteLine($"{file}");
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"Files is not exist in {dirName}");
                }
            }
            else
            {
                Console.WriteLine($@"Direction is not exist - {dirName}");
            }
        }

        private static bool CheckArgs(string[] args, out string routeNumber, out string xlsPath, out string routesDir)
        {
            routeNumber = "";
            xlsPath = "";
            routesDir = "";
            if (args.Length == 0)
            {
                ShowHelp();
                return false;
            }
            
            if (args[0] != "-add")
            {
                ShowHelp();
                return false;
            }

            routeNumber = args[1];
            
            // if (!File.Exists(args[2]))
            // {
            //     Console.WriteLine("XlsFile is not exist");
            //     ShowHelp();
            //     return false;
            // }
            // else
            //{
                xlsPath = args[2];
            //}
            
            if (!Directory.Exists(args[3]))
            {
                Console.WriteLine("Directory with routes is not exist");
                ShowHelp();
                return false;
            }
            else
            {
                routesDir = args[3];
            }
            return true;
        }

        private static void ShowHelp()
        {
            Console.WriteLine("-add routeNumber fullPathToRoutes fullPathToXls");
        }
    }
}