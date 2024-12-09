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
    public static class CommandHelper
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
    }
}
