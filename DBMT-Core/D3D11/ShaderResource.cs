using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBMT_Core
{
    public class ShaderResource
    {

        public string Index = "";
        public string Resource = "";
        public string View = "";
        public string Hash = "";

        // 构造函数接收日志行作为参数
        public ShaderResource(string logLine)
        {
            // 假设有一个静态方法 Trim 来去除字符串首尾空白字符
            var trimLogLine = logLine.Trim();
            var split = trimLogLine.Split(':');
            this.Index = split[0];

            // 获取并修剪参数部分
            var arguments = split.Length > 1 ? split[1].Trim() : "";
            var argumentSplit = arguments.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var keyValueStr in argumentSplit)
            {
                var kvList = keyValueStr.Split(new[] { '=' }, 2);
                if (kvList.Length == 2)
                {
                    var key = kvList[0].Trim();
                    var value = kvList[1].Trim();

                    switch (key.ToLower())
                    {
                        case "resource":
                            this.Resource = value;
                            break;
                        case "hash":
                            this.Hash = value;
                            break;
                        case "view":
                            this.View = value;
                            break;
                    }
                }
            }
        }
    }
}
