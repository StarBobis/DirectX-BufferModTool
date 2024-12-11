using Microsoft.UI.Xaml.Controls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace DBMT
{
    public class CommandHelper
    {
        public static async Task<bool> ShellOpenFile(string FilePath)
        {
            if (File.Exists(FilePath))
            {
                try
                {
                    string workingDirectory = System.IO.Path.GetDirectoryName(FilePath); // 获取程序所在目录

                    ProcessStartInfo startInfo = new ProcessStartInfo
                    {
                        FileName = FilePath,
                        UseShellExecute = true, // 允许操作系统决定如何打开文件
                        WorkingDirectory = workingDirectory // 设置工作路径为程序所在路径
                    };

                    Process.Start(startInfo);
                }
                catch (Exception ex)
                {
                    await MessageHelper.Show("打开文件出错: \n" + FilePath + "\n" + ex.Message);
                    return false;
                }
            }
            else
            {
                await MessageHelper.Show("要打开的文件路径不存在: \n" + FilePath);
                return false;
            }
            return true;
        }


        public static async Task<bool> ShellOpenFolder(string FolderPath)
        {
            if (Directory.Exists(FolderPath))
            {
                try
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo
                    {
                        FileName = FolderPath,
                        UseShellExecute = true, // 允许操作系统决定如何打开文件夹
                        WorkingDirectory = FolderPath // 设置工作路径为要打开的文件夹路径
                    };

                    Process.Start(startInfo);
                }
                catch (Exception ex)
                {
                    await MessageHelper.Show("打开文件夹出错: \n" + FolderPath + "\n" + ex.Message);
                    return false;
                }
            }
            else
            {
                await MessageHelper.Show("要打开的文件夹路径不存在: \n" + FolderPath);
                return false;
            }

            return true;
        }


        public static async void ConvertTexture(string SourceTextureFilePath, string TextureFormatString, string TargetOutputDirectory)
        {
            SourceTextureFilePath = SourceTextureFilePath.Replace("\\", "/");
            TargetOutputDirectory = TargetOutputDirectory.Replace("\\", "/");

            string arugmentsstr = " \"" + SourceTextureFilePath + "\" -ft \"" + TextureFormatString + "\" -o \"" + TargetOutputDirectory + "\"";
            string texconv_filepath = MainConfig.ApplicationRunPath + "Plugins\\texconv.exe";
            if (!File.Exists(texconv_filepath))
            {
                await MessageHelper.Show("当前要执行的路径不存在: " + texconv_filepath, "Current run path didn't exsits: " + texconv_filepath);
                return;
            }

            //https://github.com/microsoft/DirectXTex/wiki/Texconv
            Process process = new Process();
            process.StartInfo.FileName = texconv_filepath;
            process.StartInfo.Arguments = arugmentsstr;
            process.StartInfo.UseShellExecute = false;  // 不使用操作系统的shell启动程序
            process.StartInfo.RedirectStandardOutput = true;  // 重定向标准输出
            process.StartInfo.RedirectStandardError = true;   // 重定向标准错误输出
            process.StartInfo.CreateNoWindow = true;  // 不创建新窗口
            process.Start();
            process.WaitForExit();
        }


        public static void InitializeRunInputJson(string arguments)
        {
            //把当前运行的命令保存到RunInput.json
            string json = File.ReadAllText(MainConfig.Path_RunInputJson); // 读取文件内容
            JObject runInputJson = JObject.Parse(json);
            runInputJson["RunCommand"] = arguments;
            string runInputJsonStr = runInputJson.ToString(Formatting.Indented);
            File.WriteAllText(MainConfig.Path_RunInputJson, runInputJsonStr);
        }

        public static void InitializeRunResultJson()
        {
            JObject jsonObject = new JObject();
            jsonObject["result"] = "Unknown Error!";
            File.WriteAllText(MainConfig.Path_RunResultJson, jsonObject.ToString());
        }


        public static async Task<bool> runCommand(string arguments, string targetExe = "")
        {

            InitializeRunInputJson(arguments);
            InitializeRunResultJson();
            Process process = new Process();
            if (targetExe == "")
            {
                if (MainConfig.CurrentGameName == "Game001" || MainConfig.CurrentGameName == "Mecha" || MainConfig.CurrentGameName == "LiarsBar")
                {

                    string ProVMPPath = MainConfig.ApplicationRunPath + "Plugins\\" + "DBMT-Pro.vmp.exe";
                    string ProPath = MainConfig.ApplicationRunPath + "Plugins\\" + "DBMT-Pro.exe";
                    if (File.Exists(ProPath))
                    {
                        process.StartInfo.FileName = ProPath;
                    }
                    else
                    {
                        process.StartInfo.FileName = ProVMPPath;
                    }
                }
                else
                {
                    process.StartInfo.FileName = MainConfig.ApplicationRunPath + "Plugins\\" + MainConfig.MMT_EXE_FileName;
                }
            }
            else
            {
                process.StartInfo.FileName = MainConfig.ApplicationRunPath + "Plugins\\" + targetExe;
            }
            //运行前必须检查路径
            if (!File.Exists(process.StartInfo.FileName))
            {
                await MessageHelper.Show("Current run path didn't exsits: " + process.StartInfo.FileName, "当前要执行的插件不存在: " + process.StartInfo.FileName + "\n请联系NicoMico赞助获取此插件。");
                return false;
            }

            process.StartInfo.Arguments = arguments;  // 可选，如果该程序接受命令行参数
            //MessageBox.Show("当前运行参数： " + arguments);

            // 配置进程启动信息
            process.StartInfo.UseShellExecute = false;  // 不使用操作系统的shell启动程序
            process.StartInfo.RedirectStandardOutput = false;  // 重定向标准输出
            process.StartInfo.RedirectStandardError = false;   // 重定向标准错误输出
            process.StartInfo.CreateNoWindow = true;  // 不创建新窗口
            // 启动程序
            process.Start();
            process.WaitForExit();

            string runResultJson = File.ReadAllText(MainConfig.Path_RunResultJson);
            JObject resultJsonObject = JObject.Parse(runResultJson);
            string runResult = (string)resultJsonObject["result"];

            MainConfig.RunResult = runResult;

            if (runResult != "success")
            {
                await MessageHelper.Show(
                    "运行结果: " + runResult + ". \n\n很遗憾运行失败了，参考运行结果和运行日志改变策略再试一次吧。\n\n1.请检查您的配置是否正确.\n2.请查看日志获取更多细节信息.\n3.请检查您是否使用的是最新版本，新版本可能已修复此问题\n4.请联系NicoMico寻求帮助或反馈BUG, 别忘了把最新的FrameAnalysis文件夹、提取用的IB、运行的日志文件也打包发送给他.\n\n点击确认为后您打开本次运行日志。",
                    "Run result: " + runResult + ". \n1.Please check your config.\n2.Please check log for more information.\n3.Please ask NicoMico for help, remember to send him the latest log file.\n4.Ask @Developer in ShaderFreedom for help.\n5.Read the source code of DBMT and try analyse the reason for Error with latest log file.");
                return false;
            }else
            {
                return true;
            }
            
            
        }


        public static FileOpenPicker Get_FileOpenPicker(string Suffix)
        {
            FileOpenPicker picker = new FileOpenPicker();
            // 获取当前窗口的HWND
            nint windowHandle = WindowNative.GetWindowHandle(App.m_window);
            InitializeWithWindow.Initialize(picker, windowHandle);

            picker.ViewMode = PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = PickerLocationId.Desktop;
            picker.FileTypeFilter.Add(Suffix);
            return picker;
        }

        public static FolderPicker Get_FolderPicker()
        {
            FolderPicker picker = new FolderPicker();
            // 获取当前窗口的HWND
            nint windowHandle = WindowNative.GetWindowHandle(App.m_window);
            InitializeWithWindow.Initialize(picker, windowHandle);

            picker.ViewMode = PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = PickerLocationId.Desktop;
            return picker;
        }

        public static async Task<string> ChooseFileAndGetPath(string Suffix)
        {
            FileOpenPicker picker = CommandHelper.Get_FileOpenPicker(Suffix);
            StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                return file.Path;
            }
            else
            {
                return "";
            }
        }

        public static async Task<string> ChooseFolderAndGetPath()
        {
            FolderPicker folderPicker = CommandHelper.Get_FolderPicker();
            folderPicker.FileTypeFilter.Add("*");
            StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                return folder.Path;
            }
            else
            {
                return "";
            }
        }

    }
}
