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
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage.Pickers;
using Windows.Storage;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using WinRT.Interop;
using Windows.UI.Core;
using Windows.Storage.Pickers.Provider;
using DBMT_Core;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using Windows.UI.Popups;
using DBMT.Helper;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DBMT
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GamePage : Page
    {
        public GamePage()
        {
            this.InitializeComponent();
            LoadDirectoryNames();
            
        }

        private async void ChooseProcessPathButtonClick(object sender, RoutedEventArgs e)
        {
            string filepath = await CommandHelper.ChooseFileAndGetPath(".exe");
            if (filepath != "")
            {
                ProcessPathTextBox.Text = filepath;
            }
        }

        private async void ChooseStarterPathButtonClick(object sender, RoutedEventArgs e)
        {
            string filepath = await CommandHelper.ChooseFileAndGetPath(".exe");
            if (filepath != "")
            {
                StarterPathTextBox.Text = filepath;
            }
        }

        private void GameSelectionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // ��ȡ�����¼��� ComboBox ʵ��
            var comboBox = sender as ComboBox;

            // ����Ƿ�����ѡ�е���
            if (comboBox != null && comboBox.SelectedItem != null)
            {
                // ִ������Ҫ�Ĳ����������ȡѡ�е�����д���
                string selectedGame = comboBox.SelectedItem.ToString();

                MainConfig.SetCurrentGame(selectedGame);
                //��ȡd3dx.ini�е�����
                ReadPathSettingFromD3dxIni();

                SetGameBackGroundImage();
            }
        }

        private void SetGameBackGroundImage()
        {
            string basePath = Directory.GetCurrentDirectory();

            //���ñ���ͼƬ
            //Ĭ��Ϊ������Ϸ�û����õ�DIYͼƬ
            string imagePath = Path.Combine(basePath, "Assets", MainConfig.CurrentGameName + "_DIY.png");

            //���������DIY����ͼ����ʹ��Ĭ����Ϸ�ı���ͼ
            if (!File.Exists(imagePath))
            {
                imagePath = Path.Combine(basePath, "Assets", MainConfig.CurrentGameName + ".png");
            }

            //���Ĭ����Ϸ�ı���ͼ�������ڣ���ʹ����ҳ�ı���ͼ
            if (!File.Exists(imagePath))
            {
                imagePath = Path.Combine(basePath, "Assets", "HomePageBackGround.png");
            }

            // ���� BitmapImage ������ ImageSource
            BitmapImage bitmap = new BitmapImage(new Uri(imagePath));
            GameBGImageBrush.ImageSource = bitmap;
        }


        private async void SetDIYGameBackGroundImage(object sender, RoutedEventArgs e)
        {
            FileOpenPicker PicturePicker = CommandHelper.Get_FileOpenPicker(".png");
            StorageFile file = await PicturePicker.PickSingleFileAsync();
            if (file != null)
            {
                string AssetsFolderPath = PathHelper.GetAssetsFolderPath();
                string TargetPicturePath = Path.Combine(AssetsFolderPath, MainConfig.CurrentGameName + "_DIY.png");
                File.Copy(file.Path, TargetPicturePath, true);

                SetGameBackGroundImage();
            }
        }


        private void InitializePathConfigButtonClieck(object sender, RoutedEventArgs e)
        {
            ProcessPathTextBox.Text = "";
            StarterPathTextBox.Text = "";
        }

        private async void LoadDirectoryNames()
        {
            string CurrentDirectory = Directory.GetCurrentDirectory();
            string GamesPath = Path.Combine(CurrentDirectory, "Games\\");

            if (!Directory.Exists(GamesPath))
            {
                await MessageHelper.Show("Can't find Games folder in your run folder, Initialize Failed. : \n" + GamesPath);
                return;
            }

            // ��ȡ������Ŀ¼����
            var directories = Directory.EnumerateDirectories(GamesPath)
                                        .Select(Path.GetFileName)
                                        .Where(name => !string.IsNullOrEmpty(name))
                                        .OrderByDescending(name => name);

            // ��� ComboBox ��ǰ��
            GameSelectionComboBox.Items.Clear();

            // ��ÿ��Ŀ¼������ӵ� ComboBox ��
            foreach (var dirName in directories)
            {
                GameSelectionComboBox.Items.Add(dirName);
            }
            if (MainConfig.CurrentGameName == "")
            {
                GameSelectionComboBox.SelectedIndex = 0;
            }
            else
            {
                GameSelectionComboBox.SelectedItem = MainConfig.CurrentGameName;
            }
            
        }


        private void ReadPathSettingFromD3dxIni()
        {
            ProcessPathTextBox.Text = ConfigHelper.ReadAttributeFromD3DXIni("target");
            StarterPathTextBox.Text = ConfigHelper.ReadAttributeFromD3DXIni("launch");
        }

        private async void SavePathSettingsToD3dxIni(object sender, RoutedEventArgs e)
        {
            ConfigHelper.SaveAttributeToD3DXIni("[loader]","target",ProcessPathTextBox.Text);
            ConfigHelper.SaveAttributeToD3DXIni("[loader]","launch", StarterPathTextBox.Text);
            await MessageHelper.Show("����ɹ�");
        }

        private async void OpenD3dxIniFile(object sender, RoutedEventArgs e)
        {
            string d3dxini_path = ConfigHelper.GetD3DXIniPath();
            await CommandHelper.ShellOpenFile(d3dxini_path);
        }

        private async void Open3DmigotoLoaderExe(object sender, RoutedEventArgs e)
        {
            string MigotoLoaderExePath = Path.Combine(MainConfig.Path_LoaderFolder, "3Dmigoto Loader.exe");
            await CommandHelper.ShellOpenFile(MigotoLoaderExePath);
        }
        private async void Open3DmigotoFolder(object sender, RoutedEventArgs e)
        {
            
            await CommandHelper.ShellOpenFolder(MainConfig.Path_LoaderFolder);
        }

        private async void OpenShaderFixesFolder(object sender, RoutedEventArgs e)
        {

            await CommandHelper.ShellOpenFolder(Path.Combine(MainConfig.Path_LoaderFolder,"ShaderFixes") );
        }
    }
}
