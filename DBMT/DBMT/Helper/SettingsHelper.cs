using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBMT
{
    public static class SettingsHelper
    {
        public static bool WindowTopMost { get; set; } //暂时先别使用

        //基础设置
        public static bool AutoCleanFrameAnalysisFolder { get; set; }
        public static int FrameAnalysisFolderReserveNumber { get; set; }
        public static bool AutoCleanLogFile { get; set; }
        public static int LogFileReserveNumber { get; set; }
        public static int ModelFileNameStyle { get; set; }
        //提取设置
        public static bool MoveIBRelatedFiles { get; set; }
        public static bool DontSplitModelByMatchFirstIndex { get; set; }
        //生成Mod设置
        public static bool GenerateSeperatedMod { get; set; }
        public static string Author { get; set; }
        public static string AuthorLink { get; set; }
        public static string ModSwitchKey { get; set; }

        //贴图设置
        public static bool ForbidAutoTexture { get; set; }
        public static bool ConvertDedupedTextures { get; set; }
        public static bool UseHashTexture { get; set; }
        public static int AutoTextureFormat { get; set; }
        public static bool AutoTextureOnlyConvertDiffuseMap { get; set; }
        public static bool ForbidMoveTrianglelistTextures { get; set; }
        public static bool ForbidMoveDedupedTextures { get; set; }
        public static bool ForbidMoveRenderTextures { get; set; }


        public static void ReadGameSettingsFromConfig()
        {
            if (File.Exists(MainConfig.Path_Game_SettingJson))
            {
                string json = File.ReadAllText(MainConfig.Path_Game_SettingJson); // 读取文件内容
                JObject gameObject = JObject.Parse(json);

                if (gameObject.ContainsKey("WindowTopMost"))
                {
                    WindowTopMost = (bool)gameObject["WindowTopMost"];
                }

                if (gameObject.ContainsKey("AutoCleanFrameAnalysisFolder"))
                {
                    AutoCleanFrameAnalysisFolder = (bool)gameObject["AutoCleanFrameAnalysisFolder"];
                }

                if (gameObject.ContainsKey("FrameAnalysisFolderReserveNumber"))
                {
                    FrameAnalysisFolderReserveNumber = (int)gameObject["FrameAnalysisFolderReserveNumber"];
                }

                if (gameObject.ContainsKey("AutoCleanLogFile"))
                {
                    AutoCleanLogFile = (bool)gameObject["AutoCleanLogFile"];
                }

                if (gameObject.ContainsKey("LogFileReserveNumber"))
                {
                    LogFileReserveNumber = (int)gameObject["LogFileReserveNumber"];
                }

                if (gameObject.ContainsKey("ModelFileNameStyle"))
                {
                    ModelFileNameStyle = (int)gameObject["ModelFileNameStyle"];
                }

                if (gameObject.ContainsKey("MoveIBRelatedFiles"))
                {
                    MoveIBRelatedFiles = (bool)gameObject["MoveIBRelatedFiles"];
                }

                if (gameObject.ContainsKey("DontSplitModelByMatchFirstIndex"))
                {
                    DontSplitModelByMatchFirstIndex = (bool)gameObject["DontSplitModelByMatchFirstIndex"];
                }

                if (gameObject.ContainsKey("GenerateSeperatedMod"))
                {
                    GenerateSeperatedMod = (bool)gameObject["GenerateSeperatedMod"];
                }

                if (gameObject.ContainsKey("Author"))
                {
                    Author = (string)gameObject["Author"];
                }

                if (gameObject.ContainsKey("AuthorLink"))
                {
                    AuthorLink = (string)gameObject["AuthorLink"];
                }

                if (gameObject.ContainsKey("ModSwitchKey"))
                {
                    ModSwitchKey = (string)gameObject["ModSwitchKey"];
                }
            }
        }

        public static void ReadTextureSettingsFromConfig()
        {

            if (File.Exists(MainConfig.Path_Texture_SettingJson))
            {
                string json = File.ReadAllText(MainConfig.Path_Texture_SettingJson); // 读取文件内容
                JObject texturesObject = JObject.Parse(json);

                if (texturesObject.ContainsKey("ForbidAutoTexture"))
                {
                    ForbidAutoTexture = (bool)texturesObject["ForbidAutoTexture"];
                }

                if (texturesObject.ContainsKey("ConvertDedupedTextures"))
                {
                    ConvertDedupedTextures = (bool)texturesObject["ConvertDedupedTextures"];
                }

                if (texturesObject.ContainsKey("UseHashTexture"))
                {
                    UseHashTexture = (bool)texturesObject["UseHashTexture"];
                }

                if (texturesObject.ContainsKey("AutoTextureFormat"))
                {
                    AutoTextureFormat = (int)texturesObject["AutoTextureFormat"];
                }

                if (texturesObject.ContainsKey("AutoTextureOnlyConvertDiffuseMap"))
                {
                    AutoTextureOnlyConvertDiffuseMap = (bool)texturesObject["AutoTextureOnlyConvertDiffuseMap"];
                }

                if (texturesObject.ContainsKey("ForbidMoveTrianglelistTextures"))
                {
                    ForbidMoveTrianglelistTextures = (bool)texturesObject["ForbidMoveTrianglelistTextures"];
                }

                if (texturesObject.ContainsKey("ForbidMoveDedupedTextures"))
                {
                    ForbidMoveDedupedTextures = (bool)texturesObject["ForbidMoveDedupedTextures"];
                }

                if (texturesObject.ContainsKey("ForbidMoveRenderTextures"))
                {
                    ForbidMoveRenderTextures = (bool)texturesObject["ForbidMoveRenderTextures"];
                }

            }
        }

        public static void SaveGameSettingsToConfig()
        {
            JObject gameObject = new JObject();
            gameObject["WindowTopMost"] = WindowTopMost;
            gameObject["AutoCleanFrameAnalysisFolder"] = AutoCleanFrameAnalysisFolder;
            gameObject["AutoCleanLogFile"] = AutoCleanLogFile;
            gameObject["FrameAnalysisFolderReserveNumber"] = FrameAnalysisFolderReserveNumber;
            gameObject["LogFileReserveNumber"] = LogFileReserveNumber;
            gameObject["ModelFileNameStyle"] = ModelFileNameStyle;

            gameObject["MoveIBRelatedFiles"] = MoveIBRelatedFiles;
            gameObject["DontSplitModelByMatchFirstIndex"] = DontSplitModelByMatchFirstIndex;

            gameObject["GenerateSeperatedMod"] = GenerateSeperatedMod;
            gameObject["Author"] = Author;
            gameObject["AuthorLink"] = AuthorLink;
            gameObject["ModSwitchKey"] = ModSwitchKey;
            
            string json_string = gameObject.ToString(Formatting.Indented);
            File.WriteAllText(MainConfig.Path_Game_SettingJson, json_string);
        }

        public static void SaveTextureSettingsToConfig()
        {
            JObject textureObject = new JObject();

            textureObject["ForbidAutoTexture"] = ForbidAutoTexture;
            textureObject["ConvertDedupedTextures"] = ConvertDedupedTextures;
            textureObject["UseHashTexture"] = UseHashTexture;
            textureObject["AutoTextureFormat"] = AutoTextureFormat;
            textureObject["AutoTextureOnlyConvertDiffuseMap"] = AutoTextureOnlyConvertDiffuseMap;
            textureObject["ForbidMoveTrianglelistTextures"] = ForbidMoveTrianglelistTextures;
            textureObject["ForbidMoveDedupedTextures"] = ForbidMoveDedupedTextures;
            textureObject["ForbidMoveRenderTextures"] = ForbidMoveRenderTextures;

            string json_string = textureObject.ToString(Formatting.Indented);
            File.WriteAllText(MainConfig.Path_Texture_SettingJson, json_string);
        }

    }

}
