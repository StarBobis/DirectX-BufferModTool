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
    public class DrawIBConfig
    {

        public static List<string> GetDrawIBListFromConfig()
        {
            List<string> drawIBListValues = new List<string>();

            string Configpath = Path.Combine(GlobalConfig.Path_CurrentWorkSpaceFolder, "Config.json");
            if (File.Exists(Configpath))
            {
                //切换到对应配置
                string jsonData = File.ReadAllText(Configpath);
                JArray DrawIBListJArray = JArray.Parse(jsonData);

                foreach (JObject jkobj in DrawIBListJArray)
                {
                    string DrawIB = (string)jkobj["DrawIB"];
                    drawIBListValues.Add(DrawIB);
                }
            }

            return drawIBListValues;
        }

        /// <summary>
        /// 从提取出来的文件的tmp.json中读取ComponentName和MatchFirstIndex的字典
        /// 可以独立于FrameAnalysis文件夹之外运行
        /// </summary>
        /// <param name="DrawIB"></param>
        /// <returns></returns>
        public static Dictionary<string, UInt64> Read_ComponentName_MatchFirstIndex_Dict(string DrawIB) {
            Dictionary<string, UInt64> ComponentName_MatchFirstIndex_Dict = new Dictionary<string, UInt64>();

            //否则直接从tmp.json中读取
            //先获取所有的TYPE_开头的数据类型文件夹
            List<string> TypeFolderPathList = DrawIBConfig.GetDrawIBOutputGameTypeFolderPathList(DrawIB);
            if (TypeFolderPathList.Count != 0)
            {
                string TypeFolderPath = TypeFolderPathList[0];
                string TmpJsonPath = Path.Combine(TypeFolderPath, "tmp.json");
                JObject jObject = DBMTJsonUtils.ReadJObjectFromFile(TmpJsonPath);

                JArray match_first_index = (JArray)jObject["MatchFirstIndex"];
                JArray partname_list = (JArray)jObject["PartNameList"];

                for (int i = 0; i < partname_list.Count; i++)
                {
                    ComponentName_MatchFirstIndex_Dict["Component " + partname_list[i].ToString()] = UInt64.Parse(match_first_index[i].ToString());
                }

            }

            return ComponentName_MatchFirstIndex_Dict;
        }



        /// <summary>
        /// 获取当前DrawIB提取出来的所有以TYPE_开头的文件夹路径
        /// </summary>
        /// <param name="DrawIB"></param>
        /// <returns></returns>
        public static List<string> GetDrawIBOutputGameTypeFolderPathList(string DrawIB)
        {
            //先获取所有的TYPE_开头的数据类型文件夹
            List<string> TypeFolderPathList = new List<string>();
            string CurrentDrawIBOutputFolder = Path.Combine(GlobalConfig.Path_CurrentWorkSpaceFolder, DrawIB + "\\");

            if (!Directory.Exists(CurrentDrawIBOutputFolder))
            {
                return TypeFolderPathList;
            }

            //遍历所有以TYPE开头的文件夹
            string[] DrawIBContentFolderList = Directory.GetDirectories(CurrentDrawIBOutputFolder);
            foreach (string ContentFolderPath in DrawIBContentFolderList)
            {

                string ContentFolderName = Path.GetFileName(ContentFolderPath);

                if (!ContentFolderName.StartsWith("TYPE_"))
                {
                    continue;
                }

                string TargetFolderPath = Path.Combine(CurrentDrawIBOutputFolder, ContentFolderName + "\\");
                TypeFolderPathList.Add(TargetFolderPath);
            }
            return TypeFolderPathList;
        }



        public static List<string> Read_DrawCallIndexList(string DrawIB, string ComponentName)
        {

            List<string> DrawCallIndexList = new List<string>();
            string ReadJsonFileFolder = Path.Combine(GlobalConfig.Path_CurrentWorkSpaceFolder, DrawIB + "\\");
            if (Directory.Exists(ReadJsonFileFolder))
            {
                string ReadJsonFileName = "ComponentName_DrawCallIndexList.json";
                string ReadJsonPath = Path.Combine(ReadJsonFileFolder, ReadJsonFileName);
                if (File.Exists(ReadJsonPath))
                {
                    JObject jObject = DBMTJsonUtils.ReadJObjectFromFile(ReadJsonPath);
                    DrawCallIndexList = jObject[ComponentName].ToObject<List<string>>();
                }
            }

            return DrawCallIndexList;
        }


        public static string GetCurrentDrawIBOutputFolder(string DrawIB)
        {
            string CurrentDrawIBOutputFolder = Path.Combine(GlobalConfig.Path_CurrentWorkSpaceFolder, DrawIB + "\\");
            return CurrentDrawIBOutputFolder;
        }

        public static string GetTrianglelistTexturesFolderPath(string DrawIB)
        {
            string CurrentDrawIBOutputFolder = DrawIBConfig.GetCurrentDrawIBOutputFolder(DrawIB);
            string TrianglelistFolder = Path.Combine(CurrentDrawIBOutputFolder , "TrianglelistTextures\\");
            return TrianglelistFolder;
        }


        public static Dictionary<string, List<string>> Get_ComponentName_DrawCallIndexList_Dict_FromJson(string DrawIB)
        {
            LOG.Info("Get_ComponentName_DrawCallIndexList_Dict_FromJson::Start");
            Dictionary<string, List<string>> ComponentName_DrawCallIndexList_Dict = new Dictionary<string, List<string>>();
            string ReadJsonFileFolder = Path.Combine(GlobalConfig.Path_CurrentWorkSpaceFolder, DrawIB + "\\");

            if (Directory.Exists(ReadJsonFileFolder))
            {
                string ReadJsonFileName = "ComponentName_DrawCallIndexList.json";
                string ReadJsonPath = Path.Combine(ReadJsonFileFolder, ReadJsonFileName);
                if (File.Exists(ReadJsonPath))
                {
                    JObject jObject = DBMTJsonUtils.ReadJObjectFromFile(ReadJsonPath);
                    foreach (var obj in jObject.Properties())
                    {

                        JArray indicesArray = (JArray)obj.Value;
                        ComponentName_DrawCallIndexList_Dict[obj.Name] = indicesArray.ToObject<List<string>>();
                    }
                }
            }

            LOG.Info("Get_ComponentName_DrawCallIndexList_Dict_FromJson::End");
            return ComponentName_DrawCallIndexList_Dict;
        }

    }
}
