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
    public static class AILimit
    {
        public static string FilterTrianglelistIndex_UnityVS(List<string> TrianglelistIndexList, D3D11GameType d3D11GameType)
        {
            string FinalTrianglelistIndex = "";
            foreach (string TrianglelistIndex in TrianglelistIndexList)
            {
                bool AllSlotExists = true;
                foreach (var item in d3D11GameType.CategoryTopologyDict)
                {
                    string Category = item.Key;
                    string Topology = item.Value;

                    if (Topology != "trianglelist")
                    {
                        continue;
                    }

                    //获取当前Category的Slot
                    string CategorySlot = d3D11GameType.CategorySlotDict[Category];

                    //寻找对应Buf文件
                    string CategoryBufFileName = FrameAnalysisDataUtils.FilterFirstFile(GlobalConfig.WorkFolder, TrianglelistIndex + "-" + CategorySlot, ".buf");
                    if (CategoryBufFileName == "")
                    {
                        AllSlotExists = false;
                        break;
                    }
                }

                if (AllSlotExists)
                {
                    FinalTrianglelistIndex = TrianglelistIndex;
                    break;
                }
            }

            return FinalTrianglelistIndex;
        }

        public static List<D3D11GameType> GetPossibleGameTypeList_UnityVS(D3D11GameTypeLv2 d3D11GameTypeLv2,string PointlistIndex, List<string> TrianglelistIndexList)
        {
            List<D3D11GameType> PossibleGameTypeList = [];

            bool findAtLeastOneGPUType = false;
            foreach (D3D11GameType d3D11GameType in d3D11GameTypeLv2.Ordered_GPU_CPU_D3D11GameTypeList)
            {
                if (findAtLeastOneGPUType && !d3D11GameType.GPUPreSkinning)
                {
                    LOG.Info("自动优化:已经找到了满足条件的GPU类型，所以这个CPU类型就不用判断了");
                    continue;
                }

                LOG.Info("当前数据类型:" + d3D11GameType.GameTypeName);

                //传递过来一堆TrianglelistIndex，但是我们要找到满足条件的那个,即Buffer文件都存在的那个
                string TrianglelistIndex = FilterTrianglelistIndex_UnityVS(TrianglelistIndexList, d3D11GameType);
                LOG.Info("TrianglelistIndex: " + TrianglelistIndex);


                if (TrianglelistIndex == "")
                {
                    LOG.Info("当前GameType无法找到符合槽位存在条件的TrianglelistIndex，跳过此项");
                    continue;
                }

                //获取每个Category的Buffer文件
                Dictionary<string, string> CategoryBufFileMap = new Dictionary<string, string>();
                Dictionary<string, int> CategoryBufFileSizeMap = new Dictionary<string, int>();
                bool AllFileExists = true;
                foreach (var item in d3D11GameType.CategoryTopologyDict)
                {
                    string CategoryName = item.Key;
                    string ExtractIndex = TrianglelistIndex;
                    if (item.Value == "pointlist" && PointlistIndex != "")
                    {
                        ExtractIndex = PointlistIndex;
                    }
                    string CategorySlot = d3D11GameType.CategorySlotDict[CategoryName];
                    LOG.Info("当前分类:" + CategoryName + " 提取Index: " + ExtractIndex + " 提取槽位:" + CategorySlot);
                    //获取文件名存入对应Dict
                    string CategoryBufFileName = FrameAnalysisDataUtils.FilterFirstFile(GlobalConfig.WorkFolder, ExtractIndex + "-" + CategorySlot, ".buf");
                    CategoryBufFileMap[item.Key] = CategoryBufFileName;

                    //获取文件大小存入对应Dict
                    string CategoryBufFilePath = FrameAnalysisLogUtils.Get_DedupedFilePath(CategoryBufFileName);
                    //LOG.Info("Category: " + item.Key + " File:" + CategoryBufFilePath);
                    if (!File.Exists(CategoryBufFilePath))
                    {
                        LOG.Error("对应Buffer文件未找到,此数据类型无效。");
                        AllFileExists = false;
                        break;
                    }

                    long FileSize = DBMTFileUtils.GetFileSize(CategoryBufFilePath);
                    CategoryBufFileSizeMap[item.Key] = (int)FileSize;
                }

                if (!AllFileExists)
                {
                    LOG.Info("当前数据类型的部分槽位文件无法找到，跳过此数据类型识别。");
                    continue;
                }

                //校验顶点数是否在各Buffer中保持一致
                //TODO 通过校验顶点数的方式并不能100%确定，因为如果只有一个Category的话就会无法匹配步长
                int VertexNumber = 0;
                bool AllMatch = true;

                foreach (string CategoryName in d3D11GameType.OrderedCategoryNameList)
                {
                    int CategoryStride = d3D11GameType.CategoryStrideDict[CategoryName];
                    int FileSize = CategoryBufFileSizeMap[CategoryName];
                    int TmpNumber = FileSize / CategoryStride;

                    if (TmpNumber == 0)
                    {
                        LOG.Info("槽位的文件大小不能为0，槽位匹配失败，跳过此数据类型");
                        AllMatch = false;
                        break;
                    }



                    if (VertexNumber == 0)
                    {
                        VertexNumber = TmpNumber;
                    }
                    else if (VertexNumber != TmpNumber)
                    {
                        LOG.Info("VertexNumber: " + VertexNumber.ToString() + " 当前槽位数量: " + TmpNumber.ToString());
                        LOG.Info("槽位匹配失败");
                        LOG.NewLine();

                        AllMatch = false;
                        break;
                    }
                    else
                    {
                        LOG.Info(CategoryName + " Match!");
                        LOG.NewLine();
                    }
                }

                //LOG.Info("VertexNumber: " + VertexNumber.ToString());


                if (AllMatch)
                {
                    LOG.NewLine("MatchGameType: " + d3D11GameType.GameTypeName);
                    PossibleGameTypeList.Add(d3D11GameType);
                }

                //如果找到了一个GPUPreSkinning就标记一下，这样后面就不会匹配CPU类型了。
                if (!findAtLeastOneGPUType)
                {
                    foreach (D3D11GameType d3d11GameType in PossibleGameTypeList)
                    {
                        if (d3d11GameType.GPUPreSkinning)
                        {
                            findAtLeastOneGPUType = true;
                            break;
                        }
                    }
                }

            }


            if (PossibleGameTypeList.Count == 0)
            {
                LOG.Error("无法找到当前提取IndexBuffer Hash值对应Buffer数据的数据类型\n1.请结合日志信息，到数据类型管理页面添加此数据类型\n2.或者联系NicoMico添加支持并更新DBMT版本。");
            }
            else
            {
                LOG.Info("All Matched GameType:");
                foreach (D3D11GameType d3d11GameType in PossibleGameTypeList)
                {
                    LOG.Info(d3d11GameType.GameTypeName);
                }
            }
            return PossibleGameTypeList;
        }

        private static bool Extract_fee307b98a965c16(string DrawIB, D3D11GameTypeLv2 d3D11GameTypeLv2, string PointlistIndex, List<string> TrianglelistIndexList)
        {

            //接下来开始识别可能的数据类型。
            //此时需要先读取所有存在的数据类型。
            //此时需要我们先去生成几个数据类型用于测试。
            //还有就是数据类型的文件夹是存在哪里的
            List<D3D11GameType> PossibleD3D11GameTypeList = GetPossibleGameTypeList_UnityVS(d3D11GameTypeLv2,PointlistIndex, TrianglelistIndexList);

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
                    string CategoryBufFileName = FrameAnalysisDataUtils.FilterFirstFile(GlobalConfig.WorkFolder, ExtractIndex + "-" + CategorySlot, ".buf");
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
        public static bool ExtractUnityVS(List<DrawIBItem> DrawIBItemList)
        {
            D3D11GameTypeLv2 d3D11GameTypeLv2 = new D3D11GameTypeLv2(GlobalConfig.CurrentGameName);

            LOG.Info("开始提取:");
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

                bool result = Extract_fee307b98a965c16(DrawIB, d3D11GameTypeLv2, PointlistIndex, TrianglelistIndexList);
                if (!result)
                {
                    return false;
                }
            }


            LOG.Info("提取正常执行完成");
            return true;
        }

    }
}
