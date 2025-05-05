using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBMT_Core;

namespace DBMT
{
    public class DrawIBHelper
    {

        public static async Task<Dictionary<string, List<string>>> GetBuffHash_VSShaderHashValues_Dict()
        {
            string frameAnalyseFolder = "";
            string[] directories = Directory.GetDirectories(GlobalConfig.Path_LoaderFolder.Replace("/", "\\")); ;
            List<string> frameAnalysisFileList = new List<string>();
            foreach (string directory in directories)
            {
                string directoryName = Path.GetFileName(directory);

                if (directoryName.StartsWith("FrameAnalysis-"))
                {
                    frameAnalysisFileList.Add(directoryName);
                }
            }

            //Get FA numbers to reserve
            frameAnalysisFileList.Sort();
            if (frameAnalysisFileList.Count > 0)
            {
                //排序后是从小到大的，时间上也是如此，我们这里是找最新的一个，所以选-1个
                frameAnalyseFolder = frameAnalysisFileList[frameAnalysisFileList.Count - 1];
            }
            else
            {
                await MessageHelper.Show("未找到FrameAnalysisFolder", "Can't find any FrameAnalysisFolder");
            }

            if (frameAnalyseFolder == "")
            {
                await MessageHelper.Show("当前指定的FrameAnalysisFolder不存在，请重新设置", "Current specified FrameAnalysisFolder didn't exists, please check your setting");
            }

            string frameAnalysisFolderPath = GlobalConfig.Path_LoaderFolder + "\\" + frameAnalyseFolder + "\\";

            Dictionary<string, List<string>> buffHash_vsShaderHashValues_Dict = new Dictionary<string, List<string>>();

            // 获取当前目录下的所有文件
            string[] files = Directory.GetFiles(frameAnalysisFolderPath);
            foreach (string fileName in files)
            {
                if (!fileName.EndsWith(".txt"))
                {
                    continue;
                }

                int vsIndex = fileName.IndexOf("-vs=");
                if (vsIndex != -1)
                {
                    string bufferHash = fileName.Substring(vsIndex - 8, 8);
                    string vsShaderHash = fileName.Substring(vsIndex + 4, 16);

                    List<string> tmpList = new List<string>();
                    if (buffHash_vsShaderHashValues_Dict.ContainsKey(bufferHash))
                    {
                        tmpList = buffHash_vsShaderHashValues_Dict[bufferHash];
                    }
                    tmpList.Add(vsShaderHash);
                    buffHash_vsShaderHashValues_Dict[bufferHash] = tmpList;
                }
                else
                {
                    continue;
                }

            }

            return buffHash_vsShaderHashValues_Dict;
        }

        public static async void GenerateVSCheck(List<string> DrawIBList,string VSCheckIniFileName)
        {
            Dictionary<string, List<string>> buffHash_vsShaderHashValues_Dict = await DrawIBHelper.GetBuffHash_VSShaderHashValues_Dict();

            string outputContent = "";

            List<string> WritedHashList = new List<string>();

            foreach (string DrawIB in DrawIBList)
            {

                if (buffHash_vsShaderHashValues_Dict.ContainsKey(DrawIB))
                {
                    List<string> VSHashList = buffHash_vsShaderHashValues_Dict[DrawIB];
                    foreach (string hash in VSHashList)
                    {
                        if (WritedHashList.Contains(hash))
                        {
                            continue;
                        }
                        WritedHashList.Add(hash);
                        outputContent += "[ShaderOverride_" + hash + "]\r\n";
                        outputContent += "hash = " + hash + "\r\n";
                        outputContent += "if $costume_mods\r\n";
                        outputContent += "  checktextureoverride = ib\r\n";
                        outputContent += "endif\r\n\r\n";
                    }
                }

            }

            if (!File.Exists(GlobalConfig.Path_CurrentWorkSpaceGeneratedModFolder))
            {
                Directory.CreateDirectory(GlobalConfig.Path_CurrentWorkSpaceGeneratedModFolder);
            }

            string VSCheckIniFilePath = Path.Combine(GlobalConfig.Path_CurrentWorkSpaceGeneratedModFolder, VSCheckIniFileName + ".ini");
            File.WriteAllText(VSCheckIniFilePath, outputContent);
        }



        public static async void GenerateSkipIB(List<string> DrawIBList)
        {
            Dictionary<string, List<string>> buffHash_vsShaderHashValues_Dict = await DrawIBHelper.GetBuffHash_VSShaderHashValues_Dict();
            string outputContent = "";
            List<string> WritedHashList = new List<string>();

            foreach (string DrawIB in DrawIBList)
            {
                outputContent = outputContent + "[TextureOverride_IB_" + DrawIB + "]\r\n";
                outputContent = outputContent + "hash = " + DrawIB + "\r\n";
                outputContent = outputContent + "handling = skip\r\n";
                outputContent = outputContent + "\r\n";
            }

            if (!File.Exists(GlobalConfig.Path_CurrentWorkSpaceGeneratedModFolder))
            {
                Directory.CreateDirectory(GlobalConfig.Path_CurrentWorkSpaceGeneratedModFolder);
            }

            string outputPath = Path.Combine(GlobalConfig.Path_CurrentWorkSpaceGeneratedModFolder, "IBSkip.ini");
            File.WriteAllText(outputPath, outputContent);
        }


        

    }
}
