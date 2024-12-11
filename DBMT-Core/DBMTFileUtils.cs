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

    }
}
