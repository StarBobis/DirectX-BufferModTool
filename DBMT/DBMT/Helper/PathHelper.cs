using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBMT.Helper
{
    public class PathHelper
    {

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
            string LogFilePath = logsPath + "\\" + logFileList[logFileList.Count - 1];
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



    }
}
