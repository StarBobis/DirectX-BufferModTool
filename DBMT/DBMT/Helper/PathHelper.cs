using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBMT
{
    public class PathHelper
    {
        public static string GetCurrentGameBackGroundPicturePath()
        {
            string basePath = Directory.GetCurrentDirectory();

            //设置背景图片
            //默认为各个游戏用户设置的DIY图片
            string imagePath = Path.Combine(basePath, "Assets", MainConfig.CurrentGameName + "_DIY.png");

            //如果不存在DIY背景图，则使用默认游戏的背景图
            if (!File.Exists(imagePath))
            {
                imagePath = Path.Combine(basePath, "Assets", MainConfig.CurrentGameName + ".png");
            }

            //如果默认游戏的背景图还不存在，则使用主页的背景图
            if (!File.Exists(imagePath))
            {
                imagePath = Path.Combine(basePath, "Assets", "HomePageBackGround.png");
            }
            return imagePath;
        }

        public static string GetAssetsFolderPath()
        {
            return Path.Combine(Directory.GetCurrentDirectory(),"Assets\\");
        }


        public static string GetLatestLogFilePath()
        {
            string logsPath = MainConfig.ApplicationRunPath + "Logs";
            if (!Directory.Exists(logsPath))
            {
                return "";
            }
            string[] logFiles = Directory.GetFiles(logsPath); ;
            List<string> logFileList = new List<string>();
            foreach (string logFile in logFiles)
            {
                string logfileName = Path.GetFileName(logFile);
                if (logfileName.EndsWith(".log") && logfileName.Length > 15)
                {
                    logFileList.Add(logfileName);
                }
            }

            logFileList.Sort();
            string LogFilePath = logsPath + "\\" + logFileList[^1];
            return LogFilePath;
        }


        public static string GetLatestFrameAnalysisFolderLogFilePath()
        {
            string[] directories = Directory.GetDirectories(MainConfig.Path_LoaderFolder);
            List<string> frameAnalysisFileList = new List<string>();
            foreach (string directory in directories)
            {
                string directoryName = Path.GetFileName(directory);

                if (directoryName.StartsWith("FrameAnalysis-"))
                {
                    frameAnalysisFileList.Add(directoryName);
                }
            }

            //
            if (frameAnalysisFileList.Count > 0)
            {
                frameAnalysisFileList.Sort();

                string latestFrameAnalysisFolder = MainConfig.Path_LoaderFolder.Replace("/", "\\") + frameAnalysisFileList.Last();
                string latestLogFile = latestFrameAnalysisFolder + "\\log.txt";
                return latestLogFile;
            }
            else
            {
                return "";
            }
        }


        public static string GetLatestFrameAnalysisFolder()
        {
            string[] directories = Directory.GetDirectories(MainConfig.Path_LoaderFolder);
            List<string> frameAnalysisFileList = new List<string>();
            foreach (string directory in directories)
            {
                string directoryName = Path.GetFileName(directory);

                if (directoryName.StartsWith("FrameAnalysis-"))
                {
                    frameAnalysisFileList.Add(directoryName);
                }
            }

            //
            if (frameAnalysisFileList.Count > 0)
            {
                frameAnalysisFileList.Sort();

                string latestFrameAnalysisFolder = MainConfig.Path_LoaderFolder.Replace("/", "\\") + frameAnalysisFileList.Last();
                return latestFrameAnalysisFolder;
            }

            return "";
        }
    }
}
