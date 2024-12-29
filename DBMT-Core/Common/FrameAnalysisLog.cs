using DBMT_Core.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DBMT_Core
{
    public static class FrameAnalysisLog
    {
        /// <summary>
        /// Cache data
        /// </summary>
        public static Dictionary<string,List<string>> FrameAnalysisFolderName_LogLineList_Dict = [];

        /// <summary>
        ///  Cache data
        /// </summary>
        public static Dictionary<string,string> TextureFileName_DedupedTextureFileName_Dict = [];


        /// <summary>
        /// 所有方法都必须调用此方法判断是否已初始化最新的FrameAnalysis文件夹的内容
        /// 否则可能会读取到旧的FrameAnalysis文件夹的内容导致结果不正确
        /// </summary>
        public static List<string> GetLatestLogLineList()
        {
            if (!FrameAnalysisFolderName_LogLineList_Dict.ContainsKey(GlobalConfig.LatestFrameAnalysisFolderName))
            {
                FrameAnalysisFolderName_LogLineList_Dict[GlobalConfig.LatestFrameAnalysisFolderName] = File.ReadAllLines(GlobalConfig.Path_LatestFrameAnalysisLogTxt).ToList();
            }
            List<string> LogLineList = FrameAnalysisFolderName_LogLineList_Dict[GlobalConfig.LatestFrameAnalysisFolderName];

            return LogLineList;
        }

        //获取Trianglelist里贴图对应的Deduped文件，这样就能在转换好的Deduped文件中直接读取并且展示了
        
        public static string GetDedupedTextureFileName(string TextureFileName)
        {
            List<string> LogLineList = GetLatestLogLineList();

            LOG.Info("LogLineList Size: " + LogLineList.Count.ToString());

            if (TextureFileName_DedupedTextureFileName_Dict.Count == 0)
            {
                //分析日志并记录每个Dump文件对应的Deduped文件名称
                string FindStr = "3DMigoto Dumping Texture2D";
                foreach (string LogLine in  LogLineList)
                {
                    if (!LogLine.Contains(FindStr))
                    {
                        continue;
                    }

                    int index = LogLine.IndexOf(FindStr) + FindStr.Length;
                    string PathStr = LogLine.Substring(index).Trim();
                    string[] PathSplits = PathStr.Split("->");
                    string OriginalPath = PathSplits[0].Trim();
                    string DedupedPath = PathSplits[1].Trim();
                    string OriginalFileName = Path.GetFileName(OriginalPath);
                    string DedupedFileName = Path.GetFileName(DedupedPath);
                    TextureFileName_DedupedTextureFileName_Dict[OriginalFileName] = DedupedFileName;
                }
            }


            if (TextureFileName_DedupedTextureFileName_Dict.ContainsKey(TextureFileName))
            {
                return TextureFileName_DedupedTextureFileName_Dict[TextureFileName];
            }
            else
            {
                return "";
            }
        }


        public static List<string> Get_DrawCallIndexList_ByHash(string DrawIB,bool OnlyMatchFirst)
        {
            List<string> LogLineList = GetLatestLogLineList();

            HashSet<string> IndexSet = [];
            string CurrentIndex = "";
            foreach(string LogLine in LogLineList)
            {
                if (LogLine.StartsWith("00"))
                {
                    CurrentIndex = LogLine.Substring(0,6);
                }

                if (LogLine.Contains("hash=" + DrawIB))
                {
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

            return IndexList;
        }


        /// <summary>
        /// 从FrameAnalysis文件夹中根据DrawIB和ComponentName读取DrawCall的Index列表
        /// 强依赖于FrameAnalysis文件夹这log.txt的存在
        /// </summary>
        /// <param name="DrawIB"></param>
        /// <param name="ComponentName"></param>
        /// <returns></returns>
        public static List<string> Read_DrawCallIndexListByMatchFirstIndex(string DrawIB, UInt64 MatchFirstIndex)
        {

            //找到所有与该DrawIB有关的DrawCallIndex
            List<string> IndexList = Get_DrawCallIndexList_ByHash(DrawIB,false);
            List<string> DrawCallIndexList = new List<string>();

            //Foreach every index to see it's match_first_index.
            foreach (string Index in IndexList)
            {
                string IBTxtFileName = FrameAnalysisData.FilterFirstFile(GlobalConfig.WorkFolder, Index + "-ib", ".txt");
                IndexBufferTxtFile IBTxtFile = new IndexBufferTxtFile(GlobalConfig.WorkFolder + IBTxtFileName, false);

                if (IBTxtFile.Topology != "trianglelist")
                {
                    continue;
                }

                UInt64 FileMatchFirstIndex = UInt64.Parse(IBTxtFile.FirstIndex);
                if (FileMatchFirstIndex == MatchFirstIndex)
                {
                    DrawCallIndexList.Add(IBTxtFile.Index);
                }
            }

            return DrawCallIndexList;
        }


    }
}
