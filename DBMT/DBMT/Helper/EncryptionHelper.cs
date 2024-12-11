using DBMT_Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using Windows.Storage;

namespace DBMT.Helper
{
    public class EncryptionHelper
    {

        public static async Task<string> Obfuscate_ModFileName(string obfusVersion = "Dev")
        {
            FileOpenPicker picker = CommandHelper.Get_FileOpenPicker(".ini");
            StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                string readIniPath = file.Path;
                if (DBMTStringUtils.ContainsChinese(readIniPath))
                {
                    await MessageHelper.Show("目标路径中不能含有中文字符", "Target Path Can't Contains Chinese.");
                    return "";
                }

                if (string.IsNullOrEmpty(readIniPath))
                {
                    await MessageHelper.Show("Please select a correct ini file.");
                    return "";
                }
                string readDirectoryPath = Path.GetDirectoryName(readIniPath);

                //Read every line and obfuscate every Resource section.
                //need a dict to store the old filename and the new filename.
                string[] readIniLines = File.ReadAllLines(readIniPath);
                List<string> newIniLines = new List<string>();
                Dictionary<string, string> fileNameUuidDict = new Dictionary<string, string>();
                foreach (string iniLine in readIniLines)
                {
                    string lowerIniLine = iniLine.ToLower();
                    if (lowerIniLine.StartsWith("filename"))
                    {
                        int firstEqualSignIndex = iniLine.IndexOf("=");
                        string valSection = iniLine.Substring(firstEqualSignIndex);
                        string resourceFileName = valSection.Substring(1).Trim();
                        //generate a uuid to replace this filename
                        string randomUUID = Guid.NewGuid().ToString();

                        //因为不能有重复键
                        if (!fileNameUuidDict.ContainsKey(resourceFileName))
                        {
                            fileNameUuidDict.Add(resourceFileName, randomUUID);
                        }
                        else
                        {
                            randomUUID = fileNameUuidDict[resourceFileName];
                        }

                        string newIniLine = "";
                        if (resourceFileName.EndsWith(".dds"))
                        {
                            if (obfusVersion == "Dev")
                            {
                                newIniLine = iniLine.Replace(resourceFileName, randomUUID + ".dds");
                            }
                            else
                            {
                                newIniLine = iniLine.Replace(resourceFileName, randomUUID + ".bundle");
                            }
                        }
                        else if (resourceFileName.EndsWith(".png"))
                        {
                            newIniLine = iniLine.Replace(resourceFileName, randomUUID + ".png");
                        }
                        else
                        {
                            newIniLine = iniLine.Replace(resourceFileName, randomUUID + ".assets");
                        }
                        newIniLines.Add(newIniLine);
                    }
                    else
                    {
                        newIniLines.Add(iniLine);

                    }
                }


                string parentDirectory = Directory.GetParent(readDirectoryPath).FullName;
                string ModFolderName = Path.GetFileName(readDirectoryPath);

                string newOutputDirectory = parentDirectory + "\\" + ModFolderName + "-Release\\";

                Directory.CreateDirectory(newOutputDirectory);

                //Create a new ini file.
                string newIniFilePath = newOutputDirectory + Guid.NewGuid().ToString() + ".ini";
                File.WriteAllLines(newIniFilePath, newIniLines);

                foreach (KeyValuePair<string, string> pair in fileNameUuidDict)
                {
                    string key = pair.Key;
                    string value = pair.Value;

                    string oldResourceFilePath = readDirectoryPath + "\\" + key;


                    string newResourceFilePath = "";
                    if (key.EndsWith(".dds"))
                    {
                        if (obfusVersion == "Dev")
                        {
                            newResourceFilePath = newOutputDirectory + value + ".dds";
                        }
                        else
                        {
                            newResourceFilePath = newOutputDirectory + value + ".bundle";
                        }
                    }
                    else if (key.EndsWith(".png"))
                    {
                        newResourceFilePath = newOutputDirectory + value + ".png";
                    }
                    else
                    {
                        newResourceFilePath = newOutputDirectory + value + ".assets";
                    }

                    if (File.Exists(oldResourceFilePath))
                    {
                        File.Copy(oldResourceFilePath, newResourceFilePath, true);
                    }

                }

                await MessageHelper.Show("混淆成功", "Obfuscated success.");

                return newIniFilePath;

            }


            return "";
        }

    }
}
