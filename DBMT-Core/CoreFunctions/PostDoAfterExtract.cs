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
    public partial class CoreFunctions
    {

        /// <summary>
        /// 在正向提取和逆向提取后都需要做的事情
        /// </summary>
        public static void PostDoAfterExtract()
        {
            List<string> DrawIBList = DrawIBConfig.GetDrawIBListFromConfig();

            foreach (string DrawIB in DrawIBList)
            {
                //如果这个DrawIB的文件夹存在，说明提取成功了，否则直接跳过
                string SavePathFolder = Path.Combine(GlobalConfig.Path_CurrentWorkSpaceFolder, DrawIB + "\\");
                if (!Directory.Exists(SavePathFolder))
                {
                    continue;
                }

                Dictionary<string, ulong> ComponentName_MatchFirstIndex_Dict = DrawIBConfig.Read_ComponentName_MatchFirstIndex_Dict(DrawIB);

                JObject jObject = DBMTJsonUtils.CreateJObject();

                foreach (var item in ComponentName_MatchFirstIndex_Dict)
                {
                    List<string> DrawCallIndexList = FrameAnalysisData.Read_DrawCallIndexList(DrawIB,item.Key);

                    jObject[item.Key] = new JArray(DrawCallIndexList);
                }

                string SaveFileName = "ComponentName_DrawCallIndexList.json";
                string SaveJsonFilePath = Path.Combine(SavePathFolder,SaveFileName);
                DBMTJsonUtils.SaveJObjectToFile(jObject, SaveJsonFilePath);


                //Get all Deduped file name for all TrianglelistTextures
                string TrianglelistTexturesFolderPath = DrawIBConfig.GetTrianglelistTexturesFolderPath(DrawIB);
                if (!Directory.Exists(TrianglelistTexturesFolderPath))
                {
                    continue;
                }

                List<string> TrianglelistTextureFileNameList = DBMTFileUtils.GetFileNameListInFolder(TrianglelistTexturesFolderPath);

                JObject TrianglelistDedupedFileNameJObject = DBMTJsonUtils.CreateJObject();
                foreach (string TrianglelistTextureFileName in TrianglelistTextureFileNameList)
                {
                    string DedupedTextureFileName = FrameAnalysisLog.GetDedupedTextureFileName(TrianglelistTextureFileName);
                    string FADedupedFileName = FrameAnalysisData.GetDedupedTextureFileName(TrianglelistTextureFileName);

                    JObject TextureProperty = DBMTJsonUtils.CreateJObject();
                    TextureProperty["FALogDedupedFileName"] = DedupedTextureFileName;
                    TextureProperty["FADataDedupedFileName"] = FADedupedFileName;

                    TrianglelistDedupedFileNameJObject[TrianglelistTextureFileName] = TextureProperty;
                }

                string TrianglelistDedupedFileNameJsonName = "TrianglelistDedupedFileName.json";
                string TrianglelistDedupedFileNameJsonPath = Path.Combine(TrianglelistTexturesFolderPath, TrianglelistDedupedFileNameJsonName);
                DBMTJsonUtils.SaveJObjectToFile(TrianglelistDedupedFileNameJObject, TrianglelistDedupedFileNameJsonPath);
            }

        }

    }
}
