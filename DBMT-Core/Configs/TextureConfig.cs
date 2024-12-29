using Microsoft.UI.Xaml.Media.Imaging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBMT_Core
{
    public class ImageItem
    {
        /// <summary>
        /// 这里显示的是Trianglelist中的000001这种DrawCall开头的贴图名称
        /// </summary>
        public string FileName { get; set; } = "";
        /// <summary>
        /// ImageSource是以转换好格式的Deduped贴图路径创建的，便于正常显示在软件中
        /// </summary>
        public BitmapImage ImageSource { get; set; }
        /// <summary>
        /// 由槽位、大小、参加渲染的Deduped贴图 组成
        /// </summary>
        public string InfoBar { get; set; } = "";
        /// <summary>
        /// 当前贴图被标记的类型，被标记后将参与自动贴图过程
        /// </summary>
        public string MarkName { get; set; } = "";

        public string PixelSlot { get; set; } = "";
    }

    public class TextureConfig
    {
        /// <summary>
        /// 当前贴图配置保存到TextureConfigs里
        /// </summary>
        /// <param name="imageCollection"></param>
        /// <param name="TextureConfigSavePath"></param>
        public static void SaveTextureConfig(ObservableCollection<ImageItem> imageCollection, string TextureConfigSavePath)
        {
            JObject jObject = DBMTJsonUtils.CreateJObject();


            string PSHash = "";
            JArray SlotList = new JArray();
            foreach (ImageItem item in imageCollection)
            {
                if (PSHash == "")
                {
                    PSHash = DBMTStringUtils.GetPSHashFromFileName(item.FileName);
                }

                JObject slot_object = DBMTJsonUtils.CreateJObject();
                slot_object["Slot"] = item.PixelSlot;
                slot_object["MarkName"] = item.MarkName;

                SlotList.Add(slot_object);
            }

            JArray PixelShaderHashArray = new JArray();

            if (File.Exists(TextureConfigSavePath))
            {
                //如果存在，则读取并修改，这样能够确保PSHashList正确得到累积
                jObject = DBMTJsonUtils.ReadJObjectFromFile(TextureConfigSavePath);

                JArray jarryy = (JArray)jObject["PixelShaderHashList"];
                List<string> PixelShaderHashList = jarryy.ToObject<List<string>>();
                
                //先把已有的加回去
                foreach (string pshash in PixelShaderHashList)
                {
                    PixelShaderHashArray.Add(pshash);
                }

                //如果没有就加进去
                if (!PixelShaderHashList.Contains(PSHash))
                {
                    PixelShaderHashArray.Add(PSHash);
                }
                jObject["PixelShaderHashList"] = PixelShaderHashArray;
            }
            else
            {
                //在当前TextureConfig文件夹中，创建一个Json文件，内容来源于imageCollection
                PixelShaderHashArray.Add(PSHash);
            }

            jObject["SlotList"] = SlotList;
            jObject["PixelShaderHashList"] = PixelShaderHashArray;

            DBMTJsonUtils.SaveJObjectToFile(jObject, TextureConfigSavePath);
        }


        public static Dictionary<string, string> Read_PixeSlot_MarkName_Dict(string TextureConfigSavePath)
        {
            Dictionary<string, string> PixeSlot_MarkName_Dict = new Dictionary<string, string>();

            if(File.Exists(TextureConfigSavePath))
            {
                JObject jObject = DBMTJsonUtils.ReadJObjectFromFile(TextureConfigSavePath);
                JArray jArray = (JArray)jObject["SlotList"];
                foreach (JObject jobj in jArray)
                {
                    PixeSlot_MarkName_Dict[jobj["Slot"].ToString()] = jobj["MarkName"].ToString();
                }
            }
            
            return PixeSlot_MarkName_Dict;
        }

    }
}
