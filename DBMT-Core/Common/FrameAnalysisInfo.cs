using DBMT_Core.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBMT_Core.Common
{
    public class FrameAnalysisInfo
    {
        public string CTXCode { get; set; } = "";
        public string LogFileName { get; set; } = "";
        public string LogFilePath { get; set; } = "";

        public string FolderName { get; set; } = "";
        public string FolderPath { get; set; } = "";

        public FrameAnalysisInfo() {
            throw new Exception("不允许初始化空的FrameAnalysisInfo，请填写参数DrawIB来进行使用。");
        }

        public FrameAnalysisInfo(string DrawIB)
        {
            List<string> LogTxtFilePathList = GetCTXLogFilePathList(DrawIB);

            if (LogTxtFilePathList.Count != 0)
            {
                LOG.Info("当前遇到CTX类型FrameAnalysis文件夹。");
                //接下来遍历每个log.txt顺便获取到对应的CTX文件夹路径
                LogFilePath = GetFirstLogTxtFilePathFromDrawIB(DrawIB);
                LogFileName = Path.GetFileName(LogFilePath);
                CTXCode = LogFileName.Split("log-")[1].Split(".txt")[0];
                LOG.Info("CTXCode: " + CTXCode);

                FolderName = "ctx-" + CTXCode;
                FolderPath = Path.Combine(GlobalConfig.Path_LatestFrameAnalysisFolder, FolderName + "\\");
                LOG.Info("CTXFolderPath: " + FolderPath);
            }
            else
            {
                LogFilePath = GlobalConfig.Path_LatestFrameAnalysisLogTxt;
                LogFileName = Path.GetFileName(LogFilePath);

                FolderPath = GlobalConfig.Path_LatestFrameAnalysisFolder;
                FolderName = Path.GetFileName(FolderPath);
            }
        }

        public List<string> GetCTXLogFilePathList(string DrawIB)
        {
            string[] TotalFrameAnalysisFiles = Directory.GetFiles(GlobalConfig.Path_LatestFrameAnalysisFolder);
            //LOG.Info("FrameAnalysis文件总数量: " + TotalFrameAnalysisFiles.Length.ToString());

            List<string> LogTxtFilePathList = [];
            foreach (string LogTxtFilePath in TotalFrameAnalysisFiles)
            {
                string FileName = Path.GetFileName(LogTxtFilePath);
                if (!FileName.StartsWith("log-"))
                {
                    continue;
                }

                if (!FileName.EndsWith(".txt"))
                {
                    continue;
                }
                //这里过滤完剩下的就是log-xxx.txt了
                LogTxtFilePathList.Add(LogTxtFilePath);
            }

            return LogTxtFilePathList;
        }

        public string GetFirstLogTxtFilePathFromDrawIB(string DrawIB)
        {
            
            List<string> LogTxtFilePathList = GetCTXLogFilePathList(DrawIB);

            string FirstLogTxtFilePath = "";
            foreach (string LogTxtFilePath in LogTxtFilePathList)
            {
                string[] LogLineList = File.ReadAllLines(LogTxtFilePath);
                foreach (string LogLine in LogLineList)
                {
                    if (LogLine.Contains("hash=" + DrawIB))
                    {
                        LOG.Info("当前DrawIB在【" + LogTxtFilePath + "】中存在");
                        FirstLogTxtFilePath = LogTxtFilePath;

                        //这里Break，因为我们只需要从第一次遇到的里面提取就行了
                        //TODO 后面也许有特殊情况，但是一般一个CTX就够了
                        break;
                    }
                }
            }

            return FirstLogTxtFilePath;
        }
    }
}
