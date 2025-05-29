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
    public partial class CoreFunctions
    {
        /// <summary>
        /// 读取每个TrianglelistTextures里的贴图文件对应Deduped文件保存到Json文件供后续贴图设置使用。
        /// </summary>
        /// <param name="ReverseExtract"></param>
        public static void Generate_TrianglelistDedupedFileName_Json(bool ReverseExtract = false)
        {
            LOG.Info("Generate_TrianglelistDedupedFileName_Json::Start");
            List<string> DrawIBList = DrawIBConfig.GetDrawIBListFromConfig();

            foreach (string DrawIB in DrawIBList)
            {
                //如果这个DrawIB的文件夹存在，说明提取成功了，否则直接跳过
                if (!Directory.Exists(Path.Combine(GlobalConfig.Path_CurrentWorkSpaceFolder, DrawIB + "\\")))
                {
                    continue;
                }
                FrameAnalysisInfo FAInfo = new FrameAnalysisInfo(DrawIB);

                LOG.Info("FAInfo.FolderPath: " + FAInfo.FolderPath);
                List<string> TrianglelistTextureFileNameList = TextureConfig.Get_TrianglelistTexturesFileNameList(FAInfo.FolderPath, DrawIB, ReverseExtract);
                LOG.Info("TrianglelistTextureFileNameList Size: " + TrianglelistTextureFileNameList.Count.ToString());

                JObject Trianglelist_DedupedFileName_JObject = DBMTJsonUtils.CreateJObject();
                foreach (string TrianglelistTextureFileName in TrianglelistTextureFileNameList)
                {
                    string Hash = DBMTStringUtils.GetFileHashFromFileName(TrianglelistTextureFileName);
                    string DedupedTextureFileName = Hash + "_" + FrameAnalysisLogUtilsV2.Get_DedupedFileName(TrianglelistTextureFileName,FAInfo.FolderPath,FAInfo.LogFilePath);
                    string FADedupedFileName = FrameAnalysisDataUtils.GetDedupedTextureFileName(FAInfo.FolderPath, TrianglelistTextureFileName);
                    LOG.Info("Hash: " + Hash);
                    LOG.Info("DedupedTextureFileName: " + DedupedTextureFileName);

                    if (FADedupedFileName.Trim() != "")
                    {
                        FADedupedFileName = Hash + "_" + FADedupedFileName;
                    }

                    JObject TextureProperty = DBMTJsonUtils.CreateJObject();
                    TextureProperty["FALogDedupedFileName"] = DedupedTextureFileName;
                    TextureProperty["FADataDedupedFileName"] = FADedupedFileName;

                    Trianglelist_DedupedFileName_JObject[TrianglelistTextureFileName] = TextureProperty;
                }

                string TrianglelistDedupedFileNameJsonName = "TrianglelistDedupedFileName.json";
                string TrianglelistDedupedFileNameJsonPath = Path.Combine(GlobalConfig.Path_CurrentWorkSpaceFolder + DrawIB + "\\", TrianglelistDedupedFileNameJsonName);
                DBMTJsonUtils.SaveJObjectToFile(Trianglelist_DedupedFileName_JObject, TrianglelistDedupedFileNameJsonPath);
            }

            LOG.Info("Generate_TrianglelistDedupedFileName_Json::End");

        }

        /// <summary>
        /// 读取每个Component的DrawIndexList并保存到Json文件供贴图设置页面使用。
        /// </summary>
        /// <param name="ReverseExtract"></param>
        /// <returns></returns>
        public static Dictionary<string, Dictionary<string, List<string>>> Generate_ComponentName_DrawCallIndexList_Json(bool ReverseExtract = false)
        {
            LOG.Info("Get_ComponentName_DrawCallIndexList_Dict_FromJson::Start");
            Dictionary<string, Dictionary<string, List<string>>> DrawIB_ComponentName_DrawCallIndexList_Dict_Dict = new Dictionary<string, Dictionary<string, List<string>>>();

            List<string> DrawIBList = DrawIBConfig.GetDrawIBListFromConfig();

            foreach (string DrawIB in DrawIBList) {
                //如果这个DrawIB的文件夹存在，说明提取成功了，否则直接跳过
                if (!Directory.Exists(Path.Combine(GlobalConfig.Path_CurrentWorkSpaceFolder, DrawIB + "\\")))
                {
                    continue;
                }

                FrameAnalysisInfo FAInfo = new FrameAnalysisInfo(DrawIB);

                Dictionary<string, UInt64> ComponentName_MatchFirstIndex_Dict = FrameAnalysisDataUtils.Read_ComponentName_MatchFirstIndex_Dict(FAInfo.FolderPath,DrawIB);

                JObject ComponentName_DrawIndexList_JObject = DBMTJsonUtils.CreateJObject();

                Dictionary<string, List<string>> ComponentName_DrawCallIndexList = new Dictionary<string, List<string>>();

                foreach (var item in ComponentName_MatchFirstIndex_Dict)
                {
                    List<string> DrawCallIndexList = new List<string>();

                    if (ReverseExtract)
                    {
                        DrawCallIndexList = FrameAnalysisLogUtils.Get_DrawCallIndexList_ByMatchFirstIndex(DrawIB, item.Value);
                    }
                    else
                    {
                        DrawCallIndexList = FrameAnalysisDataUtils.Read_DrawCallIndexList(FAInfo.FolderPath, DrawIB, item.Key);
                    }

                    ComponentName_DrawIndexList_JObject[item.Key] = new JArray(DrawCallIndexList);
                    ComponentName_DrawCallIndexList[item.Key] = DrawCallIndexList;
                }

                string SaveFileName = "ComponentName_DrawCallIndexList.json";
                string SaveJsonFilePath = Path.Combine(Path.Combine(GlobalConfig.Path_CurrentWorkSpaceFolder, DrawIB + "\\"), SaveFileName);
                DBMTJsonUtils.SaveJObjectToFile(ComponentName_DrawIndexList_JObject, SaveJsonFilePath);

                DrawIB_ComponentName_DrawCallIndexList_Dict_Dict[DrawIB] = ComponentName_DrawCallIndexList;
            }
            LOG.Info("Get_ComponentName_DrawCallIndexList_Dict_FromJson::End");
            return DrawIB_ComponentName_DrawCallIndexList_Dict_Dict;
        }

        /// <summary>
        /// 在正向提取和逆向提取后都需要做的事情
        /// </summary>
        public static async Task PostDoAfterExtract(bool ReverseExtract=false)
        {
            List<string> DrawIBList = DrawIBConfig.GetDrawIBListFromConfig();

            //(1) 
            Dictionary<string, Dictionary<string, List<string>>> DrawIB_ComponentName_DrawCallIndexList_Dict_Dict = Generate_ComponentName_DrawCallIndexList_Json();
            
            //(2) 
            Generate_TrianglelistDedupedFileName_Json();

            //(3)
            foreach (string DrawIB in DrawIBList)
            {
                //如果这个DrawIB的文件夹存在，说明提取成功了，否则直接跳过
                if (!Directory.Exists(Path.Combine(GlobalConfig.Path_CurrentWorkSpaceFolder, DrawIB + "\\")))
                {
                    continue;
                }
                //(3) 开始自动贴图识别流程，自动识别满足条件的第一个贴图配置，并将其应用到自动贴图。
                //如果设置中没勾选，就直接continue
                if (!GlobalConfig.AutoDetectAndMarkTexture)
                {
                    continue;
                }

                //检测贴图配置数量，如果一个都没有那就直接跳过这个DrawIB
                Dictionary<string, JObject> TextureConfigName_JObject_Dict = TextureConfig.Get_TextureConfigName_JObject_Dict();
                if (TextureConfigName_JObject_Dict.Count == 0)
                {
                    continue;
                }

                Dictionary<string, List<string>> ComponentName_DrawCallIndexList = DrawIB_ComponentName_DrawCallIndexList_Dict_Dict[DrawIB];
                foreach (var pair in ComponentName_DrawCallIndexList)
                {
                    //对每个ComponentName都进行处理:
                    string ComponentName = pair.Key;
                    List<string> DrawCallIndexList = pair.Value;

                    bool findMatchTextureConfig = false;
                    string MatchTextureConfigName = "";
                    List<ImageItem> MatchImageList = [];
                    foreach (string DrawCallIndex in DrawCallIndexList)
                    {
                        //TODO 这里获取到的ImageList是空的
                        List<ImageItem> ImageList = await TextureConfig.Read_ImageItemList(DrawIB, DrawCallIndex);
                        Debug.WriteLine("DrawCall: " + DrawCallIndex + " ImageListSize: " + ImageList.Count.ToString());

                        List<string> MatchedTextureConfigNameList = TextureConfig.FindMatch_TextureConfigNameList(ImageList, TextureConfigName_JObject_Dict);
                        if (MatchedTextureConfigNameList.Count != 0)
                        {
                            //即使找到了多个，默认也只使用第一个
                            MatchTextureConfigName = MatchedTextureConfigNameList[0];
                            MatchImageList = ImageList;
                            findMatchTextureConfig = true;
                            break;
                        }
                    }

                    if (!findMatchTextureConfig)
                    {
                        Debug.WriteLine("未找到任何匹配的贴图");
                        continue;
                    }

                    Debug.WriteLine("找到了匹配的贴图配置: " + MatchTextureConfigName);
                    //根据MatchTextureConfigName读取MarkName

                    string TextureConfigSavePath = GlobalConfig.Path_GameTextureConfigFolder + MatchTextureConfigName + ".json";
                    Debug.WriteLine("TextureConfigSavePath: " + TextureConfigSavePath);
                    if (File.Exists(TextureConfigSavePath))
                    {
                        Dictionary<string, SlotObject> PixeSlot_SlotObject_Dict = TextureConfig.Read_PixelSlot_SlotObject_Dict(TextureConfigSavePath);

                        Debug.WriteLine("Count: " + MatchImageList.Count.ToString());
                        for (int i = 0; i < MatchImageList.Count; i++)
                        {
                            ImageItem imageItem = MatchImageList[i];
                            SlotObject sobj = PixeSlot_SlotObject_Dict[imageItem.PixelSlot];
                            string MarkName = sobj.MarkName;
                            
                            imageItem.MarkName = MarkName;
                            imageItem.MarkStyle = sobj.MarkStyle;

                            MatchImageList[i] = imageItem;

                            Debug.WriteLine(MatchImageList[i].MarkName);
                        }
                    }

                    //执行到这里说明此ComponentName已匹配到对应的贴图，那么直接应用。
                    TextureConfig.ApplyTextureConfig(MatchImageList, DrawIB, ComponentName);

                }

            }
            

        }

    }
}
