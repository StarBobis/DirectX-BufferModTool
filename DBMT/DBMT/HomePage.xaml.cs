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
using Windows.System;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage.Pickers;
using Windows.Storage;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DBMT
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HomePage : Page
    {
        public HomePage()
        {
            this.InitializeComponent();
            SetDefaultBackGroundImage();
            LocalizeLanguage();
        }

        private void LocalizeLanguage()
        {
            if (MainConfig.GameCfg.Value.Language == true)
            {
                TextBlock_Description.Text = "Developed By Trailblazers";
                TextBlock_Documents.Text = "Documents";
                TextBlock_Github.Text = "Github";
                TextBlock_TechniqueSupport.Text = "Catter";
                TextBlock_SubmitIssue.Text = "Issue";
            }
        }

        private void OpenLinkButtonClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null && Uri.IsWellFormedUriString(button.Tag.ToString(), UriKind.Absolute))
            {
                IAsyncOperation<bool> asyncOperation = Launcher.LaunchUriAsync(new Uri(button.Tag.ToString()));
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
            HomeBGImageBrush.ImageSource = bitmap;
        }

        private async void SetDIYBackGroundImage(object sender, RoutedEventArgs e)
        {
            FileOpenPicker PicturePicker =  CommandHelper.Get_FileOpenPicker(".png");
            StorageFile file = await PicturePicker.PickSingleFileAsync();
            if (file != null)
            {
                string AssetsFolderPath = PathHelper.GetAssetsFolderPath();
                string TargetPicturePath = Path.Combine(AssetsFolderPath, "HomePageBackGround_DIY.png");
                File.Copy(file.Path, TargetPicturePath,true);
                SetDefaultBackGroundImage();
            }
        }


    }
}
