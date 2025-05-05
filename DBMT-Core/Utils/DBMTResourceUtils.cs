using Microsoft.UI.Xaml.Media.Imaging;
using DBMT_Core;
using DBMT_Core.GridViewItems;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace DBMT_Core.Utils
{
    public class DBMTResourceUtils
    {
        /// <summary>
        /// 此方法基于GetGameIconList来获取当前DBMT支持的所有游戏
        /// </summary>
        /// <returns></returns>
        public static List<string> GetGameNameList()
        {
            List<GameIconItem> gameIconItems = GetGameIconItems();

            List<string> GameNameList = [];
            foreach (GameIconItem gameIconItem in gameIconItems)
            {
                GameNameList.Add(gameIconItem.GameName);
            }

            return GameNameList;
        }

        public static List<GameIconItem> GetGameIconItems()
        {
            LOG.Info("GetGameIconItems::Start");

            List<GameIconItem> gameIconItems = [];

            if (!Directory.Exists(GlobalConfig.Path_3DmigotoLoaderFolder))
            {
                return gameIconItems;
            }

            string[] MigotoLoaderFolders = Directory.GetDirectories(GlobalConfig.Path_3DmigotoLoaderFolder);

            foreach (string MigotoLoaderPath in MigotoLoaderFolders)
            {
                LOG.Info("MigotoLoaderPath: " + MigotoLoaderPath);
                if (Directory.Exists(MigotoLoaderPath))
                {
                    string GameName = Path.GetFileName(MigotoLoaderPath);
                    LOG.Info(GameName);

                    string GameIconImage = Path.Combine(GlobalConfig.Path_Base, "Assets\\GameIcon\\", GameName + ".png");
                    if (!File.Exists(GameIconImage))
                    {
                        GameIconImage = Path.Combine(GlobalConfig.Path_Base, "Assets\\GameIcon\\Default.png");
                    }
                    LOG.Info(GameIconImage);

                    string GameBackGroundImage = Path.Combine(GlobalConfig.Path_Base, "Assets\\GameBackground\\", GameName + ".png");
                    if (!File.Exists(GameBackGroundImage))
                    {
                        GameBackGroundImage = Path.Combine(GlobalConfig.Path_Base, "Assets\\GameBackground\\Default.png");
                    }
                    LOG.Info(GameBackGroundImage);

                    gameIconItems.Add(new GameIconItem
                    {
                        GameIconImage = GameIconImage,
                        GameName = GameName,
                        GameBackGroundImage = new BitmapImage(new Uri(GameBackGroundImage))
                    });
                }
            }
            LOG.Info("GetGameIconItems::End");

            return gameIconItems;
        }

    }
}
