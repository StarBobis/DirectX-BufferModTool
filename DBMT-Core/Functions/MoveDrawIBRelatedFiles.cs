using DBMT_Core.Common;
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


        public static void MoveDrawIBRelatedFiles(List<string> DrawIBList,string IBRelatedFolderName)
        {
            //TODO 这个CTX也用不了

            string OutputDrawIBRelatedFolder = GlobalConfig.Path_CurrentWorkSpaceFolder + IBRelatedFolderName + "\\FrameAnalysis-2028-08-28-666666\\";
            Directory.CreateDirectory(OutputDrawIBRelatedFolder);

            string DedupedFolderPath = OutputDrawIBRelatedFolder + "deduped\\";
            Directory.CreateDirectory(DedupedFolderPath);

            //DBMTFileUtils.CopyDirectory(GlobalConfig.WorkFolder + "deduped\\", DedupedFolderPath);

            List<string> CopyDedupedFiles = [];

            //获取所有的PointlistIndex
            foreach (string DrawIB in DrawIBList)
            {
                string PointlistIndex = FrameAnalysisLogUtils.Get_PointlistIndex_ByHash(DrawIB);
                if (PointlistIndex == "")
                {
                    continue;
                }

                FrameAnalysisInfo FAInfo = new FrameAnalysisInfo(DrawIB);

                List<string> PointlistRelatedFiles = FrameAnalysisDataUtils.FilterFrameAnalysisFile(FAInfo.FolderPath, PointlistIndex, "");
                foreach (string PointlistFileName in PointlistRelatedFiles)
                {
                    CopyDedupedFiles.Add(PointlistFileName);
                    File.Copy(GlobalConfig.WorkFolder + PointlistFileName, OutputDrawIBRelatedFolder + PointlistFileName,true);
                }
            }

            //移动所有DrawIB相关的文件，是不是Trianglelist无所谓了
            List<string> DrawIBRelatedIndexList = [];
            foreach (string DrawIB in DrawIBList)
            {
                List<string> IndexList = FrameAnalysisLogUtils.Get_DrawCallIndexList_ByHash(DrawIB,false);
                foreach (string Index in IndexList)
                {
                    if (!DrawIBRelatedIndexList.Contains(Index))
                    {
                        DrawIBRelatedIndexList.Add(Index);
                    }
                }
            }

            foreach (string DrawIBIndex in DrawIBRelatedIndexList)
            {
                List<string> DrawIBRelatedFiles = FrameAnalysisDataUtils.FilterFrameAnalysisFile(GlobalConfig.WorkFolder,DrawIBIndex, "");
                foreach (string DrawIBIndexFileName in DrawIBRelatedFiles)
                {
                    CopyDedupedFiles.Add(DrawIBIndexFileName);
                    File.Copy(GlobalConfig.WorkFolder + DrawIBIndexFileName, OutputDrawIBRelatedFolder + DrawIBIndexFileName,true);
                }
            }

            foreach (string FileName in CopyDedupedFiles)
            {
                string DedupedFilePath = FrameAnalysisLogUtils.Get_DedupedFilePath(FileName);
                string DedupedFileName = FrameAnalysisLogUtils.Get_DedupedFileName(FileName);
                File.Copy(GlobalConfig.WorkFolder + FileName, DedupedFolderPath + DedupedFileName, true);
            }


            if (File.Exists(GlobalConfig.WorkFolder + "log.txt"))
            {
                File.Copy(GlobalConfig.WorkFolder + "log.txt", OutputDrawIBRelatedFolder + "log.txt",true);
            }

            if (File.Exists(GlobalConfig.WorkFolder + "ShaderUsage.txt"))
            {
                File.Copy(GlobalConfig.WorkFolder + "ShaderUsage.txt", OutputDrawIBRelatedFolder + "ShaderUsage.txt",true);
            }
        }

    }
}
