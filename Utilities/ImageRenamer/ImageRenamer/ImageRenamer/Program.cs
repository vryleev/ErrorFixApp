using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Net.Mime;
using NetVips;


namespace ImageRenamer
{
    static class Program
    {
        static void Main(string[] args)
        {
            if (!CheckArgs(args, out var initialNumber)) return;

            var dirPath = args[1];
            var dirPathForRes = dirPath +"Renamed";
            var dirPathForUnrecognized = dirPath +"Unrecognized";
            
            var alreadyRenamed = new List<string>();

            if (args[0] == "-copy")
            {
                CreateDirectory(dirPathForRes);
            }

            if (args[0] == "-check")
            {
                CreateDirectory(dirPathForUnrecognized);
            }

            if (Directory.Exists(dirPath))
            {
                var files = Directory.GetFiles(dirPath);
                if (files.Any())
                {
                    for (int i = 0; i < files.Length; i++)
                    {
                        string vFile = files[i];
                        if (!alreadyRenamed.Contains(vFile))
                        {
                            if (KeyFileDateTime(vFile, out var vFileDate, out var vFileTime))
                            {
                                bool res = false;
                                for (int j = i + 1; j < files.Length; j++)
                                {
                                    string mFile = files[j];
                                    if (KeyFileDateTime(mFile, out var mFileDate, out var mFileTime))
                                    {
                                        if (vFileDate == mFileDate)
                                        {
                                            try
                                            {
                                                var vTime = ParseTime(vFileTime);
                                                var mTime = ParseTime(mFileTime);
                                                if ((mTime - vTime).TotalSeconds < 40)
                                                {
                                                    res = true;
                                                    alreadyRenamed.Add(vFile);
                                                    alreadyRenamed.Add(mFile);
                                                    if (args[0] == "-copy")
                                                    {
                                                        var vDestFileName =
                                                            $"{dirPathForRes}/{initialNumber}_v_{vFileDate}_{vFileTime}.jpg";
                                                        WriteImageToFile(vFile, vDestFileName);

                                                        var mDestFileName =
                                                            $"{dirPathForRes}/{initialNumber}_m_{mFileDate}_{mFileTime}.jpg";
                                                        WriteImageToFile(mFile, mDestFileName);
                                                        initialNumber++;

                                                        break;
                                                    }
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                Console.WriteLine(ex.Message);
                                            }
                                        }
                                    }
                                }

                                if (!res)
                                {
                                    Console.WriteLine($"MapFile is apsent for {vFile}");
                                    if (args[0] == "-check")
                                    {
                                        var vDestFileName =
                                            $"{dirPathForUnrecognized}/{Path.GetFileNameWithoutExtension(vFile)}.jpg";
                                        WriteImageToFile(vFile, vDestFileName);
                                    }
                                }
                            }
                            else
                            {
                                if (args[0] == "-copy")
                                {
                                    var vDestFileName =
                                        $"{dirPathForRes}/{Path.GetFileNameWithoutExtension(vFile)}.jpg";
                                    WriteImageToFile(vFile, vDestFileName);
                                }
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Directory is empty");
                }   
            }
           
        }

        private static bool CheckArgs(string[] args, out int initialNumber)
        {
            initialNumber = -1;
            if (args.Length == 0)
            {
                ShowHelp();
                return false;
            }

            if (!Directory.Exists(args[1]))
            {
                Console.WriteLine("Directory is not exist");
                ShowHelp();
                return false;
            }

            var parseRes = Int32.TryParse(args[2], out initialNumber);
            if (!parseRes)
            {
                ShowHelp();
                return false;
            }

            if (!(args[0] == "-check" || args[0] == "-copy"))
            {
                ShowHelp();
                return false;
            }

            return true;
        }

        private static void WriteImageToFile(string vFile, string vDestFileName)
        {
            using Image imageV = Image.Thumbnail(vFile, 1024, 1024);
            imageV.WriteToFile(vDestFileName);
        }

        private static void CreateDirectory(string dirPathForUnrecognized)
        {
            if (Directory.Exists(dirPathForUnrecognized))
            {
                Directory.Delete(dirPathForUnrecognized, true);
            }
            Directory.CreateDirectory(dirPathForUnrecognized);
        }

        private static DateTime ParseTime(string vFileTime)
        {
            var vTime = DateTime.ParseExact(vFileTime, "HHmmss",
                System.Globalization.CultureInfo.CurrentCulture);
            return vTime;
        }

        private static void ShowHelp()
        {
            Console.WriteLine("-check fullPath initialNumber");
            Console.WriteLine("-copy  fullPath initialNumber");
            return;
        }

        private static bool KeyFileDateTime(string keyFile,out string keyFileDate, out string keyFileTime)
        {
            bool res = false;
            keyFileDate = String.Empty;
            keyFileTime = String.Empty;
            var keyFileName = Path.GetFileNameWithoutExtension(keyFile);
            {
                var keyFileDataArray = keyFileName.Split('_');
                if (keyFileDataArray.Length == 3)
                {
                    keyFileDate = keyFileDataArray[1];
                    keyFileTime = keyFileDataArray[2];
                    res = true;
                }
            }
            return res;
        }
    }
}