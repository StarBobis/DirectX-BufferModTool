using DBMT_Core;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DBMT
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
            InitializeGUI();
        }

        private void InitializeGUI()
        {
            // C# code to set AppTitleBar uielement as titlebar
            //Window window = App.m_window;
            //window.ExtendsContentIntoTitleBar = true;  // enable custom titlebar
            //window.SetTitleBar(AppTitleBar);      // set user ui element as titlebar

            //设置标题和宽高
            this.Title = MainConfig.DBMT_Title;
            this.AppWindow.Resize(new SizeInt32(1000, 600));

            //移动窗口到屏幕中心
            MoveWindowToCenterScreen();

            //默认选中主页界面
            if (nvSample.MenuItems.Count > 0)
            {
                nvSample.SelectedItem = nvSample.MenuItems[0];
                contentFrame.Navigate(typeof(HomePage));
            }
            //设置图标
            this.AppWindow.SetIcon("Assets/XiaoMai.ico");

            //当前路径不能处于中文路径下,否则部分方法无法正确执行
            //检查当前程序是否为位于中文路径下
            if (DBMTStringUtils.ContainsChinese(MainConfig.ApplicationRunPath))
            {
                MessageHelper.Show("DBMT所在路径不能含有中文，请重新将DBMT放置到纯英文路径.", "DBMT can't be put in a path that contains Chinese, please put DBMT in pure english path!");
                App.Current.Exit();
            }

            //检查DBMT核心是否存在
            if (!File.Exists(MainConfig.ApplicationRunPath + "Plugins\\" + MainConfig.MMT_EXE_FileName))
            {
                MessageHelper.Show("未找到" + MainConfig.ApplicationRunPath + MainConfig.MMT_EXE_FileName + ",请将其放在本程序Plugins目录下，即将退出程序。","Can't find " + MainConfig.ApplicationRunPath + MainConfig.MMT_EXE_FileName + ",please put it under this program's Plugins folder.");
                App.Current.Exit();
            }

            //如果DBMT存在，则开始正常初始化。
            //初始化Logs目录
            if (!Directory.Exists("Logs"))
            {
                Directory.CreateDirectory("Logs");
            }


            if (File.Exists(MainConfig.Path_Texture_SettingJson))
            {
                string json = File.ReadAllText(MainConfig.Path_Texture_SettingJson); // 读取文件内容
                JObject jsonObject = JObject.Parse(json);

                //AutoTextureFormat
                if (jsonObject.ContainsKey("AutoTextureFormat"))
                {
                    MainConfig.AutoTextureFormat = (int)jsonObject["AutoTextureFormat"];
                }

                //ConvertDedupedTextures
                if (jsonObject.ContainsKey("ConvertDedupedTextures"))
                {
                    MainConfig.ConvertDedupedTextures = (bool)jsonObject["ConvertDedupedTextures"];
                }

                if (jsonObject.ContainsKey("AutoTextureOnlyConvertDiffuseMap"))
                {
                    MainConfig.AutoTextureOnlyConvertDiffuseMap = (bool)jsonObject["AutoTextureOnlyConvertDiffuseMap"];
                }

            }

            //读取并设置一些配置变量，用于传递给每个DrawIB的ConfigMod窗口
            if (File.Exists(MainConfig.Path_Game_SettingJson))
            {
                string json = File.ReadAllText(MainConfig.Path_Game_SettingJson); // 读取文件内容
                JObject jsonObject = JObject.Parse(json);
                MainConfig.AutoCleanFrameAnalysisFolder = (bool)jsonObject["AutoCleanFrameAnalysisFolder"];
                MainConfig.FrameAnalysisFolderReserveNumber = (int)jsonObject["FrameAnalysisFolderReserveNumber"];
                MainConfig.AutoCleanLogFile = (bool)jsonObject["AutoCleanLogFile"];
                MainConfig.LogFileReserveNumber = (int)jsonObject["LogFileReserveNumber"];
            }

        }

        private void MoveWindowToCenterScreen()
        {
            if (this.AppWindow != null)
            {
                // 获取主显示器的工作区大小
                var displayArea = DisplayArea.GetFromWindowId(this.AppWindow.Id, DisplayAreaFallback.Nearest);

                // 获取窗口当前的尺寸
                var windowSize = this.AppWindow.Size;

                // 计算窗口居中所需的左上角坐标
                int x = (int)((displayArea.WorkArea.Width - windowSize.Width) / 2);
                int y = (int)((displayArea.WorkArea.Height - windowSize.Height) / 2);

                // 设置窗口位置
                this.AppWindow.Move(new PointInt32 { X = x, Y = y });
            }
        }

        private void nvSample_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {

            // 如果点击的是设置按钮，则导航到设置页面
            if (args.IsSettingsInvoked)
            {
                contentFrame.Navigate(typeof(SettingsPage));
            }
            else if (args.InvokedItemContainer is NavigationViewItem item)
            {
                var pageTag = item.Tag.ToString();
                Type pageType = null;

                switch (pageTag)
                {
                    case "HomePage":
                        pageType = typeof(HomePage);
                        break;
                    case "GamePage":
                        pageType = typeof(GamePage);
                        break;
                    case "WorkPage":
                        pageType = typeof(WorkPage);
                        break;
                    case "SamplePage4":
                        //pageType = typeof(SamplePage4);
                        break;
                }

                if (pageType != null && contentFrame.Content?.GetType() != pageType)
                {
                    contentFrame.Navigate(pageType);
                }
            }
        }
    }
}
