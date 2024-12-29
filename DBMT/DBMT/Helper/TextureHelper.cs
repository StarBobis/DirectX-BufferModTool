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
                    string TextureFormatString = GlobalConfig.AutoTextureFormatSuffix;
                    CommandHelper.ConvertTexture(ddsFilePath, TextureFormatString, TargetFolderPath);
                }
                else if (ddsFilePath.EndsWith(".jpg") || ddsFilePath.EndsWith(".png"))
                {
                    Debug.Write("Copy: " + ddsFilePath + " To: " + TargetFolderPath);
                    File.Copy(ddsFilePath, Path.Combine(TargetFolderPath, Path.GetFileName(ddsFilePath)), true);
                }

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

        public static string GetConvertedTexturesFolderPath(string DrawIB)
        {
            string WorkSpacePath = GlobalConfig.Path_OutputFolder + GlobalConfig.CurrentWorkSpace + "\\";
            string TextureFormatString = GlobalConfig.AutoTextureFormatSuffix;
            string DedupedTexturesConvertFolderPath = WorkSpacePath + DrawIB + "\\DedupedTextures_" + TextureFormatString + "\\";
            return DedupedTexturesConvertFolderPath;
        }

        public static async void ConvertDedupedTexturesToTargetFormat()
        {

            string WorkSpacePath = GlobalConfig.Path_OutputFolder + GlobalConfig.CurrentWorkSpace + "/";
            List<string> DrawIBList = DrawIBConfig.GetDrawIBListFromConfig();
            foreach (string DrawIB in DrawIBList)
            {
                //在这里把所有output目录下的dds转为png格式
                string DedupedTexturesFolderPath = WorkSpacePath + DrawIB + "/DedupedTextures/";
                if (!Directory.Exists(DedupedTexturesFolderPath))
                {
                    await MessageHelper.Show("无法找到DedupedTextures文件夹: " + DedupedTexturesFolderPath);
                    return;
                }

                string DedupedTexturesConvertFolderPath = GetConvertedTexturesFolderPath(DrawIB);
                TextureHelper.ConvertAllTextureFilesToTargetFolder(DedupedTexturesFolderPath, DedupedTexturesConvertFolderPath);
            }
        }


        public static async void ConvertAutoExtractedTexturesInDrawIBFolderToTargetFormat()
        {
            try
            {
                string WorkSpacePath = GlobalConfig.Path_OutputFolder + GlobalConfig.CurrentWorkSpace + "/";
                List<string> DrawIBList = DrawIBConfig.GetDrawIBListFromConfig();
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
                            if (GlobalConfig.GameCfg.Value.AutoTextureOnlyConvertDiffuseMap)
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

                            string TextureFormatString = GlobalConfig.AutoTextureFormatSuffix;
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
