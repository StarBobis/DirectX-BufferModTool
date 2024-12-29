using DBMT_Core.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBMT_Core
{
    public static class FrameAnalysisLog
    {
        public static List<string> LogLineList = [];
        public static Dictionary<string,string> TextureFileName_DedupedTextureFileName_Dict = [];

        static FrameAnalysisLog()
        {
            initializeFrameAnalysisLog();
        }

        public static void initializeFrameAnalysisLog()
        {
            LogLineList = File.ReadAllLines(GlobalConfig.Path_LatestFrameAnalysisLogTxt).ToList();
        }

        //获取Trianglelist里贴图对应的Deduped文件，这样就能在转换好的Deduped文件中直接读取并且展示了
        
        public static string GetDedupedTextureFileName(string TextureFileName)
        {

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
    }
}
