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
            //LOG.Info("GetGameIconItems::Start");

            List<GameIconItem> gameIconItems = [];

            if (!Directory.Exists(GlobalConfig.Path_AssetsGamesFolder))
            {
                return gameIconItems;
            }

            string[] GamesFolderList = Directory.GetDirectories(GlobalConfig.Path_AssetsGamesFolder);

            foreach (string GameFolderPath in GamesFolderList)
            {
                //LOG.Info("GameFolder: " + GameFolderPath);
                if (Directory.Exists(GameFolderPath))
                {
                    string GameName = Path.GetFileName(GameFolderPath);
                    //LOG.Info(GameName);

                    string GameIconImage = Path.Combine(GlobalConfig.Path_AssetsGamesFolder,GameName + "\\Icon.png");
                    if (!File.Exists(GameIconImage))
                    {
                        GameIconImage = Path.Combine(GlobalConfig.Path_AssetsGamesFolder, "DefaultIcon.png");
                    }
                    //LOG.Info(GameIconImage);

                    string GameBackGroundImage = Path.Combine(GlobalConfig.Path_AssetsGamesFolder, GameName + "\\Background.png");
                    if (!File.Exists(GameBackGroundImage))
                    {
                        GameBackGroundImage = Path.Combine(GlobalConfig.Path_AssetsGamesFolder, "DefaultBackground.png");
                    }
                    //LOG.Info(GameBackGroundImage);

                    gameIconItems.Add(new GameIconItem
                    {
                        GameIconImage = GameIconImage,
                        GameName = GameName,
                        GameBackGroundImage = new BitmapImage(new Uri(GameBackGroundImage))
                    });
                }
            }
            //LOG.Info("GetGameIconItems::End");

            return gameIconItems;
        }

    }
}
