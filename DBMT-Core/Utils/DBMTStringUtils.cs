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
                // ��������������ַ������� "!S!=" ʱ
                //���磺
                //000061-ps-t7=!S!=ab2cbb0c-vs=479e531b67d3e9f3-ps=92139b61ff840c7b.dds
                //�����ȡ��posλ��Ϊ!S!= ǰ��λ������Ҫ+4
                //����Ϊʲô��������ټ�1�أ����շ���!S!=��Ȼ��5���ַ�
                //�������ʹ���ַ�����.size()������ȡ���ֲ����ˣ��ֱ�֮ǰ��һ����������Size��Ӱ�������=
                //��������ֻ�ܵ����������������
                int pos = inputMigotoFileName.IndexOf("!S!=");
                if (pos != -1 && pos + 4 + 1 + 8 <= inputMigotoFileName.Length)
                {
                    result = inputMigotoFileName.Substring(pos + 5, 8); // ע�������� "+5" ��Ϊ "!S!=" �ĳ����� 5
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
                Debug.WriteLine(PixelSlot);

                PixelSlot = PixelSlot.Substring(0,PixelSlot.LastIndexOf("-"));
                Debug.WriteLine(PixelSlot);
            }
            else
            {
                Debug.WriteLine(PixelSlot);
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
            // ʹ��������ʽƥ�������ַ�
            Regex regex = new Regex(@"[\u4e00-\u9fa5]");
            return regex.IsMatch(input);
        }

        public static bool IsHashValue(string HashValue)
        {
            //3Dmigoto��Hash����8λ��16λ
            if (HashValue.Length  != 8 && HashValue.Length != 16)
            {
                return false;
            }

            //Hash�϶�û����
            if (ContainsChinese(HashValue))
            {
                return false;
            }

            return true;
        }

    }
}
