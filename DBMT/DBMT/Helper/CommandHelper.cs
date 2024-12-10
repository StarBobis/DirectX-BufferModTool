using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBMT
{
    public class CommandHelper
    {
        public static void ShellOpenFile(string FilePath)
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
                    MessageHelper.Show("打开文件出错: \n" + FilePath + "\n" + ex.Message);
                }
            }
            else
            {
                MessageHelper.Show("要打开的文件路径不存在: \n" + FilePath);
            }

        }


        public static void ShellOpenFolder(string FolderPath)
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
                    MessageHelper.Show("打开文件夹出错: \n" + FolderPath + "\n" + ex.Message);
                }
            }
            else
            {
                MessageHelper.Show("要打开的文件夹路径不存在: \n" + FolderPath);
            }
        }


        public static void ConvertTexture(string SourceTextureFilePath, string TextureFormatString, string TargetOutputDirectory)
        {
            SourceTextureFilePath = SourceTextureFilePath.Replace("\\", "/");
            TargetOutputDirectory = TargetOutputDirectory.Replace("\\", "/");

            string arugmentsstr = " \"" + SourceTextureFilePath + "\" -ft \"" + TextureFormatString + "\" -o \"" + TargetOutputDirectory + "\"";
            string texconv_filepath = MainConfig.ApplicationRunPath + "Plugins\\texconv.exe";
            if (!File.Exists(texconv_filepath))
            {
                MessageHelper.Show("当前要执行的路径不存在: " + texconv_filepath, "Current run path didn't exsits: " + texconv_filepath);
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

    }
}
