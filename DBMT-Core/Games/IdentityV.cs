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
    public static class IdentityV
    {

        public static bool ExtractCTX(List<DrawIBItem> DrawIBItemList)
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

                FrameAnalysisInfo FAInfo = new FrameAnalysisInfo(DrawIB);

                string[] TotalFrameAnalysisFiles = Directory.GetFiles(GlobalConfig.Path_LatestFrameAnalysisFolder);

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

                List<string> LogTxtFilePathListContainsDrawIB = [];
                foreach (string LogTxtFilePath in LogTxtFilePathList)
                {
                    string[] LogLineList = File.ReadAllLines(LogTxtFilePath);
                    foreach (string LogLine in LogLineList)
                    {
                        if (LogLine.Contains("hash=" + DrawIB))
                        {
                            LOG.Info("当前DrawIB在【" + LogTxtFilePath + "】中存在");
                            LogTxtFilePathListContainsDrawIB.Add(LogTxtFilePath);
                            break;
                        }
                    }
                }

                //接下来遍历每个log.txt顺便获取到对应的CTX文件夹路径
                foreach (string LogTxtFilePath in LogTxtFilePathListContainsDrawIB)
                {
                    string LogFileName = Path.GetFileName(LogTxtFilePath);
                    string CTXCode = LogFileName.Split("log-")[1].Split(".txt")[0];
                    LOG.Info("CTXCode: " + CTXCode);
                    string CTXFolderName = "ctx-" + CTXCode;
                    string CTXLogFileName = "log-" + CTXCode + ".txt";
                    string CTXLogFilePath = Path.Combine(GlobalConfig.Path_LatestFrameAnalysisFolder, CTXLogFileName);
                    string CTXFolderPath = Path.Combine(GlobalConfig.Path_LatestFrameAnalysisFolder, CTXFolderName + "\\");
                    LOG.Info("CTXFolderPath: " + CTXFolderPath);


                    //接下来也不需要PointlistIndex，因为是在Trianglelist上直接计算骨骼变换矩阵的。
                    List<string> TrianglelistIBFileList = FrameAnalysisDataUtils.FilterFile(CTXFolderPath, "-ib=" + DrawIB, ".txt");

                    List<string> TrianglelistIndexList = [];
                    LOG.Info("TrianglelistIndexList:");
                    foreach (string TrianglelistIBFileName in TrianglelistIBFileList)
                    {
                        string Index = TrianglelistIBFileName.Substring(0, 6);
                        TrianglelistIndexList.Add(Index);
                        LOG.Info("Index: " + Index);
                    }

                    if (TrianglelistIndexList.Count == 0)
                    {
                        LOG.NewLine("当前CTX文件夹中未找到任何DrawIB相关文件，跳过提取");
                        continue;
                    }


                    //使用第一个TrianglelistIndex来进行数据类型识别
                    string TmpTrianglelistIndex = TrianglelistIndexList[0];

                    List<D3D11GameType> PossibleGameTypeList = [];
                    foreach (D3D11GameType d3D11GameType in d3D11GameTypeLv2.Ordered_GPU_CPU_D3D11GameTypeList)
                    {
                        LOG.Info("尝试匹配数据类型: " + d3D11GameType.GameTypeName);

                        Dictionary<string, string> CategoryName_BufFilePath_Dict = new Dictionary<string, string>();
                        bool AllSlotBufFileExists = true;
                        //先校验当前数据类型文件的各个槽位的Buffer文件是否存在
                        foreach (var item in d3D11GameType.CategorySlotDict)
                        {
                            string CategoryName = item.Key;
                            string CategorySlot = item.Value;

                            string CategoryBufFileName = FrameAnalysisDataUtils.FilterFirstFile(CTXFolderPath, TmpTrianglelistIndex + "-" + CategorySlot + "=", ".buf");
                            if (CategoryBufFileName == "")
                            {
                                AllSlotBufFileExists = false;
                                break;
                            }

                            string CategoryBufFilePath = Path.Combine(CTXFolderPath, CategoryBufFileName);
                            CategoryName_BufFilePath_Dict[CategoryName] = CategoryBufFilePath;
                        }

                        if (!AllSlotBufFileExists)
                        {
                            LOG.NewLine("当前数据类型并非所有的槽位Buffer文件都存在，不满足，跳过。");
                            continue;
                        }

                        int VertexNumber = 0;
                        bool AllMatch = true;
                        //到这里说明Buffer文件都存在，那就读取对应的Buffer文件，计算顶点数是否全部能够匹配。
                        foreach (string CategoryName in d3D11GameType.OrderedCategoryNameList)
                        {

                            int CategoryStride = d3D11GameType.CategoryStrideDict[CategoryName];
                            LOG.Info("当前匹配槽位: " + CategoryName + " Stride: " + CategoryStride.ToString());

                            string BufFilePath = CategoryName_BufFilePath_Dict[CategoryName];
                            int BufFileSize = (int)DBMTFileUtils.GetFileSize(BufFilePath);
                            int TmpNumber = BufFileSize / CategoryStride;

                            //IdentityV: 使用精准匹配机制来过滤数据类型，如果有余数，说明此分类不匹配。
                            int YuShu = BufFileSize % CategoryStride;
                            if (YuShu != 0)
                            {

                                LOG.Error("余数不为0: " + YuShu.ToString() + "  ，文件步长除以类别步长，不能含有余数，否则为不支持的匹配方式，比如PatchNull，或者数据类型匹配错误，类型错误时自然会产生余数。");
                                AllMatch = false;
                                break;
                            }

                            if (VertexNumber == 0)
                            {
                                //第一次不为0时赋值，方便后续匹配
                                VertexNumber = TmpNumber;
                            }


                            if (TmpNumber == 0)
                            {
                                LOG.Error("当前匹配的槽位文件大小为0: " + BufFilePath);
                                AllMatch = false;
                                break;
                            }


                            if (VertexNumber != TmpNumber)
                            {
                                LOG.Info("VertexNumber: " + VertexNumber.ToString() + " 当前槽位数量: " + TmpNumber.ToString());
                                LOG.Info("槽位匹配失败");
                                LOG.NewLine();

                                AllMatch = false;
                                break;
                            }

                        }

                        if (AllMatch)
                        {
                            PossibleGameTypeList.Add(d3D11GameType);
                        }

                    }

                    if (PossibleGameTypeList.Count == 0)
                    {
                        LOG.Error("未找到任何匹配的数据类型。");
                        return false;
                    }

                    LOG.Info("当前匹配到的数据类型列表:");
                    foreach (D3D11GameType d3d11GameType in PossibleGameTypeList)
                    {
                        LOG.Info(d3d11GameType.GameTypeName);
                    }
                    LOG.NewLine();

                    SortedDictionary<int, string> MatchFirstIndex_IBFileName_Dict = new SortedDictionary<int, string>();
                    foreach (string TrianglelistIndex in TrianglelistIndexList)
                    {
                        string IBTxtFileName = FrameAnalysisDataUtils.FilterFirstFile(CTXFolderPath, TrianglelistIndex + "-ib", ".txt");
                        if (IBTxtFileName == "")
                        {
                            continue;
                        }
                        string IBFilePath = Path.Combine(CTXFolderPath, IBTxtFileName);
                        IndexBufferTxtFile IBTxtFile = new IndexBufferTxtFile(IBFilePath, false);
                        int MatchFirstIndex = int.Parse(IBTxtFile.FirstIndex);
                        MatchFirstIndex_IBFileName_Dict[MatchFirstIndex] = IBTxtFileName;

                        LOG.Info(IBFilePath);
                    }

                    //每种可能的数据类型都要进行一次Buffer文件的读取
                    foreach (D3D11GameType d3d11GameType in PossibleGameTypeList)
                    {
                        LOG.Info("当前提取数据类型:" + d3d11GameType.GameTypeName);
                        string GameTypeFolderName = "TYPE_" + d3d11GameType.GameTypeName;
                        string DrawIBFolderPath = Path.Combine(GlobalConfig.Path_CurrentWorkSpaceFolder, DrawIB + "\\");
                        string GameTypeOutputPath = Path.Combine(DrawIBFolderPath, GameTypeFolderName + "\\");
                        if (!Directory.Exists(GameTypeOutputPath))
                        {
                            Directory.CreateDirectory(GameTypeOutputPath);
                        }

                        LOG.Info("开始从各个Buffer文件中读取数据:");
                        //接下来从各个Buffer中读取并且拼接为FinalVB0


                        Dictionary<string, string> CategoryName_BufFileName_Dict = new Dictionary<string, string>();
                        foreach (var item in d3d11GameType.CategorySlotDict)
                        {
                            string CategoryName = item.Key;
                            string CategorySlot = item.Value;

                            string CategoryBufFileName = FrameAnalysisDataUtils.FilterFirstFile(CTXFolderPath, TmpTrianglelistIndex + "-" + CategorySlot + "=", ".buf");
                            CategoryName_BufFileName_Dict[CategoryName] = CategoryBufFileName;
                        }

                        List<Dictionary<int, byte[]>> BufDictList = new List<Dictionary<int, byte[]>>();
                        foreach (string CategoryName in d3d11GameType.OrderedCategoryNameList)
                        {
                            string CtegoryBufFileName = CategoryName_BufFileName_Dict[CategoryName];
                            int CategoryStride = d3d11GameType.CategoryStrideDict[CategoryName];

                            string CtegoryBufFilePath = Path.Combine(CTXFolderPath, CtegoryBufFileName);
                            Dictionary<int, byte[]> BufDict = DBMTBinaryUtils.ReadBinaryFileByStride(CtegoryBufFilePath, CategoryStride);
                            BufDictList.Add(BufDict);
                        }

                        Dictionary<int, byte[]> MergedVB0Dict = DBMTBinaryUtils.MergeByteDicts(BufDictList);
                        byte[] FinalVB0 = DBMTBinaryUtils.MergeDictionaryValues(MergedVB0Dict);

                        //接下来遍历MatchFirstIndex_IBFileName的Map，对于每个MarchFirstIndex
                        //都读取IBTxt文件里的数值，然后进行分割并输出。
                        int OutputCount = 1;
                        foreach (var item in MatchFirstIndex_IBFileName_Dict)
                        {
                            int MatchFirstIndex = item.Key;
                            string IBTxtFileName = item.Value;
                            string IBTxtFilePath = Path.Combine(CTXFolderPath, IBTxtFileName);
                            //拼接出一个IBBufFileName
                            string IBBufFileName = Path.GetFileNameWithoutExtension(IBTxtFileName) + ".buf";
                            LOG.Info(IBBufFileName);

                            string IBBufFilePath = Path.Combine(CTXFolderPath, IBBufFileName);

                            IndexBufferTxtFile IBTxtFile = new IndexBufferTxtFile(IBTxtFilePath, true);
                            LOG.Info(IBTxtFilePath);
                            LOG.Info("FirstIndex: " + IBTxtFile.FirstIndex);
                            LOG.Info("IndexCount: " + IBTxtFile.IndexCount);

                            string NamePrefix = DrawIB + "-" + OutputCount.ToString();

                            string OutputIBBufFilePath = Path.Combine(GameTypeOutputPath, NamePrefix + ".ib");
                            string OutputVBBufFilePath = Path.Combine(GameTypeOutputPath, NamePrefix + ".vb");
                            string OutputFmtFilePath = Path.Combine(GameTypeOutputPath, NamePrefix + ".fmt");

                            //通过D3D11GameType合成一个FMT文件并且输出
                            FmtFile fmtFile = new FmtFile(d3d11GameType);
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
                                VBBufFile.SelfDivide(IBBufFile.MinNumber, IBBufFile.MaxNumber, d3d11GameType.GetSelfStride());
                            }
                            VBBufFile.SaveToFile(OutputVBBufFilePath);

                            OutputCount += 1;
                        }

                        //TODO 每个数据类型文件夹下面都需要生成一个tmp.json，但是新版应该改名为Import.json
                        //为了兼容旧版Catter，暂时先不改名

                        ImportJson importJson = new ImportJson();
                        string VB0FileName = FrameAnalysisDataUtils.FilterFirstFile(CTXFolderPath, TmpTrianglelistIndex + "-vb0", ".txt");

                        importJson.DrawIB = DrawIB;
                        importJson.VertexLimitVB = VB0FileName.Substring(11, 8);
                        importJson.d3D11GameType = d3d11GameType;
                        importJson.Category_BufFileName_Dict = CategoryName_BufFileName_Dict;
                        importJson.MatchFirstIndex_IBTxtFileName_Dict = MatchFirstIndex_IBFileName_Dict;

                        //需要额外计算CategoryHashDict;
                        Dictionary<string, string> CategoryHashDict = new Dictionary<string, string>();
                        foreach (var item in CategoryName_BufFileName_Dict)
                        {
                            string CategoryName = item.Key;
                            string BufFileName = item.Value;

                            string CategorySlot = d3d11GameType.CategorySlotDict[CategoryName];

                            //这里的8是由000001-vb0= 这里的00001为Index固定为6个，后面-固定一个，=固定一个，所以一共是8个长度
                            //然后再加上CategorySlot的长度，正好
                            int StartIndex = 8 + CategorySlot.Length;

                            string Hash = BufFileName.Substring(StartIndex, 8);
                            CategoryHashDict[CategoryName] = Hash;
                        }
                        importJson.CategoryHashDict = CategoryHashDict;


                        //TODO 暂时叫tmp.json，后面再改
                        string ImportJsonSavePath = Path.Combine(GameTypeOutputPath, "tmp.json");
                        importJson.SaveFile(ImportJsonSavePath);
                    }


                }


            }


            LOG.Info("提取正常执行完成");
            return true;
        }
    }
}
