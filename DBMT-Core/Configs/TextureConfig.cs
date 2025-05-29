using DBMT_Core.Utils;
using Microsoft.UI.Xaml.Media.Imaging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace DBMT_Core
{
    public class TextureDeduped
    {
        public string TrianglelistPsFileName { get; set; } = "";
        public string FALogDedupedFileName { get; set; } = "";
        public string FADataDedupedFileName { get; set; } = "";
    }

    public class ImageItem
    {
        /// <summary>
        /// 这里显示的是Trianglelist中的000001这种DrawCall开头的贴图名称
        /// </summary>
        public string FileName { get; set; } = "";

        /// <summary>
        /// ImageSource是以转换好格式的Deduped贴图路径创建的，便于正常显示在软件中
        /// </summary>
        public BitmapImage ImageSource { get; set; }

        /// <summary>
        /// 由槽位、大小、参加渲染的Deduped贴图 组成
        /// </summary>
        public string InfoBar { get; set; } = "";

        /// <summary>
        /// 当前贴图被标记的类型，被标记后将参与自动贴图过程
        /// </summary>
        public string MarkName { get; set; } = "";

        /// <summary>
        /// 当前贴图写到自动ini中后的类型，Hash风格或者Slot风格
        /// </summary>
        public string MarkStyle { get; set; } = "";

        public string PixelSlot { get; set; } = "";

        public bool Render { get; set; } = false;

        public string Suffix { get; set; } = "";
    }

    public class SlotObject
    {
        public string Slot { get; set; } = "";
        public string MarkName { get; set; } = "";
        public string MarkStyle { get; set; } = "";
        public bool Render { get; set; } = false;
        public string Suffix { get; set; } = "";
    }

    public class TextureConfig
    {
        /// <summary>
        /// 当前贴图配置保存到TextureConfigs里
        /// </summary>
        /// <param name="imageCollection"></param>
        /// <param name="TextureConfigSavePath"></param>
        public static void SaveTextureConfig(ObservableCollection<ImageItem> imageCollection, string TextureConfigSavePath)
        {
            JObject jObject = DBMTJsonUtils.CreateJObject();


            string PSHash = "";
            JArray SlotList = new JArray();
            foreach (ImageItem item in imageCollection)
            {
                if (PSHash == "")
                {
                    PSHash = DBMTStringUtils.GetPSHashFromFileName(item.FileName);
                }

                JObject slot_object = DBMTJsonUtils.CreateJObject();
                slot_object["Slot"] = item.PixelSlot;
                slot_object["MarkName"] = item.MarkName;
                slot_object["MarkStyle"] = item.MarkStyle;
                slot_object["Render"] = item.Render;
                slot_object["Suffix"] = Path.GetExtension(item.FileName);

                SlotList.Add(slot_object);
            }

            JArray PixelShaderHashArray = new JArray();

            if (File.Exists(TextureConfigSavePath))
            {
                //如果存在，则读取并修改，这样能够确保PSHashList正确得到累积
                jObject = DBMTJsonUtils.ReadJObjectFromFile(TextureConfigSavePath);

                JArray jarryy = (JArray)jObject["PixelShaderHashList"];
                List<string> PixelShaderHashList = jarryy.ToObject<List<string>>();
                
                //先把已有的加回去
                foreach (string pshash in PixelShaderHashList)
                {
                    PixelShaderHashArray.Add(pshash);
                }

                //如果没有就加进去
                if (!PixelShaderHashList.Contains(PSHash))
                {
                    PixelShaderHashArray.Add(PSHash);
                }
                jObject["PixelShaderHashList"] = PixelShaderHashArray;
            }
            else
            {
                //在当前TextureConfig文件夹中，创建一个Json文件，内容来源于imageCollection
                PixelShaderHashArray.Add(PSHash);
            }

            jObject["SlotList"] = SlotList;
            jObject["PixelShaderHashList"] = PixelShaderHashArray;

            DBMTJsonUtils.SaveJObjectToFile(jObject, TextureConfigSavePath);
        }


        public static Dictionary<string, string> Read_PixeSlot_MarkName_Dict(string TextureConfigSavePath)
        {
            Dictionary<string, string> PixeSlot_MarkName_Dict = new Dictionary<string, string>();

            if(File.Exists(TextureConfigSavePath))
            {
                JObject jObject = DBMTJsonUtils.ReadJObjectFromFile(TextureConfigSavePath);
                JArray jArray = (JArray)jObject["SlotList"];
                foreach (JObject jobj in jArray)
                {
                    PixeSlot_MarkName_Dict[jobj["Slot"].ToString()] = jobj["MarkName"].ToString();
                }
            }
            
            return PixeSlot_MarkName_Dict;
        }

        public static Dictionary<string, SlotObject> Read_PixelSlot_SlotObject_Dict(string TextureConfigSavePath)
        {
            Dictionary<string, SlotObject> PixeSlot_SlotObject_Dict = new Dictionary<string, SlotObject>();

            if (File.Exists(TextureConfigSavePath))
            {
                JObject jObject = DBMTJsonUtils.ReadJObjectFromFile(TextureConfigSavePath);
                JArray jArray = (JArray)jObject["SlotList"];
                foreach (JObject jobj in jArray)
                {
                    SlotObject slotobj = new SlotObject();
                    slotobj.Slot = jobj["Slot"].ToString();
                    slotobj.MarkName = jobj["MarkName"].ToString();
                    if (jobj.ContainsKey("MarkStyle"))
                    {
                        slotobj.MarkStyle = jobj["MarkStyle"].ToString();
                    }
                    else
                    {
                        slotobj.MarkStyle = "Hash";
                    }

                    slotobj.Render = (bool)jobj["Render"];
                    slotobj.Suffix = jobj["Suffix"].ToString();

                    PixeSlot_SlotObject_Dict[jobj["Slot"].ToString()] = slotobj;
                }
            }

            return PixeSlot_SlotObject_Dict;
        }


        public static Dictionary<string, JObject> Get_TextureConfigName_JObject_Dict()
        {
            LOG.Info("Get_TextureConfigName_JObject_Dict::Start");

            //读取所有的贴图配置
            Dictionary<string, JObject> TextureConfigName_JObject_Dict = new Dictionary<string, JObject>();
            LOG.Info("当前游戏的贴图配置文件夹路径: " + GlobalConfig.Path_GameTextureConfigFolder);

            if (Directory.Exists(GlobalConfig.Path_GameTextureConfigFolder))
            {
                string[] TextureConfigFilePathList = Directory.GetFiles(GlobalConfig.Path_GameTextureConfigFolder);
                foreach (string TextureConfigFilePath in TextureConfigFilePathList)
                {
                    string TextureConfigName = Path.GetFileNameWithoutExtension(TextureConfigFilePath);
                    JObject TextureConfigJObject = DBMTJsonUtils.ReadJObjectFromFile(TextureConfigFilePath);
                    TextureConfigName_JObject_Dict[TextureConfigName] = TextureConfigJObject;
                }
            }
            LOG.Info("Get_TextureConfigName_JObject_Dict::End");
            return TextureConfigName_JObject_Dict;
        }

        public static List<string> FindMatch_TextureConfigNameList(List<ImageItem> ImageList, Dictionary<string, JObject> TextureConfigName_JObject_Dict)
        {
            List<string> MatchedTextureConfigNameList = new List<string>();

            if (!Directory.Exists(GlobalConfig.Path_GameTextureConfigFolder))
            {
                return MatchedTextureConfigNameList;
            }

            foreach (var pair in TextureConfigName_JObject_Dict)
            {
                string TextureConfigName = pair.Key;
                JObject TextureConfigJObject = pair.Value;
                Dictionary<string, ImageItem> PixelSlot_ImageItem_Dict = new Dictionary<string, ImageItem>();

                JArray SlotList = (JArray)TextureConfigJObject["SlotList"];
                foreach (JObject SlotObject in SlotList)
                {
                    ImageItem imageItem = new ImageItem();
                    imageItem.PixelSlot = SlotObject["Slot"].ToString();
                    if (SlotObject.ContainsKey("Render"))
                    {
                        imageItem.Render = (bool)SlotObject["Render"];
                    }
                    if (SlotObject.ContainsKey("Suffix"))
                    {
                        imageItem.Suffix = SlotObject["Suffix"].ToString();
                    }
                    imageItem.MarkName = SlotObject["MarkName"].ToString();
                    PixelSlot_ImageItem_Dict[imageItem.PixelSlot] = imageItem;
                }

                //1.首先数量要对上
                if (ImageList.Count != SlotList.Count)
                {
                    continue;
                }

                //2.其次每个PixelSlot要对上，不然碰巧数量对上了就会显示错误的
                bool allMatch = true;
                foreach (ImageItem imageItem in ImageList)
                {
                    if (!PixelSlot_ImageItem_Dict.ContainsKey(imageItem.PixelSlot))
                    {
                        //任何一个无法匹配，都算作失败
                        allMatch = false;
                        break;
                    }

                    //判断Suffix必须相等
                    ImageItem configImageItem = PixelSlot_ImageItem_Dict[imageItem.PixelSlot];
                    if (imageItem.Suffix != configImageItem.Suffix)
                    {
                        allMatch = false;
                        break;
                    }

                    //判断Render必须相等
                    if (imageItem.Render != configImageItem.Render)
                    {
                        allMatch = false;
                        break;
                    }
                }

                if (!allMatch)
                {
                    continue;
                }

                //添加到符合条件的列表里
                MatchedTextureConfigNameList.Add(TextureConfigName);
            }

            return MatchedTextureConfigNameList;
        }

        //TODO 优化到GLobalConfig里
        public static string GetConvertedTexturesFolderPath(string DrawIB)
        {
            string TextureFormatString = GlobalConfig.AutoTextureFormat;
            string DedupedTexturesConvertFolderPath = Path.Combine(GlobalConfig.Path_CurrentWorkSpaceFolder, DrawIB + "\\DedupedTextures_" + TextureFormatString + "\\");
            return DedupedTexturesConvertFolderPath;
        }

        public static Dictionary<string,TextureDeduped> Read_TrianglelistDedupedFileNameDict_FromJson(string DrawIB)
        {
            Dictionary<string, TextureDeduped> keyValuePairs = new Dictionary<string, TextureDeduped>();

            string TrianglelistDedupedFileNameJsonPath = Path.Combine(GlobalConfig.Path_CurrentWorkSpaceFolder + DrawIB + "\\", "TrianglelistDedupedFileName.json");
            if (!File.Exists(TrianglelistDedupedFileNameJsonPath))
            {
                return keyValuePairs;
            }

            JObject TrianglelistDedupedFileNameJObject = DBMTJsonUtils.ReadJObjectFromFile(TrianglelistDedupedFileNameJsonPath);

            // 遍历JObject中的所有属性
            foreach (var property in TrianglelistDedupedFileNameJObject.Properties())
            {
                TextureDeduped textureDeduped = new TextureDeduped();
                string key = property.Name;
                textureDeduped.TrianglelistPsFileName = key;
                // 获取每个键对应的值（这也是一个JToken）
                var value = property.Value as JObject;

                if (value != null)
                {
                    textureDeduped.FALogDedupedFileName = value["FALogDedupedFileName"].ToString();
                    textureDeduped.FADataDedupedFileName = value["FADataDedupedFileName"].ToString();
                }
                keyValuePairs[key] = textureDeduped;
            }

            return keyValuePairs;
        }


        public static async Task<List<ImageItem>> Read_ImageItemList(string DrawIB,string DrawCallIndex)
        {
            Debug.WriteLine("Read_ImageItemList::");
            string ConvertedTextureFolderPath = GetConvertedTexturesFolderPath(DrawIB);

            Dictionary<string, TextureDeduped> TrianglelistFileName_TextureDeduped_Dict = Read_TrianglelistDedupedFileNameDict_FromJson(DrawIB);

            Dictionary<string, string> TextureFileName_TextureSourceFilePath_Dict = new Dictionary<string, string>();
            Dictionary<string, string> PixelSlot_TextureFileName_Dict = new Dictionary<string, string>();

            foreach (var item in TrianglelistFileName_TextureDeduped_Dict)
            {
                string TextureFileName = item.Key;

                if (!TextureFileName.StartsWith(DrawCallIndex))
                {
                    continue;
                }
                //string TrianglelistTextureHash = DBMTStringUtils.GetFileHashFromFileName(TextureFileName);
                string DedupedTextureFileName = item.Value.FALogDedupedFileName;

                string ConvertedTextureFileName = "";
                if (DedupedTextureFileName.EndsWith(".dds"))
                {
                    string AutoTextureFormat = GlobalConfig.AutoTextureFormat;
                    ConvertedTextureFileName = Path.GetFileNameWithoutExtension(DedupedTextureFileName) + "." + AutoTextureFormat;
                }
                else
                {
                    ConvertedTextureFileName = DedupedTextureFileName;
                }
                string ConvertexTextureFilePath = Path.Combine(ConvertedTextureFolderPath, ConvertedTextureFileName);
                Debug.WriteLine(TextureFileName);
                Debug.WriteLine(ConvertedTextureFileName);

                if (File.Exists(ConvertexTextureFilePath))
                {
                    string PixelSlot = DBMTStringUtils.GetPixelSlotFromTextureFileName(TextureFileName);

                    //如果已经出现了该槽位并且是dds，则不能使用jpg格式贴图进行替换
                    if (PixelSlot_TextureFileName_Dict.ContainsKey(PixelSlot))
                    {
                        string ExistsTextureFileName = PixelSlot_TextureFileName_Dict[PixelSlot];
                        if (!ExistsTextureFileName.EndsWith(".dds"))
                        {
                            TextureFileName_TextureSourceFilePath_Dict.Add(TextureFileName, ConvertexTextureFilePath);
                        }
                    }
                    else
                    {
                        PixelSlot_TextureFileName_Dict[PixelSlot] = TextureFileName;
                        TextureFileName_TextureSourceFilePath_Dict.Add(TextureFileName, ConvertexTextureFilePath);
                    }
                }
                else
                {
                    Debug.WriteLine($"ConvertexTextureFilePath: {ConvertexTextureFilePath}" + "不存在");
                }
            }


            Debug.WriteLine("贴图数量" + TextureFileName_TextureSourceFilePath_Dict.Count.ToString());

            //然后读取当前ImageCollection对应的贴图配置列表
            List<ImageItem> ImageList = [];
            foreach (var item in TextureFileName_TextureSourceFilePath_Dict)
            {
                try
                {
                    string TextureFilePath = item.Value;

                    string PixelSlot = DBMTStringUtils.GetPixelSlotFromTextureFileName(item.Key);
                    string PixelSlotStr = "Slot: " + PixelSlot;
                    //Debug.WriteLine("PixelSlot: " + PixelSlot);
                    TextureDeduped textureDeduped = TrianglelistFileName_TextureDeduped_Dict[item.Key];
                    string DedupedFileName = textureDeduped.FADataDedupedFileName;

                    string DedupedInfo = "";
                    bool Render = false;
                    if (DedupedFileName == "")
                    {
                        DedupedInfo = "Render: False";
                        Render = false;
                    }
                    else
                    {
                        DedupedInfo = "Render: " + DedupedFileName;
                        Render = true;
                    }

                    StorageFile file = await StorageFile.GetFileFromPathAsync(TextureFilePath);
                    using (var stream = await file.OpenReadAsync())
                    {
                        BitmapImage bitmap = new BitmapImage();
                        await bitmap.SetSourceAsync(stream);

                        // 添加文件名和图片到集合
                        ImageList.Add(new ImageItem
                        {
                            FileName = item.Key,
                            ImageSource = bitmap,
                            InfoBar = PixelSlotStr + "    Size: " + bitmap.PixelWidth.ToString() + " * " + bitmap.PixelHeight.ToString() + "    " + DedupedInfo,
                            PixelSlot = PixelSlot,
                            MarkName = "",
                            MarkStyle = "Hash",
                            Render = Render,
                            Suffix = Path.GetExtension(item.Key)
                        }); ;
                    }
                }
                catch (FileNotFoundException fnfEx)
                {
                    Debug.WriteLine($"File not found: {fnfEx.Message}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error loading image from {item.Value}: {ex.Message}");
                }
            }


            return ImageList;
        }


        public static void ApplyTextureConfig(List<ImageItem> imageCollection,string DrawIB, string ComponentName)
        {
            Debug.WriteLine("应用自动贴图: ");
            string PartName = ComponentName.Substring("Component ".Length);
            
            //先获取所有的TYPE_开头的数据类型文件夹
            List<string> TypeFolderPathList = DrawIBConfig.GetDrawIBOutputGameTypeFolderPathList(DrawIB);

            //随后对于每个贴图，都复制贴图过去，顺便拼装一个PartName_PixelSlot = 目标文件名列表 的Dict
            //因为我们当前只操作当前PartName，所以直接来个列表就搞定了

            Dictionary<string, TextureDeduped> dictionary = TextureConfig.Read_TrianglelistDedupedFileNameDict_FromJson(DrawIB);

            List<string> TextureResourceReplaceList = [];
            foreach (ImageItem imageItem in imageCollection)
            {

                //有标记的才能参与自动贴图
                if (imageItem.MarkName.Trim() == "")
                {
                    continue;
                }

                Debug.WriteLine("应用: " + imageItem.MarkName);

                string suffix = Path.GetExtension(imageItem.FileName);
                string CurrentDrawIBOutputFolder = Path.Combine(GlobalConfig.Path_CurrentWorkSpaceFolder, DrawIB + "\\");

                string FALogDedupedFileName = dictionary[imageItem.FileName].FALogDedupedFileName;
                string ImageSourcePath = CurrentDrawIBOutputFolder + "DedupedTextures\\" + FALogDedupedFileName;
                string TextureHash = DBMTStringUtils.GetFileHashFromFileName(imageItem.FileName);

                string TargetImageFileName = DrawIB + "_" +  PartName  + "_" + TextureHash + "_" + imageItem.MarkStyle + "_" + imageItem.MarkName  + suffix;

                //拼接ResourceReplace
                TextureResourceReplaceList.Add(imageItem.PixelSlot + " = " + TargetImageFileName);

                //复制贴图过去
                foreach (string TargetFolderPath in TypeFolderPathList)
                {
                    string TargetImageFilePath = Path.Combine(TargetFolderPath, TargetImageFileName);
                    File.Copy(ImageSourcePath, TargetImageFilePath, true);
                }
            }

            foreach (string TargetFolderPath in TypeFolderPathList)
            {
                //修改tmp.json
                string TmpJsonPath = Path.Combine(TargetFolderPath, "tmp.json");
                JObject jObject = DBMTJsonUtils.ReadJObjectFromFile(TmpJsonPath);
                JObject PartNameTextureResourceReplaceListObj = (JObject)jObject["PartNameTextureResourceReplaceList"];
                PartNameTextureResourceReplaceListObj[PartName] = new JArray(TextureResourceReplaceList);
                jObject["PartNameTextureResourceReplaceList"] = PartNameTextureResourceReplaceListObj;
                DBMTJsonUtils.SaveJObjectToFile(jObject, TmpJsonPath);
            }


        }


        public static List<string> Get_TrianglelistTexturesFileNameList(string FrameAnalysisFolderPath,string DrawIB,bool ReverseExtract = false)
        {
            LOG.Info("Get_TrianglelistTexturesFileNameList::Start");
            List<string> TrianglelistTexturesFileNameList = [];

            List<string> TrianglelistIndexList = [];
            if (!ReverseExtract)
            {
                TrianglelistIndexList = FrameAnalysisDataUtils.Get_TrianglelistIndexListByDrawIB(FrameAnalysisFolderPath, DrawIB);
            }
            else
            {
                TrianglelistIndexList = FrameAnalysisLogUtils.Get_DrawCallIndexList_ByHash(DrawIB, false);
            }

            LOG.Info("TrianglelistIndexList Size: " + TrianglelistIndexList.Count.ToString());

            foreach (string Index in TrianglelistIndexList)
            {
                List<string> PsTextureAllFileNameList = FrameAnalysisDataUtils.FilterTextureFileNameList(FrameAnalysisFolderPath, Index + "-ps-t");
                foreach (string PsTextureFileName in PsTextureAllFileNameList)
                {
                    TrianglelistTexturesFileNameList.Add(PsTextureFileName);
                }
            }
            LOG.Info("Get_TrianglelistTexturesFileNameList::End");

            return TrianglelistTexturesFileNameList;
        }


    }
}
