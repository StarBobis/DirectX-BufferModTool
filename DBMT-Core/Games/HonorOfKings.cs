using DBMT_Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBMT_Core.Common;
using System.IO;
using System.Collections;


namespace DBMT_Core.Games
{
    //这次用全新的架构设计，每个游戏都整一个提取逻辑做成工具类，当然尽可能把通用的地方做成方法
    public static class HonorOfKings
    {

        /// <summary>
        /// 普通的数据类型识别，在多个DrawIndex中的布局是相同的
        /// </summary>
        /// <param name="d3D11GameTypeLv2"></param>
        /// <param name="TrianglelistIndexList"></param>
        /// <returns></returns>
        public static List<D3D11GameType> GameTypeDetectNormal(D3D11GameTypeLv2 d3D11GameTypeLv2, List<string> TrianglelistIndexList) {
            List<D3D11GameType> PossibleD3d11GameTypeList = [];

            foreach (D3D11GameType d3d11GameType in d3D11GameTypeLv2.Ordered_GPU_CPU_D3D11GameTypeList)
            {
                LOG.Info("当前正在识别数据类型: " + d3d11GameType.GameTypeName);

                bool AllCateogryVertexCountMatch = true;
                int TmpVertexCount = 0;
                foreach (var item in d3d11GameType.CategorySlotDict)
                {
                    string CategoryName = item.Key;
                    string CategorySlot = item.Value;
                    int CategoryStride = d3d11GameType.CategoryStrideDict[CategoryName];

                    //因为王者这种类型，所有的vb0都相同的，一次提交多次使用，所以直接使用第一个TrianglelistIndex即可。
                    VertexBufferCombFile VBCombFile = new VertexBufferCombFile(GlobalConfig.WorkFolder, TrianglelistIndexList[0], CategorySlot);

                    int AssumeVertexCount = VBCombFile.ByteLength / CategoryStride;

                    LOG.Info("当前数据类型推测顶点数: " + AssumeVertexCount.ToString());
                    LOG.Info("文件标明的顶点数: " + VBCombFile.VertexCount.ToString());

                    if (AssumeVertexCount != VBCombFile.VertexCount)
                    {
                        LOG.NewLine("当前数据类型推测出的顶点数和实际提取顶点数不符，跳过此数据类型");
                        AllCateogryVertexCountMatch = false;
                        break;
                    }

                    if (TmpVertexCount == 0)
                    {
                        TmpVertexCount = AssumeVertexCount;
                    }
                    else if (TmpVertexCount != AssumeVertexCount)
                    {
                        AllCateogryVertexCountMatch = false;
                        break;
                    }
                }

                if (AllCateogryVertexCountMatch)
                {
                    PossibleD3d11GameTypeList.Add(d3d11GameType);
                }
            }



            return PossibleD3d11GameTypeList;
        }
        
