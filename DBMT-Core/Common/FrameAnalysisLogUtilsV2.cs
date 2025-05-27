using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBMT_Core.Common
{
    public static class FrameAnalysisLogUtilsV2
    {
        /// <summary>
        /// 缓存数据
        /// </summary>
        public static Dictionary<string, List<string>> FrameAnalysisLogFilePath_LogLineList_Dict = [];
        /// <summary>
        /// 从缓存中读取，多次读取时减少处理时间
        /// </summary>
        public static List<string> Get_LogLineList(string FrameAnalysisLogFilePath)
        {
            if (!FrameAnalysisLogFilePath_LogLineList_Dict.ContainsKey(FrameAnalysisLogFilePath))
            {
                FrameAnalysisLogFilePath_LogLineList_Dict[FrameAnalysisLogFilePath] = File.ReadAllLines(FrameAnalysisLogFilePath).ToList();
            }
            List<string> LogLineList = FrameAnalysisLogFilePath_LogLineList_Dict[FrameAnalysisLogFilePath];

            return LogLineList;
        }


        /// <summary>
        ///  缓存数据
        /// </summary>
        public static Dictionary<string, Dictionary<string, string>> FrameAnalysisFolderPath_TextureFileName_DedupedTextureFileName_Dict_Dict = [];
        /// <summary>
        /// 获取Deduped的文件名
        /// </summary>
        /// <param name="FrameAnalysisFileName"></param>
        /// <returns></returns>
        public static string Get_DedupedFileName(string FrameAnalysisFileName,string FrameAnalysisFolderPath,string FrameAnalysisLogFilePath)
        {
            //LOG.Info("Get_DedupedFileName::Start");
            List<string> LogLineList = Get_LogLineList(FrameAnalysisLogFilePath);

            //老规矩，如果不包含最新的，就填充它
            if (!FrameAnalysisFolderPath_TextureFileName_DedupedTextureFileName_Dict_Dict.ContainsKey(FrameAnalysisFolderPath))
            {
                FrameAnalysisFolderPath_TextureFileName_DedupedTextureFileName_Dict_Dict[FrameAnalysisFolderPath] = new Dictionary<string, string>();

                string FindStr = "Dumping Texture2D";
                string FindStr2 = "Dumping Buffer";

                foreach (string logLine in LogLineList)
                {

                    if (!logLine.Contains("->"))
                    {
                        continue;
                    }

                    int findStrIndex = logLine.IndexOf(FindStr);
                    if (findStrIndex >= 0)
                    {
                        string[] pathSplits = logLine.Substring(findStrIndex + FindStr.Length).Split("->");
                        string originalFileName = Path.GetFileName(pathSplits[0].Trim());
                        string dedupedFileName = Path.GetFileName(pathSplits[1].Trim());
                        FrameAnalysisFolderPath_TextureFileName_DedupedTextureFileName_Dict_Dict[FrameAnalysisFolderPath][originalFileName] = dedupedFileName;
                    }
                    else
                    {
                        int findStr2Index = logLine.IndexOf(FindStr2);
                        if (findStr2Index >= 0)
                        {
                            string[] pathSplits = logLine.Substring(findStr2Index + FindStr2.Length).Split("->");
                            string originalFileName = Path.GetFileName(pathSplits[0].Trim());
                            string dedupedFileName = Path.GetFileName(pathSplits[1].Trim());
                            FrameAnalysisFolderPath_TextureFileName_DedupedTextureFileName_Dict_Dict[FrameAnalysisFolderPath][originalFileName] = dedupedFileName;
                        }
                    }
                }
            }

            //从最新的里面获取然后返回对应值
            Dictionary<string, string> SymlinkFileName_DedupedFileName_Dict = FrameAnalysisFolderPath_TextureFileName_DedupedTextureFileName_Dict_Dict[FrameAnalysisFolderPath];
            //LOG.Info("Get_DedupedFileName::End");

            if (SymlinkFileName_DedupedFileName_Dict.ContainsKey(FrameAnalysisFileName))
            {
                return SymlinkFileName_DedupedFileName_Dict[FrameAnalysisFileName];
            }
            else
            {
                return "";
            }
        }



        /// <summary>
        /// 更方便的版本直接获取到路径，能节省在外面再包装一次的代码。
        /// </summary>
        /// <param name="FrameAnalysisFileName"></param>
        /// <returns></returns>
        public static string Get_DedupedFilePath(string FrameAnalysisFileName, string FrameAnalysisFolderPath, string FrameAnalysisLogFilePath)
        {
            //LOG.Info("Get_DedupedFilePath::Start");
            //LOG.Info("FrameAnalysisFileName: " + FrameAnalysisFileName);
            string DedupedFileName = Get_DedupedFileName(FrameAnalysisFileName,FrameAnalysisFolderPath,FrameAnalysisLogFilePath);
            //LOG.Info("DedupedFileName: " + DedupedFileName);
            if (DedupedFileName == "")
            {
                //LOG.Info("Get_DedupedFilePath::End");
                return "";
            }
            else
            {
                //这里Deduped文件夹用的是最新的是因为不管是CTX还是普通架构，Deduped文件夹的位置都是固定的。
                return Path.Combine(GlobalConfig.Path_LatestFrameAnalysisDedupedFolder, DedupedFileName);
            }

        }


        public static List<string> Get_DrawCallIndexList_ByHash(string DrawIB, bool OnlyMatchFirst,string FrameAnalysisLogFilePath)
        {
            Debug.WriteLine("Get_DrawCallIndexList_ByHash::" + DrawIB);
            List<string> LogLineList = Get_LogLineList(FrameAnalysisLogFilePath);

            HashSet<string> IndexSet = [];
            string CurrentIndex = "";
            foreach (string LogLine in LogLineList)
            {
                if (LogLine.StartsWith("00"))
                {
                    CurrentIndex = LogLine.Substring(0, 6);
                }

                if (LogLine.Contains("hash=" + DrawIB))
                {
                    Debug.WriteLine("Find Hash: " + LogLine);
                    IndexSet.Add(CurrentIndex);

                    if (OnlyMatchFirst)
                    {
                        break;
                    }
                }
            }

            List<string> IndexList = [];
            foreach (string Index in IndexSet)
            {
                IndexList.Add(Index);
            }
            Debug.WriteLine("Get_DrawCallIndexList_ByHash::" + DrawIB + "  End");

            return IndexList;
        }









    }
}
