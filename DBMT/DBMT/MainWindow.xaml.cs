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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
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

        public static MainWindow CurrentWindow;
        public NavigationView navigationView => nvSample;

        public Image mainWindowImageBrush => MainWindowImageBrush;


        public MainWindow()
        {
            this.InitializeComponent();
            this.ExtendsContentIntoTitleBar = true;

            CurrentWindow = this;
            
            InitializeGUI();


            double logicalWidth = GlobalConfig.WindowWidth;
            double logicalHeight = GlobalConfig.WindowHeight;

            int actualWidth = (int)(logicalWidth );
            int actualHeight = (int)(logicalHeight );

            if (actualHeight < 600)
            {
                actualHeight = 600;
            }
            if(actualWidth < 1000)
            {
                actualWidth = 1000;
            }

            // 设置窗口大小
            AppWindow.ResizeClient(new SizeInt32(actualWidth, actualHeight));

            //移动窗口到屏幕中心
            MoveWindowToCenterScreen();
        }


        private void InitializeGUI()
        {
            GlobalConfig.ReadConfig();
  

            //设置标题和宽高
            this.Title = GlobalConfig.DBMT_Title;

            //默认选中主页界面
            if (nvSample.MenuItems.Count > 0)
            {
                if (GlobalConfig.CurrentGameMigotoFolder != "" && Directory.Exists(GlobalConfig.CurrentGameMigotoFolder))
                {
                    nvSample.SelectedItem = nvSample.MenuItems[1];
                    contentFrame.Navigate(typeof(WorkPage));
                }
                else
                {
                    nvSample.SelectedItem = nvSample.MenuItems[0];
                    contentFrame.Navigate(typeof(HomePage));
                }
                    
            }
            //设置图标
            this.AppWindow.SetIcon("Assets/XiaoMai.ico");

            InitializeWorkFolder();
        }

        private void InitializeWorkFolder()
        {
            if (Directory.Exists(GlobalConfig.DBMTWorkFolder) && GlobalConfig.DBMTWorkFolder != "")
            {
                if (!Directory.Exists(GlobalConfig.Path_LogsFolder))
                {
                    Directory.CreateDirectory(GlobalConfig.Path_LogsFolder);
                }

                if (!Directory.Exists(GlobalConfig.Path_TotalWorkSpaceFolder))
                {
                    Directory.CreateDirectory(GlobalConfig.Path_TotalWorkSpaceFolder);
                }
            }
        }


        private void MoveWindowToCenterScreen()
        {
            
            // 获取与窗口关联的DisplayArea
            var displayArea = DisplayArea.GetFromWindowId(this.AppWindow.Id, DisplayAreaFallback.Nearest);
            // 获取窗口当前的尺寸
            var windowSize = this.AppWindow.Size;

            // 确保我们获取的是正确的显示器信息
            if (displayArea != null)
            {
                // 计算窗口居中所需的左上角坐标，考虑显示器的实际工作区（排除任务栏等）
                int x = (int)(displayArea.WorkArea.X + (displayArea.WorkArea.Width - windowSize.Width) / 2);
                int y = (int)(displayArea.WorkArea.Y + (displayArea.WorkArea.Height - windowSize.Height) / 2);

                // 设置窗口位置
                this.AppWindow.Move(new PointInt32 { X = x, Y = y });
            }

            int window_pos_x = GlobalConfig.WindowPositionX;
            int window_pos_y = GlobalConfig.WindowPositionY;


            if (window_pos_x <= 0)
            {
                window_pos_x = (int)(displayArea.WorkArea.X + (displayArea.WorkArea.Width - windowSize.Width) / 2);
            }
            if (window_pos_y <= 0)
            {
                window_pos_y = (int)(displayArea.WorkArea.Y + (displayArea.WorkArea.Height - windowSize.Height) / 2);
            }

            if (window_pos_x != -1 && window_pos_y != -1)
            {
                this.AppWindow.Move(new PointInt32(window_pos_x, window_pos_y));
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
                    case "WorkPage":
                        pageType = typeof(WorkPage);
                        break;
                    case "TexturePage":
                        pageType = typeof(TexturePage);
                        break;
                    case "GameTypePage":
                        pageType = typeof(GameTypePage);
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
            //保存窗口大小
            int WindowWidth = App.m_window.AppWindow.Size.Width - 16;
            int WindowHeight = App.m_window.AppWindow.Size.Height - 40;
            GlobalConfig.WindowWidth = WindowWidth;
            GlobalConfig.WindowHeight = WindowHeight;

            //保存窗口位置
            if (this.AppWindow != null)
            {
                // 获取窗口当前位置
                PointInt32 position = this.AppWindow.Position;

                // position.X 和 position.Y 分别是窗口左上角的X和Y坐标
                int x = position.X;
                int y = position.Y;

                GlobalConfig.WindowPositionX = x;
                GlobalConfig.WindowPositionY = y;
            }


            //关闭之前跳转到主页，触发Setting界面的界面切换方法从而保存设置中的内容。
            contentFrame.Navigate(typeof(HomePage));

            GlobalConfig.SaveConfig();


            if (GlobalConfig.AutoCleanFrameAnalysisFolder)
            {
                DBMTFileUtils.CleanFrameAnalysisFiles();
            }
        }
    }
}
