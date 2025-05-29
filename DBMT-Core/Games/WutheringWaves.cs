using DBMT_Core.Common;
using DBMT_Core.Utils;
using DBMT_Core.WWMI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBMT_Core.Games
{
    public static class WutheringWaves
    {

        public static bool ExtractWWMI(List<DrawIBItem> DrawIBItemList)
        {
            LOG.Info("开始提取:");
            foreach (DrawIBItem drawIBItem in DrawIBItemList)
            {
                string DrawIB = drawIBItem.DrawIB;

                if (DrawIB.Trim() == "")
                {
                    continue;
                }
                else if (DrawIB == "8d45cfee")
                {
                    LOG.Error("恭喜你触发了萌新常见错误：8d45cfee这个DrawIB是假的，无法用于模型提取，请去游戏中查找该模型的另一个真实的IB值。");
                    continue;
                }
                else
                {
                    LOG.Info("当前DrawIB: " + DrawIB);
                }
                LOG.NewLine();

                //WWMI提取逻辑

                //1.获取所有的TrianglelistIndex，而且WWMI很特殊，不能从Log获取，必须从FrameAnalyse文件里分析文件名来获取。
                List<string> TrianglelistIndexList = FrameAnalysisDataUtils.Get_TrianglelistIndexListByDrawIB(GlobalConfig.WorkFolder,DrawIB);
                if (TrianglelistIndexList.Count == 0)
                {
                    LOG.Error("无法找到当前DrawIB: " + DrawIB + " 对应的数据文件，请检查是否Dump错误或者DrawIB输入错误。");
                    return false;
                }

                Dictionary<string, string> Total_CategorySlot_Hash_Dict = new Dictionary<string, string>();

                int MaxSlotNumber = 0;
                string MaxSlotTrianglelistIndex = "";

                foreach (string TrianglelistIndex in TrianglelistIndexList)
                {
                    Dictionary<string, string> CategorySlot_Hash_Dict = FrameAnalysisLogUtils.Get_VBCategoryHashMap_FromIASetVertexBuffer_ByIndex(TrianglelistIndex);

                    foreach (var item in CategorySlot_Hash_Dict)
                    {
                        Total_CategorySlot_Hash_Dict[item.Key] = item.Value;
                    }

                    if (CategorySlot_Hash_Dict.Count > MaxSlotNumber)
                    {
                        MaxSlotNumber = CategorySlot_Hash_Dict.Count;
                        MaxSlotTrianglelistIndex = TrianglelistIndex;
                    }
                }
                LOG.NewLine();

                LOG.Info("TrianglelistIndex: " + MaxSlotTrianglelistIndex);
                LOG.Info("MaxSlotNumber: " + MaxSlotNumber.ToString());
                //输出查看一下
                foreach (var item in Total_CategorySlot_Hash_Dict)
                {
                    LOG.Info("CategorySlot: " + item.Key + "  Hash: " + item.Value);
                }
                LOG.NewLine();


                Dictionary<string, string> CategorySlot_FileName_Dict = new Dictionary<string, string>();
                foreach (var item in Total_CategorySlot_Hash_Dict)
                {
                    string CategorySlot = item.Key;

                    string CategoryFileName = FrameAnalysisDataUtils.FilterFirstFile(GlobalConfig.WorkFolder, MaxSlotTrianglelistIndex + "-" + CategorySlot, ".buf");
                    if (CategoryFileName == "")
                    {
                        continue;
                    }
                    CategorySlot_FileName_Dict[CategorySlot] = CategoryFileName;
                    LOG.Info("CategorySlot: " + CategorySlot + " ExtractFileName: " + CategoryFileName);
                }
                LOG.NewLine();



                D3D11GameTypeLv2 d3D11GameTypeLv2 = new D3D11GameTypeLv2(GlobalConfig.CurrentGameName);

                List<D3D11GameType> PossibleD3D11GameTypeList = d3D11GameTypeLv2.GetPossibleGameTypeList_UnrealVS(CategorySlot_FileName_Dict);
                if (PossibleD3D11GameTypeList.Count == 0)
                {
                    return false;
                }
                LOG.NewLine();

                foreach (D3D11GameType d3D11GameType in PossibleD3D11GameTypeList)
                {
                    string GameTypeFolderName = "TYPE_" + d3D11GameType.GameTypeName;
                    string DrawIBFolderPath = Path.Combine(GlobalConfig.Path_CurrentWorkSpaceFolder, DrawIB + "\\");
                    string GameTypeOutputPath = Path.Combine(DrawIBFolderPath, GameTypeFolderName + "\\");
                    if (!Directory.Exists(GameTypeOutputPath))
                    {
                        Directory.CreateDirectory(GameTypeOutputPath);
                    }

                    Dictionary<string, string> Category_BufFileNameDict = new Dictionary<string, string>();
                    foreach (var item in d3D11GameType.CategorySlotDict)
                    {
                        string CategoryName = item.Key;
                        string CategorySlot = item.Value;
                        string CategoryBufFileName = CategorySlot_FileName_Dict[CategorySlot];
                        Category_BufFileNameDict[CategoryName] = CategoryBufFileName;
                    }


                    LOG.Info("开始从各个Buffer文件中读取数据:");
                    //接下来从各个Buffer中读取并且拼接为FinalVB0

                    List<Dictionary<int, byte[]>> BufDictList = new List<Dictionary<int, byte[]>>();
                    foreach (var item in Category_BufFileNameDict)
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


                    //想办法获取IB文件的Format，用于判断是R16_UINT还是R32_UINT,WWMI比较特殊两种都用到了。
                    string IBTxtFileName = FrameAnalysisDataUtils.FilterFirstFile(GlobalConfig.WorkFolder, MaxSlotTrianglelistIndex + "-ib", ".txt");
                    if (IBTxtFileName == "")
                    {
                        LOG.Error("无法找到Index: " + MaxSlotTrianglelistIndex + " 的IB的txt文件，跳过此数据类型");
                        continue;
                    }
                    string IBTxtFilePath = FrameAnalysisLogUtils.Get_DedupedFilePath(IBTxtFileName);
                    string IBFileFormat = "DXGI_FORMAT_R16_UINT";
                    string ReadDXGIFormat = DBMTFileUtils.FindMigotoIniAttributeInFile(IBTxtFilePath, "format");
                    if (ReadDXGIFormat == "DXGI_FORMAT_R32_UINT")
                    {
                        IBFileFormat = "DXGI_FORMAT_R32_UINT";
                    }

                    string IBBufFileName = Path.GetFileNameWithoutExtension(IBTxtFileName) + ".buf";
                    string IBBufFilePath = FrameAnalysisLogUtils.Get_DedupedFilePath(IBBufFileName);

                    IndexBufferBufFile IBBufFile = new IndexBufferBufFile(IBBufFilePath, IBFileFormat);


                    //开始准备Metadata.json
                    MetaDataJson metaDataJson = new MetaDataJson();

                    Dictionary<string, int> BoneMatrixMap = new Dictionary<string, int>();

                    //获取vb0 hash
                    if (Total_CategorySlot_Hash_Dict.ContainsKey("vb0"))
                    {
                        metaDataJson.vb0_hash = Total_CategorySlot_Hash_Dict["vb0"];
                        LOG.Info("Metadata.json vb0_hash: " + metaDataJson.vb0_hash);
                    }
                    else
                    {
                        LOG.Error("Can't get vb0_hash for Metadata.json");
                    }

                    //获取cb4_hash
                    string cb4FileName = FrameAnalysisDataUtils.FilterFirstFile(GlobalConfig.WorkFolder, MaxSlotTrianglelistIndex + "-vs-cb4=", ".buf");
                    if (cb4FileName != "")
                    {
                        string vscb4_hash = cb4FileName.Substring(14, 8);
                        metaDataJson.cb4_hash = vscb4_hash;
                        LOG.Info("Metadata.json cb4_hash: " + metaDataJson.cb4_hash);
                    }
                    else
                    {
                        LOG.Error("Can't get cb4_hash for Metadata.json");
                    }

                    //设置vertex_count和index_count
                    metaDataJson.vertex_count = IBBufFile.UniqueVertexCount;
                    metaDataJson.index_count = IBBufFile.NumberCount;
                    LOG.Info("Metadata.json vertex_count: " + metaDataJson.vertex_count.ToString());
                    LOG.Info("Metadata.json index_count: " + metaDataJson.index_count.ToString());

                    //ShapeKey数据
                    ShapeKey shapekeydata = new ShapeKey();

                    string ShapeKeyExtractIndex = "";
                    if (Total_CategorySlot_Hash_Dict.ContainsKey("vb6"))
                    {
                        //收集脸部形态键数据,如果有的话
                        string ShapeKeyHash = Total_CategorySlot_Hash_Dict["vb6"];
                        //根据ShapeKeyHash获取对应Index列表，然后逐个判断是否含有cs-t0和cs-t1槽位
                        List<string> ShapeKeyIndexList = FrameAnalysisLogUtils.Get_DrawCallIndexList_ByHash(ShapeKeyHash, false);

                        LOG.Info("ShapeKeyIndexList:");
                        foreach (string ShapeKeyIndex in ShapeKeyIndexList)
                        {
                            LOG.Info(ShapeKeyIndex);
                        }
                        LOG.NewLine();


                        foreach (string Index in ShapeKeyIndexList)
                        {
                            LOG.Info("Index: " + Index);
                            Dictionary<string, string> ShapeKey_CategorySlot_Hash_Map = FrameAnalysisLogUtils.Get_ComputeShaderSlotHashMap_FromCSSetShaderResources_ByIndex(Index);
                            if (ShapeKey_CategorySlot_Hash_Map.ContainsKey("cs-t0") && ShapeKey_CategorySlot_Hash_Map.ContainsKey("cs-t1"))
                            {
                                LOG.Info("Slot: cs-t0 Hash: " + ShapeKey_CategorySlot_Hash_Map["cs-t0"]);
                                LOG.Info("Slot: cs-t1 Hash: " + ShapeKey_CategorySlot_Hash_Map["cs-t1"]);
                                ShapeKeyExtractIndex = Index;
                                LOG.Info("ShapeKeyExtractIndex:" + ShapeKeyExtractIndex);
                            }
                            LOG.NewLine();
                        }
                    }


                    if (ShapeKeyExtractIndex != "")
                    {
                        string CSCB0_FileName = FrameAnalysisDataUtils.FilterFirstFile(GlobalConfig.WorkFolder, ShapeKeyExtractIndex + "-cs-cb0", ".buf");
                        string CST0_FileName = FrameAnalysisDataUtils.FilterFirstFile(GlobalConfig.WorkFolder, ShapeKeyExtractIndex + "-cs-t0", ".buf");
                        string CST1_FileName = FrameAnalysisDataUtils.FilterFirstFile(GlobalConfig.WorkFolder, ShapeKeyExtractIndex + "-cs-t1", ".buf");

                        string CSCB0_FilePath = FrameAnalysisLogUtils.Get_DedupedFilePath(CSCB0_FileName);   //ShapeKeyOffset
                        string CST0_FilePath = FrameAnalysisLogUtils.Get_DedupedFilePath(CST0_FileName);     //ShapeKeyVertexId
                        string CST1_FilePath = FrameAnalysisLogUtils.Get_DedupedFilePath(CST1_FileName);     //ShapeKeyVertexOffset

                        shapekeydata = new ShapeKey(CSCB0_FilePath, CST0_FilePath, CST1_FilePath);

                        //设置Metadata.json shapekeys数据
                        //offsets_hash 就是vb6的hash
                        metaDataJson.shapekeys.offsets_hash = Total_CategorySlot_Hash_Dict["vb6"];
                        //scale_hash 就是ShapeKeyExtractIndex的cs-u1的Hash值
                        string CSU1_FileName = FrameAnalysisDataUtils.FilterFirstFile(GlobalConfig.WorkFolder, ShapeKeyExtractIndex + "-u1=", ".buf");
                        if (CSU1_FileName != "")
                        {
                            string csu1_hash = CSU1_FileName.Substring(10, 8);
                            metaDataJson.shapekeys.scale_hash = csu1_hash;
                            LOG.Info("Metadata.json shapekeys scale_hash: " + metaDataJson.shapekeys.scale_hash);
                        }
                        else
                        {
                            LOG.Error("Can't get shapekeys scale_hash for Metadata.json");
                        }

                        //vertex_count 就是有多少个VertexId 也就是有多少个VertexOffset因为它们是一对一关系，但是WWMI这里要-1
                        //TODO 虽然不知道为什么WWMI要 -1 但是我们照做就对了，后面知道了原因再补回来
                        metaDataJson.shapekeys.vertex_count = shapekeydata.ShapeKeyOffsetNumberLast - 1;
                        LOG.Info("Metadata.json shapekeys vertex_count: " + metaDataJson.shapekeys.vertex_count);
                        //dispatch_y
                        metaDataJson.shapekeys.dispatch_y = shapekeydata.ShapeKeyOffsetNumberLast / 32 + 1;
                        LOG.Info("Metadata.json shapekeys dispatch_y: " + metaDataJson.shapekeys.dispatch_y);
                        //checksum ShapeKeyOffsets的前四位的和
                        metaDataJson.shapekeys.checksum = shapekeydata.CheckSum;

                    }
                    else
                    {
                        LOG.Error("当前Draw类型存在VB6槽位，但是无法找到ShapeKey对应提取Index，可能是暂不支持的鸣潮NPC类型，暂时不提取形态键只提取模型。");
                    }


                    SortedDictionary<int, string> MatchFirstIndex_IBTxtFileName_Dict = FrameAnalysisDataUtils.Get_MatchFirstIndex_IBTxtFileName_Dict(GlobalConfig.WorkFolder,DrawIB);

                    int ComponentCount = 1;
                    int ComponentVertexOffset = 0;
                    int ComponentIndexOffset = 0;
                    int VGOffset = 0;


                    List<string> ImportModelList = [];
                    List<string> PartNameList = [];
                    List<string> MatchFirstIndexList = [];

                    foreach (var item in MatchFirstIndex_IBTxtFileName_Dict)
                    {
                        int MatchFirstIndex = item.Key;
                        string TmpIBTxtFileName = item.Value;
                        string TmpIndex = TmpIBTxtFileName.Substring(0, 6);
                        string TmpIBTxtFilePath = FrameAnalysisLogUtils.Get_DedupedFilePath(TmpIBTxtFileName);
                        string IndexCountStr = DBMTFileUtils.FindMigotoIniAttributeInFile(TmpIBTxtFilePath, "index count");
                        LOG.Info("Index: " + TmpIndex + " MatchFirstIndex: " + item.Key.ToString() + " IndexCount: " + IndexCountStr);

                        string OutputPrefix = "Component " + (ComponentCount - 1).ToString();

                        //这里 + 1的原因是，DBMT的PartName都是从1开始的，但是WWMI的是从0开始的，所以我们要+1来适配。
                        PartNameList.Add(ComponentCount.ToString());
                        ImportModelList.Add(OutputPrefix);

                        MatchFirstIndexList.Add(MatchFirstIndex.ToString());
                        int IndexCount = int.Parse(IndexCountStr);

                        //现有的IB需要拆分为独立的IB文件
                        //注意！！！因为C#中无法实现赋值拷贝，只能实现引用拷贝
                        //所以这里如果直接写IndexBufferBufFile DivideIBBufFile = IBBufFile会导致使用原本的引用
                        //导致后续出错
                        IndexBufferBufFile DivideIBBufFile = new IndexBufferBufFile(IBBufFilePath, IBFileFormat);
                        LOG.Info("IB MinNumber: " + DivideIBBufFile.MinNumber.ToString());
                        LOG.Info("IB MaxNumber: " + DivideIBBufFile.MaxNumber.ToString());

                        LOG.Info("IB SelfDivide.");

                        DivideIBBufFile.SelfDivide(MatchFirstIndex, IndexCount);
                        LOG.Info("IB MinNumber: " + DivideIBBufFile.MinNumber.ToString());
                        LOG.Info("IB MaxNumber: " + DivideIBBufFile.MaxNumber.ToString());

                        LOG.Info("IB Save To File.");
                        DivideIBBufFile.SaveToFile_UInt32(GameTypeOutputPath + OutputPrefix + ".ib", -1 * DivideIBBufFile.MinNumber);

                        int TotalStride = d3D11GameType.GetSelfStride();
                        LOG.NewLine("TotalStride: " + TotalStride.ToString());

                        VertexBufferBufFile VBBufFile = new VertexBufferBufFile(FinalVB0);

                        //3.VB0整体需要根据IB进行SelfDivide，现在我们得到了每个IB对应的VB文件
                        LOG.Info("IB MinNumber: " + DivideIBBufFile.MinNumber.ToString());
                        LOG.Info("IB MaxNumber: " + DivideIBBufFile.MaxNumber.ToString());
                        VBBufFile.SelfDivide(DivideIBBufFile.MinNumber, DivideIBBufFile.MaxNumber, TotalStride);

                        //获取其BLENDINDICES内容
                        byte[] BLENDINDICES_DATA = VBBufFile.Get_ElementName_VBData_Map(d3D11GameType)["BLENDINDICES"];
                        LOG.Info("VB VertexCount: " + (VBBufFile.FinalVB0Bytes.Length / TotalStride).ToString());
                        LOG.NewLine();

                        //3.根据每个IB，从整个ShapeKeyOffset里，查找出对应的形态键，以及每个形态键对应的起始和结束区域，也就是直接得到一个
                        //形态键(数字表示) ,VertexIDList的Map
                        LOG.Info("Find work ShapeKey for MatchFirstIndex: " + MatchFirstIndex.ToString());

                        List<int> ShapeKeyIdList = shapekeydata.GetShapeKeyIdList(DivideIBBufFile);
                        List<D3D11Element> CurrentD3D11Elements = shapekeydata.GetD3D11ElementListWithShapeKey(d3D11GameType, ShapeKeyIdList);
                        if (ShapeKeyIdList.Count != 0)
                        {
                            byte[] AppendFinalVB0Data = shapekeydata.AppendShapeKeyToFinalVB0Buf(VBBufFile.FinalVB0Bytes, ShapeKeyIdList, TotalStride, DivideIBBufFile.MinNumber);
                            VertexBufferBufFile AppendVBBufFile = new VertexBufferBufFile(AppendFinalVB0Data);
                            VBBufFile = AppendVBBufFile;
                        }
                        LOG.Info("输出文件");
                        VBBufFile.SaveToFile(GameTypeOutputPath + OutputPrefix + ".vb");

                        FmtFile fmtFile = new FmtFile(d3D11GameType);
                        fmtFile.d3D11ElementList = CurrentD3D11Elements;
                        fmtFile.Format = "DXGI_FORMAT_R32_UINT";
                        fmtFile.Prefix = OutputPrefix;
                        fmtFile.GameTypeName = d3D11GameType.GameTypeName;
                        fmtFile.OutputFmtFileByD3D11ElementList(GameTypeOutputPath + OutputPrefix + ".fmt");


                        //设置Metadata.json的Component内容
                        Component component = new Component();
                        //index_offset由绘制index数量累加而成
                        component.index_offset = ComponentIndexOffset;
                        component.index_count = DivideIBBufFile.NumberCount;

                        //计算出当前ElementList的步长
                        int totalstride = 0;
                        foreach (D3D11Element element in CurrentD3D11Elements)
                        {
                            totalstride = totalstride + element.ByteWidth;
                        }
                        //vertex_offset由顶点数累加而成
                        component.vertex_offset = ComponentVertexOffset;
                        component.vertex_count = VBBufFile.FinalVB0Bytes.Length / totalstride;

                        //通过直接把byte添加到int列表里来完成格式转换
                        //TODO 这里可能有问题，确定是转换的UINT类型吗
                        List<uint> BlendIndicesNumberList = [];
                        foreach (byte BLENDINDICES_NUM in BLENDINDICES_DATA)
                        {
                            BlendIndicesNumberList.Add((uint)BLENDINDICES_NUM);
                        }

                        //找出其中最大的值
                        uint MaxBlendindicesNumber = 0;
                        foreach (uint BLENDINDICES_NUM in BlendIndicesNumberList)
                        {
                            if (BLENDINDICES_NUM > MaxBlendindicesNumber)
                            {
                                MaxBlendindicesNumber = BLENDINDICES_NUM;
                            }
                        }

                        uint VGCount = MaxBlendindicesNumber + 1;

                        //vg_offset是顶点组数量偏移，代表在这之前有多少个顶点组
                        component.vg_offset = VGOffset;
                        //有多少个顶点组
                        component.vg_count = (int)VGCount;
                        LOG.Info("VertexGroupCount: " + VGCount.ToString());

                        // 获取vg_map的内容
                        // vg_map里key为自身的BLENDINDICES从0到最大值，value为全局对应骨骼姿态变换矩阵中的位置。
                        // 也就是说，我们需要找当前索引对应的骨骼姿态变换矩阵中的数值，然后加入到一个map里，
                        // 通过判断map里是否已经添加过这个值来进行获取对应值，如果没有就加入

                        //读取vs-cb4中的内容

                        LOG.Info("开始读取VSCB4内容:");
                        string VSCB4BufFileName = FrameAnalysisDataUtils.FilterFirstFile(GlobalConfig.WorkFolder, TmpIndex + "-vs-cb4=", ".buf");

                        LOG.Info("VSCB4BufFileName: " + VSCB4BufFileName);
                        string VSCB4BufFilePath = FrameAnalysisLogUtils.Get_DedupedFilePath(VSCB4BufFileName);

                        if (VSCB4BufFileName != "")
                        {
                            BoneMatrixBufFile BoneMatrixBuf = new BoneMatrixBufFile(VSCB4BufFilePath);

                            for (int BlendIndex = 0; BlendIndex < VGCount; BlendIndex++)
                            {
                                byte[] BoneMatrixValue = BoneMatrixBuf.GetBufDataByBlendIndex(BlendIndex);
                                //LOG.Info("当前BlendIndex: " + BlendIndex.ToString());

                                //他吗的，这里如果用byte[]作为Key，则永远无法获取重复的
                                string BoneMatrixValueString = BitConverter.ToString(BoneMatrixValue);

                                int GlobalBoneVGNumber = -1;

                                if (BoneMatrixMap.TryGetValue(BoneMatrixValueString, out GlobalBoneVGNumber))
                                {
                                    LOG.Info("BoneMatrixValue: " + BitConverter.ToString(BoneMatrixValue));
                                    LOG.Info("包含，直接使用旧的");
                                    GlobalBoneVGNumber = BoneMatrixMap[BoneMatrixValueString];
                                    component.vg_map[BlendIndex.ToString()] = GlobalBoneVGNumber;
                                    LOG.Info("BlendIndex: " + BlendIndex.ToString() + " GlobalBoneVGNumber: " + GlobalBoneVGNumber.ToString());
                                }
                                else
                                {
                                    //不包含时才会更新索引
                                    LOG.Info("BoneMatrixValue: " + BitConverter.ToString(BoneMatrixValue));
                                    LOG.Info("不包含这个，更新索引");
                                    GlobalBoneVGNumber = BlendIndex + VGOffset;
                                    BoneMatrixMap[BoneMatrixValueString] = GlobalBoneVGNumber;
                                    component.vg_map[BlendIndex.ToString()] = GlobalBoneVGNumber;
                                    LOG.Info("BlendIndex: " + BlendIndex.ToString() + " GlobalBoneAliaNumber: " + GlobalBoneVGNumber.ToString());
                                }

                            }
                        }

                        metaDataJson.ComponentList.Add(component);

                        ComponentIndexOffset = ComponentIndexOffset + component.index_count;
                        ComponentVertexOffset = ComponentVertexOffset + component.vertex_count;

                        //固定不变
                        VGOffset = VGOffset + (int)VGCount;

                        ComponentCount++;
                        LOG.NewLine();

                    }

                    //输出Json文件
                    metaDataJson.SaveToFile(GameTypeOutputPath);

                    //保存tmpjson
                    ImportJson importJson = new ImportJson();

                    importJson.DrawIB = DrawIB;
                    importJson.d3D11GameType = d3D11GameType;
                    importJson.MatchFirstIndex_IBTxtFileName_Dict = MatchFirstIndex_IBTxtFileName_Dict;
                    importJson.Category_BufFileName_Dict = Category_BufFileNameDict;

                    string ImportJsonSavePath = Path.Combine(GameTypeOutputPath, "tmp.json");
                    importJson.SaveToFileWWMI(ImportJsonSavePath);
                }
            }

            LOG.Info("提取正常执行完成");
            return true;
        }

    }
}
