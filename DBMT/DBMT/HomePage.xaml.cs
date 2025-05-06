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
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage.Pickers;
using Windows.Storage;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using WinRT.Interop;
using Windows.UI.Core;
using Windows.Storage.Pickers.Provider;
using DBMT_Core;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using Windows.UI.Popups;
using DBMT;
using System.Threading.Tasks;
using DBMT_Core.GridViewItems;
using System.Collections.ObjectModel;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml.Hosting;
using System.Numerics;
using DBMT_Core.Utils;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DBMT
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HomePage : Page
    {
        private ObservableCollection<GameIconItem> GameIconItemList = new ObservableCollection<GameIconItem>();
        private Compositor compositor;
        private Visual imageVisual;

        public HomePage()
        {
            this.InitializeComponent();

            // 初始化Composition组件
            // 获取Image控件的Visual对象
            imageVisual = ElementCompositionPreview.GetElementVisual(MainWindow.CurrentWindow.mainWindowImageBrush);
            // 获取Compositor实例
            compositor = imageVisual.Compositor;


            //这里也要读取一次配置，让用户第一次打开就会生成Main.json
            //如果DBMT路径为空，那么设为默认的
            GlobalConfig.MainCfg.Value.DBMTLocation = GlobalConfig.Path_Base;
            GlobalConfig.MainCfg.SaveConfig();

            GameIconGridView.ItemsSource = GameIconItemList;

            InitializeGameIconList();

            //根据当前配置中存储的游戏名称，依次匹配GameIconItemList
            int index = 0;
            foreach (GameIconItem gameIconItem in GameIconItemList)
            {
                if (gameIconItem.GameName == GlobalConfig.MainCfg.Value.GameName)
                {
                    GameIconGridView.SelectedIndex = index;
                    break;
                }
                index += 1;
            }

        }

  
        
        public void InitializeGameIconList()
        {
            GameIconItemList.Clear();

            List<GameIconItem> gameIconItems = DBMTResourceUtils.GetGameIconItems();

            foreach (GameIconItem gameIconItem in gameIconItems)
            {
                GameIconItemList.Add(gameIconItem);
            }

        }

        private void CreateFadeAnimation()
        {
            // 创建一个淡入淡出动画
            var fadeAnimation = compositor.CreateScalarKeyFrameAnimation();
            fadeAnimation.InsertKeyFrame(0.0f, 0.0f); // 初始透明度0%
            fadeAnimation.InsertKeyFrame(1.0f, 1.0f); // 目标透明度100%
            fadeAnimation.Duration = TimeSpan.FromMilliseconds(500); // 动画持续时间300毫秒
            fadeAnimation.DelayBehavior = AnimationDelayBehavior.SetInitialValueBeforeDelay; // 动画延迟行为

            // 应用动画到Image的Visual对象的Opacity属性
            imageVisual.StartAnimation("Opacity", fadeAnimation);
        }

        private void CreateScaleAnimation()
        {
            // 创建一个缩放动画
            var scaleAnimation = compositor.CreateVector3KeyFrameAnimation();
            scaleAnimation.InsertKeyFrame(0.0f, new Vector3(1.05f, 1.05f, 1.05f)); // 初始缩放比例110%
            scaleAnimation.InsertKeyFrame(1.0f, new Vector3(1.0f, 1.0f, 1.0f)); // 目标缩放比例100%
            scaleAnimation.Duration = TimeSpan.FromMilliseconds(500); // 动画持续时间300毫秒
            scaleAnimation.DelayBehavior = AnimationDelayBehavior.SetInitialValueBeforeDelay; // 动画延迟行为

            // 应用动画到Image的Visual对象的Scale属性
            imageVisual.StartAnimation("Scale", scaleAnimation);

        }


        public GameIconItem GetCurrentSelectedGameIconItem()
        {
            if (GameIconGridView.SelectedItem != null)
            {

                int index = GameIconGridView.SelectedIndex;
                GameIconItem gameIconItem = GameIconItemList[index];
                return gameIconItem;
            }
            else
            {
                return null;
            }
        }

        private void GameIconGridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GameIconItem gameIconItem = GetCurrentSelectedGameIconItem();
            if (gameIconItem == null)
            {
                return;
            }
            
            //设置当前游戏并且保存
            GlobalConfig.MainCfg.Value.GameName = gameIconItem.GameName;

            // 背景图切换到当前游戏的背景图
            string BackgroundPath = Path.Combine(GlobalConfig.Path_Base, "Assets/GameBackground/" + gameIconItem.GameName + ".png");
            CreateScaleAnimation();
            CreateFadeAnimation();
            MainWindow.CurrentWindow.mainWindowImageBrush.Source = gameIconItem.GameBackGroundImage;

            //如果存在配置文件存储了3Dmigoto路径则读取，如果没有就算了
            if (File.Exists(GlobalConfig.Path_CurrentGameMainConfigJsonFile))
            {
                JObject jObject = DBMTJsonUtils.ReadJObjectFromFile(GlobalConfig.Path_CurrentGameMainConfigJsonFile);
                string MigotoFolder = (string)jObject["MigotoFolder"];
                TextBox_3DmigotoPath.Text = MigotoFolder;
                GlobalConfig.MainCfg.Value.CurrentGameMigotoFolder = MigotoFolder;

                //读取d3dx.ini中的配置
                ReadPathSettingFromD3dxIni(Path.Combine(MigotoFolder, "d3dx.ini"));
            }
            else
            {
                //如果没有，那必须清空当前的所有配置
                TextBox_3DmigotoPath.Text = "";

                ProcessPathTextBox.Text = "";
                StarterPathTextBox.Text = "";
                TextBox_LaunchArgs.Text = "";

                GlobalConfig.MainCfg.Value.CurrentGameMigotoFolder = "";
            }

            //必须保存配置，因为在其它页面中可能会重新读取配置，导致当前状态的配置丢失。
            GlobalConfig.MainCfg.SaveConfig();

            //切换游戏时，需要保存填写好的进程路径、启动路径、启动参数
            SaveLaunchArgs();
            SaveLaunchPath();
            SaveTargetPath();
        }


        private async void ChooseProcessPathButtonClick(object sender, RoutedEventArgs e)
        {
            string filepath = await CommandHelper.ChooseFileAndGetPath(".exe");
            if (filepath != "")
            {
                ProcessPathTextBox.Text = filepath;
                SaveTargetPath();
            }
        }

        private async void ChooseStarterPathButtonClick(object sender, RoutedEventArgs e)
        {
            string filepath = await CommandHelper.ChooseFileAndGetPath(".exe");
            if (filepath != "")
            {
                StarterPathTextBox.Text = filepath;
                SaveLaunchPath();
            }
        }




        private void InitializePathConfigButtonClieck(object sender, RoutedEventArgs e)
        {
            ProcessPathTextBox.Text = "";
            StarterPathTextBox.Text = "";
        }


        private void ReadPathSettingFromD3dxIni(string d3dxini_path)
        {
            ProcessPathTextBox.Text = D3dxIniConfig.ReadAttributeFromD3DXIni(d3dxini_path, "target").Trim();
            StarterPathTextBox.Text = D3dxIniConfig.ReadAttributeFromD3DXIni(d3dxini_path, "launch").Trim();
            TextBox_LaunchArgs.Text = D3dxIniConfig.ReadAttributeFromD3DXIni(d3dxini_path, "launch_args").Trim();

            //开发版本中Dev为0或不设置此字段，则说明显示红字。
            string DevStr = D3dxIniConfig.ReadAttributeFromD3DXIni(d3dxini_path, "dev").Trim();
            if (DevStr.Trim() == "1")
            {
                ToggleSwitch_ShowWarning.IsOn = false;
            }
            else if (DevStr.Trim() == "0")
            {
                ToggleSwitch_ShowWarning.IsOn = true;
            }
            else
            {
                ToggleSwitch_ShowWarning.IsOn = false;
            }
        }

        private async void Open3DmigotoLoaderExe(object sender, RoutedEventArgs e)
        {
            string MigotoLoaderExePath = Path.Combine(GlobalConfig.MainCfg.Value.CurrentGameMigotoFolder, "3Dmigoto Loader.exe");
            await CommandHelper.ShellOpenFile(MigotoLoaderExePath);
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

            await CommandHelper.ShellOpenFolder(Path.Combine(GlobalConfig.Path_LoaderFolder, "ShaderFixes\\") );
        }

        private void ToggleSwitch_DllMode_Toggled(object sender, RoutedEventArgs e)
        {
            if (ToggleSwitch_DllMode.IsOn)
            {
                //切换到Play版本的d3d11.dll
                string Path_Dev3DmigotoDll = GlobalConfig.Path_3DmigotoGameModForkFolder + "ReleaseX64Play\\d3d11.dll";
                string Path_CurrentGame3DmigotoDll = Path.Combine(GlobalConfig.Path_LoaderFolder, "d3d11.dll");
                try
                {
                    File.Copy(Path_Dev3DmigotoDll, Path_CurrentGame3DmigotoDll, true);
                    //_ = CommandHelper.ShellOpenFolder(GlobalConfig.Path_LoaderFolder);
                }
                catch (Exception ex)
                {
                    _ = MessageHelper.Show("切换d3d11.dll失败! 当前游戏使用的d3d11.dll可能已被占用，请先关闭游戏进程和游戏的官方启动器。\n\n" + ex.ToString());
                }
            }
            else
            {
                //切换到开发版本的d3d11.dll
                string Path_Dev3DmigotoDll = GlobalConfig.Path_3DmigotoGameModForkFolder + "ReleaseX64Dev\\d3d11.dll";
                string Path_CurrentGame3DmigotoDll = Path.Combine(GlobalConfig.Path_LoaderFolder, "d3d11.dll");
                try
                {
                    File.Copy(Path_Dev3DmigotoDll, Path_CurrentGame3DmigotoDll, true);
                    //_ = CommandHelper.ShellOpenFolder(GlobalConfig.Path_LoaderFolder);
                }
                catch (Exception ex)
                {
                    _ = MessageHelper.Show("切换d3d11.dll失败! 当前游戏使用的d3d11.dll可能已被占用，请先关闭游戏进程和游戏的官方启动器。\n\n" + ex.ToString());
                }
            }


        }

        private void ToggleSwitch_ShowWarning_Toggled(object sender, RoutedEventArgs e)
        {
            if (ToggleSwitch_ShowWarning.IsOn)
            {
                D3dxIniConfig.SaveAttributeToD3DXIni(GlobalConfig.Path_D3DXINI,"[Logging]", "dev", "0");
            }
            else
            {
                D3dxIniConfig.SaveAttributeToD3DXIni(GlobalConfig.Path_D3DXINI, "[Logging]", "dev", "1");
            }
        }


  
    

        private void Verify3DmigotoFolderPath()
        {
            string folderPath = TextBox_3DmigotoPath.Text;
            if (!Directory.Exists(folderPath))
            {
                _ = MessageHelper.Show("您当前选择的3Dmigoto目录并不存在，请重新选择");
                return;
            }

            string d3dxini_path = Path.Combine(folderPath, "d3dx.ini");
            if (!File.Exists(d3dxini_path))
            {
                _ = MessageHelper.Show("您当前选中的目录中并未含有d3dx.ini配置文件，请确认您是否选中了正确的3Dmigoto目录。");
                return;
            }
            else
            {
                //读取配置
                ReadPathSettingFromD3dxIni(d3dxini_path);
                //把当前游戏的配置保存到Configs文件夹下
                GameIconItem gameIconItem = GetCurrentSelectedGameIconItem();
                gameIconItem.MigotoFolder = folderPath;

                gameIconItem.SaveToJson(GlobalConfig.Path_CurrentGameMainConfigJsonFile);

                GlobalConfig.MainCfg.SaveConfig();


                //设置symlink特性
                //if (GlobalConfig.MainCfg.Value.GameName != "HSR")
                //{
                //    string AnalyseOptions = D3dxIniConfig.ReadAttributeFromD3DXIni(GlobalConfig.Path_D3DXINI, "analyse_options");
                //    if (AnalyseOptions != "")
                //    {
                //        if (!AnalyseOptions.Contains("symlink"))
                //        {
                //            D3dxIniConfig.SaveAttributeToD3DXIni(GlobalConfig.Path_D3DXINI, "[hunting]", "analyse_options", AnalyseOptions + " symlink");
                //            _ = MessageHelper.Show("自动检测到您选中的3Dmigoto文件夹中d3dx.ini未开启symlink特性，已自动为您开启。d3dx.ini路径: " + GlobalConfig.Path_D3DXINI);
                //        }
                //    }
                //}
              

            }

        }

        private async void Button_Choose3DmigotoFolder_Click(object sender, RoutedEventArgs e)
        {
            string folderPath = await CommandHelper.ChooseFolderAndGetPath();
            if (folderPath != "")
            {
                TextBox_3DmigotoPath.Text = folderPath;
                GlobalConfig.MainCfg.Value.CurrentGameMigotoFolder = TextBox_3DmigotoPath.Text;
                GlobalConfig.MainCfg.SaveConfig();


            }
            else
            {
                return;
            }

            Verify3DmigotoFolderPath();

          
        }

        private async void SaveLaunchArgs()
        {
            try
            {
                D3dxIniConfig.SaveAttributeToD3DXIni(GlobalConfig.Path_D3DXINI, "[loader]", "launch_args", TextBox_LaunchArgs.Text);
            }
            catch (Exception ex)
            {
                await MessageHelper.Show("保存失败：" + ex.ToString());
            }
        }

        private async void SaveLaunchPath()
        {
            try
            {
                D3dxIniConfig.SaveAttributeToD3DXIni(GlobalConfig.Path_D3DXINI, "[loader]", "launch", StarterPathTextBox.Text);
            }
            catch (Exception ex)
            {
                await MessageHelper.Show("保存失败：" + ex.ToString());
            }
        }

        private async void SaveTargetPath()
        {
            try
            {
                D3dxIniConfig.SaveAttributeToD3DXIni(GlobalConfig.Path_D3DXINI, "[loader]", "target", ProcessPathTextBox.Text);
            }
            catch (Exception ex)
            {
                await MessageHelper.Show("保存失败：" + ex.ToString());
            }
        }

        private void TextBox_LaunchArgs_LostFocus(object sender, RoutedEventArgs e)
        {
            SaveLaunchArgs();
        }

        private void StarterPathTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            SaveLaunchPath();
        }

        private void ProcessPathTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            SaveTargetPath();
        }

        private void TextBox_3DmigotoPath_LostFocus(object sender, RoutedEventArgs e)
        {
            if (TextBox_3DmigotoPath.Text.Trim() != "")
            {
                GlobalConfig.MainCfg.Value.CurrentGameMigotoFolder = TextBox_3DmigotoPath.Text;
                GlobalConfig.MainCfg.SaveConfig();
                Verify3DmigotoFolderPath();
            }
        }


    }
}
