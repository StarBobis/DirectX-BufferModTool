using DBMT_Core;
using DBMT_Core.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBMT_Core
{
    /// <summary>
    /// 包装起来更方便使用
    /// </summary>
    public class D3D11GameTypeLv2
    {

        public List<D3D11GameType> d3D11GameTypeList { get; set; } = [];

        public Dictionary<string, D3D11GameType> GameTypeName_D3D11GameType_Dict = new Dictionary<string, D3D11GameType>();

        public List<D3D11GameType> Ordered_GPU_CPU_D3D11GameTypeList = [];

        public D3D11GameTypeLv2(string GameName)
        {
            LOG.Info("初始化D3D11GameTypeLv2::Start");
            LOG.Info("读取GameType分类: " + GameName);
            string GameTypeCategory_Path = Path.Combine(GlobalConfig.Path_GameTypeConfigsFolder, GameName + "\\");

            string[] GameTypeCategory_Files = Directory.GetFiles(GameTypeCategory_Path);

            foreach (string file_path in GameTypeCategory_Files)
            {

                if (file_path.EndsWith(".json"))
                {
                    D3D11GameType d3D11GameType = new D3D11GameType(file_path);

                    this.d3D11GameTypeList.Add(d3D11GameType);
                    LOG.Info("读取数据类型:" + d3D11GameType.GameTypeName + " PreSkinning: " + d3D11GameType.GPUPreSkinning.ToString());
                    this.GameTypeName_D3D11GameType_Dict[d3D11GameType.GameTypeName] = d3D11GameType;
                }
            }

            //先加入GPU类型
            foreach (D3D11GameType d3D11GameType1 in this.d3D11GameTypeList)
            {
                if (d3D11GameType1.GPUPreSkinning)
                {
                    this.Ordered_GPU_CPU_D3D11GameTypeList.Add(d3D11GameType1);
                    LOG.Info("加入GPU类型:" + d3D11GameType1.GameTypeName);
                }
            }

            //再加入CPU类型，这样匹配时根据这个顺序自然就能先匹配到GPU再匹配到CPU。
            foreach (D3D11GameType d3D11GameType1 in this.d3D11GameTypeList)
            {
                if (!d3D11GameType1.GPUPreSkinning)
                {
                    this.Ordered_GPU_CPU_D3D11GameTypeList.Add(d3D11GameType1);
                    LOG.Info("加入CPU类型:" + d3D11GameType1.GameTypeName);
                }
            }
            LOG.Info("初始化D3D11GameTypeLv2::End");
            LOG.NewLine();

        }

        public string FilterTrianglelistIndex_UnityVS(List<string> TrianglelistIndexList, D3D11GameType d3D11GameType)
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

        public List<D3D11GameType> GetPossibleGameTypeList_UnityVS(string PointlistIndex, List<string> TrianglelistIndexList)
        {
            List<D3D11GameType> PossibleGameTypeList = [];

            bool findAtLeastOneGPUType = false;
            foreach (D3D11GameType d3D11GameType in this.Ordered_GPU_CPU_D3D11GameTypeList)
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
                    string CategoryBufFileName = FrameAnalysisDataUtils.FilterFirstFile(GlobalConfig.WorkFolder,ExtractIndex + "-" + CategorySlot, ".buf");
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

                    if (!d3D11GameType.GPUPreSkinning)
                    {
                        string CategorySlot = d3D11GameType.CategorySlotDict[CategoryName];
                        string CategoryTxtFileName = FrameAnalysisDataUtils.FilterFirstFile(GlobalConfig.WorkFolder, TrianglelistIndex + "-" + CategorySlot, ".txt");
                        if (CategoryTxtFileName == "")
                        {
                            LOG.Info("槽位的txt文件不存在，跳过此数据类型。");
                            AllMatch = false;
                            break;
                        }
                        else
                        {
                            string CategoryTxtFilePath = FrameAnalysisLogUtils.Get_DedupedFilePath(CategoryTxtFileName);
                            string VertexCountTxtShow = DBMTFileUtils.FindMigotoIniAttributeInFile(CategoryTxtFilePath, "vertex count");
                            int TxtShowVertexCount = int.Parse(VertexCountTxtShow);
                            if (TxtShowVertexCount != TmpNumber)
                            {
                                LOG.Info("槽位的txt文件顶点数与Buffer数据类型统计顶点数不符，跳过此数据类型。");
                                AllMatch = false;
                                break;
                            }
                        }
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


        public List<D3D11GameType> GetPossibleGameTypeList_UnrealVS(Dictionary<string, string> CategorySlot_FileName_Dict)
        {
            List<D3D11GameType> PossibleGameTypeList = [];


            bool findAtLeastOneGPUType = false;
            foreach (D3D11GameType d3D11GameType in this.Ordered_GPU_CPU_D3D11GameTypeList)
            {
                if (findAtLeastOneGPUType && !d3D11GameType.GPUPreSkinning)
                {
                    LOG.Info("自动优化:已经找到了满足条件的GPU类型，所以这个CPU类型就不用判断了");
                    continue;
                }

                LOG.Info("当前数据类型:" + d3D11GameType.GameTypeName);

                //开始尝试识别数据类型：

                bool AllSlotMatch = true;
                int VertexCount = 0;

                foreach (var item in d3D11GameType.CategorySlotDict)
                {
                    string CategoryName = item.Key;
                    string CategorySlot = item.Value;

                    int CategoryStride = d3D11GameType.CategoryStrideDict[CategoryName];
                    if (!CategorySlot_FileName_Dict.ContainsKey(CategorySlot))
                    {
                        LOG.Info("未检测到当前CategorySlot文件，匹配失败");
                        AllSlotMatch = false;
                        break;
                    }

                    string CategorySlotFileName = CategorySlot_FileName_Dict[CategorySlot];
                    string CategorySlotFilePath = FrameAnalysisLogUtils.Get_DedupedFilePath(CategorySlotFileName);
                    int SlotFileSize = (int)DBMTFileUtils.GetFileSize(CategorySlotFilePath);
                    int SlotVertexCount = SlotFileSize / CategoryStride;
                    if (VertexCount == 0)
                    {
                        VertexCount = SlotVertexCount;
                    }
                    else if (VertexCount != SlotVertexCount)
                    {
                        LOG.Info("VertexCount: " + VertexCount.ToString() + "  SlotVertexCount: " + SlotVertexCount.ToString());
                        LOG.Info("当前槽位: " + CategorySlot + " 文件数据不符合当前数据类型要求，跳过此数据类型");
                        AllSlotMatch = false;
                        break;
                    }
                    else
                    {
                        VertexCount = SlotVertexCount;
                    }
                }

                if (AllSlotMatch)
                {
                    LOG.Info("识别到数据类型: " + d3D11GameType.GameTypeName);
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
                return PossibleGameTypeList;
            }

            //优化:如果剩下的全是GPU数据类型，则其中的步长必须全部是最大的
            //因为部分Element较少的数据类型会和正确的Element更多的数据类型一起识别出来
            bool AllGPUType = true;
            foreach (D3D11GameType d3D11GameType in PossibleGameTypeList)
            {
                if (!d3D11GameType.GPUPreSkinning)
                {
                    AllGPUType = false;
                    break;
                }
            }

            if (AllGPUType)
            {
                int MaxStride = 0;
                foreach (D3D11GameType d3D11GameType in PossibleGameTypeList)
                {
                    int SelfStride = d3D11GameType.GetSelfStride();
                    if (SelfStride > MaxStride)
                    {
                        MaxStride = SelfStride;
                    }
                }

                List<D3D11GameType> PossibleGPUGameTypeList = [];
                foreach (D3D11GameType d3D11GameType in PossibleGameTypeList)
                {
                    int SelfStride = d3D11GameType.GetSelfStride();
                    if (SelfStride == MaxStride)
                    {
                        PossibleGPUGameTypeList.Add(d3D11GameType);
                    }
                }

                PossibleGameTypeList = PossibleGPUGameTypeList;
            }

            LOG.Info("All Matched GameType:");
            foreach (D3D11GameType d3d11GameType in PossibleGameTypeList)
            {
                LOG.Info(d3d11GameType.GameTypeName);
            }

            return PossibleGameTypeList;
        }

    }
}
