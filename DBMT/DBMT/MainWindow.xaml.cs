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
using Windows.Graphics.Display;
using Windows.UI.WindowManagement;

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


            //如果勾选了直接启动到工作台界面
            if (GlobalConfig.GameCfg.Value.StartToWorkPage)
            {
                contentFrame.Navigate(typeof(WorkPage));
            }

            double logicalWidth = GlobalConfig.GameCfg.Value.WindowWidth;
            double logicalHeight = GlobalConfig.GameCfg.Value.WindowHeight;

            int actualWidth = (int)(logicalWidth );
            int actualHeight = (int)(logicalHeight );

            // 设置窗口大小
            AppWindow.ResizeClient(new SizeInt32(actualWidth, actualHeight));

            //移动窗口到屏幕中心
            MoveWindowToCenterScreen();

            // 语言切换
            LocalizeLanguage();

           
        }


        private void LocalizeLanguage()
        {
            if (GlobalConfig.GameCfg.Value.Language == true)
            {
                NavigationViewItem_HomePage.Content = "Home";
                NavigationViewItem_StarterPage.Content = "Starter";
                NavigationViewItem_WorkPage.Content = "Work";
            }
        }




        private async void InitializeGUI()
        {
            // C# code to set AppTitleBar uielement as titlebar
            //Window window = App.m_window;
            //window.ExtendsContentIntoTitleBar = true;  // enable custom titlebar

            //window.SetTitleBar(AppTitleBar);      // set user ui element as titlebar

            //读取配置文件
            GlobalConfig.MainCfg.LoadConfig();
            GlobalConfig.GameCfg.LoadConfig();
   

            //设置标题和宽高
            this.Title = GlobalConfig.DBMT_Title;

            

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
            if (DBMTStringUtils.ContainsChinese(GlobalConfig.ApplicationRunPath))
            {
                await MessageHelper.Show("DBMT所在路径不能含有中文，请重新将DBMT放置到纯英文路径.", "DBMT can't be put in a path that contains Chinese, please put DBMT in pure english path!");
                //注意，这里可能会导致空引用异常，App.Current.Exist()不一定会正确的结束程序
                //App.Current.Exit();
                //展示完窗口后立刻干掉程序，防止可能的异步线程空引用。
                Environment.Exit(0);
            }

            //检查DBMT核心是否存在
            if (!File.Exists(GlobalConfig.ApplicationRunPath + "Plugins\\" + GlobalConfig.MMT_EXE_FileName))
            {
                await MessageHelper.Show("未找到" + GlobalConfig.ApplicationRunPath + GlobalConfig.MMT_EXE_FileName + ",请将其放在本程序Plugins目录下，即将退出程序。", "Can't find " + GlobalConfig.ApplicationRunPath + GlobalConfig.MMT_EXE_FileName + ",please put it under this program's Plugins folder.");
                //注意，这里可能会导致空引用异常，App.Current.Exist()不一定会正确的结束程序
                //App.Current.Exit();

                //展示完窗口后立刻干掉程序，防止可能的异步线程空引用。
                Environment.Exit(0);
            }

            //如果DBMT存在，则开始正常初始化。
            //初始化Logs目录
            if (!Directory.Exists("Logs"))
            {
                Directory.CreateDirectory("Logs");
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

                ////切换到非设置界面时，保存所有数据
                //if (pageType != typeof(SettingsPage))
                //{
                //    SettingsHelper.SaveGameSettingsToConfig();
                //    SettingsHelper.SaveTextureSettingsToConfig();
                //}

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


        private void Window_Closed(object sender, WindowEventArgs args)
        {
            int WindowWidth = App.m_window.AppWindow.Size.Width - 16;
            int WindowHeight = App.m_window.AppWindow.Size.Height - 40;
            
            //只有窗口高度大于0时才会记忆
            if (GlobalConfig.GameCfg.Value.WindowHeight > 0)
            {
                GlobalConfig.GameCfg.Value.WindowWidth = WindowWidth;
                GlobalConfig.GameCfg.Value.WindowHeight = WindowHeight;
            }
          

            //关闭之前跳转到主页，触发Setting界面的界面切换方法从而保存设置中的内容。
            contentFrame.Navigate(typeof(HomePage));

            if (GlobalConfig.GameCfg.Value.AutoCleanFrameAnalysisFolder)
            {
                SettingsHelper.CleanFrameAnalysisFiles();
            }

            if (GlobalConfig.GameCfg.Value.AutoCleanLogFile)
            {
                {
                    SettingsHelper.CleanLogFiles();
                }

                GlobalConfig.GameCfg.SaveConfig();
                GlobalConfig.MainCfg.SaveConfig();
            }
        }
    }
}
