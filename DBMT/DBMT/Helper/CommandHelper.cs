using DBMT_Core;
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
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.System;
using WinRT.Interop;

namespace DBMT
{
    public class CommandHelper
    {
        public static async Task<bool> ShellOpenFile(string FilePath)
        {
            try
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
            catch(Exception ex)
            {
                await MessageHelper.Show("Error: " + ex.ToString());
                return false;
            }

        }


        public static async Task<bool> ShellOpenFolder(string FolderPath)
        {
            try
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
            catch (Exception ex)
            {
                await MessageHelper.Show("Error: " + ex.ToString());
                return false;
            }
            
        }


        public static async void ConvertTexture(string SourceTextureFilePath, string TextureFormatString, string TargetOutputDirectory)
        {
            SourceTextureFilePath = SourceTextureFilePath.Replace("\\", "/");
            TargetOutputDirectory = TargetOutputDirectory.Replace("\\", "/");

            string channels = " -f rgba ";
            if (TextureFormatString == "jpg")
            {

                if (!SourceTextureFilePath.Contains("BC5_UNORM"))
                {
                    channels = " ";
                }
            }


            string arugmentsstr = " \"" + SourceTextureFilePath + "\" -ft \"" + TextureFormatString + "\" "+ channels + " -o \"" + TargetOutputDirectory + "\"";
            string texconv_filepath = Path.Combine(GlobalConfig.Path_PluginsFolder,"texconv.exe");
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

        public static void OpenWebLink(string Url)
        {
            if (Uri.IsWellFormedUriString(Url, UriKind.Absolute))
            {
                IAsyncOperation<bool> asyncOperation = Launcher.LaunchUriAsync(new Uri(Url));
            }
        }

        public static FileOpenPicker Get_FileOpenPicker(string Suffix,string StartLocation= "")
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

        public static FileOpenPicker Get_FileOpenPicker(List<string> SuffixList)
        {
            FileOpenPicker picker = new FileOpenPicker();
            // 获取当前窗口的HWND
            nint windowHandle = WindowNative.GetWindowHandle(App.m_window);
            InitializeWithWindow.Initialize(picker, windowHandle);

            picker.ViewMode = PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = PickerLocationId.Desktop;
            foreach (string Suffix in SuffixList)
            {
                picker.FileTypeFilter.Add(Suffix);
            }
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
            try
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
            catch (Exception exception)
            {
                await MessageHelper.Show("此功能不支持管理员权限运行，请切换到普通用户打开DBMT。\n" + exception.ToString(), "This functio can't run on admin user please use normal user to open DBMT. \n" + exception.ToString());
            }
            return "";
        }

        public static async Task<string> ChooseFileAndGetPath(List<string> SuffixList)
        {
            try
            {
                FileOpenPicker picker = CommandHelper.Get_FileOpenPicker(SuffixList);
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
            catch (Exception exception)
            {
                await MessageHelper.Show("此功能不支持管理员权限运行，请切换到普通用户打开DBMT。\n" + exception.ToString(), "This functio can't run on admin user please use normal user to open DBMT. \n" + exception.ToString());
            }
            return "";
        }

        public static async Task<string> ChooseFolderAndGetPath()
        {
            try
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
            catch (Exception exception)
            {
                await MessageHelper.Show("此功能不支持管理员权限运行，请切换到普通用户打开DBMT。\n" + exception.ToString());
            }
            return "";
            
        }


    }
}
