using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBMT
{
    public static class MainConfig
    {
        public const string DBMT_Title = "DirectX Buffer Mod Tool  当前版本:V1.0.9.7 "; //程序窗口名称

        public const string MMT_EXE_FileName = "DBMT.exe"; //由C++开发的核心算法进程

        //// 配置文件路径
        public const string Path_MainConfig = "Configs\\Main.json";
        public const string Path_Game_SettingJson = "Configs\\Setting.json";
        public const string Path_Texture_SettingJson = "Configs\\TextureSetting.json";

        //// 其他文件路径
        public const string Path_RunResultJson = "Configs\\RunResult.json";
        public const string Path_RunInputJson = "Configs\\RunInput.json";

        public const string Path_DeviceKeySetting = "Configs\\DeviceKeySetting.json";
        public const string Path_ACLFolderJson = "Configs\\ACLFolder.json";


        ////当前程序运行所在位置的路径,注意这里已经包含了结尾的\\
        public static string ApplicationRunPath = AppDomain.CurrentDomain.BaseDirectory.ToString();
        public static string CurrentGameName => GetConfig<string>(ConfigFiles.Main, "GameName");
        public static string CurrentWorkSpace => GetConfig<string>(ConfigFiles.Main, "WorkSpaceName");
        public static string CurrentMode = "Dev"; //当前工作模式，分为Dev和Play，默认为Dev
        public static string RunResult = "";

        ////运行后程序动态生成
        public static string Path_Base 
        { 
            get { return Directory.GetCurrentDirectory(); }
            set { Path_Base = value; }
        }
        public static string Path_Game_ConfigJson 
        {
            get { return Path.Combine(Path_Base, "Games", MainConfig.GetConfig<string>(ConfigFiles.Main,"GameName"), "Config.json"); }
        }
        public static string Path_Game_VSCheck_Json 
        { 
            get { return Path.Combine(Path_Base, "Games", MainConfig.GetConfig<string>(ConfigFiles.Main, "GameName"), "VSCheck.json"); }
        }
        public static string Path_OutputFolder
        {
            get { return Path.Combine(Path_Base, "Games", MainConfig.GetConfig<string>(ConfigFiles.Main, "GameName"), "3Dmigoto\\Mods\\output\\"); }
        }
        public static string Path_LoaderFolder
        {
            get { return Path.Combine(Path_Base, "Games", MainConfig.GetConfig<string>(ConfigFiles.Main, "GameName"), "3Dmigoto\\"); }  
        }
        public static string Path_D3DXINI 
        { 
            get { return Path.Combine(Path_LoaderFolder, "d3dx.ini"); }
        }

        ////首选项设置
        //public static bool AutoCleanFrameAnalysisFolder = false;
        //public static bool MMTWindowTopMost = false;

        //json文件
        private static Dictionary<string, string> JsonFiles = new Dictionary<string, string>();
        private static Dictionary<string, JObject> JsonObjects = new Dictionary<string, JObject>();

        //默认配置
        // texure
        // {
        //  "ForbidAutoTexture": false,
        //  "ConvertDedupedTextures": false,
        //  "UseHashTexture": false,
        //  "AutoTextureFormat": 0,
        //  "AutoTextureOnlyConvertDiffuseMap": true,
        //  "ForbidMoveTrianglelistTextures": false,
        //  "ForbidMoveDedupedTextures": false,
        //  "ForbidMoveRenderTextures": false
        //}
        private static JObject DefaultConfig_Texure = new JObject
        {
            { "ForbidAutoTexture", false },
            { "ConvertDedupedTextures", false },
            { "UseHashTexture", false },
            { "AutoTextureFormat", 0 },
            { "AutoTextureOnlyConvertDiffuseMap", true },
            { "ForbidMoveTrianglelistTextures", false },
            { "ForbidMoveDedupedTextures", false },
            { "ForbidMoveRenderTextures", false }
        };

        // game
        //{
        //  "WindowTopMost": false,
        //  "AutoCleanFrameAnalysisFolder": true,
        //  "AutoCleanLogFile": true,
        //  "FrameAnalysisFolderReserveNumber": 0,
        //  "LogFileReserveNumber": 0,
        //  "ModelFileNameStyle": 0,
        //  "MoveIBRelatedFiles": false,
        //  "DontSplitModelByMatchFirstIndex": false,
        //  "GenerateSeperatedMod": false,
        //  "Author": "",
        //  "AuthorLink": "",
        //  "ModSwitchKey": "\"x\",\"m\",\"k\",\"l\",\"u\",\"i\",\"o\",\"p\",\"[\",\"]\",\"y\""
        //}
        private static JObject DefaultConfig_Game = new JObject
        {
            { "WindowTopMost", false },
            { "AutoCleanFrameAnalysisFolder", true },
            { "AutoCleanLogFile", true },
            { "FrameAnalysisFolderReserveNumber", 0 },
            { "LogFileReserveNumber", 0 },
            { "ModelFileNameStyle", 0 },
            { "MoveIBRelatedFiles",false },
            { "DontSplitModelByMatchFirstIndex",false },
            { "GenerateSeperatedMod",false },
            { "Author","" },
            { "AuthorLink","" },
            { "ModSwitchKey","\"x\",\"m\",\"k\",\"l\",\"u\",\"i\",\"o\",\"p\",\"[\",\"]\",\"y\""}
        };

        // main
        //{
        //  "GameName": "HSR",
        //  "WorkSpaceName": "Test222"
        //}
        private static JObject DefaultConfig_Main = new JObject
        {
            { "GameName", "HSR" },
            { "WorkSpaceName", "Test222" }
        };

        private static Dictionary<string, JObject> DefaultConfigs = new Dictionary<string, JObject>
        {
            { "Main", DefaultConfig_Main },
            { "Game_Setting", DefaultConfig_Game },
            { "Texture_Setting", DefaultConfig_Texure }
        };

        public enum ConfigFiles { Main, Game_Setting, Texture_Setting }


        public static void LoadConfigFromFiles() {
            // 先清空
            JsonFiles.Clear();
            JsonObjects.Clear();

            // 加载配置文件
            JsonFiles.Add("Main", Path_MainConfig);
            JsonFiles.Add("Game_Setting", Path_Game_SettingJson);
            JsonFiles.Add("Texture_Setting", Path_Texture_SettingJson);
            foreach (var file in JsonFiles)
            {
                if (!File.Exists(file.Value))
                {
                    // 给个 警告
                    Console.WriteLine("File not found: " + file.Value);
                    // 如果文件不存在，就用默认配置
                    JsonObjects.Add(file.Key, DefaultConfigs[file.Key]);
                    continue;
                }

                string json = File.ReadAllText(file.Value); // 读取文件内容
                JObject jsonObject = JObject.Parse(json);

                // 配置文件可能 不完整，缺少一些配置项，所以需要用默认配置来填充
                // 使得 DefaultConfigs[file.Key] 中 存在的 配置项 jsonObject 中也存在
                bool needSave = false;
                foreach (var defaultConfig in DefaultConfigs[file.Key])
                {
                    if (!jsonObject.ContainsKey(defaultConfig.Key))
                    {
                        jsonObject.Add(defaultConfig.Key, defaultConfig.Value);
                        needSave = true;
                    }
                }

                JsonObjects.Add(file.Key, jsonObject);

                if (needSave) { SaveConfig(file.Key); }
            }
        }

        /// <summary>
        /// 从指定文件 [file] 中获取配置 [key] 的值 , 并转换为 T 类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="configFiles"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static T GetConfig<T>(ConfigFiles configFiles, string key)
        {
            // 获取配置文件
            // 将 configFiles 转换为字符串 ，然后调用 GetConfig<T>(string file, string key)
            string file = configFiles.ToString();
            if (JsonObjects.ContainsKey(file) && JsonObjects[file].ContainsKey(key))
            {
                return JsonObjects[file][key].ToObject<T>();
            }
            // 抛出异常
            throw new Exception($"Key [{key}] not found in config file [{configFiles}]"); 
        }

        /// <summary>
        /// 从任意文件中获取配置，不推荐使用，如果可能的话，还是建议指定文件
        /// </summary>
        /// <typeparam name="T">目标格式</typeparam>
        /// <param name="key">配置项名称</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static T GetConfig<T>(string key)
        {
            foreach (var file in JsonFiles)
            {
                if (JsonObjects.ContainsKey(file.Key) && JsonObjects[file.Key].ContainsKey(key))
                {
                    return JsonObjects[file.Key][key].ToObject<T>();
                }
            }
            // 抛出异常
            throw new Exception($"Key [{ key }] not found in config files");
        }

        private static T GetDefaultConfig<T>(ConfigFiles configFiles, string key)
        {
            // 获取默认配置
            // 将 configFiles 转换为字符串 ，然后调用 GetConfig<T>(string file, string key)
            string file = configFiles.ToString();
            if (DefaultConfigs.ContainsKey(file) && DefaultConfigs[file].ContainsKey(key))
            {
                JsonObjects[file][key] = DefaultConfigs[file][key];
                return DefaultConfigs[file][key].ToObject<T>();
            }
            else
            {
                // 抛出异常
                throw new Exception($"Key [{key}] not found in default config file [{configFiles}]");
            }
        }

        /// <summary>
        /// 将配置文件 [file] 中的配置 [key] 的值设置为 [value] ，指定为 T 类型
        /// </summary>
        /// <typeparam name="T"> 目标格式 </typeparam>
        /// <param name="configFiles"> 配置文件 </param>
        /// <param name="key"> 配置项名称 </param>
        /// <param name="value"> 配置项值 </param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static T SetConfig<T>(ConfigFiles configFiles, string key, T value)
        {
            // 设置配置文件
            // 将 configFiles 转换为字符串 ，然后调用 SetConfig<T>(string file, string key, T value)
            string file = configFiles.ToString();
            if (JsonObjects.ContainsKey(file))
            {
                JsonObjects[file][key] = JToken.FromObject(value);
                return value;
            }
            // 抛出异常
            throw new Exception($"File [{file}] not found in config files [{JsonFiles}]");
        }

        /// <summary>
        /// 将配置文件 [file] 中的配置 [key] 的值设置为 [value]，无需指定类型
        /// </summary>
        /// <param name="configFiles"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static dynamic SetConfig(ConfigFiles configFiles, string key, dynamic value) {
            // 设置配置文件
            // 将 configFiles 转换为字符串 ，然后调用 SetConfig<T>(string file, string key, T value)
            string file = configFiles.ToString();
            if (JsonObjects.ContainsKey(file))
            {
                // 因为 这里的 JToken.FromObject(value) 能够自动 转化为对应的类型，所以说在设置的时候可以偷懒不指定类型
                // 但是 为了 类型安全，还是建议指定类型
                JsonObjects[file][key] = JToken.FromObject(value);
                return value;
            }
            // 抛出异常
            throw new Exception("File not found in config files");
        }

        /// <summary>
        /// 从任意文件中设置配置，不推荐使用，如果可能的话，还是建议指定文件
        /// </summary>
        /// <param name="key"> 配置项名称 </param>
        /// <param name="value"> 配置项值 </param>
        /// <returns> 返回配置项值 </returns>
        /// <exception cref="Exception"></exception>
        //从任意文件中设置配置，不推荐使用，如果可能的话，还是建议指定文件
        public static dynamic SetConfig(string key, dynamic value)
        {
            foreach (var file in JsonFiles)
            {
                if (JsonObjects.ContainsKey(file.Key))
                {
                    JsonObjects[file.Key][key] = JToken.FromObject(value);
                    return value;
                }
            }
            // 抛出异常
            throw new Exception("File not found cotaining key in config files");
        }


        private static void SaveConfig(string file)
        {
            if (JsonObjects.ContainsKey(file))
            {
                File.WriteAllText(JsonFiles[file], JsonObjects[file].ToString());
            }
            else
            {
                // 抛出异常
                throw new Exception($"File not found in config file [ { file} ] ");
            }
        }

        /// <summary>
        /// 保存配置文件 [file]
        /// </summary>
        /// <param name="file"> 需要保存的配置文件 </param>
        /// <exception cref="Exception"></exception>
        public static void SaveConfig(ConfigFiles configFiles) {
            // 保存配置文件
            // 将 configFiles 转换为字符串 ，然后调用 SaveConfig(string file)
            string file = configFiles.ToString();
            SaveConfig(file);
        }

        public static void SaveAllConfig()
        {
            foreach (var file in JsonFiles)
            {
                SaveConfig(file.Key);
            }
        }


        //public static void SetCurrentGame(string gameName)
        //{
        //    MainConfig.CurrentGameName = gameName;

        //    //string basePath = Directory.GetCurrentDirectory();

        //    //设置当前游戏名称
        //    //MainConfig.Path_Game_ConfigJson = Path.Combine(basePath, "Games", MainConfig.CurrentGameName, "Config.json");
        //    //MainConfig.Path_Game_VSCheck_Json = Path.Combine(basePath, "Games", MainConfig.CurrentGameName, "VSCheck.json");
        //    //MainConfig.Path_LoaderFolder = Path.Combine(basePath,"Games", MainConfig.CurrentGameName, "3Dmigoto\\");
        //    //MainConfig.Path_D3DXINI = Path.Combine(MainConfig.Path_LoaderFolder,"d3dx.ini");
        //    //MainConfig.Path_OutputFolder = Path.Combine(MainConfig.Path_LoaderFolder, "Mods","output\\");

        //    //最后把当前游戏名称和类型保存到配置文件，做到和Blender联动。
        //    SetConfig(ConfigFiles.Main, "GameName", MainConfig.CurrentGameName);
        //    SaveConfig(ConfigFiles.Main);
        //    SaveConfig(ConfigFiles.Game_Setting);
        //    //SaveCurrentGameNameToMainJson();
        //}

        //public static void ReadCurrentGameFromMainJson()
        //{
        //    // Caution
        //    // 不知道 CurrentGameName 和 GameName 有什么区别
        //    // 但这里还是按照老的逻辑来
        //    MainConfig.CurrentGameName = GetConfig<string>(ConfigFiles.Main, "GameName");
        //}

        //public static void ReadCurrentWorkSpaceFromMainJson()
        //{
        //    // Caution
        //    // 这里好像是为了防止刷新之后，当前游戏名称被覆盖，但是 mainConfig 没有更改
        //    // 但是问题是，如果所有的接口都走 mainConfig ，那么 mainConfig 的 配置的版本一定是比本地文件的版本要新的

        //    // 但是这里为了防止 bug ，所以还是读取一下
        //    LoadConfigFromFiles();
        //}

        //public static void SaveCurrentWorkSpaceToMainJson()
        //{ 
        //    SaveConfig(ConfigFiles.Main);
        //}

    }


    public static class ConfigHelper
    {
        public static List<string> GetDrawIBListFromConfig(string WorkSpaceName)
        {
            List<string> drawIBListValues = new List<string>();

            string Configpath = MainConfig.Path_OutputFolder + WorkSpaceName + "\\Config.json";
            if (File.Exists(Configpath))
            {
                //切换到对应配置
                string jsonData = File.ReadAllText(Configpath);
                JObject jobj = JObject.Parse(jsonData);
                // Access the DrawIBList property and convert it to a List<string>
                JArray drawIBList = (JArray)jobj["DrawIBList"];
                drawIBListValues = drawIBList.ToObject<List<string>>();
            }

            return drawIBListValues;
        }

        public static string GetD3DXIniPath()
        {
            string D3DXIniPath = "";
            D3DXIniPath = Path.Combine(MainConfig.Path_LoaderFolder, "d3dx.ini");
            return D3DXIniPath;
        }


        public static string ReadAttributeFromD3DXIni(string AttributeName)
        {
            string d3dxini_path = ConfigHelper.GetD3DXIniPath();
            if (File.Exists(d3dxini_path))
            {
                string[] lines = File.ReadAllLines(d3dxini_path);
                foreach (string line in lines)
                {
                    string trim_lower_line = line.Trim().ToLower();
                    if (trim_lower_line.StartsWith(AttributeName) && trim_lower_line.Contains("="))
                    {
                        string[] splits = line.Split('=');
                        string target_path = splits[1];
                        return target_path;
                    }
                    
                }
            }
            return "";
        }

        public static void SaveAttributeToD3DXIni(string SectionName,string AttributeName,string AttributeValue)
        {
            string d3dxini_path = ConfigHelper.GetD3DXIniPath();
            if (File.Exists(d3dxini_path))
            {
                string OriginalAttributeValue = ReadAttributeFromD3DXIni(AttributeName);
                //只有存在此属性时，写入才有意义，否则等于白写一遍原内容
                if(OriginalAttributeValue.Trim() != "")
                {
                    List<string> newLines = new List<string>();
                    string[] lines = File.ReadAllLines(d3dxini_path);
                    foreach (string line in lines)
                    {
                        string trim_lower_line = line.Trim().ToLower();
                        if (trim_lower_line.StartsWith(AttributeName) && trim_lower_line.Contains("="))
                        {
                            string TargetPath = AttributeName + " = " + AttributeValue;
                            newLines.Add(TargetPath);
                        }
                        else
                        {
                            newLines.Add(line);
                        }
                    }
                    File.WriteAllLines(d3dxini_path, newLines);
                }
                else
                {
                    //如果不存在此属性，则写到对应SectionName下面
                    List<string> newLines = new List<string>();
                    string[] lines = File.ReadAllLines(d3dxini_path);
                    foreach (string line in lines)
                    {
                        string trim_lower_line = line.Trim().ToLower();
                        if (trim_lower_line.StartsWith(SectionName)){
                            newLines.Add(line);
                            string TargetPath = AttributeName + " = " + AttributeValue;
                            newLines.Add(TargetPath);
                        }
                        else
                        {
                            newLines.Add(line);
                        }
                    }
                    File.WriteAllLines(d3dxini_path, newLines);
                }
            }
        }


    }


}
