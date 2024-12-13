using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using DBMT;

namespace DBMT
{
    public class ConfigLoader<T> where T : BaseConfig
     {
        public string SavePath { get; set; }
        public string LoadPath { get; set; }

        public ConfigLoader(string path)
        {
            LoadPath = path;
            SavePath = path;
        }

        public ConfigLoader(string loadPath, string savePath)
        {
            LoadPath = loadPath;
            SavePath = savePath;
        }

        public T Value { get; set; }

        public void LoadConfig()
        {
            if (string.IsNullOrEmpty(LoadPath))
            {
                throw new Exception("SavePath of" + this.GetType().Name + "is null");
            }
            if (!File.Exists(LoadPath))
            {
                // 如果文件不存在，创建一个新的配置文件
                Value = Activator.CreateInstance<T>();
                SaveConfig();
                //throw new Exception("Config file not found:" + LoadPath);
            }
            string json = File.ReadAllText(LoadPath);
            // 读取文件内容,并转换为T类型,然后赋值给当前对象
            Value = JsonConvert.DeserializeObject<T>(json);
        }

        public void SaveConfig()
        {
            if (string.IsNullOrEmpty(SavePath))
            {
                throw new Exception("SavePath of" + this.GetType().Name + "is null");
            }
            string jsonString = JsonConvert.SerializeObject(Value, Formatting.Indented);
            File.WriteAllText(SavePath, jsonString);
        }
    }
    public class BaseConfig{}

    public class GameConfig : BaseConfig
    {
        //UI行为设置
        public float GamePageBackGroundImageOpacity { get; set; } = 0.6f;
        public float WorkPageBackGroundImageOpacity { get; set; } = 0.3f;
        public bool WindowTopMost { get; set; } = false;

        public bool AutoCleanFrameAnalysisFolder { get; set; } = true;
        public bool AutoCleanLogFile { get; set; } = true;
        public int FrameAnalysisFolderReserveNumber { get; set; } = 1;
        public int LogFileReserveNumber { get; set; } = 3;
        public int ModelFileNameStyle { get; set; } = 1;
        public bool MoveIBRelatedFiles { get; set; } = false;
        public bool DontSplitModelByMatchFirstIndex { get; set; } = false;
        public bool GenerateSeperatedMod { get; set; } = false;
        public string Author { get; set; } = "";
        public string AuthorLink { get; set; } = "";
        public string ModSwitchKey { get; set; } = "\"x\",\"m\",\"k\",\"l\",\"u\",\"i\",\"o\",\"p\",\"[\",\"]\",\"y\"";
    }

    public class TextureConfig : BaseConfig
    {
        public bool ForbidAutoTexture { get; set; } = false;
        public bool ConvertDedupedTextures { get; set; } = true;
        public bool UseHashTexture { get; set; } = false;
        public int AutoTextureFormat { get; set; } = 2;
        public bool AutoTextureOnlyConvertDiffuseMap { get; set; } = true;
        public bool ForbidMoveTrianglelistTextures { get; set; } = false;
        public bool ForbidMoveDedupedTextures { get; set; } = false;
        public bool ForbidMoveRenderTextures { get; set; } = false;
    }

    public class MainSetting : BaseConfig
    {
        public string GameName { get; set; } = "HSR";
        public string WorkSpaceName { get; set; } = string.Empty;
    }

    public static class MainConfig
    {
        public const string DBMT_Title = "DirectX Buffer Mod Tool  当前版本:V1.1.0.3 "; //程序窗口名称
        public const string MMT_EXE_FileName = "DBMT.exe"; //由C++开发的核心算法进程

        ////当前程序运行所在位置的路径,注意这里已经包含了结尾的\\
        public static string ApplicationRunPath = AppDomain.CurrentDomain.BaseDirectory.ToString();

        //public static string CurrentGameName => GetConfig<string>(ConfigFiles.Main, "GameName");
        public static string CurrentGameName => MainCfg.Value.GameName;

        //public static string CurrentWorkSpace => GetConfig<string>(ConfigFiles.Main, "WorkSpaceName");
        public static string CurrentWorkSpace => MainCfg.Value.WorkSpaceName;

        public static string CurrentMode = "Dev"; //当前工作模式，分为Dev和Play，默认为Dev
        public static string RunResult = "";


