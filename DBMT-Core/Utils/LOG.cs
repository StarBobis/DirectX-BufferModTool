using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBMT_Core.Utils
{
    public static class LOG
    {
        private static List<string> LogLineList = new List<string>();
        private static bool Initialized = false;

        // 初始化日志系统，清空现有日志并准备新的会话
        public static void Initialize()
        {
            
            // 清空现有日志条目
            LogLineList.Clear();

            // 重置初始化状态
            Initialized = true;

            // 确保日志目录存在
            Directory.CreateDirectory(GlobalConfig.Path_DBMTLogFolder);

            //Console.WriteLine("日志系统已重新初始化。");
        }

        // 记录一条信息级别的日志到内存列表
        public static void Info(string message)
        {
            if (!Initialized)
            {
                throw new InvalidOperationException("请先调用Initialize()方法以初始化日志系统。");
            }

            var logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [INFO] {message}";
            //Console.WriteLine(logEntry); // 可选：同时输出到控制台
            LogLineList.Add(logEntry);
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

            // 创建一个新的日志文件名（例如使用ISO 8601格式的时间戳）
            string currentLogFileName = Path.Combine(GlobalConfig.Path_DBMTLogFolder, $"{DateTime.Now:yyyyMMddTHHmmssfff}.log");

            // 将所有日志条目写入文件
            File.WriteAllLines(currentLogFileName, LogLineList);

            //Console.WriteLine($"已成功保存日志到文件: {currentLogFileName}");

            // 清空日志条目列表以便下次使用
            LogLineList.Clear();
        }
    }
}
