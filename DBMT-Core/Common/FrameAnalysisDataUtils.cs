using DBMT_Core.Common;
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
    
    public static class FrameAnalysisDataUtils
    {
        public static Dictionary<string, List<string>> FrameAnalysisFolderPath_FileNameList_Dict = [];


        public static List<string> GetFrameAnalysisFileNameList(string FrameAnalysisFolderPath)
        {

            if (!FrameAnalysisFolderPath_FileNameList_Dict.ContainsKey(FrameAnalysisFolderPath))
            {

                List<string> TmpFileNameList = new List<string>();
                string[] FrameAnalysisFileNameArray = Directory.GetFiles(FrameAnalysisFolderPath);
                foreach (string FrameAnalysisFileName in FrameAnalysisFileNameArray)
                {
                    TmpFileNameList.Add(Path.GetFileName(FrameAnalysisFileName));
                }

                FrameAnalysisFolderPath_FileNameList_Dict[FrameAnalysisFolderPath] = TmpFileNameList;
            }

            List<string> FileNameList = FrameAnalysisFolderPath_FileNameList_Dict[FrameAnalysisFolderPath];
            return FileNameList;
        }


        public static List<string> FilterFrameAnalysisFile(string FrameAnalysisFolderPath,string Content, string Suffix)
        {
            List<string> FileNameList = GetFrameAnalysisFileNameList(FrameAnalysisFolderPath);

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


        public static List<string> FilterFile(string FrameAnalysisFolderPath, string Content, string Suffix)
        {

            List<string> SearchFileNameList = new List<string>();
            string[] FrameAnalysisFileNameArray = Directory.GetFiles(FrameAnalysisFolderPath);
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

        public static string FilterFirstFile(string FilterFolderPath, string Content, string Suffix)
        {
            List<string> FileNameList = GetFrameAnalysisFileNameList(FilterFolderPath);
            List<string> SearchFileNameList = new List<string>();

            if (FilterFolderPath == GlobalConfig.WorkFolder)
            {
                SearchFileNameList = FileNameList;
            }
            else
            {
                string[] FrameAnalysisFileNameArray = Directory.GetFiles(FilterFolderPath);
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

            List<string> FileNameList = new List<string>();
            if(FilterFolder == GlobalConfig.WorkFolder)
            {
                FileNameList = GetFrameAnalysisFileNameList(GlobalConfig.WorkFolder);
            }
            else
            {
                string[] FrameAnalysisFileNameArray = Directory.GetFiles(FilterFolder);
                foreach (string FrameAnalysisFileName in FrameAnalysisFileNameArray)
                {
                    FileNameList.Add(Path.GetFileName(FrameAnalysisFileName));
                }
            }
            //Debug.WriteLine("FileNameList Size:" + FileNameList.Count.ToString());

            List<Tuple<int,string>> FilterFileNameList = new List<Tuple<int, string>>();

            foreach (string FileName in FileNameList)
            {
                if (FileName.Contains(Content))
                {
                    if (FileName.EndsWith(".dds") || FileName.EndsWith(".jpg"))
                    {
                        int slot_number = DBMTStringUtils.GetPixelSlotNumberFromTextureFileName(FileName);
                        //LOG.Info(FileName);
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
        public static Dictionary<string, UInt64> Read_ComponentName_MatchFirstIndex_Dict(string FrameAnalysisFolderPath, string DrawIB)
        {
            Debug.WriteLine("Read_ComponentName_MatchFirstIndex_Dict::Begin");

            Dictionary<string, UInt64> ComponentName_MatchFirstIndex_Dict = new Dictionary<string, UInt64>();

            //根据DrawIB，查找所有的MatchFirstIndex
            List<string> IBTxtFileNameList = FilterFrameAnalysisFile(FrameAnalysisFolderPath,"-ib=" + DrawIB, ".txt");
            if (IBTxtFileNameList.Count != 0)
            {
                Debug.WriteLine("FrameAnalysis文件中检测到该DrawIB，所以从文件中读取新鲜的");

                List<UInt64> MatchFirstIndexList = new List<UInt64>();
                foreach (string IBTxtFileName in IBTxtFileNameList)
                {
                    IndexBufferTxtFile IBTxtFile = new IndexBufferTxtFile(FrameAnalysisFolderPath + IBTxtFileName, false);

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
        public static List<string> Read_DrawCallIndexList(string FrameAnalysisFolderPath,string DrawIB,string ComponentName)
        {
            LOG.Info("Read_DrawCallIndexList::Start");
            Dictionary<string, UInt64> ComponentName_MatchFirstIndex_Dict = Read_ComponentName_MatchFirstIndex_Dict(FrameAnalysisFolderPath,DrawIB);
            UInt64 MatchFirstIndex = ComponentName_MatchFirstIndex_Dict[ComponentName];

            List<string> IBTxtFileNameList = FilterFrameAnalysisFile(FrameAnalysisFolderPath, "-ib=" + DrawIB, ".txt");
            List<string> DrawCallIndexList = new List<string>();

            foreach (string IBTxtFileName in IBTxtFileNameList)
            {
                IndexBufferTxtFile IBTxtFile = new IndexBufferTxtFile(FrameAnalysisFolderPath + IBTxtFileName, false);

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

            LOG.Info("Read_DrawCallIndexList::End");
            return DrawCallIndexList;
        }

        public static string GetDedupedTextureFileName(string FrameAnalysisFolderPath,string TextureFileName) {
            string Hash = DBMTStringUtils.GetFileHashFromFileName(TextureFileName);
            string Suffix = Path.GetExtension(TextureFileName);
            string DedupedFileName = FilterFirstFile(FrameAnalysisFolderPath, Hash,Suffix);
            return DedupedFileName;
        }


        public static List<string> Get_TrianglelistIndexListByDrawIB(string FrameAnalysisFolderPath,string DrawIB)
        {
            LOG.Info("Get_TrianglelistIndexListByDrawIB::Start");
            List<string> IndexList = new List<string>();

            List<string> DrawIB_IBFileNameList = FilterFrameAnalysisFile(FrameAnalysisFolderPath, "-ib=" + DrawIB,".txt");
            LOG.Info("DrawIB_IBFileNameList Size: " + DrawIB_IBFileNameList.Count.ToString());

            foreach(string IBFileName in DrawIB_IBFileNameList)
            {
                string Index = IBFileName.Substring(0, 6);
                List<string> VB0FileNameList = FilterFrameAnalysisFile(FrameAnalysisFolderPath, Index + "-vb0", ".txt");
                if (VB0FileNameList.Count == 0)
                {
                    continue;
                }

                string VB0FilePath = FrameAnalysisFolderPath + VB0FileNameList[0];
                LOG.Info("VB0FilePath: " + VB0FilePath);

                string Topology = DBMTFileUtils.FindMigotoIniAttributeInFile(VB0FilePath, "topology");
                if (Topology == "trianglelist")
                {
                    LOG.Info("Detect Trianglelist Topology Index: " + Index);
                    IndexList.Add(Index);
                }

            }
            LOG.Info("Get_TrianglelistIndexListByDrawIB::End");
            return IndexList;
        }

        
        public static SortedDictionary<int,string> Get_MatchFirstIndex_IBTxtFileName_Dict(string FrameAnalysisFolderPath, string DrawIB)
        {
            SortedDictionary<int, string> MatchFirstIndex_IBFileName_Dict = new SortedDictionary<int, string>();

            List<string> TrianglelistIndexList = Get_TrianglelistIndexListByDrawIB(FrameAnalysisFolderPath, DrawIB);
            foreach (string TrianglelistIndex in TrianglelistIndexList)
            {
                string IBTxtFileName = FilterFirstFile(FrameAnalysisFolderPath, TrianglelistIndex + "-ib",".txt");
                if (IBTxtFileName == "")
                {
                    continue;
                }
                string FrameAnalysisFolderName = Path.GetFileName(FrameAnalysisFolderPath);
                string IBTxtFilePath = FrameAnalysisLogUtilsV2.Get_DedupedFilePath(FrameAnalysisFolderName, FrameAnalysisFolderPath,IBTxtFileName);
                IndexBufferTxtFile IBTxtFile = new IndexBufferTxtFile(IBTxtFilePath,false);
                int MatchFirstIndex = int.Parse(IBTxtFile.FirstIndex);
                MatchFirstIndex_IBFileName_Dict[MatchFirstIndex] = IBTxtFileName;
            }


            return MatchFirstIndex_IBFileName_Dict;
        }




    }
}
