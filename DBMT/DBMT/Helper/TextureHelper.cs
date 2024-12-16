using DBMT_Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBMT
{
    class TextureHelper
    {
        public static string GetAutoTextureFormat()
        {
            return GlobalConfig.TextureCfg.Value.AutoTextureFormat switch
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


        public static async void ConvertTexturesInMod(string ModIniFilePath)
        {
            if (!string.IsNullOrEmpty(ModIniFilePath))
            {
                string ModFolderPath = Path.GetDirectoryName(ModIniFilePath);
                List<string> result = DBMTFileUtils.FindDirectoriesWithImages(ModFolderPath);
                foreach (string TextyreFolder in result)
                {
                    string TargetTexturesFolderPath = TextyreFolder + "/ConvertedTextures/";
                    //MessageBox.Show(TargetTexturesFolderPath);
                    Directory.CreateDirectory(TargetTexturesFolderPath);
                    TextureHelper.ConvertAllTextureFilesToTargetFolder(TextyreFolder, TargetTexturesFolderPath);
                }

                string ModFolderName = Path.GetFileName(ModFolderPath);
                string ModFolderParentPath = Path.GetDirectoryName(ModFolderPath);
                string ModReverseFolderPath = ModFolderParentPath + "\\" + ModFolderName + "-Reverse\\";

                await CommandHelper.ShellOpenFolder(ModReverseFolderPath);
            }
        }

        public static async void ConvertDedupedTexturesToTargetFormat()
        {

            string WorkSpacePath = GlobalConfig.Path_OutputFolder + GlobalConfig.CurrentWorkSpace + "/";
            List<string> DrawIBList = ConfigHelper.GetDrawIBListFromConfig(GlobalConfig.CurrentWorkSpace);
            foreach (string DrawIB in DrawIBList)
            {
                //在这里把所有output目录下的dds转为png格式
                string DedupedTexturesFolderPath = WorkSpacePath + DrawIB + "/DedupedTextures/";
                if (!Directory.Exists(DedupedTexturesFolderPath))
                {
                    await MessageHelper.Show("无法找到DedupedTextures文件夹: " + DedupedTexturesFolderPath);
                    return;
                }

                string TextureFormatString = TextureHelper.GetAutoTextureFormat();
                string DedupedTexturesConvertFolderPath = WorkSpacePath + DrawIB + "/DedupedTextures_" + TextureFormatString + "/";
                TextureHelper.ConvertAllTextureFilesToTargetFolder(DedupedTexturesFolderPath, DedupedTexturesConvertFolderPath);
            }
        }


        public static async void ConvertAutoExtractedTexturesInDrawIBFolderToTargetFormat()
        {
            try
            {
                string WorkSpacePath = GlobalConfig.Path_OutputFolder + GlobalConfig.CurrentWorkSpace + "/";
                List<string> DrawIBList = ConfigHelper.GetDrawIBListFromConfig(GlobalConfig.CurrentWorkSpace);
                foreach (string DrawIB in DrawIBList)
                {
                    string DrawIBPath = WorkSpacePath + DrawIB + "/";
                    if (!Directory.Exists(DrawIBPath))
                    {
                        continue;
                    }
                    //在这里把所有output目录下的dds转为png格式
                    string[] subdirectories = Directory.GetDirectories(WorkSpacePath + DrawIB + "/");
                    foreach (string outputDirectory in subdirectories)
                    {
                        //MessageBox.Show(Path.GetDirectoryName(outputDirectory));

                        if (!Path.GetFileName(outputDirectory).StartsWith("TYPE_"))
                        {
                            continue;
                        }


                        string[] filePathArray = Directory.GetFiles(outputDirectory);
                        foreach (string ddsFilePath in filePathArray)
                        {
                            if (GlobalConfig.TextureCfg.Value.AutoTextureOnlyConvertDiffuseMap)
                            {
                                if (!ddsFilePath.EndsWith("DiffuseMap.dds"))
                                {
                                    continue;
                                }
                            }
                            else if (!ddsFilePath.EndsWith(".dds"))
                            {
                                continue;
                            }

                            string TextureFormatString = TextureHelper.GetAutoTextureFormat();
                            CommandHelper.ConvertTexture(ddsFilePath, TextureFormatString, outputDirectory);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await MessageHelper.Show("Can't convert texture files: " + ex.ToString());
            }


        }

    }
}
