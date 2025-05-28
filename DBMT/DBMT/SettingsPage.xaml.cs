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
using System.Diagnostics;
using Microsoft.UI.Xaml.Media.Animation;
using Windows.System;

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
            }
            catch (Exception ex)
            {
                _ = MessageHelper.Show("Error: " + ex.ToString());
            }
        }

        public void SaveSettingsToConfig()
        {
            Debug.WriteLine("离开设置页面，保存配置");

            GlobalConfig.AutoCleanFrameAnalysisFolder = ToggleSwitch_AutoCleanFrameAnalysisFolder.IsOn;
            GlobalConfig.FrameAnalysisFolderReserveNumber = (int)NumberBox_FrameAnalysisFolderReserveNumber.Value;
            //GlobalConfig.DontSplitModelByMatchFirstIndex = ToggleSwitch_DontSplitModelByMatchFirstIndex.IsOn;
            GlobalConfig.AutoTextureFormat = ComboBox_AutoTextureFormat.SelectedItem.ToString();
            GlobalConfig.AutoTextureOnlyConvertDiffuseMap = ToggleSwitch_AutoTextureOnlyConvertDiffuseMap.IsOn;
            GlobalConfig.AutoDetectAndMarkTexture = ToggleSwitch_AutoDetectAndMarkTexture.IsOn;
            GlobalConfig.ModSwitchKey = TextBox_ModSwitchKey.Text;

            GlobalConfig.SaveConfig();
        }

        public void ReadSettingsFromConfig()
        {
            ReadOver = false;
            //防止程序启动时没正确读取，这里冗余读取一次，后面看情况可以去掉
            GlobalConfig.ReadConfig();

            ToggleSwitch_AutoCleanFrameAnalysisFolder.IsOn = GlobalConfig.AutoCleanFrameAnalysisFolder;
            NumberBox_FrameAnalysisFolderReserveNumber.Value = GlobalConfig.FrameAnalysisFolderReserveNumber;
            //ToggleSwitch_DontSplitModelByMatchFirstIndex.IsOn = GlobalConfig.DontSplitModelByMatchFirstIndex;
            ComboBox_AutoTextureFormat.SelectedItem = GlobalConfig.AutoTextureFormat;
            ToggleSwitch_AutoTextureOnlyConvertDiffuseMap.IsOn = GlobalConfig.AutoTextureOnlyConvertDiffuseMap;
            ToggleSwitch_AutoDetectAndMarkTexture.IsOn = GlobalConfig.AutoDetectAndMarkTexture;
            TextBox_ModSwitchKey.Text = GlobalConfig.ModSwitchKey;

            TextBox_DBMTWorkFolder.Text = GlobalConfig.DBMTWorkFolder;

            ReadOver = true;
        }

        /// <summary>
        /// 任何设置项被改变后，都应该立刻调用这个方法，否则无法同步状态。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RefreshSettings(object sender, RoutedEventArgs e)
        {
            if (ReadOver)
            {
                SaveSettingsToConfig();
            }
        }

        //失去焦点后，保存配置。
        private void TextBox_ModSwitchKey_LostFocus(object sender, RoutedEventArgs e)
        {
            if (TextBox_ModSwitchKey.Text.Trim() == "")
            {
                TextBox_ModSwitchKey.Text = "\"x\",\"c\",\"v\",\"b\",\"n\",\"m\",\"j\",\"k\",\"l\",\"o\",\"p\",\"[\",\"]\",\"x\",\"c\",\"v\",\"b\",\"n\",\"m\",\"j\",\"k\",\"l\",\"o\",\"p\",\"[\",\"]\",\"x\",\"c\",\"v\",\"b\",\"n\",\"m\",\"j\",\"k\",\"l\",\"o\",\"p\",\"[\",\"]\"";
            }

            if (ReadOver)
            {
                SaveSettingsToConfig();
            }
        }

        private async void Button_ChooseDBMTWorkFolder_Click(object sender, RoutedEventArgs e)
        {
            string FolderPath = await CommandHelper.ChooseFolderAndGetPath();

            if (FolderPath == "")
            {
                return;
            }

            if (Directory.Exists(FolderPath))
            {
                TextBox_DBMTWorkFolder.Text = FolderPath;
                GlobalConfig.DBMTWorkFolder = FolderPath;
                GlobalConfig.SaveConfig();
            }
        }

        private void TextBox_DBMTWorkFolder_LostFocus(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(TextBox_DBMTWorkFolder.Text))
            {
                GlobalConfig.DBMTWorkFolder = TextBox_DBMTWorkFolder.Text;
                GlobalConfig.SaveConfig();
            }
        }

        private void Button_CheckDBMTPackageUpdate_Click(object sender, RoutedEventArgs e)
        {
            //string GithubPage = "https://github.com/StarBobis/DBMT-Package/";

        }
    }
}
