using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBMT_Core.Utils
{

    //自制简易LOG类，别老想着用什么日志框架，简单能解决问题就行。
    //不要过度设计！
    public static class LOG
    {
        private static List<string> LogLineList = new List<string>();
        private static bool Initialized = false;

        private static DateTime? StartTime = null;  // 用于存储开始时间

        // 初始化日志系统，清空现有日志并准备新的会话
        public static void Initialize()
        {
            // 清空现有日志条目
            //LogLineList.Clear();

            // 记录开始时间
            if (StartTime == null)
            {
                StartTime = DateTime.Now;
            }

            // 重置初始化状态
            Initialized = true;

            // 确保日志目录存在
            Directory.CreateDirectory(GlobalConfig.Path_LogsFolder);

        }

        public static string GetDateTimePrefix()
        {
            return $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}";
        }

        // 记录一条信息级别的日志到内存列表
        public static void Info(string message)
        {
            if (Initialized)
            {
                Debug.WriteLine(message);
                LogLineList.Add($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [INFO] {message}");
            }
            else
            {
                Debug.WriteLine(message);
            }
        }

        public static void Error(string Message)
        {
            if (Initialized)
            {
                LogLineList.Add(GetDateTimePrefix() + " [Error] " + Message);
            }
        }

        public static void NewLine(string message="")
        {
            if (Initialized)
            {
                if (message == "")
                {

                    LogLineList.Add($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [INFO] -------------------------------------------------------------------");
                }
                else
                {
                    LogLineList.Add($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [INFO] {message}");
                    LogLineList.Add($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [INFO] -------------------------------------------------------------------");
                }
            }
             
        }


        // 将内存中的日志条目写出到带有ISO 8601格式时间戳的新文件
        public static void SaveFile()
        {
            if (!Initialized)
            {
                throw new InvalidOperationException("请先调用Initialize()方法以初始化日志系统。");
            }

            if (LogLineList.Count == 0)
            {
                Console.WriteLine("没有日志条目需要保存。");
                return;
            }

            // 记录结束时间
            DateTime endTime = DateTime.Now;
            TimeSpan duration = endTime - StartTime.Value;

            // 记录结束时间和消耗时间
            Info($"日志系统结束于 {endTime:yyyy-MM-dd HH:mm:ss.fff}");
            Info($"本次会话总耗时: {duration.TotalSeconds:F3} 秒");

            // 创建一个新的日志文件名（例如使用ISO 8601格式的时间戳）
            string currentLogFileName = Path.Combine(GlobalConfig.Path_LogsFolder, $"{DateTime.Now:yyyyMMddTHHmmss}.log");


            //LogLineList.Add("=============================================");
            //LogLineList.Add("此日志由DBMT自动生成，顺序是倒过来的方便打开后就能看到最后一个报错内容，因为正常情况不会有人看运行日志");
            //LogLineList.Add("=============================================");
            //LogLineList.Reverse();

            // 将所有日志条目写入文件
            File.WriteAllLines(currentLogFileName, LogLineList);

            //Console.WriteLine($"已成功保存日志到文件: {currentLogFileName}");

            // 清空日志条目列表以便下次使用
            LogLineList.Clear();
            StartTime = null; // 重置开始时间

        }
    }
}
