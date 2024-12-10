using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBMT.Helper
{
    class TextureHelper
    {
        public static string GetAutoTextureFormat()
        {
            string TextureFormatString = "jpg";
            if (MainConfig.AutoTextureFormat == 0)
            {
                TextureFormatString = "jpg";
            }
            else if (MainConfig.AutoTextureFormat == 1)
            {
                TextureFormatString = "tga";
            }
            else if (MainConfig.AutoTextureFormat == 2)
            {
                TextureFormatString = "png";
            }
            return TextureFormatString;
        }


        public static void ConvertAllTextureFilesToTargetFolder(string SourceFolderPath, string TargetFolderPath)
        {
            if (!Directory.Exists(TargetFolderPath))
            {
                Directory.CreateDirectory(TargetFolderPath);
            }

            string[] filePathArray = Directory.GetFiles(SourceFolderPath);
            foreach (string ddsFilePath in filePathArray)
            {
                //只转换dds格式和png格式贴图
                if (!ddsFilePath.EndsWith(".dds") && !ddsFilePath.EndsWith(".jpg") && !ddsFilePath.EndsWith(".png"))
                {
                    continue;
                }

                string TextureFormatString = GetAutoTextureFormat();
                CommandHelper.ConvertTexture(ddsFilePath, TextureFormatString, TargetFolderPath);

            }
        }

    }
}
