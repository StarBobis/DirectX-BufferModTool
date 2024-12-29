using DBMT_Core.Utils;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBMT_Core
{
    public static class FrameAnalysisData
    {
        /// <summary>
        /// TODO 这个并不可靠，创建工作空间后就无法正常读取到文件了。
        /// </summary>
        public static List<string> FileNameList = [];


        /// <summary>
        /// 这个会在类被全局初始化一次时调用，所以作为初始化方法读取FrameAnalysis文件列表
        /// 这样可以确保只读取一次。
        /// </summary>
        static FrameAnalysisData()
        {
            InitializeFileNameList();
        }

        public static void InitializeFileNameList()
        {
            FileNameList = new List<string>();
            string[] FrameAnalysisFileNameArray = Directory.GetFiles(GlobalConfig.WorkFolder);
            foreach (string FrameAnalysisFileName in FrameAnalysisFileNameArray)
            {
                FileNameList.Add(Path.GetFileName(FrameAnalysisFileName));
            }
    
        }


        public static List<string> FilterFrameAnalysisFile(string Content, string Suffix)
        {
            InitializeFileNameList();

            List<string> FilterFileNameList = new List<string>();

            foreach (string FileName in FileNameList)
            {
                if (FileName.Contains(Content) && FileName.EndsWith(Suffix))
                {
                    FilterFileNameList.Add(FileName);
                }
            }

            return FilterFileNameList;
        }


        public static List<string> FilterFile(string SearchFolderPath,string Content, string Suffix)
        {
            InitializeFileNameList();
            List<string> SearchFileNameList = new List<string>();
            string[] FrameAnalysisFileNameArray = Directory.GetFiles(SearchFolderPath);
            foreach (string FrameAnalysisFileName in FrameAnalysisFileNameArray)
            {
                SearchFileNameList.Add(Path.GetFileName(FrameAnalysisFileName));
            }

            List<string> FilterFileNameList = new List<string>();
            foreach (string FileName in SearchFileNameList)
            {
                if (FileName.Contains(Content) && FileName.EndsWith(Suffix))
                {
                    FilterFileNameList.Add(FileName);
                }
            }

            return FilterFileNameList;
        }

        public static string FilterFirstFile(string SearchFolderPath, string Content, string Suffix)
        {
            InitializeFileNameList();
            List<string> SearchFileNameList = new List<string>();

            if (SearchFolderPath == GlobalConfig.WorkFolder)
            {
                SearchFileNameList = FileNameList;
            }
            else
            {
                string[] FrameAnalysisFileNameArray = Directory.GetFiles(SearchFolderPath);
                foreach (string FrameAnalysisFileName in FrameAnalysisFileNameArray)
                {
                    SearchFileNameList.Add(Path.GetFileName(FrameAnalysisFileName));
                }
            }

            List<string> FilterFileNameList = new List<string>();
            foreach (string FileName in SearchFileNameList)
            {
                if (FileName.Contains(Content) && FileName.EndsWith(Suffix))
                {
                    FilterFileNameList.Add(FileName);
                }
            }

            if (FilterFileNameList.Count == 0)
            {
                return "";
            }
            else
            {
                return FilterFileNameList[0];
            }

        }

        public static List<string> FilterTextureFileNameList(string FilterFolder, string Content)
        {
            FileNameList = new List<string>();
            string[] FrameAnalysisFileNameArray = Directory.GetFiles(FilterFolder);
            foreach (string FrameAnalysisFileName in FrameAnalysisFileNameArray)
            {
                FileNameList.Add(Path.GetFileName(FrameAnalysisFileName));
            }
            Debug.WriteLine("FileNameList Size:" + FileNameList.Count.ToString());

            List<Tuple<int,string>> FilterFileNameList = new List<Tuple<int, string>>();

            foreach (string FileName in FileNameList)
            {
                if (FileName.Contains(Content))
                {
                    if (FileName.EndsWith(".dds") || FileName.EndsWith(".jpg"))
                    {
                        int slot_number = DBMTStringUtils.GetPixelSlotNumberFromTextureFileName(FileName);
                        LOG.Info(FileName);
                        FilterFileNameList.Add(Tuple.Create(slot_number,FileName));
                    }
                }

            }

            // 这里要按ps-t几的顺序进行排序
            // 对FilterFileNameList按照slot_number进行排序
            var sortedFilterFileNameList = FilterFileNameList.OrderBy(item => item.Item1).ToList();

            List<string> OrderedFilterFileNameList = new List<string>();
            // 输出排序后的结果
            foreach (var item in sortedFilterFileNameList)
            {
                OrderedFilterFileNameList.Add(item.Item2);
            }

            return OrderedFilterFileNameList;
        }



        /// <summary>
        /// 从FrameAnalysis文件夹里，读取一个DrawIB的ComponentName列表
        /// 强依赖于FrameAnalysis文件夹存在，且必须包含此DrawIB
        /// </summary>
        /// <param name="DrawIB"></param>
        /// <returns></returns>
        public static Dictionary<string, UInt64> Read_ComponentName_MatchFirstIndex_Dict(string DrawIB)
        {
            Debug.WriteLine("Read_ComponentName_MatchFirstIndex_Dict::Begin");

            Dictionary<string, UInt64> ComponentName_MatchFirstIndex_Dict = new Dictionary<string, UInt64>();

            //根据DrawIB，查找所有的MatchFirstIndex
            List<string> IBTxtFileNameList = FilterFrameAnalysisFile("-ib=" + DrawIB, ".txt");
            if (IBTxtFileNameList.Count != 0)
            {
                Debug.WriteLine("FrameAnalysis文件中检测到该DrawIB，所以从文件中读取新鲜的");

                List<UInt64> MatchFirstIndexList = new List<UInt64>();
                foreach (string IBTxtFileName in IBTxtFileNameList)
                {
                    IndexBufferTxtFile IBTxtFile = new IndexBufferTxtFile(GlobalConfig.WorkFolder + IBTxtFileName, false);

                    if (IBTxtFile.Topology != "trianglelist")
                    {
                        continue;
                    }

                    UInt64 MatchFirstIndex = UInt64.Parse(IBTxtFile.FirstIndex);
                    if (!MatchFirstIndexList.Contains(MatchFirstIndex))
                    {
                        MatchFirstIndexList.Add(MatchFirstIndex);
                    }
                }
                MatchFirstIndexList.Sort();

                //Component是从1开始的，所以这里必须是1
                UInt64 Count = 1;
                foreach (UInt64 MatchFirstIndex in MatchFirstIndexList)
                {
                    ComponentName_MatchFirstIndex_Dict["Component " + Count] = MatchFirstIndex;
                    Count++;
                }
            }
           
            Debug.WriteLine("Read_ComponentName_MatchFirstIndex_Dict::End");
            return ComponentName_MatchFirstIndex_Dict;
        }


        /// <summary>
        /// 从FrameAnalysis文件夹中根据DrawIB和ComponentName读取DrawCall的Index列表
        /// 强依赖于FrameAnalysis文件夹的存在
        /// </summary>
        /// <param name="DrawIB"></param>
        /// <param name="ComponentName"></param>
        /// <returns></returns>
        public static List<string> Read_DrawCallIndexList(string DrawIB,string ComponentName)
        {
            Dictionary<string, UInt64> ComponentName_MatchFirstIndex_Dict = Read_ComponentName_MatchFirstIndex_Dict(DrawIB);
            UInt64 MatchFirstIndex = ComponentName_MatchFirstIndex_Dict[ComponentName];

            List<string> IBTxtFileNameList = FilterFrameAnalysisFile("-ib=" + DrawIB, ".txt");
            List<string> DrawCallIndexList = new List<string>();

            foreach (string IBTxtFileName in IBTxtFileNameList)
            {
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

        public static string GetDedupedTextureFileName(string TextureFileName) {
            string Hash = DBMTStringUtils.GetFileHashFromFileName(TextureFileName);
            string Suffix = Path.GetExtension(TextureFileName);
            string DedupedFileName = FilterFirstFile(GlobalConfig.Path_LatestFrameAnalysisDedupedFolder,Hash,Suffix);
            return DedupedFileName;
        }

    }
}
