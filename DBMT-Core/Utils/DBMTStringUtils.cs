using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public static string GetFileHashFromFileName(string inputMigotoFileName)
        {
            string result = "";

            if (!inputMigotoFileName.Contains("!S!"))
            {
                int pos = inputMigotoFileName.IndexOf('=');
                if (pos != -1 && pos + 1 + 8 <= inputMigotoFileName.Length)
                {
                    result = inputMigotoFileName.Substring(pos + 1, 8);
                }
            }
            else
            {
                // 特殊情况处理：当字符串包含 "!S!=" 时
                //例如：
                //000061-ps-t7=!S!=ab2cbb0c-vs=479e531b67d3e9f3-ps=92139b61ff840c7b.dds
                //这里获取的pos位置为!S!= 前的位置所以要+4
                //但是为什么这里必须再加1呢？最终发现!S!=居然是5个字符
                //但是如果使用字符串的.size()函数获取的又不对了，又比之前少一个，而且用Size会影响上面的=
                //所以这里只能当成特殊情况来处理

                string searchStr = "!S!=";
                int pos = inputMigotoFileName.IndexOf("!S!=");
                if (pos != -1 && pos + 4 + 1 + 8 <= inputMigotoFileName.Length)
                {
                    result = inputMigotoFileName.Substring(pos + searchStr.Length, 8);
                }
            }

            return result;
        }


        public static string GetPSHashFromFileName(string input)
        {
            string result = string.Empty;
            int pos = input.IndexOf("-ps=", StringComparison.Ordinal);
            if (pos != -1 && pos + 4 + 16 <= input.Length)
            {
                result = input.Substring(pos + 4, 16);
            }
            return result;
        }

        public static string GetVSHashFromFileName(string input)
        {
            string result = string.Empty;
            int pos = input.IndexOf("-vs=", StringComparison.Ordinal);
            if (pos != -1 && pos + 4 + 16 <= input.Length)
            {
                result = input.Substring(pos + 4, 16);
            }
            return result;
        }

        public static string GetPixelSlotFromTextureFileName(string TextureFileName)
        {
            int start_pos = TextureFileName.IndexOf("-");
            int end_pos = TextureFileName.IndexOf("=");
            string PixelSlot = TextureFileName.Substring(start_pos + 1,end_pos - start_pos - 1);

            //ps-t3-vs=
            if (PixelSlot.Contains("-vs"))
            {
                //Debug.WriteLine(PixelSlot);

                PixelSlot = PixelSlot.Substring(0,PixelSlot.LastIndexOf("-"));
                //Debug.WriteLine(PixelSlot);
            }
            else
            {
                //Debug.WriteLine(PixelSlot);
            }

            return PixelSlot;
        }

        public static int GetPixelSlotNumberFromTextureFileName(string TextureFileName)
        {
            string PixelSlot = GetPixelSlotFromTextureFileName(TextureFileName);
            string number = PixelSlot.Substring(PixelSlot.IndexOf("-t") + 2);
            return int.Parse(number);
        }

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
