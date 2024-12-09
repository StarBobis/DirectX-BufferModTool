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
            // ��ȡ�����¼��� ComboBox ʵ��
            var comboBox = sender as ComboBox;

            // ����Ƿ�����ѡ�е���
            if (comboBox != null && comboBox.SelectedItem != null)
            {
                // ִ������Ҫ�Ĳ����������ȡѡ�е�����д���
                string basePath = Directory.GetCurrentDirectory();
                string selectedGame = comboBox.SelectedItem.ToString();
                string imagePath = Path.Combine(basePath, "Assets", selectedGame + ".png");

                if (!File.Exists(imagePath))
                {
                    imagePath = Path.Combine(basePath, "Assets", "DefaultGame.png");
                }

                // ���� BitmapImage ������ ImageSource
                BitmapImage bitmap = new BitmapImage(new Uri(imagePath));
                GameBGImageBrush.ImageSource = bitmap;
            }
        }

        private void LoadDirectoryNames()
        {
            string directoryPath = @"C:\Users\Administrator\Desktop\DBMT\Games"; // ָ����Ҫ��ȡ��Ŀ¼·��
            try
            {
                // ��ȡ������Ŀ¼����
                var directories = Directory.EnumerateDirectories(directoryPath)
                                          .Select(Path.GetFileName)
                                          .Where(name => !string.IsNullOrEmpty(name));

                // ��� ComboBox ��ǰ��
                GameSelectionComboBox.Items.Clear();

                // ��ÿ��Ŀ¼������ӵ� ComboBox ��
                foreach (var dirName in directories)
                {
                    GameSelectionComboBox.Items.Add(dirName);
                }

                GameSelectionComboBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                // ������ܷ������쳣������·����Ч����ʱ��ܾ�
                // ������������ʾ������Ϣ���û�
                Console.WriteLine($"Error loading directories: {ex.Message}");
            }
        }
    }
}
