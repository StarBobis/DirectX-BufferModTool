using DBMT_Core.Utils;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBMT_Core
{
    /// <summary>
    /// 这个是写到数据类型里的tmp.json，用来导入Blender时识别用的
    /// </summary>
    public class ImportJson
    {
        public string DrawIB { get; set; } = "";
        public D3D11GameType d3D11GameType { get; set; }

        public string VertexLimitVB { get; set; } = "";

        public List<string> ImportModelList { get; set; } = [];
        public List<string> PartNameList { get; set; } = [];
        public Dictionary<string, string> CategoryHashDict { get; set; }


        public Dictionary<string,string> Category_BufFileName_Dict { get; set; }

        public SortedDictionary<int,string> MatchFirstIndex_IBTxtFileName_Dict { get; set; }
        
        public ImportJson()
        {

        }

        /// <summary>
        /// 调用此方法前需要设置CategoryHashDict以及其它属性
        /// 但是不用设置Category_BufFileName_Dict
        /// </summary>
        /// <param name="ImportJsonFilePath"></param>
        public void SaveFile(string ImportJsonFilePath)
        {
            JObject jObject = DBMTJsonUtils.CreateJObject();
            jObject["VertexLimitVB"] = this.VertexLimitVB;

            jObject["CategoryHash"] = JToken.FromObject(CategoryHashDict);

            //三个列表，MatchFirstIndexList，PartNameList,ImportModelList
            List<string> MatchFirstIndexList = [];
            List<string> PartNameList = [];
            List<string> ImportModelList = [];
            int OutputCount = 1;
            foreach (var item in this.MatchFirstIndex_IBTxtFileName_Dict)
            {
                int MatchFirstIndx = item.Key;
                //string IBTxtFileName = item.Value;

                MatchFirstIndexList.Add(MatchFirstIndx.ToString());
                PartNameList.Add(OutputCount.ToString());
                ImportModelList.Add(DrawIB + "-" + OutputCount.ToString());
                OutputCount++;
            }
            jObject["MatchFirstIndex"] = JToken.FromObject(MatchFirstIndexList);
            jObject["PartNameList"] = JToken.FromObject(PartNameList);
            jObject["ImportModelList"] = JToken.FromObject(ImportModelList);


            //这里要写出所有用到的VS，写出一个VertexShaderHashList



            //WorkGameType
            jObject["WorkGameType"] = this.d3D11GameType.GameTypeName;

            //CategoryDrawCategoryMap
            jObject["CategoryDrawCategoryMap"] = JToken.FromObject(d3D11GameType.CategoryDrawCategoryDict);
            //GPU-PreSkinning
            jObject["GPU-PreSkinning"] = d3D11GameType.GPUPreSkinning;

            //空的，要配合自动贴图识别算法来填充，所以暂时不用管
            jObject["PartNameTextureResourceReplaceList"] = JToken.FromObject(new Dictionary<string, string>());

            //
            List<JObject> elementObjectList = [];
            foreach (string ElementName in d3D11GameType.OrderedFullElementList)
            {
                D3D11Element d3D11Element = d3D11GameType.ElementNameD3D11ElementDict[ElementName];

                JObject elementObject = DBMTJsonUtils.CreateJObject();
                elementObject["SemanticName"] = d3D11Element.SemanticName;
                elementObject["SemanticIndex"] = d3D11Element.SemanticIndex.ToString();
                elementObject["Format"] = d3D11Element.Format;
                elementObject["ByteWidth"] = d3D11Element.ByteWidth;
                elementObject["ExtractSlot"] = d3D11Element.ExtractSlot;
                elementObject["Category"] = d3D11Element.Category;

                elementObjectList.Add(elementObject);
            }
            jObject["D3D11ElementList"] = JToken.FromObject(elementObjectList);

            DBMTJsonUtils.SaveJObjectToFile(jObject, ImportJsonFilePath);
        }

        public void SaveToFileWWMI(string ImportJsonFilePath)
        {

            JObject jObject = DBMTJsonUtils.CreateJObject();
            jObject["VertexLimitVB"] = this.VertexLimitVB;

            //拼接CategoryHashDict
            Dictionary<string, string> CategoryHashDict = new Dictionary<string, string>();
            foreach (var item in this.Category_BufFileName_Dict)
            {
                string Category = item.Key;
                string BufFileName = item.Value;
                string CategorySlot = d3D11GameType.CategorySlotDict[Category];

                //这里的8是由000001-vb0= 这里的00001为Index固定为6个，后面-固定一个，=固定一个，所以一共是8个长度
                //然后再加上CategorySlot的长度，正好
                int StartIndex = 8 + CategorySlot.Length;

                string Hash = BufFileName.Substring(StartIndex, 8);
                CategoryHashDict[Category] = Hash;
            }
            jObject["CategoryHash"] = JToken.FromObject(CategoryHashDict);

            //三个列表，MatchFirstIndexList，PartNameList,ImportModelList
            List<string> MatchFirstIndexList = [];
            List<string> PartNameList = [];
            List<string> ImportModelList = [];
            int OutputCount = 1;
            foreach (var item in this.MatchFirstIndex_IBTxtFileName_Dict)
            {
                int MatchFirstIndx = item.Key;
                //string IBTxtFileName = item.Value;

                MatchFirstIndexList.Add(MatchFirstIndx.ToString());
                PartNameList.Add(OutputCount.ToString());
                ImportModelList.Add("Component " + (OutputCount - 1).ToString());

                OutputCount++;
            }
            jObject["MatchFirstIndex"] = JToken.FromObject(MatchFirstIndexList);
            jObject["PartNameList"] = JToken.FromObject(PartNameList);
            jObject["ImportModelList"] = JToken.FromObject(ImportModelList);


            //WorkGameType
            jObject["WorkGameType"] = this.d3D11GameType.GameTypeName;

            //CategoryDrawCategoryMap
            jObject["CategoryDrawCategoryMap"] = JToken.FromObject(d3D11GameType.CategoryDrawCategoryDict);
            //GPU-PreSkinning
            jObject["GPU-PreSkinning"] = d3D11GameType.GPUPreSkinning;

            //空的，要配合自动贴图识别算法来填充，所以暂时不用管
            jObject["PartNameTextureResourceReplaceList"] = JToken.FromObject(new Dictionary<string, string>());

            //
            List<JObject> elementObjectList = [];
            foreach (string ElementName in d3D11GameType.OrderedFullElementList)
            {
                D3D11Element d3D11Element = d3D11GameType.ElementNameD3D11ElementDict[ElementName];

                JObject elementObject = DBMTJsonUtils.CreateJObject();
                elementObject["SemanticName"] = d3D11Element.SemanticName;
                elementObject["SemanticIndex"] = d3D11Element.SemanticIndex.ToString();
                elementObject["Format"] = d3D11Element.Format;
                elementObject["ByteWidth"] = d3D11Element.ByteWidth;
                elementObject["ExtractSlot"] = d3D11Element.ExtractSlot;
                elementObject["Category"] = d3D11Element.Category;

                elementObjectList.Add(elementObject);
            }
            jObject["D3D11ElementList"] = JToken.FromObject(elementObjectList);

            DBMTJsonUtils.SaveJObjectToFile(jObject, ImportJsonFilePath);
        }

        public void SaveToFileGF2(string ImportJsonFilePath)
        {

            JObject jObject = DBMTJsonUtils.CreateJObject();
            jObject["VertexLimitVB"] = this.VertexLimitVB;

            //拼接CategoryHashDict
            Dictionary<string, string> CategoryHashDict = new Dictionary<string, string>();
            foreach (var item in this.Category_BufFileName_Dict)
            {
                string Category = item.Key;
                string BufFileName = item.Value;
                string CategorySlot = d3D11GameType.CategorySlotDict[Category];

                //这里的8是由000001-vb0= 这里的00001为Index固定为6个，后面-固定一个，=固定一个，所以一共是8个长度
                //然后再加上CategorySlot的长度，正好
                int StartIndex = 8 + CategorySlot.Length;
                string Hash = BufFileName.Substring(StartIndex, 8);
                CategoryHashDict[Category] = Hash;

                if (Hash.Contains("VS") || Hash.Contains("=") || Hash.Contains("-"))
                {
                    LOG.Info("看起来是在提取Mod模型，尝试从log.txt中通过IASetVertexBuffer来获取原始的Hash值。");
                    //但是如果是逆向出来的Mod就没办法这样了，必须得想办法得到原始的值。
                    string Index = BufFileName.Substring(0, 6);
                    Dictionary<string, string> VBCategoryHashMap = FrameAnalysisLogUtils.Get_VBCategoryHashMap_FromIASetVertexBuffer_ByIndex(Index);
                    if (VBCategoryHashMap.ContainsKey(CategorySlot))
                    {
                        string CategoryHash = VBCategoryHashMap[CategorySlot];
                        CategoryHashDict[Category] = CategoryHash;
                    }
                }



            }
            jObject["CategoryHash"] = JToken.FromObject(CategoryHashDict);

            //三个列表，MatchFirstIndexList，PartNameList,ImportModelList
            List<string> MatchFirstIndexList = [];
            List<string> PartNameList = [];
            int OutputCount = 1;
            foreach (var item in this.MatchFirstIndex_IBTxtFileName_Dict)
            {
                int MatchFirstIndx = item.Key;
                //string IBTxtFileName = item.Value;

                MatchFirstIndexList.Add(MatchFirstIndx.ToString());
                PartNameList.Add(OutputCount.ToString());
                this.ImportModelList.Add(DrawIB + "-" + OutputCount.ToString());

                //GF2直接Break，有一个部件就够了
                break;
                
            }
            jObject["MatchFirstIndex"] = JToken.FromObject(MatchFirstIndexList);
            jObject["PartNameList"] = JToken.FromObject(PartNameList);
            jObject["ImportModelList"] = JToken.FromObject(this.ImportModelList);


            //VSList
            // 获取当前目录下的所有文件
            List<string> IBTxtFileList = FrameAnalysisDataUtils.FilterFile(GlobalConfig.WorkFolder, "-ib=" + DrawIB, ".txt");
            HashSet<string> VSHashSet = new HashSet<string>();
            foreach (string IBTxtFileName in IBTxtFileList)
            {
                int vsIndex = IBTxtFileName.IndexOf("-vs=");
                if (vsIndex != -1)
                {
                    string vsShaderHash = IBTxtFileName.Substring(vsIndex + 4, 16);
                    VSHashSet.Add(vsShaderHash);
                }
            }
            List<string> VSHashList = [];
            foreach (string VSHash in VSHashSet)
            {
                LOG.Info("VSHash: " + VSHash);
                VSHashList.Add(VSHash);
            }

            jObject["VSHashList"] = JToken.FromObject(VSHashList);

            //WorkGameType
            jObject["WorkGameType"] = this.d3D11GameType.GameTypeName;

            //CategoryDrawCategoryMap
            jObject["CategoryDrawCategoryMap"] = JToken.FromObject(d3D11GameType.CategoryDrawCategoryDict);
            //GPU-PreSkinning
            jObject["GPU-PreSkinning"] = d3D11GameType.GPUPreSkinning;

            //空的，要配合自动贴图识别算法来填充，所以暂时不用管
            jObject["PartNameTextureResourceReplaceList"] = JToken.FromObject(new Dictionary<string, string>());

            //
            List<JObject> elementObjectList = [];
            foreach (string ElementName in d3D11GameType.OrderedFullElementList)
            {
                D3D11Element d3D11Element = d3D11GameType.ElementNameD3D11ElementDict[ElementName];

                JObject elementObject = DBMTJsonUtils.CreateJObject();
                elementObject["SemanticName"] = d3D11Element.SemanticName;
                elementObject["SemanticIndex"] = d3D11Element.SemanticIndex.ToString();
                elementObject["Format"] = d3D11Element.Format;
                elementObject["ByteWidth"] = d3D11Element.ByteWidth;
                elementObject["ExtractSlot"] = d3D11Element.ExtractSlot;
                elementObject["Category"] = d3D11Element.Category;

                elementObjectList.Add(elementObject);
            }
            jObject["D3D11ElementList"] = JToken.FromObject(elementObjectList);

            DBMTJsonUtils.SaveJObjectToFile(jObject, ImportJsonFilePath);
        }

        public void SaveToFile(string ImportJsonFilePath)
        {

            JObject jObject = DBMTJsonUtils.CreateJObject();
            jObject["VertexLimitVB"] = this.VertexLimitVB;

            //拼接CategoryHashDict
            Dictionary<string, string> CategoryHashDict = new Dictionary<string, string>();
            foreach (var item in this.Category_BufFileName_Dict)
            {
                string Category = item.Key;
                string BufFileName = item.Value;
                string CategorySlot = d3D11GameType.CategorySlotDict[Category];

                //这里的8是由000001-vb0= 这里的00001为Index固定为6个，后面-固定一个，=固定一个，所以一共是8个长度
                //然后再加上CategorySlot的长度，正好
                int StartIndex = 8 + CategorySlot.Length;
                string Hash = BufFileName.Substring(StartIndex, 8);
                CategoryHashDict[Category] = Hash;

                if (Hash.Contains("VS") || Hash.Contains("=") || Hash.Contains("-"))
                {
                    LOG.Info("看起来是在提取Mod模型，尝试从log.txt中通过IASetVertexBuffer来获取原始的Hash值。");
                    //但是如果是逆向出来的Mod就没办法这样了，必须得想办法得到原始的值。
                    string Index = BufFileName.Substring(0, 6);
                    Dictionary<string, string> VBCategoryHashMap = FrameAnalysisLogUtils.Get_VBCategoryHashMap_FromIASetVertexBuffer_ByIndex(Index);
                    if (VBCategoryHashMap.ContainsKey(CategorySlot))
                    {
                        string CategoryHash = VBCategoryHashMap[CategorySlot];
                        CategoryHashDict[Category] = CategoryHash;
                    }
                }

                

            }
            jObject["CategoryHash"] = JToken.FromObject(CategoryHashDict);

            //三个列表，MatchFirstIndexList，PartNameList,ImportModelList
            List<string> MatchFirstIndexList = [];
            List<string> PartNameList = [];
            int OutputCount = 1;
            foreach (var item in this.MatchFirstIndex_IBTxtFileName_Dict)
            {
                int MatchFirstIndx = item.Key;
                //string IBTxtFileName = item.Value;

                MatchFirstIndexList.Add(MatchFirstIndx.ToString());
                PartNameList.Add(OutputCount.ToString());
                this.ImportModelList.Add(DrawIB + "-" + OutputCount.ToString());
                OutputCount++;
            }
            jObject["MatchFirstIndex"] = JToken.FromObject(MatchFirstIndexList);
            jObject["PartNameList"] = JToken.FromObject(PartNameList);
            jObject["ImportModelList"] = JToken.FromObject(this.ImportModelList);


            //VSList
            // 获取当前目录下的所有文件
            List<string> IBTxtFileList = FrameAnalysisDataUtils.FilterFile(GlobalConfig.WorkFolder, "-ib=" + DrawIB, ".txt");
            HashSet<string> VSHashSet = new HashSet<string>();
            foreach (string IBTxtFileName in IBTxtFileList)
            {
                int vsIndex = IBTxtFileName.IndexOf("-vs=");
                if (vsIndex != -1)
                {
                    string vsShaderHash = IBTxtFileName.Substring(vsIndex + 4, 16);
                    VSHashSet.Add(vsShaderHash);
                }
            }
            List<string> VSHashList = [];
            foreach (string VSHash in VSHashSet)
            {
                LOG.Info("VSHash: " + VSHash);
                VSHashList.Add(VSHash);
            }

            jObject["VSHashList"] = JToken.FromObject(VSHashList);

            //WorkGameType
            jObject["WorkGameType"] = this.d3D11GameType.GameTypeName;

            //CategoryDrawCategoryMap
            jObject["CategoryDrawCategoryMap"] = JToken.FromObject(d3D11GameType.CategoryDrawCategoryDict);
            //GPU-PreSkinning
            jObject["GPU-PreSkinning"] = d3D11GameType.GPUPreSkinning;

            //空的，要配合自动贴图识别算法来填充，所以暂时不用管
            jObject["PartNameTextureResourceReplaceList"] = JToken.FromObject(new Dictionary<string,string>());

            //
            List<JObject> elementObjectList = [];
            foreach (string ElementName in d3D11GameType.OrderedFullElementList)
            {
                D3D11Element d3D11Element = d3D11GameType.ElementNameD3D11ElementDict[ElementName];

                JObject elementObject = DBMTJsonUtils.CreateJObject();
                elementObject["SemanticName"] = d3D11Element.SemanticName;
                elementObject["SemanticIndex"] = d3D11Element.SemanticIndex.ToString();
                elementObject["Format"] = d3D11Element.Format;
                elementObject["ByteWidth"] = d3D11Element.ByteWidth;
                elementObject["ExtractSlot"] = d3D11Element.ExtractSlot;
                elementObject["Category"] = d3D11Element.Category;

                elementObjectList.Add(elementObject);
            }
            jObject["D3D11ElementList"] = JToken.FromObject(elementObjectList);

            DBMTJsonUtils.SaveJObjectToFile(jObject, ImportJsonFilePath);
        }

        /// <summary>
        /// 用于仅模型提取时提供一键导入的最小化写出
        /// </summary>
        /// <param name="ImportJsonFilePath"></param>
        public void SaveToFileMin(string ImportJsonFilePath)
        {

            JObject jObject = DBMTJsonUtils.CreateJObject();

            jObject["ImportModelList"] = JToken.FromObject(this.ImportModelList);
            jObject["PartNameList"] = JToken.FromObject(this.PartNameList);

            //WorkGameType
            jObject["WorkGameType"] = this.d3D11GameType.GameTypeName;

            List<JObject> elementObjectList = [];
            foreach (string ElementName in d3D11GameType.OrderedFullElementList)
            {
                D3D11Element d3D11Element = d3D11GameType.ElementNameD3D11ElementDict[ElementName];

                JObject elementObject = DBMTJsonUtils.CreateJObject();
                elementObject["SemanticName"] = d3D11Element.SemanticName;
                elementObject["SemanticIndex"] = d3D11Element.SemanticIndex.ToString();
                elementObject["Format"] = d3D11Element.Format;
                elementObject["ByteWidth"] = d3D11Element.ByteWidth;
                elementObject["ExtractSlot"] = d3D11Element.ExtractSlot;
                elementObject["Category"] = d3D11Element.Category;

                elementObjectList.Add(elementObject);
            }
            jObject["D3D11ElementList"] = JToken.FromObject(elementObjectList);

            DBMTJsonUtils.SaveJObjectToFile(jObject, ImportJsonFilePath);
        }

    }
}
