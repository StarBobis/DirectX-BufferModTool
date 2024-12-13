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
using DBMT;
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

            try
            {
                ReadSettingsFromConfig();
                SetDefaultBackGroundImage();
            }
            catch (Exception ex)
            {
                _ = MessageHelper.Show("Error: " + ex.ToString());
            }

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
            MainConfig.GameCfg.Value.Language = ToggleSwitch_Language.IsOn;
            MainConfig.GameCfg.Value.StartToWorkPage = ToggleSwitch_StartToWorkPage.IsOn;

            MainConfig.GameCfg.Value.AutoCleanLogFile = ToggleSwitch_AutoCleanLogFile.IsOn;
            MainConfig.GameCfg.Value.LogFileReserveNumber = (int)NumberBox_LogFileReserveNumber.Value;
            MainConfig.GameCfg.Value.AutoCleanFrameAnalysisFolder = ToggleSwitch_AutoCleanFrameAnalysisFolder.IsOn;
            MainConfig.GameCfg.Value.FrameAnalysisFolderReserveNumber = (int)NumberBox_FrameAnalysisFolderReserveNumber.Value;
            MainConfig.GameCfg.Value.ModelFileNameStyle = ComboBox_ModelFileNameStyle.SelectedIndex;

            MainConfig.GameCfg.Value.MoveIBRelatedFiles = ToggleSwitch_MoveIBRelatedFiles.IsOn;
            MainConfig.GameCfg.Value.DontSplitModelByMatchFirstIndex = ToggleSwitch_DontSplitModelByMatchFirstIndex.IsOn;

            MainConfig.GameCfg.Value.GenerateSeperatedMod = ToggleSwitch_GenerateSeperatedMod.IsOn;
            MainConfig.GameCfg.Value.Author = TextBox_Author.Text;
            MainConfig.GameCfg.Value.AuthorLink = TextBox_AuthorLink.Text;
            MainConfig.GameCfg.Value.ModSwitchKey = TextBox_ModSwitchKey.Text;

            MainConfig.TextureCfg.Value.AutoTextureFormat = ComboBox_AutoTextureFormat.SelectedIndex;
            MainConfig.TextureCfg.Value.AutoTextureOnlyConvertDiffuseMap = ToggleSwitch_AutoTextureOnlyConvertDiffuseMap.IsOn;
            MainConfig.TextureCfg.Value.ConvertDedupedTextures = ToggleSwitch_ConvertDedupedTextures.IsOn;
            MainConfig.TextureCfg.Value.ForbidMoveTrianglelistTextures = ToggleSwitch_ForbidMoveTrianglelistTextures.IsOn;
            MainConfig.TextureCfg.Value.ForbidMoveDedupedTextures = ToggleSwitch_ForbidMoveDedupedTextures.IsOn;
            MainConfig.TextureCfg.Value.ForbidMoveRenderTextures = ToggleSwitch_ForbidMoveRenderTextures.IsOn;
            MainConfig.TextureCfg.Value.ForbidAutoTexture = ToggleSwitch_ForbidAutoTexture.IsOn;
            MainConfig.TextureCfg.Value.UseHashTexture = ToggleSwitch_UseHashTexture.IsOn;

            // 保存配置
            MainConfig.GameCfg.SaveConfig();
            MainConfig.TextureCfg.SaveConfig();


            // 以下是旧的保存方式，后面看情况可以去掉
            //MainConfig.SetConfig(MainConfig.ConfigFiles.Game_Setting, "AutoCleanLogFile", ToggleSwitch_AutoCleanLogFile.IsOn);
            //MainConfig.SetConfig(MainConfig.ConfigFiles.Game_Setting, "LogFileReserveNumber", (int)NumberBox_LogFileReserveNumber.Value);
            //MainConfig.SetConfig(MainConfig.ConfigFiles.Game_Setting, "AutoCleanFrameAnalysisFolder", ToggleSwitch_AutoCleanFrameAnalysisFolder.IsOn);
            //MainConfig.SetConfig(MainConfig.ConfigFiles.Game_Setting, "FrameAnalysisFolderReserveNumber", (int)NumberBox_FrameAnalysisFolderReserveNumber.Value);
            //MainConfig.SetConfig(MainConfig.ConfigFiles.Game_Setting, "ModelFileNameStyle", ComboBox_ModelFileNameStyle.SelectedIndex);

            //MainConfig.SetConfig(MainConfig.ConfigFiles.Game_Setting, "MoveIBRelatedFiles", ToggleSwitch_MoveIBRelatedFiles.IsOn);
            //MainConfig.SetConfig(MainConfig.ConfigFiles.Game_Setting, "DontSplitModelByMatchFirstIndex", ToggleSwitch_DontSplitModelByMatchFirstIndex.IsOn);

            //MainConfig.SetConfig(MainConfig.ConfigFiles.Game_Setting, "GenerateSeperatedMod", ToggleSwitch_GenerateSeperatedMod.IsOn);
            //MainConfig.SetConfig(MainConfig.ConfigFiles.Game_Setting, "Author", TextBox_Author.Text);
            //MainConfig.SetConfig(MainConfig.ConfigFiles.Game_Setting, "AuthorLink", TextBox_AuthorLink.Text);
            //MainConfig.SetConfig(MainConfig.ConfigFiles.Game_Setting, "ModSwitchKey", TextBox_ModSwitchKey.Text);

            //MainConfig.SetConfig(MainConfig.ConfigFiles.Texture_Setting, "AutoTextureFormat", ComboBox_AutoTextureFormat.SelectedIndex);
            //MainConfig.SetConfig(MainConfig.ConfigFiles.Texture_Setting, "AutoTextureOnlyConvertDiffuseMap", ToggleSwitch_AutoTextureOnlyConvertDiffuseMap.IsOn);
            //MainConfig.SetConfig(MainConfig.ConfigFiles.Texture_Setting, "ConvertDedupedTextures", ToggleSwitch_ConvertDedupedTextures.IsOn);
            //MainConfig.SetConfig(MainConfig.ConfigFiles.Texture_Setting, "ForbidMoveTrianglelistTextures", ToggleSwitch_ForbidMoveTrianglelistTextures.IsOn);
            //MainConfig.SetConfig(MainConfig.ConfigFiles.Texture_Setting, "ForbidMoveDedupedTextures", ToggleSwitch_ForbidMoveDedupedTextures.IsOn);
            //MainConfig.SetConfig(MainConfig.ConfigFiles.Texture_Setting, "ForbidMoveRenderTextures", ToggleSwitch_ForbidMoveRenderTextures.IsOn);
            //MainConfig.SetConfig(MainConfig.ConfigFiles.Texture_Setting, "ForbidAutoTexture", ToggleSwitch_ForbidAutoTexture.IsOn);
            //MainConfig.SetConfig(MainConfig.ConfigFiles.Texture_Setting, "UseHashTexture", ToggleSwitch_UseHashTexture.IsOn);

            //保存全局设置和贴图设置
            //MainConfig.SaveConfig(MainConfig.ConfigFiles.Game_Setting);
            //MainConfig.SaveConfig(MainConfig.ConfigFiles.Texture_Setting);

        }

        public void ReadSettingsFromConfig()
        {
            //防止程序启动时没正确读取，这里冗余读取一次，后面看情况可以去掉
            MainConfig.GameCfg.LoadConfig();
            MainConfig.TextureCfg.LoadConfig();

            ToggleSwitch_Language.IsOn = MainConfig.GameCfg.Value.Language;
            ToggleSwitch_StartToWorkPage.IsOn = MainConfig.GameCfg.Value.StartToWorkPage;

            ToggleSwitch_AutoCleanLogFile.IsOn = MainConfig.GameCfg.Value.AutoCleanLogFile;
            NumberBox_LogFileReserveNumber.Value = MainConfig.GameCfg.Value.LogFileReserveNumber;
            ToggleSwitch_AutoCleanFrameAnalysisFolder.IsOn = MainConfig.GameCfg.Value.AutoCleanFrameAnalysisFolder;
            NumberBox_FrameAnalysisFolderReserveNumber.Value = MainConfig.GameCfg.Value.FrameAnalysisFolderReserveNumber;
            ComboBox_ModelFileNameStyle.SelectedIndex = MainConfig.GameCfg.Value.ModelFileNameStyle;
            ToggleSwitch_MoveIBRelatedFiles.IsOn = MainConfig.GameCfg.Value.MoveIBRelatedFiles;
            ToggleSwitch_DontSplitModelByMatchFirstIndex.IsOn = MainConfig.GameCfg.Value.DontSplitModelByMatchFirstIndex;

            ToggleSwitch_GenerateSeperatedMod.IsOn = MainConfig.GameCfg.Value.GenerateSeperatedMod;
            TextBox_Author.Text = MainConfig.GameCfg.Value.Author;
            TextBox_AuthorLink.Text = MainConfig.GameCfg.Value.AuthorLink;
            TextBox_ModSwitchKey.Text = MainConfig.GameCfg.Value.ModSwitchKey;

            ComboBox_AutoTextureFormat.SelectedIndex = MainConfig.TextureCfg.Value.AutoTextureFormat;
            ToggleSwitch_AutoTextureOnlyConvertDiffuseMap.IsOn = MainConfig.TextureCfg.Value.AutoTextureOnlyConvertDiffuseMap;
            ToggleSwitch_ConvertDedupedTextures.IsOn = MainConfig.TextureCfg.Value.ConvertDedupedTextures;
            ToggleSwitch_ForbidMoveTrianglelistTextures.IsOn = MainConfig.TextureCfg.Value.ForbidMoveTrianglelistTextures;
            ToggleSwitch_ForbidMoveDedupedTextures.IsOn = MainConfig.TextureCfg.Value.ForbidMoveDedupedTextures;
            ToggleSwitch_ForbidMoveRenderTextures.IsOn = MainConfig.TextureCfg.Value.ForbidMoveRenderTextures;
            ToggleSwitch_ForbidAutoTexture.IsOn = MainConfig.TextureCfg.Value.ForbidAutoTexture;
            ToggleSwitch_UseHashTexture.IsOn = MainConfig.TextureCfg.Value.UseHashTexture;

            //以下是旧的读取方式，后面看情况可以去掉
            //ToggleSwitch_AutoCleanLogFile.IsOn = MainConfig.GetConfig<bool>(MainConfig.ConfigFiles.Game_Setting, "AutoCleanLogFile");
            //NumberBox_LogFileReserveNumber.Value = MainConfig.GetConfig<int>(MainConfig.ConfigFiles.Game_Setting, "LogFileReserveNumber");
            //ToggleSwitch_AutoCleanFrameAnalysisFolder.IsOn = MainConfig.GetConfig<bool>(MainConfig.ConfigFiles.Game_Setting, "AutoCleanFrameAnalysisFolder");
            //NumberBox_FrameAnalysisFolderReserveNumber.Value = MainConfig.GetConfig<int>(MainConfig.ConfigFiles.Game_Setting, "FrameAnalysisFolderReserveNumber");
            //ComboBox_ModelFileNameStyle.SelectedIndex = MainConfig.GetConfig<int>(MainConfig.ConfigFiles.Game_Setting, "ModelFileNameStyle");
            //ToggleSwitch_MoveIBRelatedFiles.IsOn = MainConfig.GetConfig<bool>(MainConfig.ConfigFiles.Game_Setting, "MoveIBRelatedFiles");
            //ToggleSwitch_DontSplitModelByMatchFirstIndex.IsOn = MainConfig.GetConfig<bool>(MainConfig.ConfigFiles.Game_Setting, "DontSplitModelByMatchFirstIndex");

            //ToggleSwitch_GenerateSeperatedMod.IsOn = MainConfig.GetConfig<bool>(MainConfig.ConfigFiles.Game_Setting, "GenerateSeperatedMod");
            //TextBox_Author.Text = MainConfig.GetConfig<string>(MainConfig.ConfigFiles.Game_Setting, "Author");
            //TextBox_AuthorLink.Text = MainConfig.GetConfig<string>(MainConfig.ConfigFiles.Game_Setting, "AuthorLink");
            //TextBox_ModSwitchKey.Text = MainConfig.GetConfig<string>(MainConfig.ConfigFiles.Game_Setting, "ModSwitchKey");

            //ComboBox_AutoTextureFormat.SelectedIndex = MainConfig.GetConfig<int>(MainConfig.ConfigFiles.Texture_Setting, "AutoTextureFormat");
            //ToggleSwitch_AutoTextureOnlyConvertDiffuseMap.IsOn = MainConfig.GetConfig<bool>(MainConfig.ConfigFiles.Texture_Setting, "AutoTextureOnlyConvertDiffuseMap");
            //ToggleSwitch_ConvertDedupedTextures.IsOn = MainConfig.GetConfig<bool>(MainConfig.ConfigFiles.Texture_Setting, "ConvertDedupedTextures");
            //ToggleSwitch_ForbidMoveTrianglelistTextures.IsOn = MainConfig.GetConfig<bool>(MainConfig.ConfigFiles.Texture_Setting, "ForbidMoveTrianglelistTextures");
            //ToggleSwitch_ForbidMoveDedupedTextures.IsOn = MainConfig.GetConfig<bool>(MainConfig.ConfigFiles.Texture_Setting, "ForbidMoveDedupedTextures");
            //ToggleSwitch_ForbidMoveRenderTextures.IsOn = MainConfig.GetConfig<bool>(MainConfig.ConfigFiles.Texture_Setting, "ForbidMoveRenderTextures");
            //ToggleSwitch_ForbidAutoTexture.IsOn = MainConfig.GetConfig<bool>(MainConfig.ConfigFiles.Texture_Setting, "ForbidAutoTexture");
            //ToggleSwitch_UseHashTexture.IsOn = MainConfig.GetConfig<bool>(MainConfig.ConfigFiles.Texture_Setting, "UseHashTexture");
        }

    }
}
