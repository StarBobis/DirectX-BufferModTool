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

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DBMT
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GamePage : Page
    {
        public GamePage()
        {
            this.InitializeComponent();

            //加载可能的游戏列表
            LoadDirectoryNames();

            //使用配置文件中保存的透明度来设置背景透明度
            GameBGImageBrush.Opacity = GlobalConfig.GameCfg.Value.GamePageBackGroundImageOpacity;

            //切换语言
            LocalizeLanguage();
        }

        private void LocalizeLanguage()
        {
            if (GlobalConfig.GameCfg.Value.Language == true)
            {
                Button_Run3DmigotoLoader.Content = "Run 3Dmigoto Loader.exe";
                TextBlock_ChooseGame.Text = "Current Game:";
                TextBlock_ChooseProcessFile.Text = "Open";
                TextBlock_ChooseProcessPath.Text = "Target Path";
                TextBlock_ChooseStarterFile.Text = "Open";
                TextBlock_ChooseStarterPath.Text = "Launch Path";
                TextBlock_LaunchArgs.Text = "Launch Args";
                
                Button_InitializePath.Content = "Initialize Config";
                Button_SaveConfig.Content = "Save Config";
                Button_OpenD3DXINI.Content = "Open d3dx.ini";
                Button_Open3DmigotoFolder.Content = "Open 3Dmigoto Folder";
                Button_OpenShaderFixesFolder.Content = "Open ShaderFixes Folder";
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            // 执行你想要在这个页面被关闭或导航离开时运行的代码

            //保存全局设置因为要保存滑条透明度
            GlobalConfig.GameCfg.Value.GamePageBackGroundImageOpacity = (float)GameBGImageBrush.Opacity;
            GlobalConfig.GameCfg.SaveConfig();

            // 如果需要，可以调用基类的 OnNavigatedFrom 方法
            base.OnNavigatedFrom(e);
        }

        private async void ChooseProcessPathButtonClick(object sender, RoutedEventArgs e)
        {
            string filepath = await CommandHelper.ChooseFileAndGetPath(".exe");
            if (filepath != "")
            {
                ProcessPathTextBox.Text = filepath;
            }
        }

        private async void ChooseStarterPathButtonClick(object sender, RoutedEventArgs e)
        {
            string filepath = await CommandHelper.ChooseFileAndGetPath(".exe");
            if (filepath != "")
            {
                StarterPathTextBox.Text = filepath;
            }
        }

        private void GameSelectionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
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
                GlobalConfig.MainCfg.Value.GameName = selectedGame;
                

                //读取d3dx.ini中的设置
                ReadPathSettingFromD3dxIni();

                SetGameBackGroundImage();
            }

            //因为现在每次都从文件中读取，所以必须在这里保存到文件
            GlobalConfig.MainCfg.SaveConfig();
        }

        private void SetGameBackGroundImage()
        {
            string basePath = GlobalConfig.Path_Base;

            //设置背景图片
            // 优先级：DIY > 默认 > 主页背景
            string[] imagePaths = {
                Path.Combine(basePath, "Assets", GlobalConfig.CurrentGameName + "_DIY.png"),
                Path.Combine(basePath, "Assets", GlobalConfig.CurrentGameName + ".png"),
                Path.Combine(basePath, "Assets", "HomePageBackGround.png")
            };

            string imagePath = "";
            foreach (string path in imagePaths)
            {
                if (!File.Exists(path)) { continue; }
                imagePath = path;
                break;
            }   

            // 创建 BitmapImage 并设置 ImageSource
            BitmapImage bitmap = new BitmapImage(new Uri(imagePath));
            GameBGImageBrush.ImageSource = bitmap;
        }


        private async void SetDIYGameBackGroundImage(object sender, RoutedEventArgs e)
        {
            FileOpenPicker PicturePicker = CommandHelper.Get_FileOpenPicker(".png");
            StorageFile file = await PicturePicker.PickSingleFileAsync();
            if (file != null)
            {
                string AssetsFolderPath = PathHelper.GetAssetsFolderPath();
                string TargetPicturePath = Path.Combine(AssetsFolderPath, GlobalConfig.CurrentGameName + "_DIY.png");
                File.Copy(file.Path, TargetPicturePath, true);

                SetGameBackGroundImage();
            }
        }


        private void InitializePathConfigButtonClieck(object sender, RoutedEventArgs e)
        {
            ProcessPathTextBox.Text = "";
            StarterPathTextBox.Text = "";
        }

        

        private async void LoadDirectoryNames()
        {
            GameSelectionComboBox.Items.Clear();
            List<string> directories = await PathHelper.GetGameDirectoryNameList();
            foreach (var dirName in directories)
            {
                GameSelectionComboBox.Items.Add(dirName);
            }
            if (GlobalConfig.CurrentGameName == "")
            {
                GameSelectionComboBox.SelectedIndex = 0;
            }
            else
            {
                GameSelectionComboBox.SelectedItem = GlobalConfig.CurrentGameName;
            }
        }


        private void ReadPathSettingFromD3dxIni()
        {
            ProcessPathTextBox.Text = D3dxIniConfig.ReadAttributeFromD3DXIni("target");
            StarterPathTextBox.Text = D3dxIniConfig.ReadAttributeFromD3DXIni("launch");
            TextBox_LaunchArgs.Text = D3dxIniConfig.ReadAttributeFromD3DXIni("launch_args");
        }

        private async void SavePathSettingsToD3dxIni(object sender, RoutedEventArgs e)
        {
            try
            {
                D3dxIniConfig.SaveAttributeToD3DXIni("[loader]", "target", ProcessPathTextBox.Text);
                D3dxIniConfig.SaveAttributeToD3DXIni("[loader]", "launch", StarterPathTextBox.Text);
                D3dxIniConfig.SaveAttributeToD3DXIni("[loader]", "launch_args", TextBox_LaunchArgs.Text);

                await MessageHelper.Show("保存成功");

            }
            catch (Exception ex)
            {
                await MessageHelper.Show("保存失败：" + ex.ToString());
            }
            
        }

        private async void Open3DmigotoLoaderExe(object sender, RoutedEventArgs e)
        {
            string MigotoLoaderExePath = Path.Combine(GlobalConfig.Path_LoaderFolder, "3Dmigoto Loader.exe");
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
    }
}
