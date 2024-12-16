using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DBMT_Core
{
    public class DBMTStringUtils
    {

        public static bool ContainsChinese(string input)
        {
            // 使用正则表达式匹配中文字符
            Regex regex = new Regex(@"[\u4e00-\u9fa5]");
            return regex.IsMatch(input);
        }

        public static bool IsHashValue(string HashValue)
        {
            //3Dmigoto的Hash都是8位或16位
            if (HashValue.Length  != 8 && HashValue.Length != 16)
            {
                return false;
            }

            //Hash肯定没中文
            if (ContainsChinese(HashValue))
            {
                return false;
            }

            return true;
        }

    }
}
