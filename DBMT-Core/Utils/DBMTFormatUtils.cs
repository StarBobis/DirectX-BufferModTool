using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBMT_Core.Utils
{
    public class DBMTFormatUtils
    {
        public static int GetByteWidthFromFormat(string inputFormatStr)
        {
            // 通过统计数字个数来获取宽度
            string formatStr = inputFormatStr.ToLower();
            int totalBytes = 0;

            // 处理通道数
            for (int i = 0; i < formatStr.Length - 1; i++)
            {
                // 根据通道数的类型，假设每个通道都是相同类型
                if (formatStr[i + 1] == '8') // e.g., r8
                {
                    totalBytes += 1;
                }
                else if (i + 2 < formatStr.Length && formatStr[i + 1] == '3' && formatStr[i + 2] == '2') // e.g., r32
                {
                    totalBytes += 4;
                }
                else if (i + 2 < formatStr.Length && formatStr[i + 1] == '1' && formatStr[i + 2] == '6') // e.g., r16
                {
                    totalBytes += 2;
                }
            }

            return totalBytes;
        }
    }
}
