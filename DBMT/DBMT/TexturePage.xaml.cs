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
            this.InitializeComponent();

            ReadDrawIBList();

            // ����ListView������ԴΪimageCollection
            ImageListView.ItemsSource = imageCollection;

            //ʹ�������ļ��б����͸���������ñ���͸����
            TextureBGImageBrush.Opacity = GlobalConfig.GameCfg.Value.TexturePageBackGroundImageOpacity;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            // ִ������Ҫ�����ҳ�汻�رջ򵼺��뿪ʱ���еĴ���

            //����ȫ��������ΪҪ���滬��͸����
            GlobalConfig.GameCfg.Value.TexturePageBackGroundImageOpacity = (float)TextureBGImageBrush.Opacity;
            GlobalConfig.GameCfg.SaveConfig();

            // �����Ҫ�����Ե��û���� OnNavigatedFrom ����
            base.OnNavigatedFrom(e);
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
            Dictionary<string, ulong> ComponentName_MatchFirstIndex_Dict = DrawIBConfig.Read_ComponentName_MatchFirstIndex_Dict(DrawIB);

            if (ComponentName_MatchFirstIndex_Dict.Count != 0)
            {
                ComboBoxComponent.Items.Clear();
                foreach (var item in ComponentName_MatchFirstIndex_Dict)
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

            string ConvertedTextureFolderPath = TextureHelper.GetConvertedTexturesFolderPath(DrawIB);
            string CurrentDrawIBOutputFolder = DrawIBConfig.GetCurrentDrawIBOutputFolder(DrawIB);
            string TrianglelistFolder = CurrentDrawIBOutputFolder + "TrianglelistTextures\\";

            List<string> TextureFileNameList = FrameAnalysisData.FilterTextureFileNameList(TrianglelistFolder, DrawCallIndex);
            Debug.WriteLine("TextureFileNameList Size: " + TextureFileNameList.Count.ToString());

            Dictionary<string,string> TextureFileName_TextureSourceFilePath_Dict = new Dictionary<string,string> ();
            Dictionary<string,string> PixelSlot_TextureFileName_Dict = new Dictionary<string, string> ();

            string TrianglelistDedupedFileNameJsonName = "TrianglelistDedupedFileName.json";
            string TrianglelistDedupedFileNameJsonPath = Path.Combine(TrianglelistFolder, TrianglelistDedupedFileNameJsonName);

            if (!File.Exists(TrianglelistDedupedFileNameJsonPath))
            {
                return;
            }

            JObject TrianglelistDedupedFileNameJObject = DBMTJsonUtils.ReadJObjectFromFile(TrianglelistDedupedFileNameJsonPath);

            foreach(string TextureFileName in TextureFileNameList)
            {

                JObject TextureProperty = (JObject)TrianglelistDedupedFileNameJObject[TextureFileName];
                string DedupedTextureFileName = TextureProperty["FALogDedupedFileName"].ToString();
                LOG.Info("DedupedTextureFileName: " + DedupedTextureFileName);

                string ConvertedTextureFileName = "";

                if (DedupedTextureFileName.EndsWith(".dds"))
                {
                    string AutoTextureFormat = GlobalConfig.AutoTextureFormatSuffix;
                    ConvertedTextureFileName = Path.GetFileNameWithoutExtension(DedupedTextureFileName) + "." + AutoTextureFormat;

                }
                else
                {
                    ConvertedTextureFileName = DedupedTextureFileName;
                }

                string ConvertexTextureFilePath = Path.Combine(ConvertedTextureFolderPath, ConvertedTextureFileName);
                Debug.WriteLine(TextureFileName);

                if (File.Exists(ConvertexTextureFilePath))
                {
                    string PixelSlot = DBMTStringUtils.GetPixelSlotFromTextureFileName(TextureFileName);

                    //����Ѿ������˸ò�λ������dds������ʹ��jpg��ʽ��ͼ�����滻
                    if (PixelSlot_TextureFileName_Dict.ContainsKey(PixelSlot))
                    {
                        string ExistsTextureFileName = PixelSlot_TextureFileName_Dict[PixelSlot];
                        if (!ExistsTextureFileName.EndsWith(".dds"))
                        {
                            TextureFileName_TextureSourceFilePath_Dict.Add(TextureFileName, ConvertexTextureFilePath);
                        }
                    }
                    else
                    {
                        PixelSlot_TextureFileName_Dict[PixelSlot] = TextureFileName;
                        TextureFileName_TextureSourceFilePath_Dict.Add(TextureFileName, ConvertexTextureFilePath);
                    }
                }
         
            }


            // ��յ�ǰ�ļ���
            imageCollection.Clear();

            foreach (var item in TextureFileName_TextureSourceFilePath_Dict)
            {
                try
                {
                    string TextureFilePath = item.Value;

                    string PixelSlot = DBMTStringUtils.GetPixelSlotFromTextureFileName(item.Key);
                    string PixelSlotStr = "Slot: " + PixelSlot;
                    //Debug.WriteLine("PixelSlot: " + PixelSlot);
                    JObject TextureProperty = (JObject)TrianglelistDedupedFileNameJObject[item.Key];
                    string DedupedFileName = TextureProperty["FADataDedupedFileName"].ToString();

                    string DedupedInfo = "";
                    if (DedupedFileName == "")
                    {
                        DedupedInfo = "Render: False";
                    }
                    else
                    {
                        DedupedInfo = "Render: " + DedupedFileName;
                    }

                    StorageFile file = await StorageFile.GetFileFromPathAsync(TextureFilePath);
                    using (var stream = await file.OpenReadAsync())
                    {
                        BitmapImage bitmap = new BitmapImage();
                        await bitmap.SetSourceAsync(stream);

                        // ����ļ�����ͼƬ������
                        imageCollection.Add(new ImageItem
                        {
                            FileName = item.Key,
                            ImageSource = bitmap,
                            InfoBar = PixelSlotStr + "    Size: " + bitmap.PixelWidth.ToString() + " * " + bitmap.PixelHeight.ToString() + "    " + DedupedInfo,
                            PixelSlot = PixelSlot,
                            MarkName = ""
                        }) ;
                    }
                }
                catch (FileNotFoundException fnfEx)
                {
                    Debug.WriteLine($"File not found: {fnfEx.Message}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error loading image from {item.Value}: {ex.Message}");
                }
            }

            //Ȼ���ȡ��ǰImageCollection��Ӧ����ͼ�����б�
            ReadTextureConfigList();
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
            ReadTextureConfigList();

            _ = MessageHelper.Show("������ͼ���óɹ�!");
        }


        private void ReadTextureConfigList()
        {
            if (Directory.Exists(GlobalConfig.Path_GameTextureConfigFolder))
            {
                string[] TextureConfigFilePathList = Directory.GetFiles(GlobalConfig.Path_GameTextureConfigFolder);
                ComboBox_TextureConfigName.Items.Clear();

                List<string> TextureConfigNameList = new List<string>();
                foreach (string TextureConfigFilePath in TextureConfigFilePathList)
                {
                    //string TextureConfigFileName = Path.GetFileName(TextureConfigFilePath);
                    string TextureConfigName = Path.GetFileNameWithoutExtension(TextureConfigFilePath);
                    //ComboBox_TextureConfigName.Items.Add(TextureConfigName);

                    JObject TextureConfigJObject = DBMTJsonUtils.ReadJObjectFromFile(TextureConfigFilePath);
                    JArray SlotList = (JArray)TextureConfigJObject["SlotList"];

                    List<string> PixelSlotList = new List<string>();
                    foreach (JObject SlotObject in SlotList)
                    {
                        PixelSlotList.Add(SlotObject["Slot"].ToString());
                    }

                    //1.��������Ҫ����
                    if (imageCollection.Count != SlotList.Count)
                    {
                        continue;
                    }

                    //2.���ÿ��PixelSlotҪ���ϣ���Ȼ�������������˾ͻ���ʾ�����
                    bool allExists = true;
                    foreach (ImageItem imageItem in imageCollection)
                    {
                        if (!PixelSlotList.Contains(imageItem.PixelSlot))
                        {
                            allExists = false;
                        }
                    }
                    if (!allExists)
                    {
                        continue;
                    }

                    ComboBox_TextureConfigName.Items.Add(TextureConfigName);

                }


            }

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
            ReadTextureConfigList();

            _ = MessageHelper.Show("�������ͼ���óɹ�!");

        }

        private void Button_MarkTexture_Click(object sender, RoutedEventArgs e)
        {
            // ��ȡѡ�е���
            int selectedIndex = ImageListView.SelectedIndex;

            if (selectedIndex < 0)
            {
                _ = MessageHelper.Show("����ѡ��Ҫ��ǵ��Զ���ͼ");
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
            string TextureConfigSavePath = GlobalConfig.Path_GameTextureConfigFolder + TextureConfigName + ".json";

            if (File.Exists(TextureConfigSavePath))
            {
                Dictionary<string, string> PixeSlot_MarkName_Dict = TextureConfig.Read_PixeSlot_MarkName_Dict(TextureConfigSavePath);

                for (int i = 0; i < imageCollection.Count; i++)
                {
                    ImageItem imageItem = imageCollection[i];

                    string MarkName = PixeSlot_MarkName_Dict[imageItem.PixelSlot];
                    imageItem.MarkName = MarkName;


                    imageCollection[i] = imageItem;

                    Debug.WriteLine(imageCollection[i].MarkName);

                }
            }
        }

        private void ComboBox_TextureConfigName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Debug.WriteLine("ComboBox_TextureConfigName_SelectionChanged::");
            //TODO ѡ��һ���Զ���ͼ���ú�Ҫ�ѱ�����Ʒŵ���ǰImageCollection������
            if (ComboBox_TextureConfigName.SelectedIndex < 0)
            {
                Debug.WriteLine("-1 Skip");
                return;
            }
            FlushMarkName();
        }

        private void Button_ApplyToAutoTexture_Click(object sender, RoutedEventArgs e)
        {

            SaveTextureConfig();

            //��ȡDrawIB �� ComponentName
            string DrawIB = ComboBoxDrawIB.SelectedItem.ToString();
            string ComponentName = ComboBoxComponent.SelectedItem.ToString();
            string PartName = ComponentName.Substring("Component ".Length);
            string TextureConfigName = ComboBox_TextureConfigName.Text;
            if (TextureConfigName.Trim() == "")
            {
                _ = MessageHelper.Show("������ѡ��һ����ͼ����");
                return;
            }
            string TextureConfigSavePath = GlobalConfig.Path_GameTextureConfigFolder + TextureConfigName + ".json";

            //��ȡ��ǰ�Զ���ͼ����  ��λ��Ӧ�������
            //Dictionary<string, string> PixeSlot_MarkName_Dict = TextureConfig.Read_PixeSlot_MarkName_Dict(TextureConfigSavePath);

            //��ImageCollection�л�ȡÿ����λ��Ӧ���ļ���

            /*  ���ļ����ƹ�ȥ����tmp.json����ϳ����ָ�ʽ����Ϊ������ʱ����ı��ʽ�������ϵĸ�ʽ
             *  TODO �����ƻ���Hashֵ���Զ�ʶ���޸�tmp.json��Ȼ���ٻ��µĸ�ʽ
             *  
             "PartNameTextureResourceReplaceList": {
                "1": [
                    "ps-t0 = 7b4e1855-c5057d7e-1-DiffuseMap.dds",
                    "ps-t1 = 7b4e1855-e7392db4-1-LightMap.dds"
                ]
            }
             */

            //�Ȼ�ȡ���е�TYPE_��ͷ�����������ļ���
            List<string> TypeFolderPathList = DrawIBConfig.GetDrawIBOutputGameTypeFolderPathList(DrawIB);

            //������ÿ����ͼ����������ͼ��ȥ��˳��ƴװһ��PartName_PixelSlot = Ŀ���ļ����б� ��Dict
            //��Ϊ���ǵ�ǰֻ������ǰPartName������ֱ�������б�͸㶨��

            List<string> TextureResourceReplaceList = [];
            foreach (ImageItem imageItem in imageCollection)
            {
                //�б�ǵĲ��ܲ����Զ���ͼ
                if (imageItem.MarkName.Trim() == "")
                {
                    continue;
                }

                string suffix = Path.GetExtension(imageItem.FileName);
                string CurrentDrawIBOutputFolder = Path.Combine(GlobalConfig.Path_CurrentWorkSpaceFolder, DrawIB + "\\");
                string ImageSourcePath = CurrentDrawIBOutputFolder + "TrianglelistTextures\\" + imageItem.FileName;
                string TextureHash = DBMTStringUtils.GetFileHashFromFileName(imageItem.FileName);

                string TargetImageFileName = DrawIB + "-" + TextureHash + "-" + PartName + "-" + imageItem.MarkName + suffix;

                //ƴ��ResourceReplace
                TextureResourceReplaceList.Add(imageItem.PixelSlot + " = " + TargetImageFileName);

                //������ͼ��ȥ
                foreach (string TargetFolderPath in TypeFolderPathList)
                {
                    string TargetImageFilePath = Path.Combine(TargetFolderPath, TargetImageFileName);
                    File.Copy(ImageSourcePath, TargetImageFilePath, true);
                }
            }

            foreach (string TargetFolderPath in TypeFolderPathList)
            {
                //�޸�tmp.json
                string TmpJsonPath = Path.Combine(TargetFolderPath, "tmp.json");
                JObject jObject = DBMTJsonUtils.ReadJObjectFromFile(TmpJsonPath);
                JObject PartNameTextureResourceReplaceListObj = (JObject)jObject["PartNameTextureResourceReplaceList"];
                PartNameTextureResourceReplaceListObj[PartName] = new JArray(TextureResourceReplaceList);
                jObject["PartNameTextureResourceReplaceList"] = PartNameTextureResourceReplaceListObj;
                DBMTJsonUtils.SaveJObjectToFile(jObject, TmpJsonPath);
            }

            TextureHelper.ConvertAutoExtractedTexturesInDrawIBFolderToTargetFormat();

            _ = MessageHelper.Show("Ӧ�óɹ�");
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
        private void LightButtonClick_TurnOnOffBackGroundImageOpacity(object sender, RoutedEventArgs e)
        {
            if (TextureBGImageBrush.Opacity != 0)
            {
                GlobalConfig.GameCfg.Value.WorkPageBackGroundImageOpacity = (float)TextureBGImageBrush.Opacity;
                TextureBGImageBrush.Opacity = 0;
            }
            else
            {
                TextureBGImageBrush.Opacity = GlobalConfig.GameCfg.Value.WorkPageBackGroundImageOpacity;
            }
        }

    }
}
