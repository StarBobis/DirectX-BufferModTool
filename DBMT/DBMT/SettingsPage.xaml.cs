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
using DBMT_Core;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DBMT
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {

        bool ReadOver = false;

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

            LocalizeLanguage();
        }

        private void LocalizeLanguage()
        {
            if (GlobalConfig.GameCfg.Value.Language == true)
            {
                TextBlock_CleanFrameAnalysisFolderBeforeQuit.Text = "Clean FrameAnalysis Folders Before Exit";
                TextBlock_FrameAnalysisFolderReserveNumber.Text = "FrameAnalysis Folder Reserve Number:";
                TextBlock_CleanLogFileBeforeExit.Text = "Clean Log Files Before Exit";
                TextBlock_SaveLogFileNumber.Text = "Log File Reserve Number:";

                TextBlock_ModelNameStyle.Text = "Model File Styles:";
                TextBlock_GoToWorkPageAfterRunDBMT.Text = "Jump To WorkPage After Run DBMT";

                TextBlock_MoveDrawIBRelatedFiles.Text = "Move DrawIB RelatedFiles to 'output' Folder";
                TextBlock_DontSplitModelByMatchFirstIndex.Text = "Don't Split DrawIB By MatchFirstIndex at ModelExtract";

                //TextBlock_GenerateSeperateMod.Text = "Generate Mod To Seperate DrawIB Folders";
                //TextBlock_Author.Text = "Author:";
                //TextBlock_AuthorLink.Text = "Sponser Link:";
                //TextBlock_AuthorSwitchKey.Text = "Mod Default Swtich Keys";

                TextBlock_GlobalTextureFormat.Text = "Global Texture Convert Settings";
                TextBlock_OnlyConvertDiffuseMap.Text = "Only Convert DiffuseMap.dds After ModelExtract";
                TextBlock_OnlyConvertDedupedTextures.Text = "Convert All DedupedTextures After ModelExtract";
                TextBlock_ForbidMoveTrianglelistTextures.Text = "Forbid Move TrianglelistTextures After ModelExtract";
                TextBlock_ForbidMoveDedupedTextures.Text = "Forbid Move DedupedTextures After ModelExtract";
                TextBlock_ForbidMoveRenderTextures.Text = "Forbid Move RenderTextures After ModelExtract";
                //TextBlock_ForbidAuthTextures.Text = "Forbid Use AutoTextures At GenerateMod";
                //TextBlock_UseHashStyleTexture.Text = "Use Hash Style Textures At GenerateMod";
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
            GlobalConfig.GameCfg.Value.Language = ToggleSwitch_Language.IsOn;
            GlobalConfig.GameCfg.Value.StartToWorkPage = ToggleSwitch_StartToWorkPage.IsOn;

            GlobalConfig.GameCfg.Value.AutoCleanLogFile = ToggleSwitch_AutoCleanLogFile.IsOn;
            GlobalConfig.GameCfg.Value.LogFileReserveNumber = (int)NumberBox_LogFileReserveNumber.Value;
            GlobalConfig.GameCfg.Value.AutoCleanFrameAnalysisFolder = ToggleSwitch_AutoCleanFrameAnalysisFolder.IsOn;
            GlobalConfig.GameCfg.Value.FrameAnalysisFolderReserveNumber = (int)NumberBox_FrameAnalysisFolderReserveNumber.Value;
            GlobalConfig.GameCfg.Value.ModelFileNameStyle = ComboBox_ModelFileNameStyle.SelectedIndex;

            GlobalConfig.GameCfg.Value.MoveIBRelatedFiles = ToggleSwitch_MoveIBRelatedFiles.IsOn;
            GlobalConfig.GameCfg.Value.DontSplitModelByMatchFirstIndex = ToggleSwitch_DontSplitModelByMatchFirstIndex.IsOn;


            GlobalConfig.GameCfg.Value.AutoTextureFormat = ComboBox_AutoTextureFormat.SelectedIndex;
            GlobalConfig.GameCfg.Value.AutoTextureOnlyConvertDiffuseMap = ToggleSwitch_AutoTextureOnlyConvertDiffuseMap.IsOn;
            GlobalConfig.GameCfg.Value.ConvertDedupedTextures = ToggleSwitch_ConvertDedupedTextures.IsOn;
            GlobalConfig.GameCfg.Value.ForbidMoveTrianglelistTextures = ToggleSwitch_ForbidMoveTrianglelistTextures.IsOn;
            GlobalConfig.GameCfg.Value.ForbidMoveDedupedTextures = ToggleSwitch_ForbidMoveDedupedTextures.IsOn;
            GlobalConfig.GameCfg.Value.ForbidMoveRenderTextures = ToggleSwitch_ForbidMoveRenderTextures.IsOn;
   

            // 保存配置
            GlobalConfig.GameCfg.SaveConfig();
         
        }

        public void ReadSettingsFromConfig()
        {
            ReadOver = false;
            //防止程序启动时没正确读取，这里冗余读取一次，后面看情况可以去掉
            GlobalConfig.GameCfg.LoadConfig();
  

            ToggleSwitch_Language.IsOn = GlobalConfig.GameCfg.Value.Language;
            ToggleSwitch_StartToWorkPage.IsOn = GlobalConfig.GameCfg.Value.StartToWorkPage;

            ToggleSwitch_AutoCleanLogFile.IsOn = GlobalConfig.GameCfg.Value.AutoCleanLogFile;
            NumberBox_LogFileReserveNumber.Value = GlobalConfig.GameCfg.Value.LogFileReserveNumber;
            ToggleSwitch_AutoCleanFrameAnalysisFolder.IsOn = GlobalConfig.GameCfg.Value.AutoCleanFrameAnalysisFolder;
            NumberBox_FrameAnalysisFolderReserveNumber.Value = GlobalConfig.GameCfg.Value.FrameAnalysisFolderReserveNumber;
            ComboBox_ModelFileNameStyle.SelectedIndex = GlobalConfig.GameCfg.Value.ModelFileNameStyle;
            ToggleSwitch_MoveIBRelatedFiles.IsOn = GlobalConfig.GameCfg.Value.MoveIBRelatedFiles;
            ToggleSwitch_DontSplitModelByMatchFirstIndex.IsOn = GlobalConfig.GameCfg.Value.DontSplitModelByMatchFirstIndex;

            ComboBox_AutoTextureFormat.SelectedIndex = GlobalConfig.GameCfg.Value.AutoTextureFormat;
            ToggleSwitch_AutoTextureOnlyConvertDiffuseMap.IsOn = GlobalConfig.GameCfg.Value.AutoTextureOnlyConvertDiffuseMap;
            ToggleSwitch_ConvertDedupedTextures.IsOn = GlobalConfig.GameCfg.Value.ConvertDedupedTextures;
            ToggleSwitch_ForbidMoveTrianglelistTextures.IsOn = GlobalConfig.GameCfg.Value.ForbidMoveTrianglelistTextures;
            ToggleSwitch_ForbidMoveDedupedTextures.IsOn = GlobalConfig.GameCfg.Value.ForbidMoveDedupedTextures;
            ToggleSwitch_ForbidMoveRenderTextures.IsOn = GlobalConfig.GameCfg.Value.ForbidMoveRenderTextures;

            ReadOver = true;
        }

        private async void ToggleSwitch_Language_Toggled(object sender, RoutedEventArgs e)
        {
            if (ToggleSwitch_Language.IsOn != GlobalConfig.GameCfg.Value.Language)
            {
                GlobalConfig.GameCfg.Value.Language = ToggleSwitch_Language.IsOn;
                await MessageHelper.Show("将在页面缓存刷新后生效","Settings will effect after you refresh page cache.");
                SaveSettingsToConfig();
            }
        }

        private void RefreshSettings(object sender, RoutedEventArgs e)
        {
            if (ReadOver)
            {
                SaveSettingsToConfig();
            }
            //ReadSettingsFromConfig();
        }

 

        //private void TextBox_ModSwitchKey_LostFocus(object sender, RoutedEventArgs e)
        //{
        //    //失去焦点后检查内容如果和默认不一致就刷新
        //    if (TextBox_ModSwitchKey.Text != GlobalConfig.GameCfg.Value.ModSwitchKey)
        //    {
        //        if (ReadOver)
        //        {
        //            SaveSettingsToConfig();
        //        }
                    
        //        //ReadSettingsFromConfig();
        //    }
        //}

        //private void TextBox_AuthorLink_LostFocus(object sender, RoutedEventArgs e)
        //{
        //    if (TextBox_AuthorLink.Text != GlobalConfig.GameCfg.Value.AuthorLink)
        //    {
        //        if (ReadOver)
        //        {
        //            SaveSettingsToConfig();

        //        }
        //        //ReadSettingsFromConfig();
        //    }
        //}

        //private void TextBox_Author_LostFocus(object sender, RoutedEventArgs e)
        //{
        //    if (TextBox_Author.Text != GlobalConfig.GameCfg.Value.Author)
        //    {
        //        if (ReadOver)
        //        {
        //            SaveSettingsToConfig();

        //        }
        //        //ReadSettingsFromConfig();
        //    }
        //}

        private void ComboBox_ModelFileNameStyle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ReadOver)
            {
                SaveSettingsToConfig();

            }
            //ReadSettingsFromConfig();
        }
    }
}
