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
            try
            {
                string json = File.ReadAllText(LoadPath);
                // 读取文件内容,并转换为T类型,然后赋值给当前对象
                Value = JsonConvert.DeserializeObject<T>(json);
            }
            catch (Exception ex)
            {
                ex.ToString();
                //如果用户因为系统卡死导致DBMT配置文件损坏，我们就覆盖配置文件
                Value = new T();
                SaveConfig();
                //覆盖后再重新读取
                string json = File.ReadAllText(LoadPath);
                // 读取文件内容,并转换为T类型,然后赋值给当前对象
                Value = JsonConvert.DeserializeObject<T>(json);
            }
            
        }

        public void SaveConfig(string SpecialSavePath = "")
        {
            if (string.IsNullOrEmpty(SavePath))
            {
                throw new Exception("SavePath of" + this.GetType().Name + "is null");
            }
            string jsonString = JsonConvert.SerializeObject(Value, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(SavePath, jsonString);

            if (SavePath.EndsWith("Main.json"))
            {
                if (Directory.Exists(GlobalConfig.Path_ConfigsFolder))
                {
                    File.WriteAllText(GlobalConfig.Path_MainConfig_ConfigFolder, jsonString);
                }
            }
        }
    }
    public class BaseConfig { 
    
    }


    public class MainSetting : BaseConfig
    {
        public string GameName { get; set; } = "ZZZ";
        public string WorkSpaceName { get; set; } = string.Empty;

        //DBMT位置
        public string DBMTLocation { get; set; } = "";

        //当前游戏的3Dmigoto目录
        public string CurrentGameMigotoFolder { get; set; } = "";

        //当前DBMT的工作目录
        public string DBMTWorkFolder { get; set; } = "";

        public double WindowWidth { get; set; } = 1280;
        public double WindowHeight { get; set; } = 720;
        public int WindowPositionX { get; set; } = -1;
        public int WindowPositionY { get; set; } = -1;

        //Others
        public bool AutoCleanFrameAnalysisFolder { get; set; } = true;

        public int FrameAnalysisFolderReserveNumber { get; set; } = 1;

        // 生成Mod设置
        public string ModSwitchKey { get; set; } = "\"x\",\"c\",\"v\",\"b\",\"n\",\"m\",\"j\",\"k\",\"l\",\"o\",\"p\",\"[\",\"]\",\"x\",\"c\",\"v\",\"b\",\"n\",\"m\",\"j\",\"k\",\"l\",\"o\",\"p\",\"[\",\"]\",\"x\",\"c\",\"v\",\"b\",\"n\",\"m\",\"j\",\"k\",\"l\",\"o\",\"p\",\"[\",\"]\"";

        //Extract Options
        public bool DontSplitModelByMatchFirstIndex { get; set; } = false;

        //Texture Options
        public bool AutoTextureOnlyConvertDiffuseMap { get; set; } = true;
        public int AutoTextureFormat { get; set; } = 0;
        public bool AutoDetectAndMarkTexture { get; set; } = true;
    }


    public static class GlobalConfig
    {
        //程序窗口名称
        public const string DBMT_Title = "DBMT V1.1.8.0"; 
        
        // 本地化存储的配置
        public static readonly ConfigLoader<MainSetting> MainCfg = new ConfigLoader<MainSetting>(Path_MainConfig);


        public static string CurrentGameName => MainCfg.Value.GameName;
        public static string CurrentWorkSpace => MainCfg.Value.WorkSpaceName;
        public static string Path_DBMTWorkFolder => GlobalConfig.MainCfg.Value.DBMTWorkFolder;

        public static string Path_Base
        {
            get { return Directory.GetCurrentDirectory(); }
        }

        public static string Path_AssetsGamesFolder
        {
            get { return Path.Combine(GlobalConfig.MainCfg.Value.DBMTWorkFolder, "Games\\"); }
        }

        public static string Path_ModsFolder
        {
            get { return Path.Combine(Path_LoaderFolder, "Mods\\"); }
        }


        public static string Path_CurrentWorkSpaceGeneratedModFolder
        {
            get { return Path.Combine(GlobalConfig.Path_ModsFolder, "Mod_" + GlobalConfig.CurrentWorkSpace + "\\"); }
        }

        public static string Path_LoaderFolder
        {
            get { return MainCfg.Value.CurrentGameMigotoFolder; }
        }

        public static string Path_D3DXINI
        {
            get { return Path.Combine(MainCfg.Value.CurrentGameMigotoFolder, "d3dx.ini"); }
        }

        public static string Path_3DmigotoGameModForkFolder
        {
            get { return Path.Combine(Path_DBMTWorkFolder, "3Dmigoto-GameMod-Fork\\"); }
        }

        public static string Path_TextureConfigsFolder
        {
            get { return Path.Combine(Path_ConfigsFolder, "TextureConfigs\\"); }
        }

        public static string Path_GameTypeConfigsFolder
        {
            get { return Path.Combine(Path_ConfigsFolder, "GameTypeConfigs\\" ); }
        }

        public static string Path_GameTextureConfigFolder
        {
            get { return Path.Combine(Path_TextureConfigsFolder, GlobalConfig.MainCfg.Value.GameName + "\\"); }
        }

        //// 配置文件路径
        public static string Path_MainConfig
        {
            get { return Path.Combine(Path_AppDataLocal, "DBMT-Main.json"); }
        }
        public static string Path_Game_SettingJson
        {
            get { return Path.Combine(Path_AppDataLocal, "DBMT-Setting.json"); }
        }

        public static string Path_MainConfig_ConfigFolder
        {
            get { return Path.Combine(Path_ConfigsFolder, "Main.json"); }
        }
        public static string Path_Game_SettingJson_ConfigFolder
        {
            get { return Path.Combine(Path_ConfigsFolder, "Setting.json"); }
        }

       

     

        public static string Path_RunResultJson
        {
            get { return Path.Combine(Path_ConfigsFolder, "RunResult.json"); }
        }

        public static string Path_RunInputJson
        {
            get { return Path.Combine(Path_ConfigsFolder, "RunInput.json"); }
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
            get { return Path.Combine(Path_DBMTWorkFolder, "Plugins\\"); }
        }


        public static string LatestFrameAnalysisFolderName
        {
            get
            {
                string[] directories = Directory.GetDirectories(GlobalConfig.Path_LoaderFolder);
                List<string> frameAnalysisFileList = new List<string>();
                foreach (string directory in directories)
                {
                    string directoryName = Path.GetFileName(directory);

                    if (directoryName.StartsWith("FrameAnalysis-"))
                    {
                        frameAnalysisFileList.Add(directoryName);
                    }
                }

                //
                if (frameAnalysisFileList.Count > 0)
                {
                    return frameAnalysisFileList.Last();
                }

                return "";
            }
        }

        /// <summary>
        /// 最新的FrameAnalysis文件夹
        /// </summary>
        public static string Path_LatestFrameAnalysisFolder
        {
            get
            {
                return Path.Combine(Path_LoaderFolder,LatestFrameAnalysisFolderName + "\\");
            }
        }

        public static string Path_LatestFrameAnalysisLogTxt
        {
            get
            {
                return Path.Combine(Path_LatestFrameAnalysisFolder, "log.txt");
            }
        }
        

        //起别名方便使用
        public static string WorkFolder
        {
            get
            {
                return Path_LatestFrameAnalysisFolder;
            }
        }

        public static string Path_LatestFrameAnalysisDedupedFolder
        {
            get
            {
                return Path.Combine(Path_LatestFrameAnalysisFolder, "deduped\\");
            }
        }

        public static string Path_LogsFolder
        {
            get { return Path.Combine(Path_DBMTWorkFolder,"Logs\\"); }
        }


        public static string Path_LatestDBMTLogFile
        {
            get
            {
                string logsPath = GlobalConfig.Path_LogsFolder;
                if (!Directory.Exists(logsPath))
                {
                    return "";
                }
                string[] logFiles = Directory.GetFiles(logsPath); ;
                List<string> logFileList = new List<string>();
                foreach (string logFile in logFiles)
                {
                    string logfileName = Path.GetFileName(logFile);
                    if (logfileName.EndsWith(".log") && logfileName.Length > 15)
                    {
                        logFileList.Add(logfileName);
                    }
                }

                logFileList.Sort();
                string LogFilePath = logsPath + "\\" + logFileList[^1];
                return LogFilePath;
            }
        }

        public static string AutoTextureFormatSuffix
        {
            get
            {
                return MainCfg.Value.AutoTextureFormat switch
                {
                    0 => "jpg",
                    1 => "tga",
                    2 => "png",
                    _ => "jpg"
                };
            }
        }

        public static string Path_TotalWorkSpaceFolder
        {
            get
            {
                return Path.Combine(GlobalConfig.Path_DBMTWorkFolder, "WorkSpace\\");
            }
        }

        public static string Path_CurrentGameTotalWorkSpaceFolder
        {
            get
            {
                return Path.Combine(GlobalConfig.Path_TotalWorkSpaceFolder, GlobalConfig.CurrentGameName + "\\");
            }
        }

        public static string Path_CurrentWorkSpaceFolder
        {
            get{
                string CurrentWorkSpaceFolder = Path.Combine(GlobalConfig.Path_TotalWorkSpaceFolder, GlobalConfig.CurrentGameName + "\\" + GlobalConfig.CurrentWorkSpace + "\\");
                return CurrentWorkSpaceFolder;
            }
        }

        public static string Path_AppDataLocal
        {
            get
            { // 如果你需要非漫游配置文件路径（AppData\Local），可以这样做：
                string localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                return localAppDataPath;
            }
        }

        public static string Path_ConfigsFolder
        {
            get { return Path.Combine(Path_DBMTWorkFolder, "Configs\\"); }
        }

        public static string Path_CurrentGameMainConfigJsonFile
        {
            get { return Path.Combine(Path_AssetsGamesFolder, GlobalConfig.MainCfg.Value.GameName + "\\MainConfig.json"); }
        }

    }
}
