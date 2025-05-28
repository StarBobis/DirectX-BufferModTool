using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBMT_Core
{
    public static class D3dxIniConfig
    {

        public static string ReadAttributeFromD3DXIni(string d3dxini_path, string AttributeName)
        {
            //string d3dxini_path = ConfigHelper.GetD3DXIniPath();
            if (File.Exists(d3dxini_path))
            {
                string[] lines = File.ReadAllLines(d3dxini_path);
                foreach (string line in lines)
                {
                    string trim_lower_line = line.Trim().ToLower();
                    if (trim_lower_line.StartsWith(AttributeName) && trim_lower_line.Contains('='))
                    {
                        string[] splits = line.Split('=');

                        string arg_name = splits[0].Trim();
                        if (arg_name != AttributeName)
                        {
                            continue;
                        }

                        string target_path = splits[1];
                        return target_path;
                    }

                }
            }
            return "";
        }

        public static void SaveAttributeToD3DXIni(string d3dxini_path, string SectionName, string AttributeName, string AttributeValue)
        {

            if (File.Exists(d3dxini_path))
            {
                string OriginalAttributeValue = ReadAttributeFromD3DXIni(d3dxini_path, AttributeName);
                //只有存在此属性时，写入才有意义，否则等于白写一遍原内容
                if (OriginalAttributeValue.Trim() != "")
                {
                    List<string> newLines = new List<string>();
                    string[] lines = File.ReadAllLines(d3dxini_path);
                    foreach (string line in lines)
                    {
                        string trim_lower_line = line.Trim().ToLower();
                        if (trim_lower_line.StartsWith(AttributeName) && trim_lower_line.Contains('='))
                        {
                            string TargetPath = AttributeName + " = " + AttributeValue;
                            if (AttributeValue.Trim() == "")
                            {
                                TargetPath = "; " + TargetPath;
                            }
                            newLines.Add(TargetPath);
                        }
                        else
                        {
                            newLines.Add(line);
                        }
                    }
                    File.WriteAllLines(d3dxini_path, newLines);
                }
                else
                {
                    //如果不存在此属性，则写到对应SectionName下面
                    List<string> newLines = new List<string>();
                    string[] lines = File.ReadAllLines(d3dxini_path);
                    foreach (string line in lines)
                    {
                        string trim_lower_line = line.Trim().ToLower();
                        if (trim_lower_line.StartsWith(SectionName.Trim().ToLower()))
                        {
                            newLines.Add(line);
                            string TargetPath = AttributeName + " = " + AttributeValue;
                            if (AttributeValue.Trim() == "")
                            {
                                TargetPath = "; " + TargetPath;
                            }
                            newLines.Add(TargetPath);
                        }
                        else
                        {
                            newLines.Add(line);
                        }
                    }
                    File.WriteAllLines(d3dxini_path, newLines);
                }
            }
        }
    }
}
