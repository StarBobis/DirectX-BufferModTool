using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBMT_Core
{
    public class DrawIBConfig
    {

        public static List<string> GetDrawIBListFromConfig(string WorkSpaceName)
        {
            List<string> drawIBListValues = new List<string>();

            string Configpath = GlobalConfig.Path_OutputFolder + WorkSpaceName + "\\Config.json";
            if (File.Exists(Configpath))
            {
                //切换到对应配置
                string jsonData = File.ReadAllText(Configpath);
                JArray DrawIBListJArray = JArray.Parse(jsonData);

                foreach (JObject jkobj in DrawIBListJArray)
                {
                    string DrawIB = (string)jkobj["DrawIB"];
                    drawIBListValues.Add(DrawIB);
                }
            }

            return drawIBListValues;
        }
    }
}
