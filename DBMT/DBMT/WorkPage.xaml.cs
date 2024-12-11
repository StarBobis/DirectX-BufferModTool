using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using DBMT_Core;
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using DBMT.Helper;
using Windows.Storage.Pickers;
using Windows.Storage;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DBMT
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class WorkPage : Page
    {
        public WorkPage()
        {
            this.InitializeComponent();

            if (MainConfig.CurrentGameName == "")
            {
                MainConfig.ReadCurrentGameFromMainJson();
                MainConfig.SetCurrentGame(MainConfig.CurrentGameName);
            }

            string WorkSpaceName = MainConfig.ReadCurrentWorkSpaceFromMainJson();
            InitializeWorkSpace(MainConfig.CurrentWorkSpace);
        }

        private void ComboBoxWorkSpaceSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBoxWorkSpaceSelection.Items.Contains(ComboBoxWorkSpaceSelection.Text))
            {
                MainConfig.CurrentWorkSpace = ComboBoxWorkSpaceSelection.Text;
                MainConfig.SaveCurrentWorkSpaceToMainJson();
                ReadDrawIBListFromWorkSpace();
            }
        }

        void ReadDrawIBListFromWorkSpace()
        {
            string DrawIBListString = "";
            string Configpath = MainConfig.Path_OutputFolder + MainConfig.CurrentWorkSpace + "\\Config.json";
            if (File.Exists(Configpath))
            {
                
                //切换到对应配置
                string jsonData = File.ReadAllText(Configpath);
                JObject jobj = JObject.Parse(jsonData);
                // Access the DrawIBList property and convert it to a List<string>
                JArray drawIBList = (JArray)jobj["DrawIBList"];
                List<string> drawIBListValues = drawIBList.ToObject<List<string>>();

                foreach (string drawib in drawIBListValues)
                {
                    DrawIBListString = DrawIBListString + drawib + ",";
                }

                if (!string.IsNullOrEmpty(DrawIBListString) && DrawIBListString.Length > 1)
                {
                    // 移除最后一个字符
                    DrawIBListString = DrawIBListString.Substring(0, DrawIBListString.Length - 1);
                }
            }
            TextBoxDrawIBList.Text = DrawIBListString;
        }

        public void InitializeWorkSpace(string WorkSpaceName = "")
        {
            ComboBoxWorkSpaceSelection.Items.Clear();
            string[] WorkSpaceNameList = DBMTFileUtils.ReadWorkSpaceNameList(MainConfig.Path_OutputFolder);
            foreach (string WorkSpaceNameItem in WorkSpaceNameList)
            {
                ComboBoxWorkSpaceSelection.Items.Add(WorkSpaceNameItem);
            }

            if (ComboBoxWorkSpaceSelection.Items.Count >= 1)
            {
                if (WorkSpaceName == "")
                {
                    ComboBoxWorkSpaceSelection.SelectedIndex = 0;

                }
                else
                {
                    ComboBoxWorkSpaceSelection.SelectedItem = WorkSpaceName;
                }
            }

            ReadDrawIBListFromWorkSpace();
        }


        public async void CreateWorkSpaceFolder(object sender, RoutedEventArgs e)
        {
            if (ComboBoxWorkSpaceSelection.Text.Trim() == "")
            {
                await MessageHelper.Show("工作空间名称不能为空");
                return;
            }

            if (!ComboBoxWorkSpaceSelection.Items.Contains(ComboBoxWorkSpaceSelection.Text))
            {
                ////如果包含了此命名空间，就不创建文件夹，否则就创建
                Directory.CreateDirectory(MainConfig.Path_OutputFolder + ComboBoxWorkSpaceSelection.Text);

                await MessageHelper.Show("工作空间创建成功");

                InitializeWorkSpace(ComboBoxWorkSpaceSelection.Text);
            }
            else
            {
                await MessageHelper.Show("当前工作空间已存在,无法重复创建");
            }
        }

        public async void CleanCurrentWorkSpaceFile(object sender, RoutedEventArgs e)
        {
            string WorkSpaceFolderPath = MainConfig.Path_OutputFolder + ComboBoxWorkSpaceSelection.Text;
            Directory.Delete(WorkSpaceFolderPath, true);
            Directory.CreateDirectory(WorkSpaceFolderPath);
            await MessageHelper.Show("清理成功", "Clean Success");
        }

        public async void OpenCurrentWorkSpaceFolder(object sender, RoutedEventArgs e)
        {
            string WorkSpaceOutputFolder = MainConfig.Path_OutputFolder + ComboBoxWorkSpaceSelection.Text + "\\";
            if (!string.IsNullOrEmpty(WorkSpaceOutputFolder))
            {
                if (Directory.Exists(WorkSpaceOutputFolder))
                {
                    await CommandHelper.ShellOpenFolder(WorkSpaceOutputFolder);
                }
                else
                {
                    await MessageHelper.Show("此目录不存在，请检查您的Output文件夹是否设置正确", "This folder doesn't exists,please check if your OutputFolder is correct.");
                }
            }
        }


        public void CleanDrawIBListTextBox(object sender, RoutedEventArgs e)
        {
            TextBoxDrawIBList.Text = "";
        }

        public List<string> GetDrawIBListFromTextBoxDrawIBList()
        {
            List<string> DrawIBList = new List<string>();
            if (TextBoxDrawIBList.Text.Contains(","))
            {
                DrawIBList = TextBoxDrawIBList.Text.Split(',').ToList();
            }
            else
            {
                DrawIBList.Add(TextBoxDrawIBList.Text);
            }

            return DrawIBList;
        }

        public async void SaveDrawIBList()
        {
            //(1) 检查游戏类型是否设置
            if (MainConfig.CurrentGameName == "")
            {
                await MessageHelper.Show("请先选择游戏类型", "Please select a game before this.");
                return;
            }

            //(2) 接下来要把当前的游戏名称+类型保存到MainSetting.json里
            MainConfig.SaveCurrentGameNameToMainJson();

            //(3) 接下来把所有的drawIBList中的DrawIB保留下来存储到对应配置文件。
            List<string> drawIBList = GetDrawIBListFromTextBoxDrawIBList();

            JObject jobj = new JObject();
            jobj["DrawIBList"] = new JArray(drawIBList);
            string json_string = jobj.ToString(Formatting.Indented);

            // 将JSON字符串写入文件
            File.WriteAllText(MainConfig.Path_OutputFolder + ComboBoxWorkSpaceSelection.Text + "\\Config.Json", json_string);

        }

        public async void SaveDrawIBListToConfig(object sender, RoutedEventArgs e)
        {
            SaveDrawIBList();
            await MessageHelper.Show("保存成功");
        }

        void ConvertAutoExtractedTexturesInDrawIBFolderToTargetFormat()
        {
            string WorkSpacePath = MainConfig.Path_OutputFolder + MainConfig.CurrentWorkSpace + "/";
            List<string> DrawIBList = ConfigHelper.GetDrawIBListFromConfig(MainConfig.CurrentWorkSpace);
            foreach (string DrawIB in DrawIBList)
            {
                //在这里把所有output目录下的dds转为png格式
                string[] subdirectories = Directory.GetDirectories(WorkSpacePath + DrawIB + "/");
                foreach (string outputDirectory in subdirectories)
                {
                    //MessageBox.Show(Path.GetDirectoryName(outputDirectory));

                    if (!Path.GetFileName(outputDirectory).StartsWith("TYPE_"))
                    {
                        continue;
                    }


                    string[] filePathArray = Directory.GetFiles(outputDirectory);
                    foreach (string ddsFilePath in filePathArray)
                    {
                        if (MainConfig.AutoTextureOnlyConvertDiffuseMap)
                        {
                            if (!ddsFilePath.EndsWith("DiffuseMap.dds"))
                            {
                                continue;
                            }
                        }
                        else
                        {
                            if (!ddsFilePath.EndsWith(".dds"))
                            {
                                continue;
                            }
                        }

                        string TextureFormatString = TextureHelper.GetAutoTextureFormat();
                        CommandHelper.ConvertTexture(ddsFilePath, TextureFormatString, outputDirectory);
                    }
                }
            }

        }

        void ConvertDedupedTexturesToTargetFormat()
        {
            string WorkSpacePath = MainConfig.Path_OutputFolder + MainConfig.CurrentWorkSpace + "/";
            List<string> DrawIBList = ConfigHelper.GetDrawIBListFromConfig(MainConfig.CurrentWorkSpace);
            foreach (string DrawIB in DrawIBList)
            {
                //在这里把所有output目录下的dds转为png格式
                string DedupedTexturesFolderPath = WorkSpacePath + DrawIB + "/DedupedTextures/";
                //MessageHelper.Show(DedupedTexturesFolderPath);
                if (!Directory.Exists(DedupedTexturesFolderPath))
                {
                    return;
                }

                string TextureFormatString = TextureHelper.GetAutoTextureFormat();
                string DedupedTexturesConvertFolderPath = WorkSpacePath + DrawIB + "/DedupedTextures_" + TextureFormatString + "/";
                //MessageHelper.Show(DedupedTexturesConvertFolderPath);
                TextureHelper.ConvertAllTextureFilesToTargetFolder(DedupedTexturesFolderPath, DedupedTexturesConvertFolderPath);
            }
        }

        public async Task<bool> PreDoBeforeExtract()
        {
            if (TextBoxDrawIBList.Text.Trim() == "")
            {
                await MessageHelper.Show("在运行之前请填写您的绘制IB的哈希值并进行配置", "Please fill your DrawIB and config it before run.");
                return false;
            }

            if (ComboBoxWorkSpaceSelection.Text.Trim() == "")
            {
                await MessageHelper.Show("请先指定工作空间");
                return false;
            }

            return true;
        }

        public async void ExtractModel(object sender, RoutedEventArgs e)
        {
            bool Prepare = await PreDoBeforeExtract();
            if (!Prepare)
            {
                return;
            }

            SaveDrawIBList();

            bool RunResult = await CommandHelper.runCommand("merge");

            if (RunResult)
            {
                ConvertAutoExtractedTexturesInDrawIBFolderToTargetFormat();

                if (MainConfig.ConvertDedupedTextures)
                {
                    ConvertDedupedTexturesToTargetFormat();
                }
            }
            else
            {
                OpenLatestLogFile(sender,e);
            }
            
        }


        public async void GenerateMod(object sender, RoutedEventArgs e)
        {
            if (ComboBoxWorkSpaceSelection.Text.Trim() == "")
            {
                await MessageHelper.Show("请先选择工作空间");
                return;
            }

            bool RunResult = await CommandHelper.runCommand("split");
            if (RunResult)
            {
                OpenModsFolder(sender, e);
            }
            else
            {
                OpenLatestLogFile(sender,e);
            }
        }


        public async void OpenWorkSpaceGenerateModFolder(object sender, RoutedEventArgs e)
        {
            string GeneratedModFolderPath = MainConfig.Path_OutputFolder + "/" + MainConfig.CurrentWorkSpace + "/GeneratedMod/";
           
            if (Directory.Exists(GeneratedModFolderPath))
            {
                await CommandHelper.ShellOpenFolder(GeneratedModFolderPath);
            }
            else
            {
                await MessageHelper.Show("您还未生成二创模型", "You have not generate any mod yet");
            }
        }

        public async void OpenModsFolder(object sender, RoutedEventArgs e)
        {
            string modsFolder = MainConfig.Path_LoaderFolder + "Mods/";
            if (Directory.Exists(modsFolder))
            {
                await CommandHelper.ShellOpenFolder(modsFolder);
            }
            else
            {
                await MessageHelper.Show("此目录不存在，请检查您的Mods文件夹是否设置正确", "This path didn't exists, please check if your Mods folder is correct");
            }
        }

        string GetLatestFrameAnalysisFolder()
        {
            string[] directories = Directory.GetDirectories(MainConfig.Path_LoaderFolder);
            List<string> frameAnalysisFileList = new List<string>();
            foreach (string directory in directories)
            {
                string directoryName = Path.GetFileName(directory);

                if (directoryName.StartsWith("FrameAnalysis-"))
                {
                    frameAnalysisFileList.Add(directoryName);
                }
            }

            //
            if (frameAnalysisFileList.Count > 0)
            {
                frameAnalysisFileList.Sort();

                string latestFrameAnalysisFolder = MainConfig.Path_LoaderFolder.Replace("/", "\\") + frameAnalysisFileList.Last();
                return latestFrameAnalysisFolder;
            }

            return "";
        }

        public async void OpenLatestFrameAnalysisFolder(object sender, RoutedEventArgs e)
        {
            string latestFrameAnalysisFolder = GetLatestFrameAnalysisFolder();
            if (!string.IsNullOrEmpty(latestFrameAnalysisFolder))
            {
                await CommandHelper.ShellOpenFolder(latestFrameAnalysisFolder);
            }
            else
            {
                await MessageHelper.Show("目标目录没有任何FrameAnalysis文件夹","Target directory didn't have any FrameAnalysisFolder.");
            }
        }


        public async void OpenLatestFrameAnalysisLogTxtFile(object sender, RoutedEventArgs e)
        {
            string[] directories = Directory.GetDirectories(MainConfig.Path_LoaderFolder);
            List<string> frameAnalysisFileList = new List<string>();
            foreach (string directory in directories)
            {
                string directoryName = Path.GetFileName(directory);

                if (directoryName.StartsWith("FrameAnalysis-"))
                {
                    frameAnalysisFileList.Add(directoryName);
                }
            }

            //
            if (frameAnalysisFileList.Count > 0)
            {
                frameAnalysisFileList.Sort();

                string latestFrameAnalysisFolder = MainConfig.Path_LoaderFolder.Replace("/", "\\") + frameAnalysisFileList.Last();

                await CommandHelper.ShellOpenFile(latestFrameAnalysisFolder + "\\log.txt");
            }
            else
            {
                await MessageHelper.Show("目标目录没有任何FrameAnalysis文件夹", "Target directory didn't have any FrameAnalysisFolder.");
            }
        }

        public async void OpenLatestFrameAnalysisDedupedFolder(object sender, RoutedEventArgs e)
        {
            string latestFrameAnalysisFolder = GetLatestFrameAnalysisFolder();
            if (!string.IsNullOrEmpty(latestFrameAnalysisFolder))
            {
                await CommandHelper.ShellOpenFolder(latestFrameAnalysisFolder + "\\deduped\\");
            }
            else
            {
                await MessageHelper.Show("目标目录没有任何FrameAnalysis文件夹", "Target directory didn't have any FrameAnalysisFolder.");
            }
        }


        public async void OpenExtractTypesFolder(object sender, RoutedEventArgs e)
        {
            string ExtractTypeFolderPath = "Configs\\ExtractTypes\\";
            string GameTypeFolderPath = ExtractTypeFolderPath + MainConfig.CurrentGameName + "\\";
            if (Directory.Exists(GameTypeFolderPath))
            {
                await CommandHelper.ShellOpenFolder(GameTypeFolderPath);
            }
            else
            {
                await CommandHelper.ShellOpenFolder(ExtractTypeFolderPath);
            }
        }

        public async void OpenDBMTLocationFolder(object sender, RoutedEventArgs e)
        {
            await CommandHelper.ShellOpenFolder(Directory.GetCurrentDirectory());
        }

        public async void OpenLogsFolder(object sender, RoutedEventArgs e)
        {
            string LogsFolderPath = Path.Combine(Directory.GetCurrentDirectory() ,"Logs\\");
            await CommandHelper.ShellOpenFolder(LogsFolderPath);
        }


        public async void OpenLatestLogFile(object sender, RoutedEventArgs e)
        {
            //然后打开最新的Log文件
            string logsPath = MainConfig.ApplicationRunPath + "Logs";
            if (!Directory.Exists(logsPath))
            {
                return;
            }
            string[] logFiles = Directory.GetFiles(logsPath); ;
            List<string> logFileList = new List<string>();
            foreach (string logFile in logFiles)
            {
                string logfileName = Path.GetFileName(logFile);
                if (logfileName.EndsWith(".log") && logfileName.Length > 15)
                {
                    logFileList.Add(logfileName);
                }
            }

            logFileList.Sort();
            await CommandHelper.ShellOpenFile(logsPath + "\\" + logFileList[logFileList.Count - 1]);
        }

        public async void OpenConfigsFolder(object sender, RoutedEventArgs e)
        {
            string ConfigsFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "Configs\\");
            await CommandHelper.ShellOpenFolder(ConfigsFolderPath);
        }


        public void CleanSkipIBListTextBox(object sender, RoutedEventArgs e)
        {
            TextBoxSkipIBList.Text = "";
        }


        private async Task<Dictionary<string, List<string>>> GetBuffHash_VSShaderHashValues_Dict()
        {
            string frameAnalyseFolder = "";
            string[] directories = Directory.GetDirectories(MainConfig.Path_LoaderFolder.Replace("/", "\\")); ;
            List<string> frameAnalysisFileList = new List<string>();
            foreach (string directory in directories)
            {
                string directoryName = Path.GetFileName(directory);

                if (directoryName.StartsWith("FrameAnalysis-"))
                {
                    frameAnalysisFileList.Add(directoryName);
                }
            }

            //Get FA numbers to reserve
            frameAnalysisFileList.Sort();
            if (frameAnalysisFileList.Count > 0)
            {
                //排序后是从小到大的，时间上也是如此，我们这里是找最新的一个，所以选-1个
                frameAnalyseFolder = frameAnalysisFileList[frameAnalysisFileList.Count - 1];
            }
            else
            {
                await MessageHelper.Show("未找到FrameAnalysisFolder", "Can't find any FrameAnalysisFolder");
            }

            if (frameAnalyseFolder == "")
            {
                await MessageHelper.Show("当前指定的FrameAnalysisFolder不存在，请重新设置", "Current specified FrameAnalysisFolder didn't exists, please check your setting");
            }

            string frameAnalysisFolderPath = MainConfig.Path_LoaderFolder + frameAnalyseFolder;

            Dictionary<string, List<string>> buffHash_vsShaderHashValues_Dict = new Dictionary<string, List<string>>();

            // 获取当前目录下的所有文件
            string[] files = Directory.GetFiles(frameAnalysisFolderPath);
            foreach (string fileName in files)
            {
                if (!fileName.EndsWith(".txt"))
                {
                    continue;
                }

                int vsIndex = fileName.IndexOf("-vs=");
                if (vsIndex != -1)
                {
                    string bufferHash = fileName.Substring(vsIndex - 8, 8);
                    string vsShaderHash = fileName.Substring(vsIndex + 4, 16);

                    List<string> tmpList = new List<string>();
                    if (buffHash_vsShaderHashValues_Dict.ContainsKey(bufferHash))
                    {
                        tmpList = buffHash_vsShaderHashValues_Dict[bufferHash];
                    }
                    tmpList.Add(vsShaderHash);
                    buffHash_vsShaderHashValues_Dict[bufferHash] = tmpList;
                }
                else
                {
                    continue;
                }

            }

            return buffHash_vsShaderHashValues_Dict;
        }


        public async void ExecuteSkipIB(object sender, RoutedEventArgs e)
        {
            //这里不需要区分match_first_index,这是因为我们实际测试中不再需要用到match_first_index的过滤了。
            //直接分割然后输出即可
            List<string> DrawIBList = new List<string>();
            if (TextBoxSkipIBList.Text.Contains(","))
            {
                DrawIBList = TextBoxSkipIBList.Text.Split(',').ToList();
            }
            else
            {
                DrawIBList.Add(TextBoxSkipIBList.Text);
            }

            Dictionary<string, List<string>> buffHash_vsShaderHashValues_Dict = await GetBuffHash_VSShaderHashValues_Dict();

            string outputContent = "";

            List<string> WritedHashList = new List<string>();

            foreach (string DrawIB in DrawIBList)
            {
                outputContent = outputContent + "[TextureOverride_IB_" + DrawIB + "]\r\n";
                outputContent = outputContent + "hash = " + DrawIB + "\r\n";
                outputContent = outputContent + "handling = skip\r\n";
                outputContent = outputContent + "\r\n";

                if (MainConfig.CurrentGameName == "Game001" || MainConfig.CurrentGameName == "LiarsBar")
                {
                    if (buffHash_vsShaderHashValues_Dict.ContainsKey(DrawIB))
                    {
                        List<string> VSHashList = buffHash_vsShaderHashValues_Dict[DrawIB];
                        foreach (string hash in VSHashList)
                        {
                            if (WritedHashList.Contains(hash))
                            {
                                continue;
                            }
                            WritedHashList.Add(hash);
                            outputContent = outputContent + "[ShaderOverride_" + hash + "]\r\n";
                            outputContent = outputContent + "hash = " + hash + "\r\n";
                            outputContent = outputContent + "if $costume_mods\r\n";
                            outputContent = outputContent + "  checktextureoverride = ib\r\n";
                            outputContent = outputContent + "endif\r\n\r\n";
                        }
                    }
                }

            }

            if (!File.Exists(MainConfig.Path_OutputFolder))
            {
                Directory.CreateDirectory(MainConfig.Path_OutputFolder);
            }

            string outputPath = MainConfig.Path_OutputFolder + "IBSkip.ini";
            File.WriteAllText(outputPath, outputContent);

            await CommandHelper.ShellOpenFolder(MainConfig.Path_OutputFolder);
        }


        public async void ExecuteGenerateVSCheck(object sender, RoutedEventArgs e)
        {
            //这里不需要区分match_first_index,这是因为我们实际测试中不再需要用到match_first_index的过滤了。
            //直接分割然后输出即可
            List<string> DrawIBList = new List<string>();
            if (TextBoxSkipIBList.Text.Contains(","))
            {
                DrawIBList = TextBoxSkipIBList.Text.Split(',').ToList();
            }
            else
            {
                DrawIBList.Add(TextBoxSkipIBList.Text);
            }

            Dictionary<string, List<string>> buffHash_vsShaderHashValues_Dict = await GetBuffHash_VSShaderHashValues_Dict();

            string outputContent = "";

            List<string> WritedHashList = new List<string>();

            foreach (string DrawIB in DrawIBList)
            {

                if (buffHash_vsShaderHashValues_Dict.ContainsKey(DrawIB))
                {
                    List<string> VSHashList = buffHash_vsShaderHashValues_Dict[DrawIB];
                    foreach (string hash in VSHashList)
                    {
                        if (WritedHashList.Contains(hash))
                        {
                            continue;
                        }
                        WritedHashList.Add(hash);
                        outputContent = outputContent + "[ShaderOverride_" + hash + "]\r\n";
                        outputContent = outputContent + "hash = " + hash + "\r\n";
                        outputContent = outputContent + "if $costume_mods\r\n";
                        outputContent = outputContent + "  checktextureoverride = ib\r\n";
                        outputContent = outputContent + "endif\r\n\r\n";
                    }
                }

            }

            if (!File.Exists(MainConfig.Path_OutputFolder))
            {
                Directory.CreateDirectory(MainConfig.Path_OutputFolder);
            }

            string outputPath = MainConfig.Path_OutputFolder + "VertexShaderCheck.ini";
            File.WriteAllText(outputPath, outputContent);

            await CommandHelper.ShellOpenFolder(MainConfig.Path_OutputFolder);

        }


        public async void ReverseLv5_ReverseExtract(object sender, RoutedEventArgs e)
        {
            bool Prepare = await PreDoBeforeExtract();
            if (!Prepare)
            {
                return;
            }

            bool command_run_result =await CommandHelper.runCommand("ReverseExtract", "3Dmigoto-Sword-Lv5.vmp.exe");
            if (command_run_result)
            {
                ConvertAutoExtractedTexturesInDrawIBFolderToTargetFormat();

                await CommandHelper.ShellOpenFolder(MainConfig.Path_OutputFolder);
            }
        }

        private async Task<string> RunReverseIniCommand(string commandStr)
        {
            if (string.IsNullOrEmpty(MainConfig.CurrentGameName))
            {
                await MessageHelper.Show("在逆向Mod之前请选择当前要进行格式转换的二创模型的所属游戏", "Please select your current game before reverse.");
                return "";
            }

            FileOpenPicker picker = await CommandHelper.GetFilePicker(".ini");
            StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                string filePath = file.Path;
                if (DBMTStringUtils.ContainsChinese(filePath))
                {
                    await MessageHelper.Show("目标Mod的ini文件路径中不能出现中文", "Target mod ini file path can't contains Chinese.");
                    return "";
                }

                string json = File.ReadAllText(MainConfig.Path_RunInputJson); // 读取文件内容
                JObject runInputJson = JObject.Parse(json);
                runInputJson["GameName"] = MainConfig.CurrentGameName;
                runInputJson["ReverseFilePath"] = filePath;
                File.WriteAllText(MainConfig.Path_RunInputJson, runInputJson.ToString());

                bool RunResult = await CommandHelper.runCommand(commandStr, "3Dmigoto-Sword-Lv5.vmp.exe");
                if (RunResult)
                {
                    return filePath;
                }
                else
                {
                    return "";
                }
            }
            else
            {
                return "";
            }
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

        private async void ConvertTexturesInMod(string ModIniFilePath)
        {
            if (!string.IsNullOrEmpty(ModIniFilePath))
            {
                string ModFolderPath = Path.GetDirectoryName(ModIniFilePath);
                List<string> result = FindDirectoriesWithImages(ModFolderPath);
                foreach (string TextyreFolder in result)
                {
                    string TargetTexturesFolderPath = TextyreFolder + "/ConvertedTextures/";
                    //MessageBox.Show(TargetTexturesFolderPath);
                    Directory.CreateDirectory(TargetTexturesFolderPath);
                    TextureHelper.ConvertAllTextureFilesToTargetFolder(TextyreFolder, TargetTexturesFolderPath);
                }

                string ModFolderName = Path.GetFileName(ModFolderPath);
                string ModFolderParentPath = Path.GetDirectoryName(ModFolderPath);
                string ModReverseFolderPath = ModFolderParentPath + "\\" + ModFolderName + "-Reverse\\";

                await CommandHelper.ShellOpenFolder(ModReverseFolderPath);
            }
        }

        public async void ReverseLv5_ReverseSingleIni(object sender, RoutedEventArgs e)
        {
            string ModIniFilePath =await RunReverseIniCommand("ReverseSingleLv5");
            ConvertTexturesInMod(ModIniFilePath);
        }


        public async void ReverseLv5_ReverseToggleSwitchIni(object sender, RoutedEventArgs e)
        {
            string ModIniFilePath = await RunReverseIniCommand("ReverseMergedLv5");
            ConvertTexturesInMod(ModIniFilePath);
        }


        public async void ReverseLv5_ReverseDrawIndexedSwitchIni(object sender, RoutedEventArgs e)
        {
            string ModIniFilePath = await RunReverseIniCommand("ReverseOutfitCompilerLv4");
            ConvertTexturesInMod(ModIniFilePath);
        }

    }
}
