using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using DBMT_Core;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using DBMT_Core.Utils;
using DirectXTexNet;
using static System.Net.Mime.MediaTypeNames;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DBMT
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TexturePage : Page
    {

        private ObservableCollection<ImageItem> imageCollection = new ObservableCollection<ImageItem>();

        public TexturePage()
        {
            //Debug.WriteLine("�л�����ͼҳ��");
            //Debug.WriteLine("��ǰ��ȡ����ͼ��ʽ:" + GlobalConfig.GameCfg.Value.AutoTextureFormat);

            this.InitializeComponent();


            ReadDrawIBList();

            // ����ListView������ԴΪimageCollection
            ImageListView.ItemsSource = imageCollection;


            if (GlobalConfig.AutoTextureFormat == "tga")
            {
                _ = MessageHelper.Show("��ͼ�����ݲ�֧��tga��ʽ�����������а�ȫ����ͼת����ʽ����Ϊ.jpg��.png��ʽ����������ȡģ��", "Texture settings currently do not support the TGA format. Please adjust the GlobalTextureConvertSetting in the settings to .jpg or .png, and do Extract Model again.");
            }

           
        }

        public void ReadDrawIBList()
        {
            ComboBoxDrawIB.Items.Clear();
            List<string> DrawIBList = DrawIBConfig.GetDrawIBListFromConfig();
            foreach (string DrawIB in DrawIBList)
            {
                ComboBoxDrawIB.Items.Add(DrawIB);
            }
            ComboBoxDrawIB.SelectedIndex = 0;
        }

        

        private void ComboBoxDrawIB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Debug.WriteLine("ComboBoxDrawIB_SelectionChanged::Begin");

            string DrawIB = ComboBoxDrawIB.SelectedItem.ToString();

            Dictionary<string, List<string>> ComponentName_DrawCallIndexList_Dict = DrawIBConfig.Get_ComponentName_DrawCallIndexList_Dict_FromJson(DrawIB);


            if (ComponentName_DrawCallIndexList_Dict.Count != 0)
            {
                ComboBoxComponent.Items.Clear();
                foreach (var item in ComponentName_DrawCallIndexList_Dict)
                {
                    ComboBoxComponent.Items.Add(item.Key);
                }

                ComboBoxComponent.SelectedIndex = 0;
            }
            
            Debug.WriteLine("ComboBoxDrawIB_SelectionChanged::End");
        }

        private void ComboBoxComponent_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Debug.WriteLine("ComboBoxComponent_SelectionChanged::Begin");
            if (ComboBoxComponent.SelectedIndex < 0)
            {
                return;
            }
            
            string DrawIB = ComboBoxDrawIB.SelectedItem.ToString();
            string ComponentName = ComboBoxComponent.SelectedItem.ToString();
            List<string> DrawCallIndexList = DrawIBConfig.Read_DrawCallIndexList(DrawIB, ComponentName);

            ComboBoxDrawCall.Items.Clear();
            foreach (string DrawCallIndex in DrawCallIndexList)
            {
                ComboBoxDrawCall.Items.Add(DrawCallIndex);
            }

            ComboBoxDrawCall.SelectedIndex = 0;
            Debug.WriteLine("ComboBoxComponent_SelectionChanged::End");
        }

        private async void ComboBoxDrawCall_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Debug.WriteLine("ComboBoxDrawCall_SelectionChanged::Begin");
            if (ComboBoxDrawCall.SelectedItem == null)
            {
                Debug.WriteLine("Select Nothing, Skip!");
                return;
            }
            Debug.WriteLine("Load New Config");


            string DrawCallIndex = ComboBoxDrawCall.SelectedItem.ToString();
            string DrawIB = ComboBoxDrawIB.SelectedItem.ToString();
            
          

            List<ImageItem> ImageList = await TextureConfig.Read_ImageItemList(DrawIB,DrawCallIndex );
            

            Debug.WriteLine("ImageList Size: " + ImageList.Count.ToString());

            // ��յ�ǰ����ͼ���ϣ������µ���Ϣ����GUI����ʾ������ͼ��Ϣ��������
            imageCollection.Clear();
            foreach (ImageItem item in ImageList)
            {
                imageCollection.Add(item);
            }

            //���ǿ����������imageCollection���ڣ���Ϊ�ᴥ��FLushMarkName
            ReadTextureConfigList(ImageList);

            Debug.WriteLine("ComboBoxDrawCall_SelectionChanged::End");
        }

        public void SaveTextureConfig()
        {
            string TextureConfigName = ComboBox_TextureConfigName.Text;
            if (TextureConfigName.Trim() == "")
            {
                _ = MessageHelper.Show("����ָ����ͼ���õ����ơ�", "Please fill Texture Config's Name before create.");
                return;
            }
            string TextureConfigSavePath = GlobalConfig.Path_GameTextureConfigFolder + TextureConfigName + ".json";

            TextureConfig.SaveTextureConfig(imageCollection, TextureConfigSavePath);

        }

        private void Button_SaveTextureConfig_Click(object sender, RoutedEventArgs e)
        {

            SaveTextureConfig();

            //�����Ҳ�����¶�ȡ����֤�б�����һ��
            List<ImageItem> ImageList = imageCollection.ToList();
            ReadTextureConfigList(ImageList);

            _ = MessageHelper.Show("������ͼ���óɹ�!");
        }


        private void ReadTextureConfigList(List<ImageItem> ImageList)
        {
            //�����������ͼ�����ļ��еĻ���ֱ�Ӳ���ȡ�Ϳ�����
            if (!Directory.Exists(GlobalConfig.Path_GameTextureConfigFolder))
            {
                return;
            }

            //����ɵ�
            ComboBox_TextureConfigName.Items.Clear();

            //�ҵ�ƥ�����ͼ�����ļ���
            Dictionary<string, JObject> TextureConfigName_JObject_Dict = TextureConfig.Get_TextureConfigName_JObject_Dict();

            List<string> MatchedTextureConfigNameList = TextureConfig.FindMatch_TextureConfigNameList(ImageList, TextureConfigName_JObject_Dict);

            //��ӵ�ҳ���н�����ʾ
            foreach(string TextureConfigName in MatchedTextureConfigNameList)
            {
                ComboBox_TextureConfigName.Items.Add(TextureConfigName);
            }
            //Ĭ��ѡ�е�һ��
            ComboBox_TextureConfigName.SelectedIndex = 0;
        }

        private void Menu_OpenGameTextureConfigsFolder_Click(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(GlobalConfig.Path_TextureConfigsFolder))
            {
                _ = CommandHelper.ShellOpenFolder(GlobalConfig.Path_GameTextureConfigFolder);
            }
            else
            {
                _ = MessageHelper.Show("��ǰ��Ϸ����ͼĿ¼�����ڣ������ٱ���һ����ͼ���á�");
            }
        }

        private void Button_CleanTextureConfig_Click(object sender, RoutedEventArgs e)
        {
            string TextureConfigName = ComboBox_TextureConfigName.Text;
            if (TextureConfigName.Trim() == "")
            {
                _ = MessageHelper.Show("������ָ����ͼ���õ����ơ�", "Please at least fill Texture Config's Name before create.");
                return;
            }
            string TextureConfigSavePath = GlobalConfig.Path_GameTextureConfigFolder + TextureConfigName + ".json";
            if (File.Exists(TextureConfigSavePath))
            {
                File.Delete(TextureConfigSavePath);
            }

            //��������¶�ȡ
            List<ImageItem> ImageList = imageCollection.ToList();
            ReadTextureConfigList(ImageList);

            _ = MessageHelper.Show("�������ͼ���óɹ�!");

        }

        private void Button_MarkTexture_Click(object sender, RoutedEventArgs e)
        {
            // ��ȡѡ�е���
            int selectedIndex = ImageListView.SelectedIndex;

            if (selectedIndex < 0)
            {
                _ = MessageHelper.Show("����ѡ��Ҫ��ǵ���ͼ","Please select a texture");
                return;
            }

            ImageItem selected_item = imageCollection[selectedIndex];

            selected_item.MarkName = ComboBox_MarkName.Text;

            imageCollection[selectedIndex] = selected_item;

            ImageListView.SelectedIndex = selectedIndex;

        }

        private void Button_CancelMarkTexture_Click(object sender, RoutedEventArgs e)
        {
            // ��ȡѡ�е���
            int selectedIndex = ImageListView.SelectedIndex;

            if (selectedIndex < 0)
            {
                _ = MessageHelper.Show("����ѡ��Ҫȡ����ǵ��Զ���ͼ");
                return;
            }

            ImageItem selected_item = imageCollection[selectedIndex];

            selected_item.MarkName = "";

            imageCollection[selectedIndex] = selected_item;

            ImageListView.SelectedIndex = selectedIndex;

        }

        private void FlushMarkName()
        {
            Debug.WriteLine("FlushMarkName::");
            string TextureConfigName = ComboBox_TextureConfigName.Text;
            Debug.WriteLine("TextureConfigName: " + TextureConfigName);
            string TextureConfigSavePath = GlobalConfig.Path_GameTextureConfigFolder + TextureConfigName + ".json";
            Debug.WriteLine("TextureConfigSavePath: " + TextureConfigSavePath);
            if (File.Exists(TextureConfigSavePath))
            {
                Dictionary<string, SlotObject> PixeSlot_MarkName_Dict = TextureConfig.Read_PixelSlot_SlotObject_Dict(TextureConfigSavePath);

                Debug.WriteLine("Count: " + imageCollection.Count.ToString());
                for (int i = 0; i < imageCollection.Count; i++)
                {
                    ImageItem imageItem = imageCollection[i];

                    SlotObject slot_obj = PixeSlot_MarkName_Dict[imageItem.PixelSlot];
                    imageItem.MarkName = slot_obj.MarkName;
                    imageItem.MarkStyle = slot_obj.MarkStyle;

                    imageCollection[i] = imageItem;

                    Debug.WriteLine(imageCollection[i].MarkName);

                }
            }
            else
            {
                Debug.WriteLine("TextureConfigSavePath doesn't exists: " + TextureConfigSavePath);
            }


        }

        private void ComboBox_TextureConfigName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Debug.WriteLine("ComboBox_TextureConfigName_SelectionChanged::");
            if (ComboBox_TextureConfigName.SelectedIndex < 0)
            {
                Debug.WriteLine("-1 Skip");
                return;
            }
            FlushMarkName();
        }

        private async void Button_ApplyToAutoTexture_Click(object sender, RoutedEventArgs e)
        {

            SaveTextureConfig();

            //��ȡDrawIB �� ComponentName
            string DrawIB = ComboBoxDrawIB.SelectedItem.ToString();
            string ComponentName = ComboBoxComponent.SelectedItem.ToString();
            
            string TextureConfigName = ComboBox_TextureConfigName.Text;
            if (TextureConfigName.Trim() == "")
            {
                _ = MessageHelper.Show("������ѡ��һ����ͼ����");
                return;
            }

            //Ӧ�ñ�ǵ���ͼ
            TextureConfig.ApplyTextureConfig(imageCollection.ToList(),DrawIB, ComponentName);

            //�����ƶ���ȥ����ͼת��ΪĿ���ʽ
            await TextureHelper.ConvertAutoExtractedTexturesInDrawIBFolderToTargetFormat();

            _ = MessageHelper.Show("Ӧ�óɹ�","Apply Texture Success!");
        }

        private void Menu_OpenCurrentWorkSpaceFolder_Click(object sender, RoutedEventArgs e)
        {
            _ = CommandHelper.ShellOpenFolder(GlobalConfig.Path_CurrentWorkSpaceFolder);
        }




        /// <summary>
        /// ���Ͻǰ������أ����ƿ��ر�����ʾ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //private void LightButtonClick_TurnOnOffBackGroundImageOpacity(object sender, RoutedEventArgs e)
        //{
        //    if (TextureBGImageBrush.Opacity != 0)
        //    {
        //        GlobalConfig.GameCfg.Value.TexturePageBackGroundImageOpacity = (float)TextureBGImageBrush.Opacity;
        //        TextureBGImageBrush.Opacity = 0;
        //    }
        //    else
        //    {
        //        TextureBGImageBrush.Opacity = GlobalConfig.GameCfg.Value.TexturePageBackGroundImageOpacity;
        //    }
        //}

        private void Menu_GenerateHashStyleTextureModTemplate_Click(object sender, RoutedEventArgs e)
        {
            //��ȡDrawIB
            string DrawIB = ComboBoxDrawIB.SelectedItem.ToString();
            string ComponentName = ComboBoxComponent.SelectedItem.ToString();
            string PartName = ComponentName.Substring("Component ".Length);
            //����������ͼMod���ļ���
            string GenerateTextureModFolderPath = GlobalConfig.Path_CurrentWorkSpaceFolder + "GeneratedTextureMod\\";
            Directory.CreateDirectory(GenerateTextureModFolderPath);

            Dictionary<string, TextureDeduped> dictionary = TextureConfig.Read_TrianglelistDedupedFileNameDict_FromJson(DrawIB);
            //
            List<string> TextureModIniList = [];
            TextureModIniList.Add("[TextureOverride_IB_SlotCheck]");
            TextureModIniList.Add("hash = " + DrawIB);
            TextureModIniList.Add("match_priority = 0");

            //���SlotCheck
            if (GlobalConfig.CurrentGameName == "ZZZ")
            {
                TextureModIniList.Add("run = CommandListSkinTexture");
                TextureModIniList.Add("");

            }
            else
            {
                foreach (ImageItem imageItem in imageCollection)
                {
                    if (imageItem.MarkName.Trim() == "")
                    {
                        continue;
                    }

                    string PixelSlot = DBMTStringUtils.GetPixelSlotFromTextureFileName(imageItem.FileName);
                    TextureModIniList.Add("checktextureoverride = " + PixelSlot);
                }
                TextureModIniList.Add("");
            }

            foreach (ImageItem imageItem in imageCollection)
            {
                //�б�ǵĲ��ܲ�������Mod
                if (imageItem.MarkName.Trim() == "")
                {
                    continue;
                }

                string suffix = Path.GetExtension(imageItem.FileName);
                string CurrentDrawIBOutputFolder = Path.Combine(GlobalConfig.Path_CurrentWorkSpaceFolder, DrawIB + "\\");

                string FALogDedupedFileName = dictionary[imageItem.FileName].FALogDedupedFileName;
                string ImageSourcePath = CurrentDrawIBOutputFolder + "DedupedTextures\\" + FALogDedupedFileName;
                string TextureHash = DBMTStringUtils.GetFileHashFromFileName(imageItem.FileName);

                //string TargetImageFileName = DrawIB + "-" + TextureHash + "-" + PartName + "-" + imageItem.MarkName + suffix;
                string TargetImageFileName = imageItem.MarkName + "_" + FALogDedupedFileName;

                TextureModIniList.Add("[TextureOverride_Texture_" + TextureHash + "]");
                TextureModIniList.Add("hash = " + TextureHash);
                TextureModIniList.Add("this = ResourceTexture_" + TextureHash);
                TextureModIniList.Add("");

                TextureModIniList.Add("[ResourceTexture_" + TextureHash + "]");
                TextureModIniList.Add("filename = " + TargetImageFileName);
                TextureModIniList.Add("");


                //������ͼ��ȥ
                File.Copy(ImageSourcePath, GenerateTextureModFolderPath + TargetImageFileName, true);
            }

            string TextureModIniFileName = DrawIB + "_TextureMod.ini";
            File.WriteAllLines(GenerateTextureModFolderPath +  TextureModIniFileName, TextureModIniList );

            _ = CommandHelper.ShellOpenFolder(GenerateTextureModFolderPath);
        }



        private async void Button_ChooseDynamicTextureFolderPath_Click(object sender, RoutedEventArgs e)
        {
            string selected_folder_path = await CommandHelper.ChooseFolderAndGetPath();
            if (selected_folder_path == "")
            {
                return;
            }

            TextBox_DynamicTextureFolderPath.Text = selected_folder_path;
        }


        private void Button_GenerateDynamicTextureMod_Click(object sender, RoutedEventArgs e)
        {
            string DynamicTextureFolderPath = TextBox_DynamicTextureFolderPath.Text + "\\";
            string TexturePrefix = TextBox_DynamicTextureName.Text;
            string TextureHash = TextBox_DynamicTextureHash.Text;
            string TextureSuffix = TextBox_DynamicTextureSuffix.Text;

            CoreFunctions.GenerateDynamicTextureMod(DynamicTextureFolderPath, TexturePrefix, TextureHash,TextureSuffix);

            _ = CommandHelper.ShellOpenFolder(DynamicTextureFolderPath);
        }

        private void Button_MarkAutoTextureHashStyle_Click(object sender, RoutedEventArgs e)
        {
            // ��ȡѡ�е���
            int selectedIndex = ImageListView.SelectedIndex;

            if (selectedIndex < 0)
            {
                _ = MessageHelper.Show("����ѡ��Ҫ��ǵ���ͼ", "Please select a texture");
                return;
            }

            ImageItem selected_item = imageCollection[selectedIndex];

            selected_item.MarkStyle = "Hash";

            imageCollection[selectedIndex] = selected_item;

            ImageListView.SelectedIndex = selectedIndex;
        }

        private void Button_MarkAutoTextureSlotStyle_Click(object sender, RoutedEventArgs e)
        {
            // ��ȡѡ�е���
            int selectedIndex = ImageListView.SelectedIndex;

            if (selectedIndex < 0)
            {
                _ = MessageHelper.Show("����ѡ��Ҫ��ǵ���ͼ", "Please select a texture");
                return;
            }

            ImageItem selected_item = imageCollection[selectedIndex];

            selected_item.MarkStyle = "Slot";

            imageCollection[selectedIndex] = selected_item;

            ImageListView.SelectedIndex = selectedIndex;
        }

        private async void Menu_SeeDDSInfo_Click(object sender, RoutedEventArgs e)
        {
            string FilePath = await CommandHelper.ChooseFileAndGetPath(".dds");
            if (FilePath != "" && File.Exists(FilePath))
            {
                TexHelper.Instance.LoadFromDDSFile(FilePath, DDS_FLAGS.NONE);
                TexMetadata tex = TexHelper.Instance.GetMetadataFromDDSFile(FilePath, DDS_FLAGS.NONE);
                // ������Ϣ�ַ���
                string info = $"���: {tex.Width}\n" +
                              $"�߶�: {tex.Height}\n" +
                              $"���: {tex.Depth}\n" +
                              $"Mip ����: {tex.MipLevels}\n" +
                              $"�����С: {tex.ArraySize}\n" +
                              $"��ʽ: {tex.Format}\n" +
                              $"ά��: {tex.Dimension}\n" +
                              $"�����־: {tex.MiscFlags}\n" +
                              $"�����־2: {tex.MiscFlags2}";

                _ = MessageHelper.Show(info);
            }
            
        }
    }
}
