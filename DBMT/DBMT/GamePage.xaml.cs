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

        private void GameSelectionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // 获取触发事件的 ComboBox 实例
            var comboBox = sender as ComboBox;

            // 检查是否有新选中的项
            if (comboBox != null && comboBox.SelectedItem != null)
            {
                // 执行你想要的操作，例如获取选中的项并进行处理
                string basePath = Directory.GetCurrentDirectory();
                string selectedGame = comboBox.SelectedItem.ToString();
                string imagePath = Path.Combine(basePath, "Assets", selectedGame + ".png");

                if (!File.Exists(imagePath))
                {
                    imagePath = Path.Combine(basePath, "Assets", "DefaultGame.png");
                }

                // 创建 BitmapImage 并设置 ImageSource
                BitmapImage bitmap = new BitmapImage(new Uri(imagePath));
                GameBGImageBrush.ImageSource = bitmap;
            }
        }

        private void LoadDirectoryNames()
        {
            string directoryPath = @"C:\Users\Administrator\Desktop\DBMT\Games"; // 指定你要读取的目录路径
            try
            {
                // 获取所有子目录名称
                var directories = Directory.EnumerateDirectories(directoryPath)
                                          .Select(Path.GetFileName)
                                          .Where(name => !string.IsNullOrEmpty(name));

                // 清空 ComboBox 当前项
                GameSelectionComboBox.Items.Clear();

                // 将每个目录名称添加到 ComboBox 中
                foreach (var dirName in directories)
                {
                    GameSelectionComboBox.Items.Add(dirName);
                }

                GameSelectionComboBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                // 处理可能发生的异常，例如路径无效或访问被拒绝
                // 可以在这里显示错误信息给用户
                Console.WriteLine($"Error loading directories: {ex.Message}");
            }
        }
    }
}
