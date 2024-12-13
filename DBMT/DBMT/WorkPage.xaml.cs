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
using System.Collections.ObjectModel;
using CommunityToolkit.WinUI.UI.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DBMT
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class WorkPage : Page
    {
        // 定义一个简单的数据模型类
        public class MyItem
        {
            public string DrawIB { get; set; }
            public string Alias { get; set; }
        }

        private ObservableCollection<MyItem> items = new ObservableCollection<MyItem>();

        public WorkPage()
        {
            this.InitializeComponent();
            try
            {
                //MainConfig.LoadConfigFile(MainConfig.ConfigFiles.Game_Setting);
                MainConfig.GameCfg.LoadConfig();
                InitializeWorkSpace(MainConfig.CurrentWorkSpace);
                SetDefaultBackGroundImage();

                WorkBGImageBrush.Opacity = MainConfig.GameCfg.Value.WorkPageBackGroundImageOpacity;

                LoadDirectoryNames();


                // 添加一个空白行作为初始数据
                items.Add(new MyItem { DrawIB = "", Alias = "" });

                // 设置 DataGrid 的数据源
                myDataGrid.ItemsSource = items;

            }
            catch (Exception ex)
            {
                _ = MessageHelper.Show("Error: " + ex.ToString());
            }
            
        }

        private void MyDataGrid_CellEditEnding(object sender, CommunityToolkit.WinUI.UI.Controls.DataGridCellEditEndingEventArgs e)
        {
            // 检查是否是最后一行被编辑
            if (e.EditAction == CommunityToolkit.WinUI.UI.Controls.DataGridEditAction.Commit &&
                e.Row.GetIndex() == items.Count - 1)
            {
                // 如果是最后一行，则添加新的一行
                AddBlankRow();
            }
        }

        private void AddBlankRow()
        {
            items.Add(new MyItem { DrawIB = "", Alias = "" });
        }


        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            // 执行你想要在这个页面被关闭或导航离开时运行的代码

            //保存全局设置因为要保存滑条透明度
            MainConfig.GameCfg.Value.WorkPageBackGroundImageOpacity = (float)WorkBGImageBrush.Opacity;
            //MainConfig.SaveConfig(MainConfig.ConfigFiles.Game_Setting);
            MainConfig.GameCfg.SaveConfig();

            // 如果需要，可以调用基类的 OnNavigatedFrom 方法
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
            // 获取触发事件的 ComboBox 实例
            var comboBox = sender as ComboBox;

            // 检查是否有新选中的项
            if (comboBox != null && comboBox.SelectedItem != null)
            {
                // 执行你想要的操作，例如获取选中的项并进行处理
                string selectedGame = comboBox.SelectedItem.ToString();

                //MainConfig.SetCurrentGame(selectedGame);
                //MainConfig.SetConfig(MainConfig.ConfigFiles.Main, "GameName", selectedGame);
                //MainConfig.SaveConfig(MainConfig.ConfigFiles.Main);
                MainConfig.MainCfg.Value.GameName = selectedGame;
                MainConfig.MainCfg.SaveConfig();

                SetDefaultBackGroundImage();
                InitializeWorkSpace(MainConfig.CurrentWorkSpace);

                //依次检测并判断是否显示对应启动按钮
                if (!File.Exists(MainConfig.Path_3Dmigoto_Loader_EXE))
                {
                    Menu_Open3DmigotoLoaderEXE.Visibility = Visibility.Collapsed;
                }
                if (!File.Exists(MainConfig.Path_3Dmigoto_Loader_PY))
                {
                    Menu_Open3DmigotoLoaderPY.Visibility = Visibility.Collapsed;
                }
                if (!File.Exists(MainConfig.Path_3Dmigoto_Loader_ByPassACE_EXE))
                {
                    Menu_Open3DmigotoLoaderByPassACE.Visibility = Visibility.Collapsed;
                }

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
                //MainConfig.SetConfig(MainConfig.ConfigFiles.Main, "WorkSpaceName", ComboBoxWorkSpaceSelection.Text);
                //MainConfig.SaveConfig(MainConfig.ConfigFiles.Main);
                MainConfig.MainCfg.Value.WorkSpaceName = ComboBoxWorkSpaceSelection.Text;
                MainConfig.MainCfg.SaveConfig();
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
            //如果存储工作空间的output文件夹不存在就创建
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
            //MainConfig.SaveConfig(MainConfig.ConfigFiles.Main);
            MainConfig.MainCfg.SaveConfig();

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

                if (MainConfig.TextureCfg.Value.ConvertDedupedTextures)
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
                await MessageHelper.Show("请先选择工作空间");
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

        public async void OpenLatestFrameAnalysisFolder(object sender, RoutedEventArgs e)
        {
            string latestFrameAnalysisFolder = PathHelper.GetLatestFrameAnalysisFolder();
            if (!string.IsNullOrEmpty(latestFrameAnalysisFolder))
            {
                await CommandHelper.ShellOpenFolder(latestFrameAnalysisFolder);
            }
            else
            {
                await MessageHelper.Show("目标目录没有任何FrameAnalysis文件夹", "Target directory didn't have any FrameAnalysisFolder.");
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
                await MessageHelper.Show("没有找到任何FrameAnalysis文件夹", "Target directory didn't have any FrameAnalysisFolder.");
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
                await MessageHelper.Show("目标目录没有任何FrameAnalysis文件夹", "Target directory didn't have any FrameAnalysisFolder.");
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

            //逆向提取之前要保存DrawIB列表。
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
            //混淆并返回新的ini文件的路径
            string NewModInIPath = await EncryptionHelper.Obfuscate_ModFileName("Play");
            if (NewModInIPath == "")
            {
                return;
            }
            //调用加密Buffer并加密ini文件
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
                    await MessageHelper.Show("目标路径中不能含有中文字符", "Target Path Can't Contains Chinese.");
                    return;
                }

                //判断目标路径下是否有ini文件
                // 使用Directory.GetFiles方法，并指定搜索模式为*.ini
                string[] iniFiles = Directory.GetFiles(selected_folder_path, "*.ini");
                if (iniFiles.Length == 0)
                {
                    await MessageHelper.Show("目标路径中无法找到mod的ini文件", "Target Path Can't find ini file.");
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



        private async void Open3DmigotoLoaderEXE(object sender, RoutedEventArgs e)
        {
            await CommandHelper.ShellOpenFile(MainConfig.Path_3Dmigoto_Loader_EXE);
        }

        private async void Open3DmigotoLoaderPY(object sender, RoutedEventArgs e)
        {
            await CommandHelper.ShellOpenFile(MainConfig.Path_3Dmigoto_Loader_PY);
        }

        private async void Open3DmigotoLoaderByPassACE(object sender, RoutedEventArgs e)
        {
            await CommandHelper.ShellOpenFile(MainConfig.Path_3Dmigoto_Loader_ByPassACE_EXE);
        }

        private async void OpenD3dxIniFile(object sender, RoutedEventArgs e)
        {
            await CommandHelper.ShellOpenFile(MainConfig.Path_D3DXINI);
        }

        private async void Open3DmigotoFolder(object sender, RoutedEventArgs e)
        {

            await CommandHelper.ShellOpenFolder(MainConfig.Path_LoaderFolder);
        }

        private async void OpenShaderFixesFolder(object sender, RoutedEventArgs e)
        {

            await CommandHelper.ShellOpenFolder(Path.Combine(MainConfig.Path_LoaderFolder, "ShaderFixes\\"));
        }

    }
}
