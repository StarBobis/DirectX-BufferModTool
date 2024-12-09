using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
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
            //���ñ���Ϳ��
            this.Title = "DirectX Buffer Mod Tool V1.0.9.4 ���԰�";
            this.AppWindow.Resize(new SizeInt32(1000, 600));

            //�ƶ����ڵ���Ļ����
            MoveWindowToCenterScreen();

            //Ĭ��ѡ����ҳ����
            if (nvSample.MenuItems.Count > 0)
            {
                nvSample.SelectedItem = nvSample.MenuItems[0];
                contentFrame.Navigate(typeof(HomePage));
            }
            this.AppWindow.SetIcon("Assets/XiaoMai.ico");
        }

        private void MoveWindowToCenterScreen()
        {
            if (this.AppWindow != null)
            {
                // ��ȡ����ʾ���Ĺ�������С
                var displayArea = DisplayArea.GetFromWindowId(this.AppWindow.Id, DisplayAreaFallback.Nearest);

                // ��ȡ���ڵ�ǰ�ĳߴ�
                var windowSize = this.AppWindow.Size;

                // ���㴰�ھ�����������Ͻ�����
                int x = (int)((displayArea.WorkArea.Width - windowSize.Width) / 2);
                int y = (int)((displayArea.WorkArea.Height - windowSize.Height) / 2);

                // ���ô���λ��
                this.AppWindow.Move(new PointInt32 { X = x, Y = y });
            }
        }

        private void nvSample_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {

            // �������������ð�ť���򵼺�������ҳ��
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
