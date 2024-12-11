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
        public const string DBMT_Title = "DirectX Buffer Mod Tool  当前版本:V1.0.9.6 "; //程序窗口名称
        public const string MMT_EXE_FileName = "DBMT.exe"; //由C++开发的核心算法进程

        public const string Path_MainConfig = "Configs\\Main.json";
        public const string Path_RunResultJson = "Configs\\RunResult.json";
        public const string Path_RunInputJson = "Configs\\RunInput.json";

        public const string Path_DeviceKeySetting = "Configs\\DeviceKeySetting.json";
        public const string Path_ACLFolderJson = "Configs\\ACLFolder.json";

        //两个主要设置配置
        public const string Path_Game_SettingJson = "Configs\\Setting.json";
        public const string Path_Texture_SettingJson = "Configs\\TextureSetting.json";

        //当前程序运行所在位置的路径,注意这里已经包含了结尾的\\
        public static string ApplicationRunPath = AppDomain.CurrentDomain.BaseDirectory.ToString();
        public static string CurrentGameName = ""; //当前选择的游戏名称
        public static string CurrentWorkSpace = ""; //当前的工作空间名称
        public static string CurrentMode = "Dev"; //当前工作模式，分为Dev和Play，默认为Dev
        public static string RunResult = "";

        //运行后程序动态生成
        public static string Path_Game_ConfigJson = "";
        public static string Path_Game_VSCheck_Json = "";
        public static string Path_OutputFolder = "";
        public static string Path_LoaderFolder = "";
        public static string Path_D3DXINI = "";

        //首选项设置
        public static bool AutoCleanFrameAnalysisFolder = false;
        public static int FrameAnalysisFolderReserveNumber = 0;
        public static bool AutoCleanLogFile = false;
        public static int LogFileReserveNumber = 0;
        public static bool AutoTextureOnlyConvertDiffuseMap = true;
        public static bool MMTWindowTopMost = false;

        public static int AutoTextureFormat = 0;
        public static bool ConvertDedupedTextures = false;

        //json文件
        private static Dictionary<string, string> JsonFiles = new Dictionary<string, string>();
        private static Dictionary<string, JObject> JsonObjects = new Dictionary<string, JObject>();

        //默认配置
        // texure
        //        {
        //  "ForbidAutoTexture": false,
        //  "ConvertDedupedTextures": false,
        //  "UseHashTexture": false,
        //  "AutoTextureFormat": 2,
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
            { "AutoTextureFormat", 2 },
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
        //  "FrameAnalysisFolderReserveNumber": 1,
        //  "LogFileReserveNumber": 3,
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
            { "FrameAnalysisFolderReserveNumber", 1 },
            { "LogFileReserveNumber", 3 },
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
                if (File.Exists(file.Value))
                {
                    string json = File.ReadAllText(file.Value); // 读取文件内容
                    JObject jsonObject = JObject.Parse(json);
                    JsonObjects.Add(file.Key, jsonObject);
                    continue;
                }
                // 给个 警告
                Console.WriteLine("File not found: " + file.Value);
                // 如果文件不存在，就用默认配置
                JsonObjects.Add(file.Key, DefaultConfigs[file.Key]);
            }
        }

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
            throw new Exception("Key not found in config file");
        }

        // 从任意文件中获取配置，不推荐使用，如果可能的话，还是建议指定文件
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
            throw new Exception("Key not found in config files");
        }

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
            throw new Exception("File not found in config files");
        }

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

        public static dynamic SetConfig(string file, string key, dynamic value)
        {
            if (JsonObjects.ContainsKey(file))
            {
                JsonObjects[file][key] = JToken.FromObject(value);
                return value;
            }
            // 抛出异常
            throw new Exception("File not found in config files");
        }

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

        public static void SaveConfig(string file)
        {
            if (JsonObjects.ContainsKey(file))
            {
                File.WriteAllText(JsonFiles[file], JsonObjects[file].ToString());
            }
            else
            {
                // 抛出异常
                throw new Exception("File not found in config files");
            }
        }

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


        public static void SetCurrentGame(string gameName)
        {
            MainConfig.CurrentGameName = gameName;

            string basePath = Directory.GetCurrentDirectory();

            //设置当前游戏名称
            MainConfig.Path_Game_ConfigJson = Path.Combine(basePath, "Games", MainConfig.CurrentGameName, "Config.json");
            MainConfig.Path_Game_VSCheck_Json = Path.Combine(basePath, "Games", MainConfig.CurrentGameName, "VSCheck.json");
            MainConfig.Path_LoaderFolder = Path.Combine(basePath,"Games", MainConfig.CurrentGameName, "3Dmigoto\\");
            MainConfig.Path_D3DXINI = Path.Combine(MainConfig.Path_LoaderFolder,"d3dx.ini");
            MainConfig.Path_OutputFolder = Path.Combine(MainConfig.Path_LoaderFolder, "Mods","output\\");

            //最后把当前游戏名称和类型保存到配置文件，做到和Blender联动。
            SaveCurrentGameNameToMainJson();
        }

        public static void SaveCurrentGameNameToMainJson()
        {
            if (File.Exists(MainConfig.Path_MainConfig))
            {
                string json = File.ReadAllText(MainConfig.Path_MainConfig); // 读取文件内容
                JObject jsonObject = JObject.Parse(json);
                jsonObject["GameName"] = MainConfig.CurrentGameName;
                File.WriteAllText(MainConfig.Path_MainConfig, jsonObject.ToString());
            }
            else
            {
                JObject jsonObject = new JObject();
                jsonObject["GameName"] = MainConfig.CurrentGameName;
                File.WriteAllText(MainConfig.Path_MainConfig, jsonObject.ToString());
            }
        }

        public static void ReadCurrentGameFromMainJson()
        {
            if (File.Exists(MainConfig.Path_MainConfig))
            {
                string json = File.ReadAllText(MainConfig.Path_MainConfig); // 读取文件内容
                JObject jsonObject = JObject.Parse(json);
                if (jsonObject.ContainsKey("GameName"))
                {
                    MainConfig.CurrentGameName = (string)jsonObject["GameName"];
                }
               
            }
        }

        public static void ReadCurrentWorkSpaceFromMainJson()
        {
            if (File.Exists(MainConfig.Path_MainConfig))
            {
                string json = File.ReadAllText(MainConfig.Path_MainConfig); // 读取文件内容
                JObject jsonObject = JObject.Parse(json);
                if (jsonObject.ContainsKey("WorkSpaceName"))
                {
                    string WorkSpaceName = (string)jsonObject["WorkSpaceName"];
                    MainConfig.CurrentWorkSpace = WorkSpaceName;
                }

            }
        }

        public static void SaveCurrentWorkSpaceToMainJson()
        {
            if (File.Exists(MainConfig.Path_MainConfig))
            {
                string json = File.ReadAllText(MainConfig.Path_MainConfig); // 读取文件内容
                JObject jsonObject = JObject.Parse(json);
                jsonObject["GameName"] = MainConfig.CurrentGameName;
                jsonObject["WorkSpaceName"] = MainConfig.CurrentWorkSpace;
                File.WriteAllText(MainConfig.Path_MainConfig, jsonObject.ToString());
            }
            else
            {
                JObject jsonObject = new JObject();
                jsonObject["GameName"] = MainConfig.CurrentGameName;
                jsonObject["WorkSpaceName"] = MainConfig.CurrentWorkSpace;
                File.WriteAllText(MainConfig.Path_MainConfig, jsonObject.ToString());
            }
        }

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
