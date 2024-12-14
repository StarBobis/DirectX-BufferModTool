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
            // 优先级为：DIY图片 > 游戏图片 > 主页图片
            string[] imagePaths = [
                Path.Combine(basePath, "Assets", MainConfig.CurrentGameName + "_DIY.png"),
                Path.Combine(basePath, "Assets", MainConfig.CurrentGameName + ".png"),
                Path.Combine(basePath, "Assets", "HomePageBackGround.png")
                ];

            // 检查图片是否存在
            foreach (string path in imagePaths)
            {
                if (File.Exists(path))
                {
                    // 可以直接返回path，不需要再赋值给imagePath
                    return path;
                }
            }
            return imagePaths[^1];
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



        public static async Task<List<string>> GetGameDirectoryNameList()
        {
            List<string> directories = new List<string>();

            string CurrentDirectory = Directory.GetCurrentDirectory();
            string GamesPath = Path.Combine(CurrentDirectory, "Games\\");

            if (!Directory.Exists(GamesPath))
            {
                await MessageHelper.Show("Can't find Games folder in your run folder, Initialize Failed. : \n" + GamesPath);
                return directories;
            }

            // 获取所有子目录名称
            directories = Directory.EnumerateDirectories(GamesPath)
                                        .Select(Path.GetFileName)
                                        .Where(name => !string.IsNullOrEmpty(name))
                                        .OrderByDescending(name => name).ToList();
            return directories;
        }

    }
}
