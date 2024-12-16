using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBMT_Core
{
    public class DBMTFileUtils
    {

        public static string[] ReadWorkSpaceNameList(string WorkSpaceFolderPath)
        {
            List<string> WorkSpaceNameList = new List<string>();

            if (Directory.Exists(WorkSpaceFolderPath))
            {
                string[] WorkSpaceList = Directory.GetDirectories(WorkSpaceFolderPath);
                foreach (string WorkSpacePath in WorkSpaceList)
                {
                    string WorkSpaceName = Path.GetFileName(WorkSpacePath);
                    WorkSpaceNameList.Add(WorkSpaceName);
                }
            }

            return WorkSpaceNameList.ToArray();
        }


        private static void SearchDirectory(string currentDirectory, List<string> result)
        {
            // 获取当前目录下的所有文件
            string[] files = Directory.GetFiles(currentDirectory);

            // 检查是否有.dds或.png文件
            foreach (string file in files)
            {
                string extension = Path.GetExtension(file).ToLower();
                if (extension == ".dds" || extension == ".png")
                {
                    // 如果找到了目标文件，将目录加入结果列表
                    if (!result.Contains(currentDirectory))
                    {
                        result.Add(currentDirectory);
                    }
                    break; // 找到了一种类型的文件就不再继续查找当前目录中的其他文件
                }
            }

            // 递归搜索子目录
            string[] subdirectories = Directory.GetDirectories(currentDirectory);
            foreach (string subdirectory in subdirectories)
            {
                SearchDirectory(subdirectory, result);
            }

        }


        public static List<string> FindDirectoriesWithImages(string rootDirectory)
        {
            if (string.IsNullOrEmpty(rootDirectory) || !Directory.Exists(rootDirectory))
            {
                throw new ArgumentException("The specified directory does not exist or is invalid.", nameof(rootDirectory));
            }

            var directoriesWithImages = new List<string>();
            SearchDirectory(rootDirectory, directoriesWithImages);
            return directoriesWithImages;
        }


    }
}
