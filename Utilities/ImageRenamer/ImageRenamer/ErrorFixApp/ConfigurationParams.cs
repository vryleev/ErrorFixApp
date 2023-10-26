using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using ErrorFixApp.Properties;

namespace ErrorFixApp
{
    public static class ConfigurationParams
    {
        public static readonly string TrainerPath = ConfigurationManager.AppSettings.Get("TrainerPath");
        public static readonly string SceneGeneratorPath = ConfigurationManager.AppSettings.Get("SceneGeneratorPath");
        public static readonly string FileNamePos = ConfigurationManager.AppSettings.Get("FileNamePos");

        public static readonly string PositionFilePath = $"{TrainerPath}/{FileNamePos}";
        public static readonly string PositionFilePathSetup = $"{TrainerPath}/{FileNamePos}_setup";
        public static readonly string PositionFilePathSgSetup = $"{SceneGeneratorPath}/{FileNamePos}_setup";
        
        public static readonly string User = ConfigurationManager.AppSettings.Get("User");
        public static readonly string WorkingType = ConfigurationManager.AppSettings.Get("WorkingType");
        
        
        public static void ParseRectConfiguration(string visualRect, int screenLeft, int screenTop, int screenWidth,
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
                        //MessageBox.Show($"Rect is not corrected {e.Message}");
                        visualLeft = screenLeft;
                        visualTop = screenTop;
                        visualWidth = screenWidth / 2;
                        visualHeight = screenHeight;
                    }
                }
            }
        }
        
        public static List<string>  GetErrorTypeList()
        {
            return ConfigurationManager.AppSettings.Get("ErrorTypes").Split(',').ToList();
        }
        
        
        public static List<string>  GetPriorityList()
        {
            return ConfigurationManager.AppSettings.Get("Priority").Split(',').ToList();
        }
    }
}