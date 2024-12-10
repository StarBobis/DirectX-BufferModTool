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
using DBMT_Core;
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using DBMT.Helper;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DBMT
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class WorkPage : Page
    {
        public WorkPage()
        {
            this.InitializeComponent();

            if (MainConfig.CurrentGameName == "")
            {
                MainConfig.ReadCurrentGameFromMainJson();
                MainConfig.SetCurrentGame(MainConfig.CurrentGameName);
            }

            string WorkSpaceName = MainConfig.ReadCurrentWorkSpaceFromMainJson();
            InitializeWorkSpace(MainConfig.CurrentWorkSpace);
        }

        private void ComboBoxWorkSpaceSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBoxWorkSpaceSelection.Items.Contains(ComboBoxWorkSpaceSelection.Text))
            {
                MainConfig.CurrentWorkSpace = ComboBoxWorkSpaceSelection.Text;
                MainConfig.SaveCurrentWorkSpaceToMainJson();
                ReadDrawIBListFromWorkSpace();
            }
        }

        void ReadDrawIBListFromWorkSpace()
        {
            string DrawIBListString = "";
            string Configpath = MainConfig.Path_OutputFolder + MainConfig.CurrentWorkSpace + "\\Config.json";
            if (File.Exists(Configpath))
            {
                
                //�л�����Ӧ����
                string jsonData = File.ReadAllText(Configpath);
                JObject jobj = JObject.Parse(jsonData);
                // Access the DrawIBList property and convert it to a List<string>
                JArray drawIBList = (JArray)jobj["DrawIBList"];
                List<string> drawIBListValues = drawIBList.ToObject<List<string>>();

                foreach (string drawib in drawIBListValues)
                {
                    DrawIBListString = DrawIBListString + drawib + ",";
                }

                if (!string.IsNullOrEmpty(DrawIBListString) && DrawIBListString.Length > 1)
                {
                    // �Ƴ����һ���ַ�
                    DrawIBListString = DrawIBListString.Substring(0, DrawIBListString.Length - 1);
                }
            }
            TextBoxDrawIBList.Text = DrawIBListString;
        }

        public void InitializeWorkSpace(string WorkSpaceName = "")
        {
            ComboBoxWorkSpaceSelection.Items.Clear();
            string[] WorkSpaceNameList = DBMTFileUtils.ReadWorkSpaceNameList(MainConfig.Path_OutputFolder);
            foreach (string WorkSpaceNameItem in WorkSpaceNameList)
            {
                ComboBoxWorkSpaceSelection.Items.Add(WorkSpaceNameItem);
            }

            if (ComboBoxWorkSpaceSelection.Items.Count >= 1)
            {
                if (WorkSpaceName == "")
                {
                    ComboBoxWorkSpaceSelection.SelectedIndex = 0;

                }
                else
                {
                    ComboBoxWorkSpaceSelection.SelectedItem = WorkSpaceName;
                }
            }

            ReadDrawIBListFromWorkSpace();
        }


        public async void CreateWorkSpaceFolder(object sender, RoutedEventArgs e)
        {
            if (ComboBoxWorkSpaceSelection.Text.Trim() == "")
            {
                await MessageHelper.Show("�����ռ����Ʋ���Ϊ��");
                return;
            }

            if (!ComboBoxWorkSpaceSelection.Items.Contains(ComboBoxWorkSpaceSelection.Text))
            {
                ////��������˴������ռ䣬�Ͳ������ļ��У�����ʹ���
                Directory.CreateDirectory(MainConfig.Path_OutputFolder + ComboBoxWorkSpaceSelection.Text);

                await MessageHelper.Show("�����ռ䴴���ɹ�");

                InitializeWorkSpace(ComboBoxWorkSpaceSelection.Text);
            }
            else
            {
                await MessageHelper.Show("��ǰ�����ռ��Ѵ���,�޷��ظ�����");
            }
        }

        public async void CleanCurrentWorkSpaceFile(object sender, RoutedEventArgs e)
        {
            string WorkSpaceFolderPath = MainConfig.Path_OutputFolder + ComboBoxWorkSpaceSelection.Text;
            Directory.Delete(WorkSpaceFolderPath, true);
            Directory.CreateDirectory(WorkSpaceFolderPath);
            await MessageHelper.Show("����ɹ�", "Clean Success");
        }

        public async void OpenCurrentWorkSpaceFolder(object sender, RoutedEventArgs e)
        {
            string WorkSpaceOutputFolder = MainConfig.Path_OutputFolder + ComboBoxWorkSpaceSelection.Text + "\\";
            if (!string.IsNullOrEmpty(WorkSpaceOutputFolder))
            {
                if (Directory.Exists(WorkSpaceOutputFolder))
                {
                    CommandHelper.ShellOpenFolder(WorkSpaceOutputFolder);
                }
                else
                {
                    await MessageHelper.Show("��Ŀ¼�����ڣ���������Output�ļ����Ƿ�������ȷ", "This folder doesn't exists,please check if your OutputFolder is correct.");
                }
            }
        }


        public void CleanDrawIBListTextBox(object sender, RoutedEventArgs e)
        {
            TextBoxDrawIBList.Text = "";
        }

        public List<string> GetDrawIBListFromTextBoxDrawIBList()
        {
            List<string> DrawIBList = new List<string>();
            if (TextBoxDrawIBList.Text.Contains(","))
            {
                DrawIBList = TextBoxDrawIBList.Text.Split(',').ToList();
            }
            else
            {
                DrawIBList.Add(TextBoxDrawIBList.Text);
            }

            return DrawIBList;
        }

        public async void SaveDrawIBList()
        {
            //(1) �����Ϸ�����Ƿ�����
            if (MainConfig.CurrentGameName == "")
            {
                await MessageHelper.Show("����ѡ����Ϸ����", "Please select a game before this.");
                return;
            }

            //(2) ������Ҫ�ѵ�ǰ����Ϸ����+���ͱ��浽MainSetting.json��
            MainConfig.SaveCurrentGameNameToMainJson();

            //(3) �����������е�drawIBList�е�DrawIB���������洢����Ӧ�����ļ���
            List<string> drawIBList = GetDrawIBListFromTextBoxDrawIBList();

            JObject jobj = new JObject();
            jobj["DrawIBList"] = new JArray(drawIBList);
            string json_string = jobj.ToString(Formatting.Indented);

            // ��JSON�ַ���д���ļ�
            File.WriteAllText(MainConfig.Path_OutputFolder + ComboBoxWorkSpaceSelection.Text + "\\Config.Json", json_string);

        }

        public async void SaveDrawIBListToConfig(object sender, RoutedEventArgs e)
        {
            SaveDrawIBList();
            await MessageHelper.Show("����ɹ�");
        }

        void ConvertAutoExtractedTexturesInDrawIBFolderToTargetFormat()
        {
            string WorkSpacePath = MainConfig.Path_OutputFolder + MainConfig.CurrentWorkSpace + "/";
            List<string> DrawIBList = ConfigHelper.GetDrawIBListFromConfig(MainConfig.CurrentWorkSpace);
            foreach (string DrawIB in DrawIBList)
            {
                //�����������outputĿ¼�µ�ddsתΪpng��ʽ
                string[] subdirectories = Directory.GetDirectories(WorkSpacePath + DrawIB + "/");
                foreach (string outputDirectory in subdirectories)
                {
                    //MessageBox.Show(Path.GetDirectoryName(outputDirectory));

                    if (!Path.GetFileName(outputDirectory).StartsWith("TYPE_"))
                    {
                        continue;
                    }


                    string[] filePathArray = Directory.GetFiles(outputDirectory);
                    foreach (string ddsFilePath in filePathArray)
                    {
                        if (MainConfig.AutoTextureOnlyConvertDiffuseMap)
                        {
                            if (!ddsFilePath.EndsWith("DiffuseMap.dds"))
                            {
                                continue;
                            }
                        }
                        else
                        {
                            if (!ddsFilePath.EndsWith(".dds"))
                            {
                                continue;
                            }
                        }

                        string TextureFormatString = TextureHelper.GetAutoTextureFormat();
                        CommandHelper.ConvertTexture(ddsFilePath, TextureFormatString, outputDirectory);
                    }
                }
            }

        }

        void ConvertDedupedTexturesToTargetFormat()
        {
            string WorkSpacePath = MainConfig.Path_OutputFolder + MainConfig.CurrentWorkSpace + "/";
            List<string> DrawIBList = ConfigHelper.GetDrawIBListFromConfig(MainConfig.CurrentWorkSpace);
            foreach (string DrawIB in DrawIBList)
            {
                //�����������outputĿ¼�µ�ddsתΪpng��ʽ
                string DedupedTexturesFolderPath = WorkSpacePath + DrawIB + "/DedupedTextures/";
                //MessageHelper.Show(DedupedTexturesFolderPath);
                if (!Directory.Exists(DedupedTexturesFolderPath))
                {
                    return;
                }

                string TextureFormatString = TextureHelper.GetAutoTextureFormat();
                string DedupedTexturesConvertFolderPath = WorkSpacePath + DrawIB + "/DedupedTextures_" + TextureFormatString + "/";
                //MessageHelper.Show(DedupedTexturesConvertFolderPath);
                TextureHelper.ConvertAllTextureFilesToTargetFolder(DedupedTexturesFolderPath, DedupedTexturesConvertFolderPath);
            }
        }

        public async void ExtractModel(object sender, RoutedEventArgs e)
        {
            if (TextBoxDrawIBList.Text.Trim() == "")
            {
                await MessageHelper.Show("������֮ǰ����д���Ļ���IB�Ĺ�ϣֵ����������","Please fill your DrawIB and config it before run.");
                return;
            }

            if (ComboBoxWorkSpaceSelection.Text.Trim() == "")
            {
                await MessageHelper.Show("����ָ�������ռ�");
                return;
            }

            SaveDrawIBList();

            bool RunResult = await CommandHelper.runCommand("merge");

            if (RunResult)
            {
                ConvertAutoExtractedTexturesInDrawIBFolderToTargetFormat();

                if (MainConfig.ConvertDedupedTextures)
                {
                    ConvertDedupedTexturesToTargetFormat();
                }
            }
            else
            {
                //TODO �����±�����־�ļ�
            }
            
        }


        public async void GenerateMod(object sender, RoutedEventArgs e)
        {
            if (ComboBoxWorkSpaceSelection.Text.Trim() == "")
            {
                await MessageHelper.Show("����ѡ�����ռ�");
                return;
            }

            bool RunResult = await CommandHelper.runCommand("split");
            if (RunResult)
            {
                OpenModsFolder(sender, e);
            }
        }


        public async void OpenWorkSpaceGenerateModFolder(object sender, RoutedEventArgs e)
        {
            string GeneratedModFolderPath = MainConfig.Path_OutputFolder + "/" + MainConfig.CurrentWorkSpace + "/GeneratedMod/";
           
            if (Directory.Exists(GeneratedModFolderPath))
            {
                CommandHelper.ShellOpenFolder(GeneratedModFolderPath);
            }
            else
            {
                await MessageHelper.Show("����δ���ɶ���ģ��", "You have not generate any mod yet");
            }
        }

        public async void OpenModsFolder(object sender, RoutedEventArgs e)
        {
            string modsFolder = MainConfig.Path_LoaderFolder + "Mods/";
            if (Directory.Exists(modsFolder))
            {
                CommandHelper.ShellOpenFolder(modsFolder);
            }
            else
            {
                await MessageHelper.Show("��Ŀ¼�����ڣ���������Mods�ļ����Ƿ�������ȷ", "This path didn't exists, please check if your Mods folder is correct");
            }
        }

        string GetLatestFrameAnalysisFolder()
        {
            string[] directories = Directory.GetDirectories(MainConfig.Path_LoaderFolder);
            List<string> frameAnalysisFileList = new List<string>();
            foreach (string directory in directories)
            {
                string directoryName = Path.GetFileName(directory);

                if (directoryName.StartsWith("FrameAnalysis-"))
                {
                    frameAnalysisFileList.Add(directoryName);
                }
            }

            //
            if (frameAnalysisFileList.Count > 0)
            {
                frameAnalysisFileList.Sort();

                string latestFrameAnalysisFolder = MainConfig.Path_LoaderFolder.Replace("/", "\\") + frameAnalysisFileList.Last();
                return latestFrameAnalysisFolder;
            }

            return "";
        }

        public async void OpenLatestFrameAnalysisFolder(object sender, RoutedEventArgs e)
        {
            string latestFrameAnalysisFolder = GetLatestFrameAnalysisFolder();
            if (!string.IsNullOrEmpty(latestFrameAnalysisFolder))
            {
                CommandHelper.ShellOpenFolder(latestFrameAnalysisFolder);
            }
            else
            {
                await MessageHelper.Show("Ŀ��Ŀ¼û���κ�FrameAnalysis�ļ���","Target directory didn't have any FrameAnalysisFolder.");
            }
        }


        public async void OpenLatestFrameAnalysisLogTxtFile(object sender, RoutedEventArgs e)
        {
            string[] directories = Directory.GetDirectories(MainConfig.Path_LoaderFolder);
            List<string> frameAnalysisFileList = new List<string>();
            foreach (string directory in directories)
            {
                string directoryName = Path.GetFileName(directory);

                if (directoryName.StartsWith("FrameAnalysis-"))
                {
                    frameAnalysisFileList.Add(directoryName);
                }
            }

            //
            if (frameAnalysisFileList.Count > 0)
            {
                frameAnalysisFileList.Sort();

                string latestFrameAnalysisFolder = MainConfig.Path_LoaderFolder.Replace("/", "\\") + frameAnalysisFileList.Last();

                CommandHelper.ShellOpenFile(latestFrameAnalysisFolder + "\\log.txt");
            }
            else
            {
                await MessageHelper.Show("Ŀ��Ŀ¼û���κ�FrameAnalysis�ļ���", "Target directory didn't have any FrameAnalysisFolder.");
            }
        }

        public async void OpenLatestFrameAnalysisDedupedFolder(object sender, RoutedEventArgs e)
        {
            string latestFrameAnalysisFolder = GetLatestFrameAnalysisFolder();
            if (!string.IsNullOrEmpty(latestFrameAnalysisFolder))
            {
                CommandHelper.ShellOpenFolder(latestFrameAnalysisFolder + "\\deduped\\");
            }
            else
            {
                await MessageHelper.Show("Ŀ��Ŀ¼û���κ�FrameAnalysis�ļ���", "Target directory didn't have any FrameAnalysisFolder.");
            }
        }


        public async void Open3DmigotoFolder(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(MainConfig.Path_LoaderFolder))
            {
                CommandHelper.ShellOpenFolder(MainConfig.Path_LoaderFolder.Replace("/", "\\"));
            }
            else
            {
                await MessageHelper.Show("��Ŀ¼�����ڣ�����3Dmigoto�ļ����Ƿ�������ȷ", "This directory doesn't exists.");
            }
        }


    }
}