        ////运行后程序动态生成
        public static string Path_Base
        {
            get { return Directory.GetCurrentDirectory(); }
        }
        public static string Path_Game_ConfigJson
        {
            get { return Path.Combine(Path_Base, "Games", MainConfig.MainCfg.Value.GameName, "Config.json"); }
        }
        public static string Path_Game_VSCheck_Json
        {
            get { return Path.Combine(Path_Base, "Games", MainConfig.MainCfg.Value.GameName, "VSCheck.json"); }
        }
        public static string Path_OutputFolder
        {
            get { return Path.Combine(Path_Base, "Games", MainConfig.MainCfg.Value.GameName, "3Dmigoto\\Mods\\output\\"); }
        }
        public static string Path_LoaderFolder
        {
            get { return Path.Combine(Path_Base, "Games", MainConfig.MainCfg.Value.GameName, "3Dmigoto\\"); }
        }
        public static string Path_D3DXINI
        {
            get { return Path.Combine(Path_LoaderFolder, "d3dx.ini"); }
        }

        public static string Path_ExtractTypesFolder
        {
            get { return Path.Combine(Path_Base,"Configs\\ExtractTypes\\"); }
        }

        public static string Path_GameTypeFolder
        {
            get { return Path.Combine(Path_ExtractTypesFolder, MainConfig.MainCfg.Value.GameName + "\\"); }
        }


        //// 配置文件路径
        public static string Path_MainConfig 
        {
            get { return Path.Combine(Path_Base, "Configs\\Main.json") ; }
        }

        public static string Path_Game_SettingJson
        {
            get { return Path.Combine(Path_Base, "Configs\\Setting.json"); }
        }

        public static string Path_Texture_SettingJson
        {
            get { return Path.Combine(Path_Base, "Configs\\TextureSetting.json"); }
        }

        public static string Path_RunResultJson
        {
            get { return Path.Combine(Path_Base, "Configs\\RunResult.json"); }
        }

        public static string Path_RunInputJson
        {
            get { return Path.Combine(Path_Base, "Configs\\RunInput.json"); }
        }

        public static string Path_DeviceKeySetting
        {
            get { return Path.Combine(Path_Base, "Configs\\DeviceKeySetting.json"); }
        }

        public static string Path_ACLFolderJson
        {
            get { return Path.Combine(Path_Base, "Configs\\ACLFolder.json"); }
        }


        //三种注入器的路径
        public static string Path_3Dmigoto_Loader_EXE
        {
            get { return Path.Combine(Path_LoaderFolder, "3Dmigoto Loader.exe"); }
        }

        public static string Path_3Dmigoto_Loader_PY
        {
            get { return Path.Combine(Path_LoaderFolder, "3Dmigoto Loader.py"); }
        }

        public static string Path_3Dmigoto_Loader_ByPassACE_EXE
        {
            get { return Path.Combine(Path_LoaderFolder, "3Dmigoto Loader-ByPassACE.exe"); }
        }


        public static ConfigLoader<MainSetting> MainCfg = new ConfigLoader<MainSetting>(Path_MainConfig);
        public static ConfigLoader<GameConfig> GameCfg = new ConfigLoader<GameConfig>(Path_Game_SettingJson);
        public static ConfigLoader<TextureConfig> TextureCfg = new ConfigLoader<TextureConfig>(Path_Texture_SettingJson);


        // 使用枚举而不是魔法值
        public enum ConfigFiles { Main, Game_Setting, Texture_Setting }

        //贴图配置
        private static JObject DefaultConfig_Texure = new JObject
        {
            { "ForbidAutoTexture", false },
            { "ConvertDedupedTextures", true },
            { "UseHashTexture", false },
            { "AutoTextureFormat", 2 },
            { "AutoTextureOnlyConvertDiffuseMap", true },
            { "ForbidMoveTrianglelistTextures", false },
            { "ForbidMoveDedupedTextures", false },
            { "ForbidMoveRenderTextures", false }
        };

        //全局配置
        private static readonly JObject DefaultConfig_Game = new JObject
        {
            { "WindowTopMost", false },
            { "AutoCleanFrameAnalysisFolder", true },
            { "AutoCleanLogFile", true },
            { "FrameAnalysisFolderReserveNumber", 1 },
            { "LogFileReserveNumber", 3 },
            { "ModelFileNameStyle", 1 },
            { "MoveIBRelatedFiles",false },
            { "DontSplitModelByMatchFirstIndex",false },
            { "GenerateSeperatedMod",false },
            { "Author","" },
            { "AuthorLink","" },
            { "ModSwitchKey","\"x\",\"m\",\"k\",\"l\",\"u\",\"i\",\"o\",\"p\",\"[\",\"]\",\"y\""}
        };