        /// <summary>
        /// 逐个IB提取
        /// HOK所有计算都在VertexShader里，并且都是trianglelist
        /// </summary>
        /// <param name="DrawIB"></param>
        /// <param name="d3D11GameTypeLv2"></param>
        /// <param name="TrianglelistIndexList"></param>
        /// <returns></returns>
        public static bool ExtractPerDrawIB(string DrawIB, D3D11GameTypeLv2 d3D11GameTypeLv2, List<string> TrianglelistIndexList)
        {
            string DrawIBFolder = Path.Combine(GlobalConfig.Path_CurrentWorkSpaceFolder, DrawIB + "\\");
            if (!Directory.Exists(DrawIBFolder))
            {
                Directory.CreateDirectory(DrawIBFolder);
            }

            //数据类型识别
            List<D3D11GameType> PossibleD3d11GameTypeList = GameTypeDetectNormal(d3D11GameTypeLv2,TrianglelistIndexList);
            if (PossibleD3d11GameTypeList.Count == 0)
            {
                LOG.Error("暂无数据类型，请去DBMT的数据类型页面添加对应数据类型来提取，或联系NicoMico获得技术支持。");
                return false;
            }
            else
            {
                LOG.Info("识别到的数据类型: ");
                foreach (D3D11GameType d3d11GameType in PossibleD3d11GameTypeList)
                {
                    LOG.Info(d3d11GameType.GameTypeName);
                }
                LOG.NewLine();
            }


            //对每一个数据类型，都进行模型提取。
            foreach (D3D11GameType d3d11GameType in PossibleD3d11GameTypeList)
            {
                LOG.Info(d3d11GameType.GameTypeName);
                //创建DrawIB下面的数据类型文件夹
                string GameTypeOutputFolder = Path.Combine(DrawIBFolder, "TYPE_" + d3d11GameType.GameTypeName);
                if (!Directory.Exists(GameTypeOutputFolder))
                {
                    Directory.CreateDirectory(GameTypeOutputFolder);
                }

                //因为不考虑Mod制作，只考虑模型提取，所以这里每一个TrianglelistIndex都当作一个Component即可
                int OutputCount = 1;

                List<string> ImportModelList = [];
                List<string> PartNameList = [];

                foreach (string TrianglelistIndex in TrianglelistIndexList)
                {
                    LOG.Info("TrianglelistIndex: " + TrianglelistIndex);

                    string NamePrefix = DrawIB + "-" + OutputCount.ToString();

                    string OutputIBBufFilePath = Path.Combine(GameTypeOutputFolder, NamePrefix + ".ib");
                    string OutputVBBufFilePath = Path.Combine(GameTypeOutputFolder, NamePrefix + ".vb");
                    string OutputFmtFilePath = Path.Combine(GameTypeOutputFolder, NamePrefix + ".fmt");

                    //构建BufDictList
                    List<Dictionary<int, byte[]>> BufDictList = new List<Dictionary<int, byte[]>>();

                    foreach (var item in d3d11GameType.CategorySlotDict)
                    {
                        string CategoryName = item.Key;
                        string CategorySlot = item.Value;
                        VertexBufferCombFile VBCombFile = new VertexBufferCombFile(GlobalConfig.WorkFolder, TrianglelistIndex, CategorySlot);
                        BufDictList.Add(VBCombFile.BufDict);
                    }


                    //读取IBBufFile
                    string IBBufFileName = FrameAnalysisDataUtils.FilterFirstFile(GlobalConfig.WorkFolder, TrianglelistIndex + "-ib", ".buf");
                    string IBBufFilePath = Path.Combine(GlobalConfig.WorkFolder, IBBufFileName);

                    IndexBufferBufFile IBBufFile = new IndexBufferBufFile(IBBufFilePath, "DXGI_FORMAT_R16_UINT");

                    //构建fmt文件
                    FmtFile fmtFile = new FmtFile(d3d11GameType);
                    fmtFile.Format = "DXGI_FORMAT_R16_UINT";

                    //输出VB文件
                    VertexBufferBufFile VBBufFile = new VertexBufferBufFile(BufDictList);
                    VBBufFile.SaveToFile(OutputVBBufFilePath);

                    //输出IB文件
                    IBBufFile.SaveToFile_UInt16(OutputIBBufFilePath, 0);

                    //输出fmt文件
                    fmtFile.OutputFmtFile(OutputFmtFilePath);

                    ImportModelList.Add(DrawIB + "-" + OutputCount.ToString());
                    PartNameList.Add(OutputCount.ToString());
                    OutputCount++;
                }

                //输出tmp.json
                ImportJson importJson = new ImportJson();
                importJson.ImportModelList = ImportModelList;
                importJson.PartNameList = PartNameList;
                importJson.d3D11GameType = d3d11GameType;
                string ImportJsonSavePath = Path.Combine(GameTypeOutputFolder, "tmp.json");
                importJson.SaveToFileMin(ImportJsonSavePath);


            }
            return true;
        }

        /// <summary>
        /// 总的入口
        /// </summary>
        /// <param name="DrawIBItemList"></param>
        /// <returns></returns>
        public static bool ExtractModel(List<DrawIBItem> DrawIBItemList)
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

                List<string> TrianglelistIndexList = FrameAnalysisLogUtilsV2.Get_DrawCallIndexList_ByHash(DrawIB, false,GlobalConfig.Path_LatestFrameAnalysisLogTxt);
                foreach (string TrianglelistIndex in TrianglelistIndexList)
                {
                    LOG.Info("TrianglelistIndex: " + TrianglelistIndex);
                }
                LOG.NewLine();

                bool result = ExtractPerDrawIB(DrawIB, d3D11GameTypeLv2, TrianglelistIndexList);
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
