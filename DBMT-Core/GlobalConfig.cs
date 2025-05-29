using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Windows.Globalization;

namespace DBMT_Core
{
    
    public static class GlobalConfig
    {
        //程序窗口名称
        public const string DBMT_Title = "DBMT V1.1.9.4";
        public static string CurrentGameName { get; set; } = "ZZZ";
        public static string CurrentWorkSpace { get; set; } = "";
        public static string CurrentGameMigotoFolder { get; set; } = "";
        public static string DBMTWorkFolder { get; set; } = "";

        //窗口大小
        public static double WindowWidth { get; set; } = 1280;
        public static double WindowHeight { get; set; } = 720;
        public static int WindowPositionX { get; set; } = -1;
        public static int WindowPositionY { get; set; } = -1;

        //Others
        public static bool AutoCleanFrameAnalysisFolder { get; set; } = true;
        public static int FrameAnalysisFolderReserveNumber { get; set; } = 1;
        // 生成Mod设置
        public static string ModSwitchKey { get; set; } = "\"x\",\"c\",\"v\",\"b\",\"n\",\"m\",\"j\",\"k\",\"l\",\"o\",\"p\",\"[\",\"]\",\"x\",\"c\",\"v\",\"b\",\"n\",\"m\",\"j\",\"k\",\"l\",\"o\",\"p\",\"[\",\"]\",\"x\",\"c\",\"v\",\"b\",\"n\",\"m\",\"j\",\"k\",\"l\",\"o\",\"p\",\"[\",\"]\"";
        //Texture Options
        public static bool AutoTextureOnlyConvertDiffuseMap { get; set; } = true;
        public static string AutoTextureFormat { get; set; } = "jpg";
        public static bool AutoDetectAndMarkTexture { get; set; } = true;


        //// 配置文件路径
        public static string Path_MainConfig
        {
            get { return Path.Combine(Path_ConfigsFolder, "DBMT-Config.json"); }
        }

        public static string Path_MainConfig_Global
        {
            get { return Path.Combine(Path_AppDataLocal, "DBMT-Config.json"); }
        }

        public static void ReadConfig()
        {
            try
            {
                //读取配置时优先读取全局的
                JObject SettingsJsonObject = DBMTJsonUtils.CreateJObject();
                try
                {
                    if (File.Exists(GlobalConfig.Path_MainConfig_Global))
                    {
                        string json = File.ReadAllText(GlobalConfig.Path_MainConfig_Global);
                        SettingsJsonObject = JObject.Parse(json);
                    }
                }
                catch (Exception ex) {
                    ex.ToString();
                    //如果全局配置文件被蓝屏破坏等原因读取不到，或者Linux Wine模拟不存在时，读取工作空间下的配置文件
                    if (File.Exists(GlobalConfig.Path_MainConfig))
                    {
                        string json = File.ReadAllText(GlobalConfig.Path_MainConfig); // 读取文件内容
                        SettingsJsonObject = JObject.Parse(json);
                    }
                }
                
                //古法读取
                if (SettingsJsonObject.ContainsKey("CurrentGameName"))
                {
                    CurrentGameName = (string)SettingsJsonObject["CurrentGameName"];
                }

                if (SettingsJsonObject.ContainsKey("CurrentWorkSpace"))
                {
                    CurrentWorkSpace = (string)SettingsJsonObject["CurrentWorkSpace"];
                }

                if (SettingsJsonObject.ContainsKey("CurrentGameMigotoFolder"))
                {
                    CurrentGameMigotoFolder = (string)SettingsJsonObject["CurrentGameMigotoFolder"];
                }

                if (SettingsJsonObject.ContainsKey("DBMTWorkFolder"))
                {
                    DBMTWorkFolder = (string)SettingsJsonObject["DBMTWorkFolder"];
                }

                //WindowWidth
                if (SettingsJsonObject.ContainsKey("WindowWidth"))
                {
                    WindowWidth = (double)SettingsJsonObject["WindowWidth"];
                }

                //WindowHeight
                if (SettingsJsonObject.ContainsKey("WindowHeight"))
                {
                    WindowHeight = (double)SettingsJsonObject["WindowHeight"];
                }

                //WindowPositionX
                if (SettingsJsonObject.ContainsKey("WindowPositionX"))
                {
                    WindowPositionX = (int)SettingsJsonObject["WindowPositionX"];
                }

                //WindowPositionY
                if (SettingsJsonObject.ContainsKey("WindowPositionY"))
                {
                    WindowPositionY = (int)SettingsJsonObject["WindowPositionY"];
                }

                //AutoCleanFrameAnalysisFolder
                if (SettingsJsonObject.ContainsKey("AutoCleanFrameAnalysisFolder"))
                {
                    AutoCleanFrameAnalysisFolder = (bool)SettingsJsonObject["AutoCleanFrameAnalysisFolder"];
                }

                //FrameAnalysisFolderReserveNumber
                if (SettingsJsonObject.ContainsKey("FrameAnalysisFolderReserveNumber"))
                {
                    FrameAnalysisFolderReserveNumber = (int)SettingsJsonObject["FrameAnalysisFolderReserveNumber"];
                }

                //ModSwitchKey
                if (SettingsJsonObject.ContainsKey("ModSwitchKey"))
                {
                    ModSwitchKey = (string)SettingsJsonObject["ModSwitchKey"];
                }



                //AutoTextureOnlyConvertDiffuseMap
                if (SettingsJsonObject.ContainsKey("AutoTextureOnlyConvertDiffuseMap"))
                {
                    AutoTextureOnlyConvertDiffuseMap = (bool)SettingsJsonObject["AutoTextureOnlyConvertDiffuseMap"];
                }

                //AutoTextureFormat
                if (SettingsJsonObject.ContainsKey("AutoTextureFormat"))
                {
                    AutoTextureFormat = (string)SettingsJsonObject["AutoTextureFormat"];
                }

                //AutoDetectAndMarkTexture
                if (SettingsJsonObject.ContainsKey("AutoDetectAndMarkTexture"))
                {
                    AutoDetectAndMarkTexture = (bool)SettingsJsonObject["AutoDetectAndMarkTexture"];
                }


            }
            catch (Exception ex) {
                ex.ToString();
            }
        }

