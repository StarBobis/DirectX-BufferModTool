using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBMT_Core;

namespace DBMT
{
    public class PathHelper
    {
        public static string GetCurrentGameBackGroundPicturePath()
        {
            string basePath = Directory.GetCurrentDirectory();

            //设置背景图片
            //默认为各个游戏用户设置的DIY图片
            string imagePath = Path.Combine(basePath, "Assets", GlobalConfig.CurrentGameName + "_DIY.png");

            //如果不存在DIY背景图，则使用默认游戏的背景图
            if (!File.Exists(imagePath))
            {
                imagePath = Path.Combine(basePath, "Assets", GlobalConfig.CurrentGameName + ".png");
            }

            //如果默认游戏的背景图还不存在，则使用主页的背景图
            if (!File.Exists(imagePath))
            {
                imagePath = Path.Combine(basePath, "Assets", "HomePageBackGround.png");
            }
            return imagePath;
        }

        public static string GetAssetsFolderPath()
        {
            return Path.Combine(Directory.GetCurrentDirectory(),"Assets\\");
        }


        public static async Task<List<string>> GetGameDirectoryNameList()
        {
            List<string> directories = new List<string>();

            string CurrentDirectory = Directory.GetCurrentDirectory();
            string GamesPath = Path.Combine(CurrentDirectory, "Games\\");

            if (!Directory.Exists(GamesPath))
            {
                await MessageHelper.Show("Can't find Games folder in your run folder, Initialize Failed. : \n" + GamesPath);
                return directories;
            }

            // 获取所有子目录名称
            directories = Directory.EnumerateDirectories(GamesPath)
                                        .Select(Path.GetFileName)
                                        .Where(name => !string.IsNullOrEmpty(name))
                                        .OrderByDescending(name => name).ToList();
            return directories;
        }

    }
}
