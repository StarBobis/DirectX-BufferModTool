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
using DBMT.Helper;
using Microsoft.UI.Xaml.Media.Imaging;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DBMT
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            this.InitializeComponent();

            ReadSettingsFromConfig();
            SetDefaultBackGroundImage();
        }

        private void SetDefaultBackGroundImage()
        {
            string AssetsFolderPath = PathHelper.GetAssetsFolderPath();
            string imagePath = Path.Combine(AssetsFolderPath, "HomePageBackGround_DIY.png");
            if (!File.Exists(imagePath))
            {
                imagePath = Path.Combine(AssetsFolderPath, "HomePageBackGround.png");
            }

            // 创建 BitmapImage 并设置 ImageSource
            BitmapImage bitmap = new BitmapImage(new Uri(imagePath));
            SettingsBGImageBrush.ImageSource = bitmap;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            // 执行你想要在这个页面被关闭或导航离开时运行的代码
            SaveSettingsToConfig();
            // 如果需要，可以调用基类的 OnNavigatedFrom 方法
            base.OnNavigatedFrom(e);
        }

        public void SaveSettingsToConfig()
        {
            SettingsHelper.AutoCleanLogFile = ToggleSwitch_AutoCleanLogFile.IsOn;
            SettingsHelper.LogFileReserveNumber = (int)NumberBox_LogFileReserveNumber.Value;
            SettingsHelper.AutoCleanFrameAnalysisFolder = ToggleSwitch_AutoCleanFrameAnalysisFolder.IsOn;
            SettingsHelper.FrameAnalysisFolderReserveNumber = (int)NumberBox_FrameAnalysisFolderReserveNumber.Value;
            SettingsHelper.ModelFileNameStyle = ComboBox_ModelFileNameStyle.SelectedIndex;

            SettingsHelper.MoveIBRelatedFiles = ToggleSwitch_MoveIBRelatedFiles.IsOn;
            SettingsHelper.DontSplitModelByMatchFirstIndex = ToggleSwitch_DontSplitModelByMatchFirstIndex.IsOn;

            SettingsHelper.GenerateSeperatedMod = ToggleSwitch_GenerateSeperatedMod.IsOn;
            SettingsHelper.Author = TextBox_Author.Text;
            SettingsHelper.AuthorLink = TextBox_AuthorLink.Text;
            SettingsHelper.ModSwitchKey = TextBox_ModSwitchKey.Text;

            SettingsHelper.AutoTextureFormat = ComboBox_AutoTextureFormat.SelectedIndex ;
            SettingsHelper.AutoTextureOnlyConvertDiffuseMap = ToggleSwitch_AutoTextureOnlyConvertDiffuseMap.IsOn ;
            SettingsHelper.ConvertDedupedTextures = ToggleSwitch_ConvertDedupedTextures.IsOn ;
            SettingsHelper.ForbidMoveTrianglelistTextures = ToggleSwitch_ForbidMoveTrianglelistTextures.IsOn ;
            SettingsHelper.ForbidMoveDedupedTextures = ToggleSwitch_ForbidMoveDedupedTextures.IsOn;
            SettingsHelper.ForbidMoveRenderTextures = ToggleSwitch_ForbidMoveRenderTextures.IsOn;
            SettingsHelper.ForbidAutoTexture = ToggleSwitch_ForbidAutoTexture.IsOn;
            SettingsHelper.UseHashTexture = ToggleSwitch_UseHashTexture.IsOn;

            SettingsHelper.SaveTextureSettingsToConfig();
            SettingsHelper.SaveGameSettingsToConfig();
        }

        public void ReadSettingsFromConfig()
        {
            //防止程序启动时没正确读取，这里冗余读取一次，后面看情况可以去掉。
            SettingsHelper.ReadGameSettingsFromConfig();
            SettingsHelper.ReadTextureSettingsFromConfig();

            ToggleSwitch_AutoCleanLogFile.IsOn = SettingsHelper.AutoCleanLogFile;
            NumberBox_LogFileReserveNumber.Value = SettingsHelper.LogFileReserveNumber;

            ToggleSwitch_AutoCleanFrameAnalysisFolder.IsOn = SettingsHelper.AutoCleanFrameAnalysisFolder;
            NumberBox_FrameAnalysisFolderReserveNumber.Value = SettingsHelper.FrameAnalysisFolderReserveNumber;
           
            ComboBox_ModelFileNameStyle.SelectedIndex = SettingsHelper.ModelFileNameStyle;

            ToggleSwitch_MoveIBRelatedFiles.IsOn = SettingsHelper.MoveIBRelatedFiles;
            ToggleSwitch_DontSplitModelByMatchFirstIndex.IsOn = SettingsHelper.DontSplitModelByMatchFirstIndex;

            ToggleSwitch_GenerateSeperatedMod.IsOn = SettingsHelper.GenerateSeperatedMod;
            TextBox_Author.Text = SettingsHelper.Author;
            TextBox_AuthorLink.Text = SettingsHelper.AuthorLink;
            TextBox_ModSwitchKey.Text = SettingsHelper.ModSwitchKey;

            ComboBox_AutoTextureFormat.SelectedIndex = SettingsHelper.AutoTextureFormat;
            ToggleSwitch_AutoTextureOnlyConvertDiffuseMap.IsOn = SettingsHelper.AutoTextureOnlyConvertDiffuseMap;
            ToggleSwitch_ConvertDedupedTextures.IsOn = SettingsHelper.ConvertDedupedTextures;
            ToggleSwitch_ForbidMoveTrianglelistTextures.IsOn = SettingsHelper.ForbidMoveTrianglelistTextures;
            ToggleSwitch_ForbidMoveDedupedTextures.IsOn = SettingsHelper.ForbidMoveDedupedTextures;
            ToggleSwitch_ForbidMoveRenderTextures.IsOn = SettingsHelper.ForbidMoveRenderTextures;
            ToggleSwitch_ForbidAutoTexture.IsOn = SettingsHelper.ForbidAutoTexture;
            ToggleSwitch_UseHashTexture.IsOn = SettingsHelper.UseHashTexture;

        }

    }
}