        //Main.json
        private static readonly JObject DefaultConfig_Main = new JObject
        {
            { "GameName", "HSR" },
            { "WorkSpaceName", "" }
        };

        private static readonly Dictionary<string, JObject> DefaultConfigs = new Dictionary<string, JObject>
        {
            { ConfigFiles.Main.ToString(), DefaultConfig_Main },
            { ConfigFiles.Game_Setting.ToString(), DefaultConfig_Game },
            { ConfigFiles.Texture_Setting.ToString(), DefaultConfig_Texure }
        };

        private static Dictionary<string, string> ConfigName_FilePath_Dict = new Dictionary<string, string>(){
            { ConfigFiles.Main.ToString(),Path_MainConfig},
            { ConfigFiles.Game_Setting.ToString(),Path_Game_SettingJson},
            { ConfigFiles.Texture_Setting.ToString(),Path_Texture_SettingJson}
        };

        //动态读取的配置保存在内存中
        private static Dictionary<string, JObject> JsonObjects = new();


        public static void LoadConfigFile(ConfigFiles configFiles)
        {
            switch (configFiles)
            {
                case ConfigFiles.Main:
                    MainCfg.LoadConfig();
                    break;
                case ConfigFiles.Game_Setting:
                    GameCfg.LoadConfig();
                    break;
                case ConfigFiles.Texture_Setting:
                    TextureCfg.LoadConfig();
                    break;
            }

            return;
        }

        /// <summary>
        /// 从指定文件 [file] 中获取配置 [key] 的值 , 并转换为 T 类型, 此方法已被弃用，请换用新方法：
        /// <para>MainCfg.Value.Key;</para>
        ///  <para>GameCfg.Value.Key;</para>
        ///  <para>TextureCfg.Value.Key;</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="configFiles"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static T GetConfig<T>(ConfigFiles configFiles, string key)
        {
            //----- 新旧兼容
            BaseConfig baseConfig = configFiles switch
            {
                ConfigFiles.Main => MainCfg.Value,
                ConfigFiles.Game_Setting => GameCfg.Value,
                ConfigFiles.Texture_Setting => TextureCfg.Value,
                _ => throw new Exception("ConfigFiles not found")
            };

            // 通过反射获取配置项的值
            var value = baseConfig.GetType().GetProperty(key).GetValue(baseConfig);
            return (T)value;
            
        }

        /// <summary>
        /// 将配置文件 [file] 中的配置 [key] 的值设置为 [value] ，指定为 T 类型
        /// 此方法已被弃用，请换用新方法：
        /// <para>MainCfg.Value.Key = value;</para>
        /// <para>GameCfg.Value.Key = value;</para>
        /// <para>TextureCfg.Value.Key = value;</para>
        /// </summary>
        /// <typeparam name="T"> 目标格式 </typeparam>
        /// <param name="configFiles"> 配置文件 </param>
        /// <param name="key"> 配置项名称 </param>
        /// <param name="value"> 配置项值 </param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static T SetConfig<T>(ConfigFiles configFiles, string key, T value)
        {
            //----------新旧兼容
            BaseConfig baseConfig = configFiles switch
            {
                ConfigFiles.Main => MainCfg.Value,
                ConfigFiles.Game_Setting => GameCfg.Value,
                ConfigFiles.Texture_Setting => TextureCfg.Value,
                _ => throw new Exception("ConfigFiles not found")
            };

            // 通过反射设置配置项的值
            baseConfig.GetType().GetProperty(key).SetValue(baseConfig, value);
            return value;
        }

        /// <summary>
        /// 保存配置，建议改为使用新方法：
        /// <para>MainCfg.SaveConfig();</para>
        /// <para>GameCfg.SaveConfig();</para>
        /// <para>TextureCfg.SaveConfig();</para>
        /// </summary>
        /// <param name="configFile"></param>
        /// <exception cref="Exception"></exception>
        public static void SaveConfig(ConfigFiles configFile)
        {
            //---------- 新旧兼容
            switch (configFile)
            {
                case ConfigFiles.Main:
                    MainCfg.SaveConfig();
                    break;
                case ConfigFiles.Game_Setting:
                    GameCfg.SaveConfig();
                    break;
                case ConfigFiles.Texture_Setting:
                    TextureCfg.SaveConfig();
                    break;
            }
            return;

          
        }

    }
}
