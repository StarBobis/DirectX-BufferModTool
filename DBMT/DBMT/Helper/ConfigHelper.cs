using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBMT
{
    public static class MainConfig
    {
        public const string DBMT_Title = "DirectX Buffer Mod Tool  当前版本:V1.0.9.3 "; //程序窗口名称
        public const string MMT_EXE_FileName = "DBMT.exe"; //由C++开发的核心算法进程

        public const string Path_MainConfig = "Configs\\Main.json";
        public const string Path_RunResultJson = "Configs\\RunResult.json";
        public const string Path_RunInputJson = "Configs\\RunInput.json";
        public const string Path_Game_SettingJson = "Configs\\Setting.json";
        public const string Path_Texture_SettingJson = "Configs\\TextureSetting.json";
        public const string Path_DeviceKeySetting = "Configs\\DeviceKeySetting.json";
        public const string Path_ACLFolderJson = "Configs\\ACLFolder.json";


        //当前程序运行所在位置的路径,注意这里已经包含了结尾的\\
        public static string ApplicationRunPath = AppDomain.CurrentDomain.BaseDirectory.ToString();
        public static string CurrentGameName = ""; //当前选择的游戏名称
        public static string CurrentMode = "Dev"; //当前工作模式，分为Dev和Play，默认为Dev


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

    }
}
