using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBMT_Core
{
    public partial class CoreFunctions
    {

        public static void DetectPointlistDrawIBList()
        {
            List<string> PointlistDrawIBList = [];
            List<string> IBFileNameList = FrameAnalysisDataUtils.FilterFrameAnalysisFile(GlobalConfig.WorkFolder,"-ib=",".buf");
            foreach (string IBFileName in IBFileNameList)
            {
                string DrawIB = IBFileName.Substring(10, 8);
                Debug.WriteLine("正在检测DrawIB:" + DrawIB);
                string PointlistIndex = FrameAnalysisLogUtils.Get_PointlistIndex_ByHash(DrawIB);
                if (PointlistIndex == "")
                {
                    continue;
                }

                List<string> PointlistVB0FileNameList = FrameAnalysisDataUtils.FilterFrameAnalysisFile(GlobalConfig.WorkFolder,PointlistIndex + "-vb0",".txt");
                if (PointlistVB0FileNameList.Count == 0)
                {
                    continue;
                }

                string Topology = DBMTFileUtils.FindMigotoIniAttributeInFile(GlobalConfig.WorkFolder + PointlistVB0FileNameList[0], "topology");
                if (Topology == "pointlist")
                {
                    if (!PointlistDrawIBList.Contains(DrawIB))
                    {
                        PointlistDrawIBList.Add(DrawIB);
                    }
                }

            }


            //保存到Json文件
            JArray jArray = new JArray();
            foreach(string IB in PointlistDrawIBList)
            {
                JObject jObject = DBMTJsonUtils.CreateJObject();
                jObject["DrawIB"] = IB;
                jObject["Alias"] = "";
                jArray.Add(jObject);
                Debug.WriteLine(IB);
            }

            DBMTJsonUtils.SaveJObjectToFile(jArray,GlobalConfig.Path_CurrentWorkSpaceFolder + "Config.json");
        }


        public static void DetectTrianglelistDrawIBList()
        {
            //TODO CTX无法使用此功能
            List<string> TrianglelistDrawIBList = [];
            List<string> IBFileNameList = FrameAnalysisDataUtils.FilterFrameAnalysisFile(GlobalConfig.WorkFolder, "-ib=", ".buf");
            foreach (string IBFileName in IBFileNameList)
            {
                string DrawIB = IBFileName.Substring(10, 8);
                string Index = IBFileName.Substring(0, 6);

                List<string> VB0FileNameList = FrameAnalysisDataUtils.FilterFrameAnalysisFile(GlobalConfig.WorkFolder, Index + "-vb0", ".txt");
                if (VB0FileNameList.Count == 0)
                {
                    continue;
                }

                string Topology = DBMTFileUtils.FindMigotoIniAttributeInFile(GlobalConfig.WorkFolder + VB0FileNameList[0], "topology");
                if (Topology == "trianglelist")
                {
                    if (!TrianglelistDrawIBList.Contains(DrawIB))
                    {
                        TrianglelistDrawIBList.Add(DrawIB);
                    }
                }

            }

            //保存到Json文件
            JArray jArray = new JArray();
            foreach (string IB in TrianglelistDrawIBList)
            {
                JObject jObject = DBMTJsonUtils.CreateJObject();
                jObject["DrawIB"] = IB;
                jObject["Alias"] = "";
                jArray.Add(jObject);
                Debug.WriteLine(IB);
            }

            DBMTJsonUtils.SaveJObjectToFile(jArray, GlobalConfig.Path_CurrentWorkSpaceFolder + "Config.json");
        }


    }
}
