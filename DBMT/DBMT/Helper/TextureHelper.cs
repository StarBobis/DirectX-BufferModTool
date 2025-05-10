using DBMT_Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBMT
{
    class TextureHelper
    {



        public static void ConvertAllTextureFilesToTargetFolder(string SourceFolderPath, string TargetFolderPath)
        {
            Debug.Write("ConvertAllTextureFilesToTargetFolder::");
            if (!Directory.Exists(TargetFolderPath))
            {
                Directory.CreateDirectory(TargetFolderPath);
            }

            string[] filePathArray = Directory.GetFiles(SourceFolderPath);
            foreach (string ddsFilePath in filePathArray)
            {
                //只转换dds格式和png格式贴图
                if (ddsFilePath.EndsWith(".dds"))
                {
                    string TextureFormatString = GlobalConfig.AutoTextureFormat;
                    CommandHelper.ConvertTexture(ddsFilePath, TextureFormatString, TargetFolderPath);
                }
                else if (ddsFilePath.EndsWith(".jpg") || ddsFilePath.EndsWith(".png"))
                {
                    Debug.Write("Copy: " + ddsFilePath + " To: " + TargetFolderPath);
                    File.Copy(ddsFilePath, Path.Combine(TargetFolderPath, Path.GetFileName(ddsFilePath)), true);
                }

            }
        }

        public static void ConvertAllTexturesIntoConvertedTextures(string TargetConvertFolderPath)
        {
            List<string> result = DBMTFileUtils.FindDirectoriesWithImages(TargetConvertFolderPath);
            foreach (string TextureFolder in result)
            {
                string TargetTexturesFolderPath = TextureFolder + "/ConvertedTextures/";
                //MessageBox.Show(TargetTexturesFolderPath);
                Directory.CreateDirectory(TargetTexturesFolderPath);
                ConvertAllTextureFilesToTargetFolder(TextureFolder, TargetTexturesFolderPath);
            }
        }

        public static async void ConvertTexturesInMod(string ModIniFilePath)
        {
            if (!string.IsNullOrEmpty(ModIniFilePath))
            {
                string ModFolderPath = Path.GetDirectoryName(ModIniFilePath);
                ConvertAllTexturesIntoConvertedTextures(ModFolderPath);
                

                string ModFolderName = Path.GetFileName(ModFolderPath);
                string ModFolderParentPath = Path.GetDirectoryName(ModFolderPath);
                string ModReverseFolderPath = ModFolderParentPath + "\\" + ModFolderName + "-Reverse\\";

                await CommandHelper.ShellOpenFolder(ModReverseFolderPath);
            }
        }



        public static async void ConvertDedupedTexturesToTargetFormat()
        {

            List<string> DrawIBList = DrawIBConfig.GetDrawIBListFromConfig();
            foreach (string DrawIB in DrawIBList)
            {
                //在这里把所有output目录下的dds转为png格式
                string DedupedTexturesFolderPath = Path.Combine(GlobalConfig.Path_CurrentWorkSpaceFolder ,DrawIB + "\\DedupedTextures\\");
                if (!Directory.Exists(DedupedTexturesFolderPath))
                {
                    await MessageHelper.Show("无法找到DedupedTextures文件夹: " + DedupedTexturesFolderPath);
                    return;
                }

                string DedupedTexturesConvertFolderPath = TextureConfig.GetConvertedTexturesFolderPath(DrawIB);
                TextureHelper.ConvertAllTextureFilesToTargetFolder(DedupedTexturesFolderPath, DedupedTexturesConvertFolderPath);
            }
        }


        public static async Task ConvertAutoExtractedTexturesInDrawIBFolderToTargetFormat()
        {
            try
            {
                List<string> DrawIBList = DrawIBConfig.GetDrawIBListFromConfig();
                foreach (string DrawIB in DrawIBList)
                {
                    string DrawIBPath = Path.Combine(GlobalConfig.Path_CurrentWorkSpaceFolder,DrawIB + "\\");
                    if (!Directory.Exists(DrawIBPath))
                    {
                        continue;
                    }
                    string[] subdirectories = Directory.GetDirectories(DrawIBPath);
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
                            if (GlobalConfig.AutoTextureOnlyConvertDiffuseMap)
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

                            string TextureFormatString = GlobalConfig.AutoTextureFormat;
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
