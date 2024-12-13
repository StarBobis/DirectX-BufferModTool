using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DBMT_Core;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Windows.Storage.Pickers;
using Windows.Storage;
using Microsoft.UI.Xaml.Navigation;

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
            try
            {
                MainConfig.LoadConfigFile(MainConfig.ConfigFiles.Game_Setting);
                InitializeWorkSpace(MainConfig.CurrentWorkSpace);
                SetDefaultBackGroundImage();

                WorkBGImageBrush.Opacity = MainConfig.GameCfg.Value.WorkPageBackGroundImageOpacity;

                LoadDirectoryNames();
            }
            catch (Exception ex)
            {
                _ = MessageHelper.Show("Error: " + ex.ToString());
            }
            
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            // ִ������Ҫ�����ҳ�汻�رջ򵼺��뿪ʱ���еĴ���

            //����ȫ��������ΪҪ���滬��͸����
            MainConfig.GameCfg.Value.WorkPageBackGroundImageOpacity = (float)WorkBGImageBrush.Opacity;
            MainConfig.SaveConfig(MainConfig.ConfigFiles.Game_Setting);
            // �����Ҫ�����Ե��û���� OnNavigatedFrom ����
            base.OnNavigatedFrom(e);
        }


        private async void LoadDirectoryNames()
        {
            WorkGameSelectionComboBox.Items.Clear();
            List<string> directories = await PathHelper.GetGameDirectoryNameList();
            foreach (var dirName in directories)
            {
                WorkGameSelectionComboBox.Items.Add(dirName);
            }
            if (MainConfig.CurrentGameName == "")
            {
                WorkGameSelectionComboBox.SelectedIndex = 0;
            }
            else
            {
                WorkGameSelectionComboBox.SelectedItem = MainConfig.CurrentGameName;
            }
        }


        private void WorkGameSelectionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // ��ȡ�����¼��� ComboBox ʵ��
            var comboBox = sender as ComboBox;

            // ����Ƿ�����ѡ�е���
            if (comboBox != null && comboBox.SelectedItem != null)
            {
                // ִ������Ҫ�Ĳ����������ȡѡ�е�����д���
                string selectedGame = comboBox.SelectedItem.ToString();

                //MainConfig.SetCurrentGame(selectedGame);
                MainConfig.SetConfig(MainConfig.ConfigFiles.Main, "GameName", selectedGame);
                MainConfig.SaveConfig(MainConfig.ConfigFiles.Main);

                SetDefaultBackGroundImage();
                InitializeWorkSpace(MainConfig.CurrentWorkSpace);
            }
        }


        private void SetDefaultBackGroundImage()
        {
            string imagePath = PathHelper.GetCurrentGameBackGroundPicturePath();
            BitmapImage bitmap = new BitmapImage(new Uri(imagePath));
            WorkBGImageBrush.ImageSource = bitmap;
        }

        private void ComboBoxWorkSpaceSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBoxWorkSpaceSelection.Items.Contains(ComboBoxWorkSpaceSelection.Text))
            {
                MainConfig.SetConfig(MainConfig.ConfigFiles.Main, "WorkSpaceName", ComboBoxWorkSpaceSelection.Text);
                MainConfig.SaveConfig(MainConfig.ConfigFiles.Main);
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
            MainConfig.SaveConfig(MainConfig.ConfigFiles.Main);

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

        private async void AutoDetectPointlistDrawIBList_UnityVSPreSkinning(object sender, RoutedEventArgs e)
        {
            await CommandHelper.runCommand("DetectPointlistDrawIBList");
            InitializeWorkSpace();
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
                TextureHelper.ConvertAutoExtractedTexturesInDrawIBFolderToTargetFormat();

                if (MainConfig.GetConfig<bool>(MainConfig.ConfigFiles.Texture_Setting, "ConvertDedupedTextures"))
                {
                    TextureHelper.ConvertDedupedTexturesToTargetFormat();
                }

                OpenCurrentWorkSpaceFolder(sender, e);
            }
            else
            {
                OpenLatestLogFile(sender, e);
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
                OpenLatestLogFile(sender, e);
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

        public async void OpenLatestFrameAnalysisFolder(object sender, RoutedEventArgs e)
        {
            string latestFrameAnalysisFolder = PathHelper.GetLatestFrameAnalysisFolder();
            if (!string.IsNullOrEmpty(latestFrameAnalysisFolder))
            {
                await CommandHelper.ShellOpenFolder(latestFrameAnalysisFolder);
            }
            else
            {
                await MessageHelper.Show("Ŀ��Ŀ¼û���κ�FrameAnalysis�ļ���", "Target directory didn't have any FrameAnalysisFolder.");
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
            string latestFrameAnalysisFolder = PathHelper.GetLatestFrameAnalysisFolder();
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
            if (Directory.Exists(MainConfig.Path_GameTypeFolder))
            {
                await CommandHelper.ShellOpenFolder(MainConfig.Path_GameTypeFolder);
            }
            else
            {
                await CommandHelper.ShellOpenFolder(MainConfig.Path_ExtractTypesFolder);
            }
        }

        public async void OpenDBMTLocationFolder(object sender, RoutedEventArgs e)
        {
            await CommandHelper.ShellOpenFolder(Directory.GetCurrentDirectory());
        }

        public async void OpenLogsFolder(object sender, RoutedEventArgs e)
        {
            string LogsFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "Logs\\");
            await CommandHelper.ShellOpenFolder(LogsFolderPath);
        }


        public async void OpenLatestLogFile(object sender, RoutedEventArgs e)
        {
            string LogFilePath = PathHelper.GetLatestLogFilePath();
            if (File.Exists(LogFilePath))
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

        private List<string> GetSkipIBList()
        {
            List<string> SkipIBList = new List<string>();
            if (TextBoxSkipIBList.Text.Contains(","))
            {
                SkipIBList = TextBoxSkipIBList.Text.Split(',').ToList();
            }
            else
            {
                SkipIBList.Add(TextBoxSkipIBList.Text);
            }
            return SkipIBList; 
        }

        public async void ExecuteSkipIB(object sender, RoutedEventArgs e)
        {
            List<string> DrawIBList = GetSkipIBList();
            DrawIBHelper.GenerateSkipIB(DrawIBList);
            await CommandHelper.ShellOpenFolder(MainConfig.Path_OutputFolder);
        }


        public async void ExecuteGenerateVSCheck(object sender, RoutedEventArgs e)
        {
            List<string> DrawIBList = GetSkipIBList();
            DrawIBHelper.GenerateVSCheck(DrawIBList);
            await CommandHelper.ShellOpenFolder(MainConfig.Path_OutputFolder);
        }


        public async void ReverseLv5_ReverseExtract(object sender, RoutedEventArgs e)
        {
            bool Prepare = await PreDoBeforeExtract();
            if (!Prepare)
            {
                return;
            }

            //������ȡ֮ǰҪ����DrawIB�б�
            SaveDrawIBList();

            bool command_run_result = await CommandHelper.runCommand("ReverseExtract", "3Dmigoto-Sword-Lv5.vmp.exe");
            if (command_run_result)
            {
                TextureHelper.ConvertAutoExtractedTexturesInDrawIBFolderToTargetFormat();

                OpenCurrentWorkSpaceFolder(sender,e);
            }
        }


        public async void ReverseLv5_ReverseSingleIni(object sender, RoutedEventArgs e)
        {
            string ModIniFilePath = await CommandHelper.RunReverseIniCommand("ReverseSingleLv5");
            TextureHelper.ConvertTexturesInMod(ModIniFilePath);
        }

        public async void ReverseLv5_ReverseToggleSwitchIni(object sender, RoutedEventArgs e)
        {
            string ModIniFilePath = await CommandHelper.RunReverseIniCommand("ReverseMergedLv5");
            TextureHelper.ConvertTexturesInMod(ModIniFilePath);
        }

        public async void ReverseLv5_ReverseDrawIndexedSwitchIni(object sender, RoutedEventArgs e)
        {
            string ModIniFilePath = await CommandHelper.RunReverseIniCommand("ReverseOutfitCompilerLv4");
            TextureHelper.ConvertTexturesInMod(ModIniFilePath);
        }

        public async void Encryption_EncryptAll(object sender, RoutedEventArgs e)
        {
            //�����������µ�ini�ļ���·��
            string NewModInIPath = await EncryptionHelper.Obfuscate_ModFileName("Play");
            if (NewModInIPath == "")
            {
                return;
            }
            //���ü���Buffer������ini�ļ�
            await EncryptionHelper.DBMT_Encryption_RunCommand("encrypt_buffer_ini_v5", NewModInIPath);
        }

        public async void Encryption_EncryptBufferAndIni(object sender, RoutedEventArgs e)
        {
            string ini_file_path = await CommandHelper.ChooseFileAndGetPath(".ini");
            if (ini_file_path != "")
            {
                await EncryptionHelper.DBMT_Encryption_RunCommand("encrypt_buffer_ini_v5", ini_file_path);
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
                await EncryptionHelper.DBMT_Encryption_RunCommand("encrypt_ini_acptpro_V5", ini_file_path);
            }
        }

        private void LightButtonClick_TurnOnOffBackGroundImageOpacity(object sender, RoutedEventArgs e)
        {
            if (WorkBGImageBrush.Opacity != 0)
            {
                MainConfig.GameCfg.Value.WorkPageBackGroundImageOpacity =(float)WorkBGImageBrush.Opacity;
                WorkBGImageBrush.Opacity = 0;
            }
            else
            {
                WorkBGImageBrush.Opacity = MainConfig.GameCfg.Value.WorkPageBackGroundImageOpacity;
            }
        }
    }
}
