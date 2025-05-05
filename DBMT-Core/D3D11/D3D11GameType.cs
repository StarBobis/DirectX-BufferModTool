using DBMT_Core;
using DBMT_Core.Utils;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DBMT_Core
{
    public class D3D11GameType
    {
        
        public string GameTypeName { get; set; } = "";
        public bool GPUPreSkinning { get; set; } = false;


        public List<D3D11Element> D3D11ElementList = [];

        public Dictionary<string, string> CategoryDrawCategoryDict = new Dictionary<string, string>();
        public List<string> OrderedCategoryNameList = [];

        public Dictionary<string, string> CategorySlotDict = new Dictionary<string, string>();
        public Dictionary<string, string> CategoryTopologyDict = new Dictionary<string, string>();

        public Dictionary<string, int> CategoryStrideDict = new Dictionary<string, int>();

        public List<string> OrderedFullElementList = [];
        public Dictionary<string, D3D11Element> ElementNameD3D11ElementDict = new Dictionary<string, D3D11Element>();

        public int GetSelfStride()
        {
            return this.GetElementListTotalStride(this.OrderedFullElementList);
        }

        public int GetElementListTotalStride(List<string> ElementNameList)
        {
            int TotalStride = 0;
            foreach (string ElementName in ElementNameList)
            {
                D3D11Element d3D11Element = this.ElementNameD3D11ElementDict[ElementName];
                TotalStride += d3D11Element.ByteWidth;
            }
            return TotalStride;
        }

        public void Initialize()
        {
            List<D3D11Element> elements = this.D3D11ElementList;

            foreach (D3D11Element d3D11Element in elements)
            {
                //判断是否为GPUPreSkinning
                if (d3D11Element.SemanticName == "BLENDINDICES")
                {
                    this.GPUPreSkinning = true;
                }

                if (!OrderedCategoryNameList.Contains(d3D11Element.Category))
                {
                    OrderedCategoryNameList.Add(d3D11Element.Category);
                }

                CategorySlotDict[d3D11Element.Category] = d3D11Element.ExtractSlot;
                CategoryTopologyDict[d3D11Element.Category] = d3D11Element.ExtractTechnique;
                this.CategoryDrawCategoryDict[d3D11Element.Category] = d3D11Element.DrawCategory;
            }


            //TODO 剩余部分内容需要完全照着MMT中的东西照搬过来

            //首先给每个元素设置SemanticIndex
            Dictionary<string, int> SemanticName_Index_Dict = new Dictionary<string, int>();

            List<D3D11Element> d3D11Elements01 = [];
            foreach (D3D11Element d3D11Element in elements)
            {
                if (SemanticName_Index_Dict.ContainsKey(d3D11Element.SemanticName))
                {
                    int number = SemanticName_Index_Dict[d3D11Element.SemanticName];

                    d3D11Element.SemanticIndex = number + 1;
                    d3D11Element.ByteWidth = DBMTFormatUtils.GetByteWidthFromFormat(d3D11Element.Format);
                    d3D11Element.ElementName = d3D11Element.SemanticName + d3D11Element.SemanticIndex.ToString();
                    d3D11Elements01.Add(d3D11Element);

                    OrderedFullElementList.Add(d3D11Element.ElementName);

                    SemanticName_Index_Dict[d3D11Element.SemanticName] = number + 1;
                }
                else
                {
                    d3D11Element.SemanticIndex = 0;
                    d3D11Element.ByteWidth = DBMTFormatUtils.GetByteWidthFromFormat(d3D11Element.Format);
                    d3D11Element.ElementName = d3D11Element.SemanticName;
                    d3D11Elements01.Add(d3D11Element);

                    OrderedFullElementList.Add(d3D11Element.ElementName);

                    SemanticName_Index_Dict[d3D11Element.SemanticName] = 0;
                }
            }

            foreach (D3D11Element d11Element in d3D11Elements01)
            {
                ElementNameD3D11ElementDict[d11Element.ElementName] = d11Element;

                if (CategoryStrideDict.ContainsKey(d11Element.Category))
                {
                    int stride = CategoryStrideDict[d11Element.Category];
                    CategoryStrideDict[d11Element.Category] = stride + d11Element.ByteWidth;
                }
                else
                {
                    CategoryStrideDict[d11Element.Category] = d11Element.ByteWidth;
                }

            }
        }

        public D3D11GameType(string GameTypeJsonFilePath)
        {
            if (!File.Exists(GameTypeJsonFilePath))
            {
                return;
            }
            this.GameTypeName = Path.GetFileNameWithoutExtension(GameTypeJsonFilePath);

            JObject jObject = DBMTJsonUtils.ReadJObjectFromFile(GameTypeJsonFilePath);
            List<D3D11Element> elements = jObject["D3D11ElementList"].ToObject<List<D3D11Element>>();
            this.D3D11ElementList = elements;
            this.Initialize();
        }

    }
}