        public static void SaveConfig()
        {
            //古法保存
            JObject SettingsJsonObject = new JObject();

            SettingsJsonObject["CurrentGameName"] = CurrentGameName;
            SettingsJsonObject["CurrentWorkSpace"] = CurrentWorkSpace;
            SettingsJsonObject["CurrentGameMigotoFolder"] = CurrentGameMigotoFolder;
            SettingsJsonObject["DBMTWorkFolder"] = DBMTWorkFolder;
            SettingsJsonObject["WindowWidth"] = WindowWidth;
            SettingsJsonObject["WindowHeight"] = WindowHeight;
            SettingsJsonObject["WindowPositionX"] = WindowPositionX;
            SettingsJsonObject["WindowPositionY"] = WindowPositionY;
            SettingsJsonObject["AutoCleanFrameAnalysisFolder"] = AutoCleanFrameAnalysisFolder;
            SettingsJsonObject["FrameAnalysisFolderReserveNumber"] = FrameAnalysisFolderReserveNumber;
            SettingsJsonObject["ModSwitchKey"] = ModSwitchKey;
            SettingsJsonObject["AutoTextureOnlyConvertDiffuseMap"] = AutoTextureOnlyConvertDiffuseMap;
            SettingsJsonObject["AutoTextureFormat"] = AutoTextureFormat;
            SettingsJsonObject["AutoDetectAndMarkTexture"] = AutoDetectAndMarkTexture;

            //写出内容
            string WirteStirng = SettingsJsonObject.ToString();

            //保存到DBMT的工作空间文件夹下面
            if (Directory.Exists(GlobalConfig.Path_ConfigsFolder))
            {
                File.WriteAllText(Path_MainConfig, WirteStirng);
            }

            //保存配置时，全局配置也顺便保存一份
            File.WriteAllText(Path_MainConfig_Global, WirteStirng);
        }


        public static string Path_AssetsGamesFolder
        {
            get { return Path.Combine(GlobalConfig.DBMTWorkFolder, "Games\\"); }
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
            get { return GlobalConfig.CurrentGameMigotoFolder; }
        }

        public static string Path_D3DXINI
        {
            get { return Path.Combine(GlobalConfig.CurrentGameMigotoFolder, "d3dx.ini"); }
        }

        public static string Path_3DmigotoGameModForkFolder
        {
            get { return Path.Combine(GlobalConfig.DBMTWorkFolder, "3Dmigoto-GameMod-Fork\\"); }
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
            get { return Path.Combine(Path_TextureConfigsFolder, GlobalConfig.CurrentGameName + "\\"); }
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
            get { return Path.Combine(GlobalConfig.DBMTWorkFolder, "Plugins\\"); }
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

                if (LatestFrameAnalysisFolderName != "")
                {
                    return Path.Combine(Path_LoaderFolder, LatestFrameAnalysisFolderName + "\\");
                }
                else
                {
                    throw new Exception("当前3Dmigoto目录下没有任何FrameAnalysis文件夹");
                }
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
            get { return Path.Combine(GlobalConfig.DBMTWorkFolder,"Logs\\"); }
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


        public static string Path_TotalWorkSpaceFolder
        {
            get
            {
                return Path.Combine(GlobalConfig.DBMTWorkFolder, "WorkSpace\\");
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
            get { return Path.Combine(GlobalConfig.DBMTWorkFolder, "Configs\\"); }
        }

        public static string Path_CurrentGameMainConfigJsonFile
        {
            get { return Path.Combine(Path_AssetsGamesFolder, GlobalConfig.CurrentGameName + "\\MainConfig.json"); }
        }

    }
}
