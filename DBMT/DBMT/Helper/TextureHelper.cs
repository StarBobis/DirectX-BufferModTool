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
            return MainConfig.GetConfig<int>("AutoTextureFormat") switch
            {
                0 => "jpg",
                1 => "tga",
                2 => "png",
                _ => "jpg",
            };
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
