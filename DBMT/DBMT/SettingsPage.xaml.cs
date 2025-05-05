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

            GlobalConfig.GameCfg.Value.AutoCleanFrameAnalysisFolder = ToggleSwitch_AutoCleanFrameAnalysisFolder.IsOn;
            GlobalConfig.GameCfg.Value.FrameAnalysisFolderReserveNumber = (int)NumberBox_FrameAnalysisFolderReserveNumber.Value;
            GlobalConfig.GameCfg.Value.DontSplitModelByMatchFirstIndex = ToggleSwitch_DontSplitModelByMatchFirstIndex.IsOn;
            GlobalConfig.GameCfg.Value.AutoTextureFormat = ComboBox_AutoTextureFormat.SelectedIndex;
            GlobalConfig.GameCfg.Value.AutoTextureOnlyConvertDiffuseMap = ToggleSwitch_AutoTextureOnlyConvertDiffuseMap.IsOn;
            GlobalConfig.GameCfg.Value.AutoDetectAndMarkTexture = ToggleSwitch_AutoDetectAndMarkTexture.IsOn;
            GlobalConfig.GameCfg.Value.ModSwitchKey = TextBox_ModSwitchKey.Text;

            GlobalConfig.GameCfg.SaveConfig();
        }

        public void ReadSettingsFromConfig()
        {
            ReadOver = false;
            //防止程序启动时没正确读取，这里冗余读取一次，后面看情况可以去掉
            GlobalConfig.GameCfg.LoadConfig();

            ToggleSwitch_AutoCleanFrameAnalysisFolder.IsOn = GlobalConfig.GameCfg.Value.AutoCleanFrameAnalysisFolder;
            NumberBox_FrameAnalysisFolderReserveNumber.Value = GlobalConfig.GameCfg.Value.FrameAnalysisFolderReserveNumber;
            ToggleSwitch_DontSplitModelByMatchFirstIndex.IsOn = GlobalConfig.GameCfg.Value.DontSplitModelByMatchFirstIndex;
            ComboBox_AutoTextureFormat.SelectedIndex = GlobalConfig.GameCfg.Value.AutoTextureFormat;
            ToggleSwitch_AutoTextureOnlyConvertDiffuseMap.IsOn = GlobalConfig.GameCfg.Value.AutoTextureOnlyConvertDiffuseMap;
            ToggleSwitch_AutoDetectAndMarkTexture.IsOn = GlobalConfig.GameCfg.Value.AutoDetectAndMarkTexture;
            TextBox_ModSwitchKey.Text = GlobalConfig.GameCfg.Value.ModSwitchKey;

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
        
    }
}
