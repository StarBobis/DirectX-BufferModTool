using DBMT_Core.Common;
using DBMT_Core.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBMT_Core.Games
{
    public static class HonkaiStarRail
    {

        private static bool IsPositionBlendSlotMatch(string PointlistIndex,int PositionStride, int BlendStride,int VertexCount,string PositionSlot,string BlendSlot)
        {
            string CST0BufferFileName = FrameAnalysisDataUtils.FilterFirstFile(GlobalConfig.WorkFolder, PointlistIndex + "-" + PositionSlot + "=", ".buf");
            string CST0BufferFilePath = FrameAnalysisLogUtils.Get_DedupedFilePath(CST0BufferFileName);
            int CST0BufferFileSize = (int)DBMTFileUtils.GetFileSize(CST0BufferFilePath);
            int CST0VertexCount = CST0BufferFileSize / PositionStride;

            if (CST0VertexCount == VertexCount)
            {
                //此时再判断t5的顶点数和BlendStride是否相等
                string CST5BufferFileName = FrameAnalysisDataUtils.FilterFirstFile(GlobalConfig.WorkFolder, PointlistIndex + "-"+ BlendSlot + "=", ".buf");
                string CST5BufferFilePath = FrameAnalysisLogUtils.Get_DedupedFilePath(CST5BufferFileName);
                int CST5BufferFileSize = (int)DBMTFileUtils.GetFileSize(CST5BufferFilePath);
                int CST5VertexCount = CST5BufferFileSize / BlendStride;

                if (CST5VertexCount == VertexCount)
                {
                    return true;
                }
            }

            return false;
        }


        private static bool IsBlendSlotMatch(string PointlistIndex,int BlendStride, int VertexCount, string BlendSlot)
        {
            //此时再判断t5的顶点数和BlendStride是否相等
            string CST5BufferFileName = FrameAnalysisDataUtils.FilterFirstFile(GlobalConfig.WorkFolder, PointlistIndex + "-" + BlendSlot + "=", ".buf");
            string CST5BufferFilePath = FrameAnalysisLogUtils.Get_DedupedFilePath(CST5BufferFileName);
            int CST5BufferFileSize = (int)DBMTFileUtils.GetFileSize(CST5BufferFilePath);
            int CST5VertexCount = CST5BufferFileSize / BlendStride;

            if (CST5VertexCount == VertexCount)
            {
                return true;
            }

            return false;
        }

        private static string GetPrePositionIndex(string PointlistIndex,int PositionStride, int VertexCount,string Slot)
        {
            //寻找Position的上一个Hash
            string CSU0BufferFileName = FrameAnalysisDataUtils.FilterFirstFile(GlobalConfig.WorkFolder, PointlistIndex + "-" + Slot + "=", ".buf");
            string CSU0Hash = CSU0BufferFileName.Substring(10, 8);

            //这里不是查找第一个，而是查找最后一个出现的，并且Index要比PointlistIndex还小。
            string PrePositionBufferFileName = "";
            List<string> FileNameList = FrameAnalysisDataUtils.FilterFile(GlobalConfig.WorkFolder, CSU0Hash, "-cs=4e03bd5b704abbdd.buf");
            if (FileNameList.Count >= 1)
            {
                PrePositionBufferFileName = FileNameList[FileNameList.Count - 1];
            }

            //string PrePositionBufferFileName = FrameAnalysisData.FilterFirstFile(GlobalConfig.WorkFolder, CSU0Hash, "-cs=4e03bd5b704abbdd.buf");


            string PrePositionBufferFilePath = FrameAnalysisLogUtils.Get_DedupedFilePath(PrePositionBufferFileName);

            int PrePositionSize = (int)DBMTFileUtils.GetFileSize(PrePositionBufferFilePath);
            int PRePositionVertexCount = PrePositionSize / PositionStride;

            if (PRePositionVertexCount == VertexCount)
            {
                string Index = PrePositionBufferFileName.Substring(0, 6);
                return Index;
            }

            return "";
        }


        private static bool Extract_1c932707d4d8df41(string DrawIB, D3D11GameTypeLv2 d3D11GameTypeLv2,string PointlistIndex, List<string> TrianglelistIndexList)
        {
            string PositionSlot = "";
            string BlendSlot = "";

            //先匹配出正确的数据类型，顺便得到从哪个Slot中提取的。
            List<D3D11GameType> PossibleD3d11GameTypeList = [];
            foreach (D3D11GameType d3D11GameType in d3D11GameTypeLv2.Ordered_GPU_CPU_D3D11GameTypeList)
            {

                //首先肯定得有vb1槽位，否则无法提取
                bool ExistsVB1Slot = false;
                foreach (var item in d3D11GameType.CategorySlotDict)
                {
                    if (item.Value == "vb1")
                    {
                        ExistsVB1Slot = true;
                        break;
                    }
                }

                if (!ExistsVB1Slot)
                {
                    continue;
                }


                //获取第一个TrianglelistIndex
                string TrianglelistIndex = d3D11GameTypeLv2.FilterTrianglelistIndex_UnityVS(TrianglelistIndexList, d3D11GameType);


                //获取Buffer文件
                string VB1BufferFileName = FrameAnalysisDataUtils.FilterFirstFile(GlobalConfig.WorkFolder, TrianglelistIndex + "-vb1=", ".buf");


                string VB1BufferFilePath = FrameAnalysisLogUtils.Get_DedupedFilePath(VB1BufferFileName);
                int VB1Size = (int)DBMTFileUtils.GetFileSize(VB1BufferFilePath);

                //求出预期顶点数
                int VertexCount = VB1Size / d3D11GameType.CategoryStrideDict["Texcoord"];

                int PositionStride = d3D11GameType.CategoryStrideDict["Position"];
                int BlendStride = d3D11GameType.CategoryStrideDict["Blend"];

                //随后依次判断t0到t5的Position的顶点数
                bool t0t5 = IsPositionBlendSlotMatch(PointlistIndex, PositionStride, BlendStride, VertexCount, "cs-t0", "cs-t5");
                if (t0t5)
                {
                    PossibleD3d11GameTypeList.Add(d3D11GameType);
                    PositionSlot = "cs-t0";
                    BlendSlot = "cs-t5";
                    LOG.Info("识别到数据类型: " + d3D11GameType.GameTypeName);
                    continue;
                }

                bool t1t6 = IsPositionBlendSlotMatch(PointlistIndex, PositionStride, BlendStride, VertexCount, "cs-t1", "cs-t6");
                if (t1t6)
                {
                    PossibleD3d11GameTypeList.Add(d3D11GameType);
                    PositionSlot = "cs-t1";
                    BlendSlot = "cs-t6";
                    LOG.Info("识别到数据类型: " + d3D11GameType.GameTypeName);
                    continue;
                }

                bool t2t7 = IsPositionBlendSlotMatch(PointlistIndex, PositionStride, BlendStride, VertexCount, "cs-t2", "cs-t7");
                if (t2t7)
                {
                    PossibleD3d11GameTypeList.Add(d3D11GameType);
                    PositionSlot = "cs-t2";
                    BlendSlot = "cs-t7";
                    LOG.Info("识别到数据类型: " + d3D11GameType.GameTypeName);
                    continue;
                }

                bool t3t8 = IsPositionBlendSlotMatch(PointlistIndex, PositionStride, BlendStride, VertexCount, "cs-t3", "cs-t8");
                if (t3t8)
                {
                    PossibleD3d11GameTypeList.Add(d3D11GameType);
                    PositionSlot = "cs-t3";
                    BlendSlot = "cs-t8";
                    LOG.Info("识别到数据类型: " + d3D11GameType.GameTypeName);
                    continue;
                }

                bool t4t9 = IsPositionBlendSlotMatch(PointlistIndex, PositionStride, BlendStride, VertexCount, "cs-t4", "cs-t9");
                if (t4t9)
                {
                    PossibleD3d11GameTypeList.Add(d3D11GameType);
                    PositionSlot = "cs-t4";
                    BlendSlot = "cs-t9";
                    LOG.Info("识别到数据类型: " + d3D11GameType.GameTypeName);
                    continue;
                }
            }
            LOG.NewLine();

            LOG.Info("PositionSlot: " + PositionSlot);
            LOG.Info("BlendSlot: " + BlendSlot);
            LOG.NewLine();

            LOG.Info("识别到的数据类型: ");
            foreach (D3D11GameType d3D11GameType in PossibleD3d11GameTypeList)
            {
                LOG.Info(d3D11GameType.GameTypeName);
            }
            LOG.NewLine();


            if (PossibleD3d11GameTypeList.Count == 0)
            {
                LOG.Error("无法找到任何已知数据类型，请进行添加");
                return false;
            }

            //直接走提取逻辑
            //接下来提取出每一种可能性
            //读取一个MatchFirstIndex_IBFileName_Dict
            SortedDictionary<int, string> MatchFirstIndex_IBTxtFileName_Dict = new SortedDictionary<int, string>();
            foreach (string TrianglelistIndex in TrianglelistIndexList)
            {
                string IBFileName = FrameAnalysisDataUtils.FilterFirstFile(GlobalConfig.WorkFolder, TrianglelistIndex + "-ib", ".txt");
                if (IBFileName == "")
                {
                    continue;
                }
                string IBFilePath = FrameAnalysisLogUtils.Get_DedupedFilePath(IBFileName);
                IndexBufferTxtFile IBTxtFile = new IndexBufferTxtFile(IBFilePath, false);
                MatchFirstIndex_IBTxtFileName_Dict[int.Parse(IBTxtFile.FirstIndex)] = IBFileName;
            }

            foreach (var item in MatchFirstIndex_IBTxtFileName_Dict)
            {
                LOG.Info("MatchFirstIndex: " + item.Key.ToString() + " IBFileName: " + item.Value);
            }
            LOG.NewLine();

            foreach (D3D11GameType d3D11GameType in PossibleD3d11GameTypeList)
            {
                //先设置一下CategorySlot，因为可能数据类型文件里面没写
                d3D11GameType.CategorySlotDict["Position"] = PositionSlot;
                d3D11GameType.CategorySlotDict["Blend"] = BlendSlot;


                string TrianglelistIndex = d3D11GameTypeLv2.FilterTrianglelistIndex_UnityVS(TrianglelistIndexList, d3D11GameType);

                Dictionary<string, string> CategoryBufFileMap = new Dictionary<string, string>();
                foreach (var item in d3D11GameType.CategoryTopologyDict)
                {
                    string CategoryName = item.Key;
                    string ExtractIndex = TrianglelistIndex;
                    if (item.Value == "pointlist" && PointlistIndex != "")
                    {
                        ExtractIndex = PointlistIndex;
                    }
                    string CategorySlot = d3D11GameType.CategorySlotDict[item.Key];

                    if (CategoryName == "Position")
                    {
                        CategorySlot = PositionSlot;
                    }
                    else if (CategoryName == "Blend")
                    {
                        CategorySlot = BlendSlot;
                    }

                    //获取文件名存入对应Dict
                    string CategoryBufFileName = FrameAnalysisDataUtils.FilterFirstFile(GlobalConfig.WorkFolder, ExtractIndex + "-" + CategorySlot + "=", ".buf");
                    CategoryBufFileMap[item.Key] = CategoryBufFileName;
                }

                string GameTypeFolderName = "TYPE_" + d3D11GameType.GameTypeName;
                string DrawIBFolderPath = Path.Combine(GlobalConfig.Path_CurrentWorkSpaceFolder, DrawIB + "\\");
                string GameTypeOutputPath = Path.Combine(DrawIBFolderPath, GameTypeFolderName + "\\");
                if (!Directory.Exists(GameTypeOutputPath))
                {
                    Directory.CreateDirectory(GameTypeOutputPath);
                }

                LOG.Info("开始从各个Buffer文件中读取数据:");
                //接下来从各个Buffer中读取并且拼接为FinalVB0

                List<Dictionary<int, byte[]>> BufDictList = new List<Dictionary<int, byte[]>>();
                foreach (var item in CategoryBufFileMap)
                {
                    string CategoryName = item.Key;
                    string CategoryBufFileName = item.Value;
                    string CategoryBufFilePath = FrameAnalysisLogUtils.Get_DedupedFilePath(CategoryBufFileName);
                    int CategoryStride = d3D11GameType.CategoryStrideDict[CategoryName];

                    Dictionary<int, byte[]> BufDict = DBMTBinaryUtils.ReadBinaryFileByStride(CategoryBufFilePath, CategoryStride);
                    BufDictList.Add(BufDict);
                }
                LOG.NewLine();

                Dictionary<int, byte[]> MergedVB0Dict = DBMTBinaryUtils.MergeByteDicts(BufDictList);
                byte[] FinalVB0 = DBMTBinaryUtils.MergeDictionaryValues(MergedVB0Dict);

                //接下来遍历MatchFirstIndex_IBFileName的Map，对于每个MarchFirstIndex
                //都读取IBTxt文件里的数值，然后进行分割并输出。
                int OutputCount = 1;
                foreach (var item in MatchFirstIndex_IBTxtFileName_Dict)
                {
                    int MatchFirstIndex = item.Key;
                    string IBTxtFileName = item.Value;
                    //拼接出一个IBBufFileName
                    string IBBufFileName = Path.GetFileNameWithoutExtension(IBTxtFileName) + ".buf";

                    string IBTxtFilePath = FrameAnalysisLogUtils.Get_DedupedFilePath(IBTxtFileName);
                    string IBBufFilePath = FrameAnalysisLogUtils.Get_DedupedFilePath(IBBufFileName);

                    IndexBufferTxtFile IBTxtFile = new IndexBufferTxtFile(IBTxtFilePath, true);
                    LOG.Info(IBTxtFilePath);
                    LOG.Info("FirstIndex: " + IBTxtFile.FirstIndex);
                    LOG.Info("IndexCount: " + IBTxtFile.IndexCount);

                    string NamePrefix = DrawIB + "-" + OutputCount.ToString();

                    string OutputIBBufFilePath = Path.Combine(GameTypeOutputPath, NamePrefix + ".ib");
                    string OutputVBBufFilePath = Path.Combine(GameTypeOutputPath, NamePrefix + ".vb");
                    string OutputFmtFilePath = Path.Combine(GameTypeOutputPath, NamePrefix + ".fmt");

                    //通过D3D11GameType合成一个FMT文件并且输出
                    FmtFile fmtFile = new FmtFile(d3D11GameType);
                    fmtFile.OutputFmtFile(OutputFmtFilePath);

                    //写出IBBufFile
                    IndexBufferBufFile IBBufFile = new IndexBufferBufFile(IBBufFilePath, IBTxtFile.Format);

             
                    //这里使用IndexNumberCount的话，只能用于正向提取
                    //如果要兼容逆向提取，需要换成IndexCount
                    //但是还有个问题，那就是即使换成IndexCount，如果IB文件的替换不是一个整体的Buffer，而是各个独立分开的Buffer
                    //则这里的SelfDivide是不应该存在的步骤，所以这里是无法逆向提取的。
                    //综合来看，逆向提取其实是一种适用性不强，并且很容易受到ini中各种因素干扰的提取方式
                    //但是如果能获取到DrawIndexed的具体数值呢？可以通过解析log.txt的方式进行获取
                    //但是解析很玛法，而且就算能获取到，那如果有复杂的CommandList混淆，投入与产出不成正比了就
                    //使用逆向Mod的ini的方式更加优雅。
                    IBBufFile.SelfDivide(int.Parse(IBTxtFile.FirstIndex), (int)IBTxtFile.IndexNumberCount);
                    IBBufFile.SaveToFile_UInt32(OutputIBBufFilePath, -1 * IBBufFile.MinNumber);

                    //写出VBBufFile
                    VertexBufferBufFile VBBufFile = new VertexBufferBufFile(FinalVB0);
                    if (IBBufFile.MinNumber > IBBufFile.MaxNumber)
                    {
                        LOG.Error("当前IB文件最小值大于IB文件中的最大值，疑似逆向提取Mod模型出错，跳过此模型输出。");
                    }

                    VBBufFile.SelfDivide(IBBufFile.MinNumber, IBBufFile.MaxNumber, d3D11GameType.GetSelfStride());
                    VBBufFile.SaveToFile(OutputVBBufFilePath);

                    OutputCount += 1;
                }

                //TODO 每个数据类型文件夹下面都需要生成一个tmp.json，但是新版应该改名为Import.json
                //为了兼容旧版Catter，暂时先不改名

                LOG.Info("TrianglelistIndex: " + TrianglelistIndex);


                ImportJson importJson = new ImportJson();
                string VB0FileName = FrameAnalysisDataUtils.FilterFirstFile(GlobalConfig.WorkFolder, TrianglelistIndex + "-vb0", ".txt");
                LOG.Info("VB0FileName: " + VB0FileName);

                importJson.DrawIB = DrawIB;
                importJson.VertexLimitVB = VB0FileName.Substring(11, 8);

                LOG.Info("VertexLimitVB: " + importJson.VertexLimitVB);

                importJson.d3D11GameType = d3D11GameType;
                importJson.Category_BufFileName_Dict = CategoryBufFileMap;
                importJson.MatchFirstIndex_IBTxtFileName_Dict = MatchFirstIndex_IBTxtFileName_Dict;

                //TODO 暂时叫tmp.json，后面再改
                string ImportJsonSavePath = Path.Combine(GameTypeOutputPath, "tmp.json");
                importJson.SaveToFile(ImportJsonSavePath);
            }

            LOG.NewLine();

            return true;
        }


        private static bool Extract_fee307b98a965c16(string DrawIB, D3D11GameTypeLv2 d3D11GameTypeLv2, string PointlistIndex, List<string> TrianglelistIndexList)
        {

            //接下来开始识别可能的数据类型。
            //此时需要先读取所有存在的数据类型。
            //此时需要我们先去生成几个数据类型用于测试。
            //还有就是数据类型的文件夹是存在哪里的
            List<D3D11GameType> PossibleD3D11GameTypeList = d3D11GameTypeLv2.GetPossibleGameTypeList_UnityVS(PointlistIndex, TrianglelistIndexList);

            if (PossibleD3D11GameTypeList.Count == 0)
            {
                return false;
            }


            //接下来提取出每一种可能性
            //读取一个MatchFirstIndex_IBFileName_Dict
            SortedDictionary<int, string> MatchFirstIndex_IBTxtFileName_Dict = new SortedDictionary<int, string>();
            foreach (string TrianglelistIndex in TrianglelistIndexList)
            {
                string IBFileName = FrameAnalysisDataUtils.FilterFirstFile(GlobalConfig.WorkFolder, TrianglelistIndex + "-ib", ".txt");
                if (IBFileName == "")
                {
                    continue;
                }
                string IBFilePath = FrameAnalysisLogUtils.Get_DedupedFilePath(IBFileName);
                IndexBufferTxtFile IBTxtFile = new IndexBufferTxtFile(IBFilePath, false);
                MatchFirstIndex_IBTxtFileName_Dict[int.Parse(IBTxtFile.FirstIndex)] = IBFileName;
            }

            foreach (var item in MatchFirstIndex_IBTxtFileName_Dict)
            {
                LOG.Info("MatchFirstIndex: " + item.Key.ToString() + " IBFileName: " + item.Value);
            }
            LOG.NewLine();

            foreach (D3D11GameType d3D11GameType in PossibleD3D11GameTypeList)
            {
                string TrianglelistIndex = d3D11GameTypeLv2.FilterTrianglelistIndex_UnityVS(TrianglelistIndexList, d3D11GameType);

                Dictionary<string, string> CategoryBufFileMap = new Dictionary<string, string>();
                foreach (var item in d3D11GameType.CategoryTopologyDict)
                {
                    string ExtractIndex = TrianglelistIndex;
                    if (item.Value == "pointlist" && PointlistIndex != "")
                    {
                        ExtractIndex = PointlistIndex;
                    }
                    string CategorySlot = d3D11GameType.CategorySlotDict[item.Key];

                    //获取文件名存入对应Dict
                    string CategoryBufFileName = FrameAnalysisDataUtils.FilterFirstFile(GlobalConfig.WorkFolder, ExtractIndex + "-" + CategorySlot , ".buf");
                    CategoryBufFileMap[item.Key] = CategoryBufFileName;
                }

                string GameTypeFolderName = "TYPE_" + d3D11GameType.GameTypeName;
                string DrawIBFolderPath = Path.Combine(GlobalConfig.Path_CurrentWorkSpaceFolder, DrawIB + "\\");
                string GameTypeOutputPath = Path.Combine(DrawIBFolderPath, GameTypeFolderName + "\\");
                if (!Directory.Exists(GameTypeOutputPath))
                {
                    Directory.CreateDirectory(GameTypeOutputPath);
                }

                LOG.Info("开始从各个Buffer文件中读取数据:");
                //接下来从各个Buffer中读取并且拼接为FinalVB0

                List<Dictionary<int, byte[]>> BufDictList = new List<Dictionary<int, byte[]>>();
                foreach (var item in CategoryBufFileMap)
                {
                    string CategoryName = item.Key;
                    string CategoryBufFileName = item.Value;
                    string CategoryBufFilePath = FrameAnalysisLogUtils.Get_DedupedFilePath(CategoryBufFileName);
                    int CategoryStride = d3D11GameType.CategoryStrideDict[CategoryName];

                    Dictionary<int, byte[]> BufDict = DBMTBinaryUtils.ReadBinaryFileByStride(CategoryBufFilePath, CategoryStride);
                    BufDictList.Add(BufDict);
                }
                LOG.NewLine();

                Dictionary<int, byte[]> MergedVB0Dict = DBMTBinaryUtils.MergeByteDicts(BufDictList);
                byte[] FinalVB0 = DBMTBinaryUtils.MergeDictionaryValues(MergedVB0Dict);

                //接下来遍历MatchFirstIndex_IBFileName的Map，对于每个MarchFirstIndex
                //都读取IBTxt文件里的数值，然后进行分割并输出。
                int OutputCount = 1;
                foreach (var item in MatchFirstIndex_IBTxtFileName_Dict)
                {
                    int MatchFirstIndex = item.Key;
                    string IBTxtFileName = item.Value;
                    //拼接出一个IBBufFileName
                    string IBBufFileName = Path.GetFileNameWithoutExtension(IBTxtFileName) + ".buf";
                    LOG.Info(IBBufFileName);


                    string IBTxtFilePath = FrameAnalysisLogUtils.Get_DedupedFilePath(IBTxtFileName);
                    string IBBufFilePath = FrameAnalysisLogUtils.Get_DedupedFilePath(IBBufFileName);

                    IndexBufferTxtFile IBTxtFile = new IndexBufferTxtFile(IBTxtFilePath, true);
                    LOG.Info(IBTxtFilePath);
                    LOG.Info("FirstIndex: " + IBTxtFile.FirstIndex);
                    LOG.Info("IndexCount: " + IBTxtFile.IndexCount);

                    string NamePrefix = DrawIB + "-" + OutputCount.ToString();

                    string OutputIBBufFilePath = Path.Combine(GameTypeOutputPath, NamePrefix + ".ib");
                    string OutputVBBufFilePath = Path.Combine(GameTypeOutputPath, NamePrefix + ".vb");
                    string OutputFmtFilePath = Path.Combine(GameTypeOutputPath, NamePrefix + ".fmt");

                    //通过D3D11GameType合成一个FMT文件并且输出
                    FmtFile fmtFile = new FmtFile(d3D11GameType);
                    fmtFile.OutputFmtFile(OutputFmtFilePath);

                    //写出IBBufFile
                    IndexBufferBufFile IBBufFile = new IndexBufferBufFile(IBBufFilePath, IBTxtFile.Format);

                   
                    //这里使用IndexNumberCount的话，只能用于正向提取
                    //如果要兼容逆向提取，需要换成IndexCount
                    //但是还有个问题，那就是即使换成IndexCount，如果IB文件的替换不是一个整体的Buffer，而是各个独立分开的Buffer
                    //则这里的SelfDivide是不应该存在的步骤，所以这里是无法逆向提取的。
                    //综合来看，逆向提取其实是一种适用性不强，并且很容易受到ini中各种因素干扰的提取方式
                    //但是如果能获取到DrawIndexed的具体数值呢？可以通过解析log.txt的方式进行获取
                    //但是解析很玛法，而且就算能获取到，那如果有复杂的CommandList混淆，投入与产出不成正比了就
                    //使用逆向Mod的ini的方式更加优雅。

                    if (IBBufFile.MinNumber != 0)
                    {
                        IBBufFile.SaveToFile_UInt32(OutputIBBufFilePath, -1 * IBBufFile.MinNumber);
                    }
                    else
                    {
                        IBBufFile.SelfDivide(int.Parse(IBTxtFile.FirstIndex), (int)IBTxtFile.IndexNumberCount);
                        IBBufFile.SaveToFile_UInt32(OutputIBBufFilePath, -1 * IBBufFile.MinNumber);
                    }

                    //写出VBBufFile
                    VertexBufferBufFile VBBufFile = new VertexBufferBufFile(FinalVB0);
                    if (IBBufFile.MinNumber > IBBufFile.MaxNumber)
                    {
                        LOG.Error("当前IB文件最小值大于IB文件中的最大值，跳过vb文件输出，因为无法SelfDivide");
                        continue;
                    }

                    if (IBBufFile.MinNumber != 0)
                    {
                        VBBufFile.SelfDivide(IBBufFile.MinNumber, IBBufFile.MaxNumber, d3D11GameType.GetSelfStride());
                    }
                    VBBufFile.SaveToFile(OutputVBBufFilePath);

                    OutputCount += 1;
                }

                //TODO 每个数据类型文件夹下面都需要生成一个tmp.json，但是新版应该改名为Import.json
                //为了兼容旧版Catter，暂时先不改名

                ImportJson importJson = new ImportJson();
                string VB0FileName = FrameAnalysisDataUtils.FilterFirstFile(GlobalConfig.WorkFolder, TrianglelistIndex + "-vb0", ".txt");

                importJson.DrawIB = DrawIB;
                importJson.VertexLimitVB = VB0FileName.Substring(11, 8);
                importJson.d3D11GameType = d3D11GameType;
                importJson.Category_BufFileName_Dict = CategoryBufFileMap;
                importJson.MatchFirstIndex_IBTxtFileName_Dict = MatchFirstIndex_IBTxtFileName_Dict;

                //TODO 暂时叫tmp.json，后面再改
                string ImportJsonSavePath = Path.Combine(GameTypeOutputPath, "tmp.json");
                importJson.SaveToFile(ImportJsonSavePath);
            }

            LOG.NewLine();

            return true;
        }


        private static bool Extract_d50694eedd2a8595(string DrawIB, D3D11GameTypeLv2 d3D11GameTypeLv2, string PointlistIndex, List<string> TrianglelistIndexList)
        {

            string PositionSlot = "";
            string BlendSlot = "";
            string PositionExtractIndex = "";

            //先匹配出正确的数据类型，顺便得到从哪个Slot中提取的。
            List<D3D11GameType> PossibleD3d11GameTypeList = [];
            foreach (D3D11GameType d3D11GameType in d3D11GameTypeLv2.Ordered_GPU_CPU_D3D11GameTypeList)
            {

                //首先肯定得有vb1槽位，否则无法提取
                bool ExistsVB1Slot = false;
                foreach (var item in d3D11GameType.CategorySlotDict)
                {
                    if (item.Value == "vb1")
                    {
                        ExistsVB1Slot = true;
                        break;
                    }
                }

                if (!ExistsVB1Slot)
                {
                    continue;
                }


                //获取满足条件的TrianglelistIndex
                string TrianglelistIndex = d3D11GameTypeLv2.FilterTrianglelistIndex_UnityVS(TrianglelistIndexList, d3D11GameType);

                //获取Buffer文件
                string VB1BufferFileName = FrameAnalysisDataUtils.FilterFirstFile(GlobalConfig.WorkFolder, TrianglelistIndex + "-vb1=", ".buf");
                string VB1BufferFilePath = FrameAnalysisLogUtils.Get_DedupedFilePath(VB1BufferFileName);
                int VB1Size = (int)DBMTFileUtils.GetFileSize(VB1BufferFilePath);

                //求出预期顶点数
                int VertexCount = VB1Size / d3D11GameType.CategoryStrideDict["Texcoord"];

                int PositionStride = d3D11GameType.CategoryStrideDict["Position"];
                int BlendStride = d3D11GameType.CategoryStrideDict["Blend"];

                //随后依次判断t0到t6的Position的顶点数,对应u0到u6
                bool BlendT0 = IsBlendSlotMatch(PointlistIndex, BlendStride, VertexCount, "cs-t0");
                if (BlendT0)
                {
                    BlendSlot = "cs-t0";
                    PositionSlot = "u0";

                    //寻找Position的上一个Hash
                    string Index = GetPrePositionIndex(PointlistIndex,PositionStride,VertexCount,"u0");
                    if (Index != "")
                    {
                        PositionExtractIndex = Index;
                        PossibleD3d11GameTypeList.Add(d3D11GameType);
                        continue;
                    }
                }

                bool BlendT1 = IsBlendSlotMatch(PointlistIndex, BlendStride, VertexCount, "cs-t1");
                if (BlendT1)
                {
                    BlendSlot = "cs-t1";
                    PositionSlot = "u1";

                    //寻找Position的上一个Hash
                    string Index = GetPrePositionIndex(PointlistIndex, PositionStride, VertexCount, "u1");
                    if (Index != "")
                    {
                        PositionExtractIndex = Index;
                        PossibleD3d11GameTypeList.Add(d3D11GameType);
                        continue;
                    }
                }

                bool BlendT2 = IsBlendSlotMatch(PointlistIndex, BlendStride, VertexCount, "cs-t2");
                if (BlendT2)
                {
                    BlendSlot = "cs-t2";
                    PositionSlot = "u2";

                    //寻找Position的上一个Hash
                    string Index = GetPrePositionIndex(PointlistIndex, PositionStride, VertexCount, "u2");
                    if (Index != "")
                    {
                        PositionExtractIndex = Index;
                        PossibleD3d11GameTypeList.Add(d3D11GameType);
                        continue;
                    }
                }

                bool BlendT3 = IsBlendSlotMatch(PointlistIndex, BlendStride, VertexCount, "cs-t3");
                if (BlendT3)
                {
                    BlendSlot = "cs-t3";
                    PositionSlot = "u3";

                    //寻找Position的上一个Hash
                    string Index = GetPrePositionIndex(PointlistIndex, PositionStride, VertexCount, "u3");
                    if (Index != "")
                    {
                        PositionExtractIndex = Index;
                        PossibleD3d11GameTypeList.Add(d3D11GameType);
                        continue;
                    }
                }

                bool BlendT4 = IsBlendSlotMatch(PointlistIndex, BlendStride, VertexCount, "cs-t4");
                if (BlendT4)
                {
                    BlendSlot = "cs-t4";
                    PositionSlot = "u4";

                    //寻找Position的上一个Hash
                    string Index = GetPrePositionIndex(PointlistIndex, PositionStride, VertexCount, "u4");
                    if (Index != "")
                    {
                        PositionExtractIndex = Index;
                        PossibleD3d11GameTypeList.Add(d3D11GameType);
                        continue;
                    }
                }

                bool BlendT5 = IsBlendSlotMatch(PointlistIndex, BlendStride, VertexCount, "cs-t5");
                if (BlendT5)
                {
                    BlendSlot = "cs-t5";
                    PositionSlot = "u5";

                    //寻找Position的上一个Hash
                    string Index = GetPrePositionIndex(PointlistIndex, PositionStride, VertexCount, "u5");
                    if (Index != "")
                    {
                        PositionExtractIndex = Index;
                        PossibleD3d11GameTypeList.Add(d3D11GameType);
                        continue;
                    }
                }

                bool BlendT6 = IsBlendSlotMatch(PointlistIndex, BlendStride, VertexCount, "cs-t6");
                if (BlendT6)
                {
                    BlendSlot = "cs-t6";
                    PositionSlot = "u6";

                    //寻找Position的上一个Hash
                    string Index = GetPrePositionIndex(PointlistIndex, PositionStride, VertexCount, "u6");
                    if (Index != "")
                    {
                        PositionExtractIndex = Index;
                        PossibleD3d11GameTypeList.Add(d3D11GameType);
                        continue;
                    }
                }

            }
            LOG.NewLine();

            LOG.Info("PositionSlot: " + PositionSlot);
            LOG.Info("BlendSlot: " + BlendSlot);
            LOG.Info("PositionExtractIndex: " + PositionExtractIndex);
            LOG.NewLine();

            LOG.Info("识别到的数据类型: ");
            foreach (D3D11GameType d3D11GameType in PossibleD3d11GameTypeList)
            {
                LOG.Info(d3D11GameType.GameTypeName);
            }
            LOG.NewLine();


            if (PossibleD3d11GameTypeList.Count == 0)
            {
                LOG.Error("无法找到任何已知数据类型，请进行添加");
                return false;
            }

            //直接走提取逻辑
            //接下来提取出每一种可能性
            //读取一个MatchFirstIndex_IBFileName_Dict
            SortedDictionary<int, string> MatchFirstIndex_IBTxtFileName_Dict = new SortedDictionary<int, string>();
            foreach (string TrianglelistIndex in TrianglelistIndexList)
            {
                string IBFileName = FrameAnalysisDataUtils.FilterFirstFile(GlobalConfig.WorkFolder, TrianglelistIndex + "-ib", ".txt");
                if (IBFileName == "")
                {
                    continue;
                }
                string IBFilePath = FrameAnalysisLogUtils.Get_DedupedFilePath(IBFileName);
                IndexBufferTxtFile IBTxtFile = new IndexBufferTxtFile(IBFilePath, false);
                MatchFirstIndex_IBTxtFileName_Dict[int.Parse(IBTxtFile.FirstIndex)] = IBFileName;
            }

            foreach (var item in MatchFirstIndex_IBTxtFileName_Dict)
            {
                LOG.Info("MatchFirstIndex: " + item.Key.ToString() + " IBFileName: " + item.Value);
            }
            LOG.NewLine();

            foreach (D3D11GameType d3D11GameType in PossibleD3d11GameTypeList)
            {
                //先设置一下CategorySlot，因为可能数据类型文件里面没写
                d3D11GameType.CategorySlotDict["Position"] = PositionSlot;
                d3D11GameType.CategorySlotDict["Blend"] = BlendSlot;

                string TrianglelistIndex = d3D11GameTypeLv2.FilterTrianglelistIndex_UnityVS(TrianglelistIndexList, d3D11GameType);

                Dictionary<string, string> CategoryBufFileMap = new Dictionary<string, string>();
                foreach (var item in d3D11GameType.CategoryTopologyDict)
                {
                    string CategoryName = item.Key;
                    string ExtractIndex = TrianglelistIndex;
                    if (item.Value == "pointlist" && PointlistIndex != "")
                    {
                        ExtractIndex = PointlistIndex;

                        if (CategoryName == "Position")
                        {
                            ExtractIndex = PositionExtractIndex;
                        }
                    }
                    string CategorySlot = d3D11GameType.CategorySlotDict[item.Key];

                    if (CategoryName == "Position")
                    {
                        CategorySlot = PositionSlot;
                    }
                    else if (CategoryName == "Blend")
                    {
                        CategorySlot = BlendSlot;
                    }

                    //获取文件名存入对应Dict
                    string CategoryBufFileName = FrameAnalysisDataUtils.FilterFirstFile(GlobalConfig.WorkFolder, ExtractIndex + "-" + CategorySlot + "=", ".buf");
                    CategoryBufFileMap[item.Key] = CategoryBufFileName;
                }

                string GameTypeFolderName = "TYPE_" + d3D11GameType.GameTypeName;
                string DrawIBFolderPath = Path.Combine(GlobalConfig.Path_CurrentWorkSpaceFolder, DrawIB + "\\");
                string GameTypeOutputPath = Path.Combine(DrawIBFolderPath, GameTypeFolderName + "\\");
                if (!Directory.Exists(GameTypeOutputPath))
                {
                    Directory.CreateDirectory(GameTypeOutputPath);
                }

                LOG.Info("开始从各个Buffer文件中读取数据:");
                //接下来从各个Buffer中读取并且拼接为FinalVB0

                List<Dictionary<int, byte[]>> BufDictList = new List<Dictionary<int, byte[]>>();
                foreach (var item in CategoryBufFileMap)
                {
                    string CategoryName = item.Key;
                    string CategoryBufFileName = item.Value;
                    string CategoryBufFilePath = FrameAnalysisLogUtils.Get_DedupedFilePath(CategoryBufFileName);
                    int CategoryStride = d3D11GameType.CategoryStrideDict[CategoryName];

                    Dictionary<int, byte[]> BufDict = DBMTBinaryUtils.ReadBinaryFileByStride(CategoryBufFilePath, CategoryStride);
                    BufDictList.Add(BufDict);
                }
                LOG.NewLine();

                Dictionary<int, byte[]> MergedVB0Dict = DBMTBinaryUtils.MergeByteDicts(BufDictList);
                byte[] FinalVB0 = DBMTBinaryUtils.MergeDictionaryValues(MergedVB0Dict);

                //接下来遍历MatchFirstIndex_IBFileName的Map，对于每个MarchFirstIndex
                //都读取IBTxt文件里的数值，然后进行分割并输出。
                int OutputCount = 1;
                foreach (var item in MatchFirstIndex_IBTxtFileName_Dict)
                {
                    int MatchFirstIndex = item.Key;
                    string IBTxtFileName = item.Value;
                    //拼接出一个IBBufFileName
                    string IBBufFileName = Path.GetFileNameWithoutExtension(IBTxtFileName) + ".buf";

                    string IBTxtFilePath = FrameAnalysisLogUtils.Get_DedupedFilePath(IBTxtFileName);
                    string IBBufFilePath = FrameAnalysisLogUtils.Get_DedupedFilePath(IBBufFileName);

                    IndexBufferTxtFile IBTxtFile = new IndexBufferTxtFile(IBTxtFilePath, true);
                    LOG.Info(IBTxtFilePath);
                    LOG.Info("FirstIndex: " + IBTxtFile.FirstIndex);
                    LOG.Info("IndexCount: " + IBTxtFile.IndexCount);

                    string NamePrefix = DrawIB + "-" + OutputCount.ToString();

                    string OutputIBBufFilePath = Path.Combine(GameTypeOutputPath, NamePrefix + ".ib");
                    string OutputVBBufFilePath = Path.Combine(GameTypeOutputPath, NamePrefix + ".vb");
                    string OutputFmtFilePath = Path.Combine(GameTypeOutputPath, NamePrefix + ".fmt");

                    //通过D3D11GameType合成一个FMT文件并且输出
                    FmtFile fmtFile = new FmtFile(d3D11GameType);
                    fmtFile.OutputFmtFile(OutputFmtFilePath);

                    //写出IBBufFile
                    IndexBufferBufFile IBBufFile = new IndexBufferBufFile(IBBufFilePath, IBTxtFile.Format);

               
                    //这里使用IndexNumberCount的话，只能用于正向提取
                    //如果要兼容逆向提取，需要换成IndexCount
                    //但是还有个问题，那就是即使换成IndexCount，如果IB文件的替换不是一个整体的Buffer，而是各个独立分开的Buffer
                    //则这里的SelfDivide是不应该存在的步骤，所以这里是无法逆向提取的。
                    //综合来看，逆向提取其实是一种适用性不强，并且很容易受到ini中各种因素干扰的提取方式
                    //但是如果能获取到DrawIndexed的具体数值呢？可以通过解析log.txt的方式进行获取
                    //但是解析很玛法，而且就算能获取到，那如果有复杂的CommandList混淆，投入与产出不成正比了就
                    //使用逆向Mod的ini的方式更加优雅。
                    IBBufFile.SelfDivide(int.Parse(IBTxtFile.FirstIndex), (int)IBTxtFile.IndexNumberCount);
                    IBBufFile.SaveToFile_UInt32(OutputIBBufFilePath, -1 * IBBufFile.MinNumber);

                    //写出VBBufFile
                    VertexBufferBufFile VBBufFile = new VertexBufferBufFile(FinalVB0);
                    if (IBBufFile.MinNumber > IBBufFile.MaxNumber)
                    {
                        LOG.Error("当前IB文件最小值大于IB文件中的最大值，疑似逆向提取Mod模型出错，跳过此模型输出。");
                    }

                    VBBufFile.SelfDivide(IBBufFile.MinNumber, IBBufFile.MaxNumber, d3D11GameType.GetSelfStride());
                    VBBufFile.SaveToFile(OutputVBBufFilePath);

                    OutputCount += 1;
                }


                ImportJson importJson = new ImportJson();
                string VB0FileName = FrameAnalysisDataUtils.FilterFirstFile(GlobalConfig.WorkFolder, TrianglelistIndex + "-vb0", ".txt");

                importJson.DrawIB = DrawIB;
                importJson.VertexLimitVB = VB0FileName.Substring(11, 8);
                importJson.d3D11GameType = d3D11GameType;
                importJson.Category_BufFileName_Dict = CategoryBufFileMap;
                importJson.MatchFirstIndex_IBTxtFileName_Dict = MatchFirstIndex_IBTxtFileName_Dict;

                //TODO 暂时叫tmp.json，后面再改
                string ImportJsonSavePath = Path.Combine(GameTypeOutputPath, "tmp.json");
                importJson.SaveToFile(ImportJsonSavePath);
            }

            LOG.NewLine();

            return true;
        }


        public static bool ExtractHSR32(List<DrawIBItem> DrawIBItemList)
        {
            D3D11GameTypeLv2 d3D11GameTypeLv2 = new D3D11GameTypeLv2(GlobalConfig.CurrentGameName);

            LOG.Info("开始提取 HSR 3.2 测试:");
            foreach (DrawIBItem drawIBItem in DrawIBItemList)
            {
                string DrawIB = drawIBItem.DrawIB;

                if (DrawIB.Trim() == "")
                {
                    continue;
                }
                else
                {
                    LOG.Info("当前DrawIB: " + DrawIB);
                }
                LOG.NewLine();

                string PointlistIndex = FrameAnalysisLogUtils.Get_PointlistIndex_ByHash(DrawIB);
                LOG.Info("当前识别到的PointlistIndex: " + PointlistIndex);
                if (PointlistIndex == "")
                {
                    LOG.Info("当前识别到的PointlistIndex为空，此DrawIB对应的模型可能为CPU-PreSkinning类型。");
                }
                LOG.NewLine();

                List<string> TrianglelistIndexList = FrameAnalysisLogUtils.Get_DrawCallIndexList_ByHash(DrawIB, false);
                foreach (string TrianglelistIndex in TrianglelistIndexList)
                {
                    LOG.Info("TrianglelistIndex: " + TrianglelistIndex);
                }
                LOG.NewLine();


                /*
                 * 接下来要判断是否是脸部和头发的特殊Shader
                 因为在崩铁3.2版本更新后，有多种ComputeShader分别负责不同部分的渲染。
                    - 脸部、头发 1c932707d4d8df41
                    - 身体
                    - 组队界面多角色同时渲染 1c932707d4d8df41
                    - NPC集体渲染
                 */

                string CSCB0FileName = FrameAnalysisDataUtils.FilterFirstFile(GlobalConfig.WorkFolder, PointlistIndex + "-cs-cb0=", ".buf");
                if (CSCB0FileName.Contains("1c932707d4d8df41"))
                {
                    //t0t5到t4t9特殊提取
                    bool result = Extract_1c932707d4d8df41(DrawIB,d3D11GameTypeLv2,PointlistIndex,TrianglelistIndexList);
                    if (!result)
                    {
                        return false;
                    }
                }
                else if (CSCB0FileName.Contains("fee307b98a965c16"))
                {
                    //普通ComputeShader cs-t0 cs-t1提取
                    bool result = Extract_fee307b98a965c16(DrawIB, d3D11GameTypeLv2, PointlistIndex, TrianglelistIndexList);
                    if (!result)
                    {
                        return false;
                    }
                }
                //组队界面的，比较特殊，需要重新写提取逻辑。
                else if (CSCB0FileName.Contains("d50694eedd2a8595"))
                {
                    //普通ComputeShader cs-t0 cs-t1提取
                    bool result = Extract_d50694eedd2a8595(DrawIB, d3D11GameTypeLv2, PointlistIndex, TrianglelistIndexList);
                    if (!result)
                    {
                        return false;
                    }
                }
                else
                {
                    //普通提取，主要是为了支持CPU类型，但是它和另一个是通用的
                    bool result = Extract_fee307b98a965c16(DrawIB, d3D11GameTypeLv2, PointlistIndex, TrianglelistIndexList);
                    if (!result)
                    {
                        return false;
                    }
                }






            }




            LOG.NewLine("提取正常执行完成");
            return true;
        }


    }
}
