using DBMT_Core.Common;
using DBMT_Core;
using DBMT_Core.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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


        public static bool ExtractUnityVS(List<DrawIBItem> DrawIBItemList)
        {
            D3D11GameTypeLv2 d3D11GameTypeLv2 = new D3D11GameTypeLv2(GlobalConfig.CurrentGameName);

            LOG.Info("开始提取:");
            foreach (DrawIBItem drawIBItem in DrawIBItemList)
            {
                string DrawIB = drawIBItem.DrawIB;

                if (DrawIB.Trim() == "")
                {
                    continue;
                }
                else
                {
                    LOG.Info("当前DrawIB: " + DrawIB);
                }
                LOG.NewLine();

                string PointlistIndex = FrameAnalysisLogUtils.Get_PointlistIndex_ByHash(DrawIB);
                LOG.Info("当前识别到的PointlistIndex: " + PointlistIndex);
                if (PointlistIndex == "")
                {
                    LOG.Info("当前识别到的PointlistIndex为空，此DrawIB对应的模型可能为CPU-PreSkinning类型。");
                }
                LOG.NewLine();


                List<string> TrianglelistIndexList = FrameAnalysisLogUtils.Get_DrawCallIndexList_ByHash(DrawIB,false);
                foreach (string TrianglelistIndex in TrianglelistIndexList)
                {
                    LOG.Info("TrianglelistIndex: " + TrianglelistIndex);
                }
                LOG.NewLine();

                bool result = Extract_fee307b98a965c16(DrawIB, d3D11GameTypeLv2, PointlistIndex, TrianglelistIndexList);
                if (!result)
                {
                    return false;
                }
            }


            LOG.Info("提取正常执行完成");
            return true;
        }

    }
}
