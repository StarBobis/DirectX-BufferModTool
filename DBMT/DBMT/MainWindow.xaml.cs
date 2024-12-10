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

            //���ñ���Ϳ��
            this.Title = MainConfig.DBMT_Title;
            this.AppWindow.Resize(new SizeInt32(1000, 600));

            //�ƶ����ڵ���Ļ����
            MoveWindowToCenterScreen();

            //Ĭ��ѡ����ҳ����
            if (nvSample.MenuItems.Count > 0)
            {
                nvSample.SelectedItem = nvSample.MenuItems[0];
                contentFrame.Navigate(typeof(HomePage));
            }
            //����ͼ��
            this.AppWindow.SetIcon("Assets/XiaoMai.ico");

            //��ǰ·�����ܴ�������·����,���򲿷ַ����޷���ȷִ��
            //��鵱ǰ�����Ƿ�Ϊλ������·����
            if (DBMTStringUtils.ContainsChinese(MainConfig.ApplicationRunPath))
            {
                MessageHelper.Show("DBMT����·�����ܺ������ģ������½�DBMT���õ���Ӣ��·��.", "DBMT can't be put in a path that contains Chinese, please put DBMT in pure english path!");
                App.Current.Exit();
            }

            //���DBMT�����Ƿ����
            if (!File.Exists(MainConfig.ApplicationRunPath + "Plugins\\" + MainConfig.MMT_EXE_FileName))
            {
                MessageHelper.Show("δ�ҵ�" + MainConfig.ApplicationRunPath + MainConfig.MMT_EXE_FileName + ",�뽫����ڱ�����PluginsĿ¼�£������˳�����","Can't find " + MainConfig.ApplicationRunPath + MainConfig.MMT_EXE_FileName + ",please put it under this program's Plugins folder.");
                App.Current.Exit();
            }

            //���DBMT���ڣ���ʼ������ʼ����
            //��ʼ��LogsĿ¼
            if (!Directory.Exists("Logs"))
            {
                Directory.CreateDirectory("Logs");
            }


            if (File.Exists(MainConfig.Path_Texture_SettingJson))
            {
                string json = File.ReadAllText(MainConfig.Path_Texture_SettingJson); // ��ȡ�ļ�����
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

            //��ȡ������һЩ���ñ��������ڴ��ݸ�ÿ��DrawIB��ConfigMod����
            if (File.Exists(MainConfig.Path_Game_SettingJson))
            {
                string json = File.ReadAllText(MainConfig.Path_Game_SettingJson); // ��ȡ�ļ�����
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
