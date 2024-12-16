using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DBMT_Core
{

    public class ConfigLoader<T> where T : BaseConfig, new()
    {
        public string SavePath { get; set; }
        public string LoadPath { get; set; }
        public T Value { get; set; }

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

        public void LoadConfig()
        {
            if (string.IsNullOrEmpty(LoadPath))
            {
                throw new Exception("SavePath of" + this.GetType().Name + "is null");
            }
            if (!File.Exists(LoadPath))
            {
                // 如果文件不存在，创建一个新的配置文件
                Value = new T();
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
            string jsonString = JsonConvert.SerializeObject(Value, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(SavePath, jsonString);
        }
    }
    public class BaseConfig { }

    public class GameConfig : BaseConfig
    {
        //UI Behaviour
        public bool Language { get; set; } = true;
        public double WindowWidth { get; set; } = 1000;
        public double WindowHeight { get; set; } = 600;
        public float GamePageBackGroundImageOpacity { get; set; } = 0.6f;
        public float WorkPageBackGroundImageOpacity { get; set; } = 0.3f;
        public bool StartToWorkPage { get; set; } = false;
        public bool WindowTopMost { get; set; } = false;
        //Others
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


        public bool AutoTextureOnlyConvertDiffuseMap { get; set; } = true;
        public bool ConvertDedupedTextures { get; set; } = true;
        public int AutoTextureFormat { get; set; } = 2;

        public bool ForbidMoveTrianglelistTextures { get; set; } = false;
        public bool ForbidMoveDedupedTextures { get; set; } = false;
        public bool ForbidMoveRenderTextures { get; set; } = false;
        public bool ForbidAutoTexture { get; set; } = false;
        public bool UseHashTexture { get; set; } = false;

    }


    public class MainSetting : BaseConfig
    {
        public string GameName { get; set; } = "HSR";
        public string WorkSpaceName { get; set; } = string.Empty;
    }

    public static class GlobalConfig
    {
        public const string DBMT_Title = "DirectX Buffer Mod Tool V1.1.1.2 "; //程序窗口名称
        public const string MMT_EXE_FileName = "DBMT.exe"; //由C++开发的核心算法进程

        ////当前程序运行所在位置的路径,注意这里已经包含了结尾的\\
        public static string ApplicationRunPath = AppDomain.CurrentDomain.BaseDirectory.ToString();
        public static string CurrentGameName => MainCfg.Value.GameName;
        public static string CurrentWorkSpace => MainCfg.Value.WorkSpaceName;

        public static string CurrentMode = "Dev"; //当前工作模式，分为Dev和Play，默认为Dev
        public static string RunResult = "";

        public static string Path_Base
        {
            get { return ApplicationRunPath; }
        }

        public static string Path_Game_ConfigJson
        {
            get { return Path.Combine(Path_Base, "Games", GlobalConfig.MainCfg.Value.GameName, "Config.json"); }
        }
        public static string Path_Game_VSCheck_Json
        {
            get { return Path.Combine(Path_Base, "Games", GlobalConfig.MainCfg.Value.GameName, "VSCheck.json"); }
        }
        public static string Path_OutputFolder
        {
            get { return Path.Combine(Path_Base, "Games", GlobalConfig.MainCfg.Value.GameName, "3Dmigoto\\Mods\\output\\"); }
        }
        public static string Path_LoaderFolder
        {
            get { return Path.Combine(Path_Base, "Games", GlobalConfig.MainCfg.Value.GameName, "3Dmigoto\\"); }
        }
        public static string Path_D3DXINI
        {
            get { return Path.Combine(Path_LoaderFolder, "d3dx.ini"); }
        }

        public static string Path_ExtractTypesFolder
        {
            get { return Path.Combine(Path_Base, "Configs\\ExtractTypes\\"); }
        }

        public static string Path_GameTypeFolder
        {
            get { return Path.Combine(Path_ExtractTypesFolder, GlobalConfig.MainCfg.Value.GameName + "\\"); }
        }


        //// 配置文件路径
        public static string Path_MainConfig
        {
            get { return Path.Combine(Path_Base, "Configs\\Main.json"); }
        }

        public static string Path_Game_SettingJson
        {
            get { return Path.Combine(Path_Base, "Configs\\Setting.json"); }
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

        public static string Path_PluginsFolder
        {
            get { return Path.Combine(Path_Base, "Plugins\\"); }
        }

        public static string Path_3DmigotoSwordLv5VMPEXE
        {
            get { return Path.Combine(Path_PluginsFolder, "3Dmigoto-Sword-Lv5.vmp.exe"); }
        }

        public static string Path_EncryptorVMPEXE
        {
            get { return Path.Combine(Path_PluginsFolder, "DBMT-Encryptor.vmp.exe"); }
        }

        // 本地化存储的配置
        public static readonly ConfigLoader<MainSetting> MainCfg = new ConfigLoader<MainSetting>(Path_MainConfig);
        public static readonly ConfigLoader<GameConfig> GameCfg = new ConfigLoader<GameConfig>(Path_Game_SettingJson);

    }


}
