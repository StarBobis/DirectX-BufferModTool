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
using System.Diagnostics;
using DBMT_Core.Utils;
using DBMT_Core.GridViewItems;
using DBMT_Core.Games;

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
        private ObservableCollection<DrawIBItem> DrawIBItems = new ObservableCollection<DrawIBItem>();

        public WorkPage()
        {
            this.InitializeComponent();

            this.Loaded += OnMyCustomPageLoaded;

        }

        private void OnMyCustomPageLoaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("WorkPageLoadStart");
            try
            {
                GlobalConfig.ReadConfig();


                //如果此时没有Main.json就保存配置
                if (Path.Exists(GlobalConfig.Path_MainConfig))
                {
                    GlobalConfig.SaveConfig();
                }

                //初始化游戏列表，会触发工作空间改变
                InitializeGameNameComboBox();

                Debug.WriteLine("切换到工作空间页面，当前工作空间: " + GlobalConfig.CurrentWorkSpace);
                InitializeWorkSpace(GlobalConfig.CurrentWorkSpace);

                // 添加一个空白行作为初始数据
                DrawIBItems.Add(new DrawIBItem { DrawIB = "", Alias = "" });

                // 设置 DataGrid 的数据源
                myDataGrid.ItemsSource = DrawIBItems;

            }
            catch (Exception ex)
            {
                _ = MessageHelper.Show(this.XamlRoot, "Error: " + ex.ToString());
            }
        }


        private void MyDataGrid_CellEditEnding(object sender, CommunityToolkit.WinUI.UI.Controls.DataGridCellEditEndingEventArgs e)
        {
            // 检查是否是最后一行被编辑
            if (e.EditAction == CommunityToolkit.WinUI.UI.Controls.DataGridEditAction.Commit &&
                e.Row.GetIndex() == DrawIBItems.Count - 1)
            {
                // 如果是最后一行，则添加新的一行
                AddBlankRow();
            }


        }

        private void AddBlankRow()
        {
            DrawIBItems.Add(new DrawIBItem { DrawIB = "", Alias = "" });
        }

        private void InitializeGameNameComboBox()
        {
            Debug.WriteLine("InitializeGameNameComboBox::Start");
            List<GameIconItem> gameIconItems = DBMTResourceUtils.GetGameIconItems();

            WorkGameSelectionComboBox.Items.Clear();
            foreach (GameIconItem gameIconItem in gameIconItems)
            {
                WorkGameSelectionComboBox.Items.Add(gameIconItem.GameName);
            }

            if (GlobalConfig.CurrentGameName == "")
            {
                WorkGameSelectionComboBox.SelectedIndex = 0;
            }
            else
            {
                WorkGameSelectionComboBox.SelectedItem = GlobalConfig.CurrentGameName;
            }
            Debug.WriteLine("InitializeGameNameComboBox::End");

        }

        private void InitializeRunButton()
        {
            //依次检测并判断是否显示对应启动按钮
            if (!File.Exists(GlobalConfig.Path_3Dmigoto_Loader_EXE))
            {
                Menu_Open3DmigotoLoaderEXE.Visibility = Visibility.Collapsed;
            }
            if (!File.Exists(GlobalConfig.Path_3Dmigoto_Loader_PY))
            {
                Menu_Open3DmigotoLoaderPY.Visibility = Visibility.Collapsed;
            }
            if (!File.Exists(GlobalConfig.Path_3Dmigoto_Loader_ByPassACE_EXE))
            {
                Menu_Open3DmigotoLoaderByPassACE.Visibility = Visibility.Collapsed;
            }
        }



        private async void WorkGameSelectionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Debug.WriteLine("WorkGameSelectionComboBox_SelectionChanged::Start");
            // 获取触发事件的 ComboBox 实例
            var comboBox = sender as ComboBox;

            // 检查是否有新选中的项
            if (comboBox != null && comboBox.SelectedItem != null)
            {
                // 执行你想要的操作，例如获取选中的项并进行处理
                string selectedGame = comboBox.SelectedItem.ToString();
                GlobalConfig.CurrentGameName = selectedGame;

                // 背景图切换到当前游戏的背景图
                string BackgroundPath = Path.Combine(GlobalConfig.Path_AssetsGamesFolder, selectedGame + "\\Background.png");
                if (!File.Exists(BackgroundPath))
                {
                    BackgroundPath = Path.Combine(GlobalConfig.Path_AssetsGamesFolder, "DefaultBackground.png");
                }
                MainWindow.CurrentWindow.mainWindowImageBrush.Source = new BitmapImage(new Uri(BackgroundPath));

                //读取并设置当前3Dmigoto文件夹，如果没读取到则弹出对话框提醒。
                if (File.Exists(GlobalConfig.Path_CurrentGameMainConfigJsonFile))
                {
                    JObject jObject = DBMTJsonUtils.ReadJObjectFromFile(GlobalConfig.Path_CurrentGameMainConfigJsonFile);
                    string MigotoFolder = (string)jObject["MigotoFolder"];
                    GlobalConfig.CurrentGameMigotoFolder = MigotoFolder;
                }
                else
                {
                    GlobalConfig.CurrentGameMigotoFolder = "";
                    await MessageHelper.Show(this.XamlRoot,"您当前选中的游戏尚未设置3Dmigoto文件夹，请到主页进行设置。");

                    if (MainWindow.CurrentWindow.navigationView.MenuItems.Count > 0)
                    {
                        MainWindow.CurrentWindow.navigationView.SelectedItem = MainWindow.CurrentWindow.navigationView.MenuItems[0];
                        Frame.Navigate(typeof(HomePage));
                    }
                }


                GlobalConfig.SaveConfig();


                //切换游戏后，要读取当前游戏的配置，来确定当前选择的是哪个工作空间
                //如果没有就算了

                string SavedWorkSpace = "";
                if (File.Exists(GlobalConfig.Path_CurrentGameMainConfigJsonFile))
                {
                    JObject jobj = DBMTJsonUtils.ReadJObjectFromFile(GlobalConfig.Path_CurrentGameMainConfigJsonFile);
                    if (jobj.ContainsKey("WorkSpace"))
                    {
                        SavedWorkSpace = (string)jobj["WorkSpace"];
                    }
                }

                if (SavedWorkSpace == "")
                {
                    InitializeWorkSpace();
                }
                else
                {
                    string TargetWorkSpaceFolder = Path.Combine(GlobalConfig.Path_TotalWorkSpaceFolder, GlobalConfig.CurrentGameName + "\\" + SavedWorkSpace + "\\");
                    if (Directory.Exists(TargetWorkSpaceFolder))
                    {
                        InitializeWorkSpace(SavedWorkSpace);
                    }
                    else
                    {
                        InitializeWorkSpace();
                    }
                }


                InitializeRunButton();
            }

            Debug.WriteLine("WorkGameSelectionComboBox_SelectionChanged::End");

        }




        private void ComboBoxWorkSpaceSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Debug.WriteLine("切换工作空间::Start");
            if (ComboBoxWorkSpaceSelection.Items.Contains(ComboBoxWorkSpaceSelection.SelectedItem))
            {
                //切换游戏前，保存当前的工作空间
                GlobalConfig.CurrentWorkSpace = (string)ComboBoxWorkSpaceSelection.SelectedItem;
                GlobalConfig.SaveConfig();

                //并且要把当前工作空间保存到当前游戏的配置里
                if (File.Exists(GlobalConfig.Path_CurrentGameMainConfigJsonFile))
                {
                    JObject jobj = DBMTJsonUtils.ReadJObjectFromFile(GlobalConfig.Path_CurrentGameMainConfigJsonFile);
                    jobj["WorkSpace"] = GlobalConfig.CurrentWorkSpace;
                    DBMTJsonUtils.SaveJObjectToFile(jobj, GlobalConfig.Path_CurrentGameMainConfigJsonFile);
                }

                ReadDrawIBListFromWorkSpace();
            }
            Debug.WriteLine("切换工作空间::End");

        }

        void ReadDrawIBListFromWorkSpace()
        {
            Debug.WriteLine("读取DrawIB列表::Start");
            if (GlobalConfig.CurrentWorkSpace != "")
            {
                DrawIBItems.Clear();
                

                string Configpath = GlobalConfig.Path_CurrentWorkSpaceFolder + "Config.json";
                Debug.WriteLine("读取配置文件路径: " + Configpath);

                if (File.Exists(Configpath))
                {
                    //切换到对应配置
                    string jsonData = File.ReadAllText(Configpath);
                    JArray DrawIBListJArray = JArray.Parse(jsonData);

                    foreach (JObject jkobj in DrawIBListJArray)
                    {
                        DrawIBItem item = new DrawIBItem();
                        item.DrawIB = (string)jkobj["DrawIB"];
                        item.Alias = (string)jkobj["Alias"];
                        DrawIBItems.Add(item);
                    }

                }

                //保底确保用户可以编辑
                //确保永远有一个新的空行可供编辑
                AddBlankRow();
            }
            Debug.WriteLine("读取DrawIB列表::End");

        }

        public void InitializeWorkSpace(string WorkSpaceName = "")
        {
            Debug.WriteLine("初始化工作空间::Start");
            Debug.WriteLine("工作空间名称: " + WorkSpaceName);
            GlobalConfig.CurrentWorkSpace = WorkSpaceName;

            if (!Directory.Exists(GlobalConfig.Path_CurrentWorkSpaceFolder))
            {
                Debug.WriteLine("创建工作空间文件夹: " + GlobalConfig.Path_CurrentWorkSpaceFolder);
                Directory.CreateDirectory(GlobalConfig.Path_CurrentWorkSpaceFolder);
            }

            ComboBoxWorkSpaceSelection.Items.Clear();

            string[] WorkSpaceNameList = DBMTFileUtils.ReadWorkSpaceNameList(GlobalConfig.Path_CurrentGameTotalWorkSpaceFolder);
            foreach (string WorkSpaceNameItem in WorkSpaceNameList)
            {
                ComboBoxWorkSpaceSelection.Items.Add(WorkSpaceNameItem);
            }

            if (ComboBoxWorkSpaceSelection.Items.Count >= 1)
            {
                if (WorkSpaceName != "" && ComboBoxWorkSpaceSelection.Items.Contains(WorkSpaceName))
                {
                    ComboBoxWorkSpaceSelection.SelectedItem = WorkSpaceName;
                }
                //判断当前WorkSpace是否在Items里，如果在的话就设为当前工作空间
                else if (ComboBoxWorkSpaceSelection.Items.Contains(GlobalConfig.CurrentWorkSpace))
                {
                    ComboBoxWorkSpaceSelection.SelectedItem = GlobalConfig.CurrentWorkSpace;
                }
                else
                {
                    //否则默认选择第一个工作空间
                    Debug.WriteLine("默认选择第一个工作空间");
                    ComboBoxWorkSpaceSelection.SelectedItem = ComboBoxWorkSpaceSelection.Items[0];
                }
                //ReadDrawIBListFromWorkSpace();
            }

            Debug.WriteLine("初始化工作空间::End");

        }

        public async void CreateWorkSpaceFolder(object sender, RoutedEventArgs e)
        {
            if (ComboBoxWorkSpaceSelection.Text.Trim() == "")
            {
                await MessageHelper.Show("工作空间名称不能为空","WorkSpace name can't be empty.");
                return;
            }


            if (!ComboBoxWorkSpaceSelection.Items.Contains(ComboBoxWorkSpaceSelection.Text))
            {
                ////如果包含了此命名空间，就不创建文件夹，否则就创建
                Directory.CreateDirectory(GlobalConfig.Path_CurrentGameTotalWorkSpaceFolder + ComboBoxWorkSpaceSelection.Text);

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
            try
            {
                bool confirm = await MessageHelper.ShowConfirm("请再次确认是否清除当前工作空间","Please confirm if you want to clean WorkSpace");
                if (confirm)
                {
                    string WorkSpaceFolderPath = GlobalConfig.Path_CurrentGameTotalWorkSpaceFolder + ComboBoxWorkSpaceSelection.Text;
                    Directory.Delete(WorkSpaceFolderPath, true);
                    Directory.CreateDirectory(WorkSpaceFolderPath);
                    InitializeWorkSpace();
                    _ = MessageHelper.Show("清理成功", "Clean Success");
                }
                
            }
            catch(Exception ex)
            {
                _ = MessageHelper.Show(ex.ToString());
            }
           
        }

        public void OpenCurrentWorkSpaceFolder(object sender, RoutedEventArgs e)
        {
            string WorkSpaceOutputFolder = GlobalConfig.Path_CurrentGameTotalWorkSpaceFolder + ComboBoxWorkSpaceSelection.Text + "\\";
            if (!string.IsNullOrEmpty(WorkSpaceOutputFolder))
            {
                if (Directory.Exists(WorkSpaceOutputFolder))
                {
                    _ = CommandHelper.ShellOpenFolder(WorkSpaceOutputFolder);
                }
                else
                {
                    _ = MessageHelper.Show("此目录不存在，请检查您的Output文件夹是否设置正确", "This folder doesn't exists,please check if your OutputFolder is correct.");
                }
            }
        }

        public void SaveDrawIBListConfigToFolder(string SaveFolderPath)
        {
            if (Directory.Exists(SaveFolderPath))
            {
                bool AllEmpry = true;

                JArray DrawIBJarrayList = new JArray();
                foreach (DrawIBItem item in DrawIBItems)
                {
                    if (item.DrawIB.Trim() != "")
                    {
                        JObject jobj = new JObject();
                        jobj["DrawIB"] = item.DrawIB;
                        jobj["Alias"] = item.Alias;
                        DrawIBJarrayList.Add(jobj);


                        if (item.DrawIB.Trim() != "")
                        {
                            AllEmpry = false;
                        }
                        else if (item.Alias.Trim() != "")
                        {
                            AllEmpry = false;
                        }
                    }
                }

                //只有不为空时才保存。

                if (!AllEmpry)
                {
                    string json_string = DrawIBJarrayList.ToString(Formatting.Indented);
                    // 将JSON字符串写入文件
                    File.WriteAllText(SaveFolderPath + "Config.Json", json_string);
                }
            
            }
       
        }


        /// <summary>
        /// 这个DrawIB列表必须得保存到配置文件，因为后面Blender导入模型的时候会用到。
        /// 除非更改Blender逻辑导入所有的模型。
        /// </summary>
        public async void SaveDrawIBList()
        {
            Debug.WriteLine("保存当前DrawIB列表::Start");
            //(1) 检查游戏类型是否设置
            if (GlobalConfig.CurrentGameName == "")
            {
                await MessageHelper.Show("请先选择游戏类型", "Please select a game before this.");
                return;
            }

            if (GlobalConfig.CurrentWorkSpace == "")
            {
                await MessageHelper.Show("请先选择工作空间", "Please select a workspace before this.");
                return;
            }

            Debug.WriteLine("当前工作空间: " + GlobalConfig.CurrentWorkSpace);

            //(2) 接下来要把当前的游戏名称+类型保存到MainSetting.json里
            GlobalConfig.SaveConfig();

            //(3) 接下来把所有的drawIBList中的DrawIB保留下来存储到对应配置文件。
            SaveDrawIBListConfigToFolder(GlobalConfig.Path_CurrentWorkSpaceFolder);

            Debug.WriteLine("保存当前DrawIB列表::End");
            Debug.WriteLine("----------------------------------");
        }



        public async void CleanDrawIBList(object sender, RoutedEventArgs e)
        {
            bool confirm = await MessageHelper.ShowConfirm("请再次确认是否清除当前DrawIB列表","Please confirm if you want to clean DrawIB list");
            if (confirm)
            {
                DrawIBItems.Clear();
                AddBlankRow();
            }
        }


        public async Task<bool> PreCheckBeforeExtract()
        {
            bool findvalidDrawIB = false;
            foreach (DrawIBItem item in DrawIBItems)
            {
                if (item.DrawIB.Trim() != "")
                {
                    findvalidDrawIB = true;
                    break;
                }
            }

            if (!findvalidDrawIB)
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

        private void AutoDetectPointlistDrawIBList_UnityVSPreSkinning(object sender, RoutedEventArgs e)
        {
            CoreFunctions.DetectPointlistDrawIBList();
            InitializeWorkSpace();
            _ = MessageHelper.Show("检测Pointlist DrawIB列表成功","Read DrawIB List Success");
        }



        public async void ExtractModel(object sender, RoutedEventArgs e)
        {
            bool Prepare = await PreCheckBeforeExtract();
            if (!Prepare)
            {
                return;
            }

            SaveDrawIBList();

            List<DrawIBItem> DrawIBItemList = [];
            foreach (DrawIBItem drawIBItem in DrawIBItems)
            {
                DrawIBItemList.Add(drawIBItem);
            }

            bool RunResult = CoreFunctions.ExtractModel(DrawIBItemList);
            if (RunResult)
            {
                OpenCurrentWorkSpaceFolder(sender, e);
                TextureHelper.ConvertDedupedTexturesToTargetFormat();
                await CoreFunctions.PostDoAfterExtract();
                _ = TextureHelper.ConvertAutoExtractedTexturesInDrawIBFolderToTargetFormat();
            }
            else
            {
                OpenLatestLogFile(sender, e);
            }
        }



        public async void OpenWorkSpaceGenerateModFolder(object sender, RoutedEventArgs e)
        {

            if (Directory.Exists(GlobalConfig.Path_CurrentWorkSpaceGeneratedModFolder))
            {
                await CommandHelper.ShellOpenFolder(GlobalConfig.Path_CurrentWorkSpaceGeneratedModFolder);
            }
            else
            {
                await MessageHelper.Show("您还未生成任何Mod", "You have not generate any mod yet");
            }
        }

        public async void OpenModsFolder(object sender, RoutedEventArgs e)
        {
            string modsFolder = Path.Combine(GlobalConfig.Path_LoaderFolder , "Mods\\");
            if (Directory.Exists(modsFolder))
            {
                await CommandHelper.ShellOpenFolder(modsFolder);
            }
            else
            {
                await MessageHelper.Show("此目录不存在，请检查您的Mods文件夹是否设置正确：" +modsFolder , "This path didn't exists, please check if your Mods folder is correct");
            }
        }

        public async void OpenLatestFrameAnalysisFolder(object sender, RoutedEventArgs e)
        {
            string latestFrameAnalysisFolder = GlobalConfig.Path_LatestFrameAnalysisFolder;
            Debug.WriteLine("latestFrameAnalysisFolder: " + latestFrameAnalysisFolder);

            if (latestFrameAnalysisFolder.Trim() == "\\")
            {
                await MessageHelper.Show("目标目录没有任何FrameAnalysis文件夹", "Target directory didn't have any FrameAnalysisFolder.");
            }
            else
            {
                if (!string.IsNullOrEmpty(latestFrameAnalysisFolder))
                {
                    await CommandHelper.ShellOpenFolder(latestFrameAnalysisFolder);
                }
                else
                {
                    await MessageHelper.Show("目标目录没有任何FrameAnalysis文件夹", "Target directory didn't have any FrameAnalysisFolder.");
                }
            }
        }


        public async void OpenLatestFrameAnalysisLogTxtFile(object sender, RoutedEventArgs e)
        {
            string LatestFrameAnalysisFolderLogTxtFilePath = GlobalConfig.Path_LatestFrameAnalysisLogTxt;

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
            string LatestFrameAnalysisDedupedFolder = GlobalConfig.Path_LatestFrameAnalysisDedupedFolder;
            if (!string.IsNullOrEmpty(LatestFrameAnalysisDedupedFolder))
            {
                await CommandHelper.ShellOpenFolder(LatestFrameAnalysisDedupedFolder);
            }
            else
            {
                await MessageHelper.Show("目标目录没有任何FrameAnalysis文件夹", "Target directory didn't have any FrameAnalysisFolder.");
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


        public void OpenLatestLogFile(object sender, RoutedEventArgs e)
        {
            string LogFilePath = GlobalConfig.Path_LatestDBMTLogFile;
            if (File.Exists(LogFilePath))
            {
                _ = CommandHelper.ShellOpenFile(LogFilePath);
            }
        }

        public void OpenConfigsFolder(object sender, RoutedEventArgs e)
        {
            Task.Run(() => {
                _ = CommandHelper.ShellOpenFolder(GlobalConfig.Path_ConfigsFolder);
            });
        }


        public async void CleanSkipIBListTextBox(object sender, RoutedEventArgs e)
        {
            bool confirm = await MessageHelper.ShowConfirm("请再次确认是否清除当前SkipIB列表","Please confirm if you want to clean SkipIB list");
            if (confirm)
            {
                TextBoxSkipIBList.Text = "";
            }
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
            List<string> SkipIBList = GetSkipIBList();
            DrawIBHelper.GenerateSkipIB(SkipIBList);
            await CommandHelper.ShellOpenFolder(GlobalConfig.Path_CurrentWorkSpaceGeneratedModFolder);
        }


        public async void ExecuteGenerateVSCheck(object sender, RoutedEventArgs e)
        {
            List<string> SkipIBList = GetSkipIBList();
            DrawIBHelper.GenerateVSCheck(SkipIBList,"SkipIB_VSCheck");
            await CommandHelper.ShellOpenFolder(GlobalConfig.Path_CurrentWorkSpaceGeneratedModFolder);
        }

        private async void Open3DmigotoLoaderEXE(object sender, RoutedEventArgs e)
        {
            await CommandHelper.ShellOpenFile(GlobalConfig.Path_3Dmigoto_Loader_EXE);
        }

        private async void Open3DmigotoLoaderPY(object sender, RoutedEventArgs e)
        {
            await CommandHelper.ShellOpenFile(GlobalConfig.Path_3Dmigoto_Loader_PY);
        }

        private async void Open3DmigotoLoaderByPassACE(object sender, RoutedEventArgs e)
        {
            await CommandHelper.ShellOpenFile(GlobalConfig.Path_3Dmigoto_Loader_ByPassACE_EXE);
        }

        private async void OpenD3dxIniFile(object sender, RoutedEventArgs e)
        {
            await CommandHelper.ShellOpenFile(GlobalConfig.Path_D3DXINI);
        }

        private async void Open3DmigotoFolder(object sender, RoutedEventArgs e)
        {

            await CommandHelper.ShellOpenFolder(GlobalConfig.Path_LoaderFolder);
        }

        private async void OpenShaderFixesFolder(object sender, RoutedEventArgs e)
        {

            await CommandHelper.ShellOpenFolder(Path.Combine(GlobalConfig.Path_LoaderFolder, "ShaderFixes\\"));
        }



        private void Menu_MoveDrawIBRelatedFiles_Click(object sender, RoutedEventArgs e)
        {
            SaveDrawIBList();

            //1.获取当前DrawIBList
            List<string> DrawIBList = DrawIBConfig.GetDrawIBListFromConfig();

            try
            {
                //2.拼接目标目录名称
                string IBRelatedFolderName = "DrawIBRelatedFiles" + "_" + GlobalConfig.CurrentGameName;
                string IBRelatedFolderPath = GlobalConfig.Path_CurrentWorkSpaceFolder + IBRelatedFolderName + "\\";
                Directory.CreateDirectory(IBRelatedFolderPath);

                SaveDrawIBListConfigToFolder(IBRelatedFolderPath);
                CoreFunctions.MoveDrawIBRelatedFiles(DrawIBList, IBRelatedFolderName);

                _ = CommandHelper.ShellOpenFolder(IBRelatedFolderPath);

            }
            catch (Exception ex)
            {
                _ = MessageHelper.Show(ex.ToString(), ex.ToString());
            }

            //_ = MessageHelper.Show("移动DrawIB相关文件成功","Move DrawIB Related Files Success.");
        }

        private void Menu_AutoDetect_CPUPreSkinning_DrawIBList_Click(object sender, RoutedEventArgs e)
        {
            CoreFunctions.DetectTrianglelistDrawIBList();
            InitializeWorkSpace();
            _ = MessageHelper.Show("检测Trianglelist DrawIB列表成功", "Read DrawIB List Success");
        }

        private void Menu_ExtractTextureFiles_Click(object sender, RoutedEventArgs e)
        {
            SaveDrawIBList();
            CoreFunctions.ExtractTextures();
            TextureHelper.ConvertDedupedTexturesToTargetFormat();
            _ = CommandHelper.ShellOpenFolder(GlobalConfig.Path_CurrentWorkSpaceFolder);
        }

        private async void Menu_ConvertDDSToTargetFormat_Click(object sender, RoutedEventArgs e)
        {
            string selected_folder_path = await CommandHelper.ChooseFolderAndGetPath();
            if (selected_folder_path == "")
            {
                return;
            }

            try
            {
                TextureHelper.ConvertAllTexturesIntoConvertedTextures(selected_folder_path);

                _ = CommandHelper.ShellOpenFolder(selected_folder_path + "\\ConvertedTextures\\");

            }
            catch (Exception ex)
            {
                _ = MessageHelper.Show(ex.ToString());
            }


        }

        private void Menu_ExtractDedupedTextures_Click(object sender, RoutedEventArgs e)
        {
            SaveDrawIBList();
            CoreFunctions.ExtractDedupedTextures();
            TextureHelper.ConvertDedupedTexturesToTargetFormat();
            _ = CommandHelper.ShellOpenFolder(GlobalConfig.Path_CurrentWorkSpaceFolder);
        }

        private void Menu_ExtractTrianglelistTextures_Click(object sender, RoutedEventArgs e)
        {
            SaveDrawIBList();
            CoreFunctions.ExtractTrianglelistTextures();
            _ = CommandHelper.ShellOpenFolder(GlobalConfig.Path_CurrentWorkSpaceFolder);
        }

        private void Menu_ExtractRenderTextures_Click(object sender, RoutedEventArgs e)
        {
            SaveDrawIBList();
            CoreFunctions.ExtractRenderTextures();
            _ = CommandHelper.ShellOpenFolder(GlobalConfig.Path_CurrentWorkSpaceFolder);
        }

        private void Menu_OpenPluginsFolder_Click(object sender, RoutedEventArgs e)
        {
            _ = CommandHelper.ShellOpenFolder(GlobalConfig.Path_PluginsFolder);
        }



        private void myDataGrid_CurrentCellChanged(object sender, EventArgs e)
        {

            //当前Cell改变时，会触发保存，但是由于在切换工作空间时也会触发这个
            //所以应该加上条件判断。或者这个东西应该在页面离开时保存而不是每次都保存。
            //或者如果是空的就不保存。

            //Debug.WriteLine("CurrentCellChanged::Start");
            SaveDrawIBList();
            //Debug.WriteLine("CurrentCellChanged::End");
        }

        private void Menu_GameTypeFolder_Click(object sender, RoutedEventArgs e)
        {
            string CurrentGameTypeFolder = Path.Combine(GlobalConfig.Path_GameTypeConfigsFolder, GlobalConfig.CurrentGameName + "\\");
            if (Directory.Exists(CurrentGameTypeFolder))
            {
                _ = CommandHelper.ShellOpenFolder(CurrentGameTypeFolder);
            }
        }


        private async void Button_SaveDrawIBList_Click(object sender, RoutedEventArgs e)
        {
            SaveDrawIBList();
            await MessageHelper.Show("保存成功");
        }



        private void Button_CleanLastExtract_Click(object sender, RoutedEventArgs e)
        {
            List<string> DrawIBList = DrawIBConfig.GetDrawIBListFromConfig();
            try
            {
                foreach (string DrawIB in DrawIBList)
                {
                
                        string DrawIBOutputFolder = Path.Combine(GlobalConfig.Path_CurrentWorkSpaceFolder, DrawIB + "\\");
                        if (Directory.Exists(DrawIBOutputFolder))
                        {
                            Directory.Delete(DrawIBOutputFolder,true);
                        }

               
                }

                _ = MessageHelper.Show("清理完成");
            }
            catch (Exception ex)
            {
                _ = MessageHelper.Show("清理失败，文件可能被进程占用: " + ex.ToString());
            }
        }
    }
}
