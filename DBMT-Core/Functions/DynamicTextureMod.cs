using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBMT_Core
{
    public partial class CoreFunctions
    {

        public static void GenerateDynamicTextureMod(string DynamicTextureFolderPath, string TexturePrefix, string TextureHash,string TextureSuffix)
        {

            static string AddLeadingZeros(int a, int b)
            {
                // 获取数字b的长度（位数）
                int lengthOfB = b.ToString().Length;

                // 使用ToString方法和"D"格式说明符为a添加前导零
                return a.ToString($"D{lengthOfB}");
            }
            string ConfigIniPath = DynamicTextureFolderPath + "Config.ini";
            if (File.Exists(ConfigIniPath))
            {
                File.Delete(ConfigIniPath);
            }

            string[] TextureFileArray = Directory.GetFiles(DynamicTextureFolderPath);
            int TextureFileNumber = TextureFileArray.Length - 1;

            List<string> IniLineList = [];
            IniLineList.Add("[Constants]");
            IniLineList.Add("global $framevar = 0");
            IniLineList.Add("global $active");
            IniLineList.Add("global $fpsvar = 0");
            IniLineList.Add("");

            IniLineList.Add("[Present]");
            IniLineList.Add("post $active = 0");
            IniLineList.Add("");

            IniLineList.Add("if $active == 1 && $fpsvar < 60");
            IniLineList.Add("  $fpsvar = $fpsvar + 6");
            IniLineList.Add("endif");
            IniLineList.Add("");

            IniLineList.Add("if $fpsvar >= 60");
            IniLineList.Add("  $fpsvar = $fpsvar - 60");
            IniLineList.Add("  $framevar = $framevar + 1");
            IniLineList.Add("endif");
            IniLineList.Add("");

            IniLineList.Add(" if $framevar > " + TextureFileNumber.ToString());
            IniLineList.Add("  $framevar = 0");
            IniLineList.Add("endif");
            IniLineList.Add("");

            IniLineList.Add("[TextureOverride_" + TexturePrefix + "]");
            IniLineList.Add("hash = " + TextureHash);
            IniLineList.Add("run = CommandlistFrame");
            IniLineList.Add("$active = 1");
            IniLineList.Add("");

            IniLineList.Add("[CommandlistFrame]");
            for (int i = 0; i <= TextureFileNumber; i++)
            {
                string CurrentSuffix = AddLeadingZeros(i, TextureFileNumber);
                if (i == 0)
                {
                    IniLineList.Add("if $framevar == " + CurrentSuffix);
                }
                else 
                {
                    IniLineList.Add("else if $framevar == " + CurrentSuffix);
                }

                IniLineList.Add("  this = Resource_" + TexturePrefix + "_" + CurrentSuffix);


                if (i == TextureFileNumber)
                {
                    IniLineList.Add("endif");
                }
            }
            IniLineList.Add("");


            
            for (int i = 0; i <= TextureFileNumber; i++)
            {
                string CurrentSuffix = AddLeadingZeros(i, TextureFileNumber);
                IniLineList.Add("[Resource_" +TexturePrefix + "_" + CurrentSuffix + "]");
                IniLineList.Add("filename = " + TexturePrefix + CurrentSuffix + TextureSuffix);
                IniLineList.Add("");

            }

            File.WriteAllLines(DynamicTextureFolderPath + "Config.ini", IniLineList);
        }

    }
}
