using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBMT.Helper
{
    public class DrawIBHelper
    {

        public static async Task<Dictionary<string, List<string>>> GetBuffHash_VSShaderHashValues_Dict()
        {
            string frameAnalyseFolder = "";
            string[] directories = Directory.GetDirectories(MainConfig.Path_LoaderFolder.Replace("/", "\\")); ;
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

            string frameAnalysisFolderPath = MainConfig.Path_LoaderFolder + frameAnalyseFolder;

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



    }
}
