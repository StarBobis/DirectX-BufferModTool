using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBMT_Core
{
    public class LogCommand
    {
        // 调用索引 eg:000001
        public string CallIndex = "";

        // 调用的参数列表 eg:pAsync=0x00000000312A2838,pData=0x000000004108FCD0,DataSize=8,GetDataFlags=1
        public Dictionary<string, string> ArgumentKVMap = new Dictionary<string, string>();

        // 构造函数接收日志行作为参数
        public LogCommand(string logLine)
        {
            Console.WriteLine("初始化LogCommand::");

            this.CallIndex = logLine.Substring(0, 6);

            int startIndex = 0;
            int endIndex = 0;

            startIndex = logLine.IndexOf('(') + 1;
            endIndex = logLine.IndexOf(')');
            var arguments = logLine.Substring(startIndex, endIndex - startIndex);

            // 假设有一个静态方法 SplitString 可以分割字符串
            // 这里我们使用 C# 的 Split 方法代替
            foreach (var argumentStr in arguments.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var keyValueList = argumentStr.Trim().Split(new[] { ':' }, 2);
                if (keyValueList.Length == 2)
                {
                    var key = keyValueList[0].Trim();
                    var value = keyValueList[1].Trim();
                    Console.WriteLine($"Key: {key} Value: {value}");
                    this.ArgumentKVMap[key] = value;
                }
            }

            Console.WriteLine(); // 新行
        }
    }
}
