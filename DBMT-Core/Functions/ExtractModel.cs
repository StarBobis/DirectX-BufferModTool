using DBMT_Core.Games;
using DBMT_Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBMT_Core
{
    public partial class CoreFunctions
    {

        public static bool ExtractModel(List<DrawIBItem> DrawIBItemList)
        {

            //提取DedupedTextures和RenderTextures方便Mod制作时使用。
            bool RunResult = false;
            //HSR重写渲染管线和Shader，很特殊
            if (GlobalConfig.CurrentGameName == "HSR")
            {
                LOG.Initialize();
                try
                {
                    CoreFunctions.ExtractDedupedTextures();
                    CoreFunctions.ExtractRenderTextures();
                    RunResult = HonkaiStarRail.ExtractHSR32(DrawIBItemList);
                }
                catch (Exception ex)
                {
                    LOG.Error(ex.ToString());
                    RunResult = false;
                }
                LOG.SaveFile();
            }
            //Unreal引擎，且使用形态键技术，复杂度比Unity高
            else if (GlobalConfig.CurrentGameName == "WWMI")
            {
                LOG.Initialize();
                try
                {
                    CoreFunctions.ExtractDedupedTextures();
                    CoreFunctions.ExtractRenderTextures();
                    RunResult = WutheringWaves.ExtractWWMI(DrawIBItemList);

                }
                catch (Exception ex)
                {
                    LOG.Error(ex.ToString());
                    RunResult = false;
                }
                LOG.SaveFile();
            }
            else if (GlobalConfig.CurrentGameName == "YYSLS" )
            {
                LOG.Initialize();
                try
                {
                    CoreFunctions.ExtractDedupedTextures();
                    CoreFunctions.ExtractRenderTextures();

                    RunResult = WhereWindsMeet.ExtractModel(DrawIBItemList);
                }
                catch (Exception ex)
                {
                    LOG.Error(ex.ToString());
                    RunResult = false;
                }
                LOG.SaveFile();
            }
            //CTX类型比较特殊
            else if (GlobalConfig.CurrentGameName == "IdentityV")
            {
                LOG.Initialize();
                try
                {
                    CoreFunctions.ExtractDedupedTextures();
                    CoreFunctions.ExtractRenderTextures();
                    RunResult = IdentityV.ExtractCTX(DrawIBItemList);
                }
                catch (Exception ex)
                {
                    LOG.Error(ex.ToString());
                    RunResult = false;
                }
                LOG.SaveFile();
            }
            else if (GlobalConfig.CurrentGameName == "HOK")
            {
                LOG.Initialize();
                try
                {
                    CoreFunctions.ExtractDedupedTextures();
                    CoreFunctions.ExtractRenderTextures();
                    RunResult = HonorOfKings.ExtractModel(DrawIBItemList);
                }
                catch (Exception ex)
                {
                    LOG.Error(ex.ToString());
                    RunResult = false;
                }
                LOG.SaveFile();
            }
            else if (GlobalConfig.CurrentGameName == "ZZZ")
            {
                LOG.Initialize();
                try
                {
                    CoreFunctions.ExtractDedupedTextures();
                    CoreFunctions.ExtractRenderTextures();
                    RunResult = ZenlessZoneZero.ExtractModel(DrawIBItemList);
                }
                catch (Exception ex)
                {
                    LOG.Error(ex.ToString());
                    RunResult = false;
                }
                LOG.SaveFile();
            }
            else if (GlobalConfig.CurrentGameName == "GF2")
            {
                LOG.Initialize();
                try
                {
                    CoreFunctions.ExtractDedupedTextures();
                    CoreFunctions.ExtractRenderTextures();
                    RunResult = GirlsFrontline2.ExtractModel(DrawIBItemList);
                }
                catch (Exception ex)
                {
                    LOG.Error(ex.ToString());
                    RunResult = false;
                }
                LOG.SaveFile();
            }
            else if (GlobalConfig.CurrentGameName == "AILIMIT")
            {
                LOG.Initialize();
                try
                {
                    CoreFunctions.ExtractDedupedTextures();
                    CoreFunctions.ExtractRenderTextures();
                    RunResult = AILimit.ExtractUnityVS(DrawIBItemList);
                }
                catch (Exception ex)
                {
                    LOG.Error(ex.ToString());
                    RunResult = false;
                }
                LOG.SaveFile();
            }
            else
            {
                LOG.Initialize();
                try
                {
                    CoreFunctions.ExtractDedupedTextures();
                    CoreFunctions.ExtractRenderTextures();
                    RunResult = UnityGames.ExtractUnityVS(DrawIBItemList);
                }
                catch (Exception ex)
                {
                    LOG.Error(ex.ToString());
                    RunResult = false;
                }
                LOG.SaveFile();
            }

            return RunResult;
        }
    }
}
