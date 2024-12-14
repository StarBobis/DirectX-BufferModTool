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
using Microsoft.UI.Xaml.Media.Animation;

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
            LoadDirectoryNames();

            GameBGImageBrush.Opacity = MainConfig.GameCfg.Value.GamePageBackGroundImageOpacity;

            LocalizeLanguage();
        }

        private void LocalizeLanguage()
        {
            if (MainConfig.GameCfg.Value.Language == true)
            {
                Button_Run3DmigotoLoader.Content = "Run 3Dmigoto Loader.exe";
                TextBlock_ChooseGame.Text = "Current Game:";
                TextBlock_ChooseProcessFile.Text = "Open";
                TextBlock_ChooseProcessPath.Text = "Process Path";
                TextBlock_ChooseStarterFile.Text = "Open";
                TextBlock_ChooseStarterPath.Text = "Starter Path";
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
            MainConfig.GameCfg.Value.GamePageBackGroundImageOpacity = (float)GameBGImageBrush.Opacity;
            MainConfig.GameCfg.SaveConfig();
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
                MainConfig.MainCfg.Value.GameName = selectedGame;
                

                //读取d3dx.ini中的设置
                ReadPathSettingFromD3dxIni();

                SetGameBackGroundImage();
            }

            //因为现在每次都从文件中读取，所以必须在这里保存到文件
            MainConfig.MainCfg.SaveConfig();
        }

        private async void SetGameBackGroundImage()
        {
            string imagePath = PathHelper.GetCurrentGameBackGroundPicturePath();

            // 新图片资源
            BitmapImage bitmap = new BitmapImage(new Uri(imagePath));

            if (MainConfig.GameCfg.Value.BackGroundFadeInOutTime == 0)
            {
                GameBGImageBrush.ImageSource = bitmap;
                return;
            }
            // 创建淡出动画
            var fadeOut = new Microsoft.UI.Xaml.Media.Animation.DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = new Microsoft.UI.Xaml.Duration(TimeSpan.FromSeconds(MainConfig.GameCfg.Value.BackGroundFadeInOutTime/2)),
            };

            // 应用淡出动画
            Storyboard fadeOutStoryboard = new Storyboard();
            Storyboard.SetTarget(fadeOut, BackgroundGrid);
            Storyboard.SetTargetProperty(fadeOut, "Opacity");
            fadeOutStoryboard.Children.Add(fadeOut);

            // 在淡出完成后切换图片并淡入
            fadeOutStoryboard.Completed += (s, e) =>
            {
                // 更新背景图片
                GameBGImageBrush.ImageSource = bitmap;

                // 强制刷新控件（确保图片立即加载）
                BackgroundGrid.UpdateLayout();

                // 创建淡入动画
                var fadeIn = new Microsoft.UI.Xaml.Media.Animation.DoubleAnimation
                {
                    From = 0,
                    To = 1,
                    Duration = new Microsoft.UI.Xaml.Duration(TimeSpan.FromSeconds(MainConfig.GameCfg.Value.BackGroundFadeInOutTime)),
                };

                // 应用淡入动画
                Storyboard fadeInStoryboard = new Storyboard();
                Storyboard.SetTarget(fadeIn, BackgroundGrid);
                Storyboard.SetTargetProperty(fadeIn, "Opacity");
                fadeInStoryboard.Children.Add(fadeIn);
                fadeInStoryboard.Begin();
            };

            // 开始淡出动画
            fadeOutStoryboard.Begin();
        }




        private async void SetDIYGameBackGroundImage(object sender, RoutedEventArgs e)
        {
            FileOpenPicker PicturePicker = CommandHelper.Get_FileOpenPicker(".png");
            StorageFile file = await PicturePicker.PickSingleFileAsync();
            if (file != null)
            {
                string AssetsFolderPath = PathHelper.GetAssetsFolderPath();
                string TargetPicturePath = Path.Combine(AssetsFolderPath, MainConfig.CurrentGameName + "_DIY.png");
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
            if (MainConfig.CurrentGameName == "")
            {
                GameSelectionComboBox.SelectedIndex = 0;
            }
            else
            {
                GameSelectionComboBox.SelectedItem = MainConfig.CurrentGameName;
            }
        }


        private void ReadPathSettingFromD3dxIni()
        {
            ProcessPathTextBox.Text = ConfigHelper.ReadAttributeFromD3DXIni("target");
            StarterPathTextBox.Text = ConfigHelper.ReadAttributeFromD3DXIni("launch");
        }

        private async void SavePathSettingsToD3dxIni(object sender, RoutedEventArgs e)
        {
            try
            {
                ConfigHelper.SaveAttributeToD3DXIni("[loader]", "target", ProcessPathTextBox.Text);
                ConfigHelper.SaveAttributeToD3DXIni("[loader]", "launch", StarterPathTextBox.Text);

                await MessageHelper.Show("保存成功");

            }
            catch (Exception ex)
            {
                await MessageHelper.Show("保存失败：" + ex.ToString());
            }
            
        }
        private async void Open3DmigotoLoaderExe(object sender, RoutedEventArgs e)
        {
            string MigotoLoaderExePath = Path.Combine(MainConfig.Path_LoaderFolder, "3Dmigoto Loader.exe");
            await CommandHelper.ShellOpenFile(MigotoLoaderExePath);
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

            await CommandHelper.ShellOpenFolder(Path.Combine(MainConfig.Path_LoaderFolder, "ShaderFixes\\") );
        }
    }
}
