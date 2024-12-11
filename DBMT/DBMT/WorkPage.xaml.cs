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

            MainConfig.ReadCurrentWorkSpaceFromMainJson();
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
                
                //�л�����Ӧ����
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
                    // �Ƴ����һ���ַ�
                    DrawIBListString = DrawIBListString.Substring(0, DrawIBListString.Length - 1);
                }
            }
            TextBoxDrawIBList.Text = DrawIBListString;
        }

        public void InitializeWorkSpace(string WorkSpaceName = "")
        {
            //����洢�����ռ��output�ļ��в����ھʹ���
            if (!Directory.Exists(MainConfig.Path_OutputFolder))
            {
                Directory.CreateDirectory(MainConfig.Path_OutputFolder);
            }


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
                await MessageHelper.Show("�����ռ����Ʋ���Ϊ��");
                return;
            }

            if (!ComboBoxWorkSpaceSelection.Items.Contains(ComboBoxWorkSpaceSelection.Text))
            {
                ////��������˴������ռ䣬�Ͳ������ļ��У�����ʹ���
                Directory.CreateDirectory(MainConfig.Path_OutputFolder + ComboBoxWorkSpaceSelection.Text);

                await MessageHelper.Show("�����ռ䴴���ɹ�");

                InitializeWorkSpace(ComboBoxWorkSpaceSelection.Text);
            }
            else
            {
                await MessageHelper.Show("��ǰ�����ռ��Ѵ���,�޷��ظ�����");
            }
        }

        public async void CleanCurrentWorkSpaceFile(object sender, RoutedEventArgs e)
        {
            string WorkSpaceFolderPath = MainConfig.Path_OutputFolder + ComboBoxWorkSpaceSelection.Text;
            Directory.Delete(WorkSpaceFolderPath, true);
            Directory.CreateDirectory(WorkSpaceFolderPath);
            await MessageHelper.Show("����ɹ�", "Clean Success");
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
                    await MessageHelper.Show("��Ŀ¼�����ڣ���������Output�ļ����Ƿ�������ȷ", "This folder doesn't exists,please check if your OutputFolder is correct.");
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
            //(1) �����Ϸ�����Ƿ�����
            if (MainConfig.CurrentGameName == "")
            {
                await MessageHelper.Show("����ѡ����Ϸ����", "Please select a game before this.");
                return;
            }

            //(2) ������Ҫ�ѵ�ǰ����Ϸ����+���ͱ��浽MainSetting.json��
            MainConfig.SaveCurrentGameNameToMainJson();

            //(3) �����������е�drawIBList�е�DrawIB���������洢����Ӧ�����ļ���
            List<string> drawIBList = GetDrawIBListFromTextBoxDrawIBList();

            JObject jobj = new JObject();
            jobj["DrawIBList"] = new JArray(drawIBList);
            string json_string = jobj.ToString(Formatting.Indented);

            // ��JSON�ַ���д���ļ�
            File.WriteAllText(MainConfig.Path_OutputFolder + ComboBoxWorkSpaceSelection.Text + "\\Config.Json", json_string);

        }

        public async void SaveDrawIBListToConfig(object sender, RoutedEventArgs e)
        {
            SaveDrawIBList();
            await MessageHelper.Show("����ɹ�");
        }

        void ConvertAutoExtractedTexturesInDrawIBFolderToTargetFormat()
        {
            string WorkSpacePath = MainConfig.Path_OutputFolder + MainConfig.CurrentWorkSpace + "/";
            List<string> DrawIBList = ConfigHelper.GetDrawIBListFromConfig(MainConfig.CurrentWorkSpace);
            foreach (string DrawIB in DrawIBList)
            {
                //�����������outputĿ¼�µ�ddsתΪpng��ʽ
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

        async void ConvertDedupedTexturesToTargetFormat()
        {

            string WorkSpacePath = MainConfig.Path_OutputFolder + MainConfig.CurrentWorkSpace + "/";
            List<string> DrawIBList = ConfigHelper.GetDrawIBListFromConfig(MainConfig.CurrentWorkSpace);
            foreach (string DrawIB in DrawIBList)
            {
                //�����������outputĿ¼�µ�ddsתΪpng��ʽ
                string DedupedTexturesFolderPath = WorkSpacePath + DrawIB + "/DedupedTextures/";
                //MessageHelper.Show(DedupedTexturesFolderPath);
                if (!Directory.Exists(DedupedTexturesFolderPath))
                {
                    await MessageHelper.Show("�޷��ҵ�DedupedTextures�ļ���: " + DedupedTexturesFolderPath);
                    return;
                }

                string TextureFormatString = TextureHelper.GetAutoTextureFormat();
                string DedupedTexturesConvertFolderPath = WorkSpacePath + DrawIB + "/DedupedTextures_" + TextureFormatString + "/";
                //MessageHelper.Show(DedupedTexturesConvertFolderPath);
                TextureHelper.ConvertAllTextureFilesToTargetFolder(DedupedTexturesFolderPath, DedupedTexturesConvertFolderPath);
                //await MessageHelper.Show("ת���ɹ�");
            }
        }

        public async Task<bool> PreDoBeforeExtract()
        {
            if (TextBoxDrawIBList.Text.Trim() == "")
            {
                await MessageHelper.Show("������֮ǰ����д���Ļ���IB�Ĺ�ϣֵ����������", "Please fill your DrawIB and config it before run.");
                return false;
            }

            if (ComboBoxWorkSpaceSelection.Text.Trim() == "")
            {
                await MessageHelper.Show("����ָ�������ռ�");
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
                    //await MessageHelper.Show("��ѡ��ת��Deduped��ͼ����ʼת��Deduped��ͼ");
                    ConvertDedupedTexturesToTargetFormat();
                }

                OpenCurrentWorkSpaceFolder(sender, e);
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
                await MessageHelper.Show("����ѡ�����ռ�");
                return;
            }

            bool RunResult = await CommandHelper.runCommand("split");
            if (RunResult)
            {
                OpenWorkSpaceGenerateModFolder(sender, e);
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
                await MessageHelper.Show("����δ���ɶ���ģ��", "You have not generate any mod yet");
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
                await MessageHelper.Show("��Ŀ¼�����ڣ���������Mods�ļ����Ƿ�������ȷ", "This path didn't exists, please check if your Mods folder is correct");
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
                await MessageHelper.Show("Ŀ��Ŀ¼û���κ�FrameAnalysis�ļ���","Target directory didn't have any FrameAnalysisFolder.");
            }
        }


        public async void OpenLatestFrameAnalysisLogTxtFile(object sender, RoutedEventArgs e)
        {
            string LatestFrameAnalysisFolderLogTxtFilePath = PathHelper.GetLatestFrameAnalysisFolderLogFilePath();

            if (LatestFrameAnalysisFolderLogTxtFilePath != "")
            {
                if (File.Exists(LatestFrameAnalysisFolderLogTxtFilePath))
                {
                    await CommandHelper.ShellOpenFile(LatestFrameAnalysisFolderLogTxtFilePath);
                }
            }
            else
            {
                await MessageHelper.Show("û���ҵ��κ�FrameAnalysis�ļ���", "Target directory didn't have any FrameAnalysisFolder.");
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
                await MessageHelper.Show("Ŀ��Ŀ¼û���κ�FrameAnalysis�ļ���", "Target directory didn't have any FrameAnalysisFolder.");
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
            string LogFilePath = PathHelper.GetLatestLogFilePath();
            if(File.Exists(LogFilePath))
            {
                await CommandHelper.ShellOpenFile(LogFilePath);
            }
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


       

        public async void ExecuteSkipIB(object sender, RoutedEventArgs e)
        {
            //���ﲻ��Ҫ����match_first_index,������Ϊ����ʵ�ʲ����в�����Ҫ�õ�match_first_index�Ĺ����ˡ�
            //ֱ�ӷָ�Ȼ���������
            List<string> DrawIBList = new List<string>();
            if (TextBoxSkipIBList.Text.Contains(","))
            {
                DrawIBList = TextBoxSkipIBList.Text.Split(',').ToList();
            }
            else
            {
                DrawIBList.Add(TextBoxSkipIBList.Text);
            }

            Dictionary<string, List<string>> buffHash_vsShaderHashValues_Dict = await DrawIBHelper.GetBuffHash_VSShaderHashValues_Dict();

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
            //���ﲻ��Ҫ����match_first_index,������Ϊ����ʵ�ʲ����в�����Ҫ�õ�match_first_index�Ĺ����ˡ�
            //ֱ�ӷָ�Ȼ���������
            List<string> DrawIBList = new List<string>();
            if (TextBoxSkipIBList.Text.Contains(","))
            {
                DrawIBList = TextBoxSkipIBList.Text.Split(',').ToList();
            }
            else
            {
                DrawIBList.Add(TextBoxSkipIBList.Text);
            }

            Dictionary<string, List<string>> buffHash_vsShaderHashValues_Dict = await DrawIBHelper.GetBuffHash_VSShaderHashValues_Dict();

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
                await MessageHelper.Show("������Mod֮ǰ��ѡ��ǰҪ���и�ʽת���Ķ���ģ�͵�������Ϸ", "Please select your current game before reverse.");
                return "";
            }

            FileOpenPicker picker =  CommandHelper.Get_FileOpenPicker(".ini");
            StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                string filePath = file.Path;
                if (DBMTStringUtils.ContainsChinese(filePath))
                {
                    await MessageHelper.Show("Ŀ��Mod��ini�ļ�·���в��ܳ�������", "Target mod ini file path can't contains Chinese.");
                    return "";
                }

                string json = File.ReadAllText(MainConfig.Path_RunInputJson); // ��ȡ�ļ�����
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
            // ��ȡ��ǰĿ¼�µ������ļ�
            string[] files = Directory.GetFiles(currentDirectory);

            // ����Ƿ���.dds��.png�ļ�
            foreach (string file in files)
            {
                string extension = Path.GetExtension(file).ToLower();
                if (extension == ".dds" || extension == ".png")
                {
                    // ����ҵ���Ŀ���ļ�����Ŀ¼�������б�
                    if (!result.Contains(currentDirectory))
                    {
                        result.Add(currentDirectory);
                    }
                    break; // �ҵ���һ�����͵��ļ��Ͳ��ټ������ҵ�ǰĿ¼�е������ļ�
                }
            }

            // �ݹ�������Ŀ¼
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

        private async Task<bool> DBMT_Encryption_RunCommand(string CommandString, string IniPath)
        {
            if (DBMTStringUtils.ContainsChinese(IniPath))
            {
                await MessageHelper.Show("Ŀ��·���в��ܺ��������ַ�", "Target Path Can't Contains Chinese.");
                return false;
            }
            JObject jsonObject = new JObject();
            jsonObject["EncryptFilePath"] = IniPath;
            File.WriteAllText("Configs\\ArmorSetting.json", jsonObject.ToString());

            await CommandHelper.runCommand(CommandString, "DBMT-Encryptor.vmp.exe");
            return true;
        }


        

        public async void Encryption_EncryptAll(object sender, RoutedEventArgs e)
        {

            //�����������µ�ini�ļ���·��
            string NewModInIPath =await EncryptionHelper.Obfuscate_ModFileName("Play");
            if (NewModInIPath == "")
            {
                return;
            }

            //���ü���Buffer������ini�ļ�
            await DBMT_Encryption_RunCommand("encrypt_buffer_ini_v5", NewModInIPath);
        }

        public async void Encryption_EncryptBufferAndIni(object sender, RoutedEventArgs e)
        {
            string ini_file_path = await CommandHelper.ChooseFileAndGetPath(".ini");
            if (ini_file_path != "")
            {
                await DBMT_Encryption_RunCommand("encrypt_buffer_ini_v5", ini_file_path);
            }
        }

        public async void Encryption_Obfuscate(object sender, RoutedEventArgs e)
        {
            await EncryptionHelper.Obfuscate_ModFileName("Play");
        }

        public async void Encryption_EncryptBuffer(object sender, RoutedEventArgs e)
        {
            string EncryptionCommand = "encrypt_buffer_acptpro_V4";

            string selected_folder_path = await CommandHelper.ChooseFolderAndGetPath();
            if (selected_folder_path != "")
            {
                if (DBMTStringUtils.ContainsChinese(selected_folder_path))
                {
                    await MessageHelper.Show("Ŀ��·���в��ܺ��������ַ�", "Target Path Can't Contains Chinese.");
                    return;
                }

                //�ж�Ŀ��·�����Ƿ���ini�ļ�
                // ʹ��Directory.GetFiles��������ָ������ģʽΪ*.ini
                string[] iniFiles = Directory.GetFiles(selected_folder_path, "*.ini");
                if (iniFiles.Length == 0)
                {
                    await MessageHelper.Show("Ŀ��·�����޷��ҵ�mod��ini�ļ�", "Target Path Can't find ini file.");
                    return;
                }


                JObject jsonObject = new JObject();
                jsonObject["targetACLFile"] = "Configs\\ACLSetting.json";
                string json_string = jsonObject.ToString(Formatting.Indented);
                File.WriteAllText(selected_folder_path, json_string);

                await CommandHelper.runCommand(EncryptionCommand, "DBMT-Encryptor.vmp.exe");
            }
        }

        public async void Encryption_EncryptIni(object sender, RoutedEventArgs e)
        {
            string ini_file_path = await CommandHelper.ChooseFileAndGetPath(".ini");
            if (ini_file_path != "")
            {
                await DBMT_Encryption_RunCommand("encrypt_ini_acptpro_V5", ini_file_path);
            }
        }

    }
}
