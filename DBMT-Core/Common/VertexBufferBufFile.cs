using DBMT_Core.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DBMT_Core
{
    public class VertexBufferBufFile
    {

        public byte[] FinalVB0Bytes;

        public VertexBufferBufFile(byte[] VB0Bytes)
        {
            this.FinalVB0Bytes = VB0Bytes;
        }


        public VertexBufferBufFile(List<Dictionary<int, byte[]>> BufDictList) {
            Dictionary<int, byte[]> MergedVB0Dict = DBMTBinaryUtils.MergeByteDicts(BufDictList);
            byte[] FinalVB0 = DBMTBinaryUtils.MergeDictionaryValues(MergedVB0Dict);
            this.FinalVB0Bytes = FinalVB0;
        }


        public void SaveToFile(string OutputVBBufFilePath)
        {
            File.WriteAllBytes(OutputVBBufFilePath, FinalVB0Bytes);
        }

        public void SelfDivide(int MinNumber, int MaxNumber, int Stride)
        {
            LOG.Info("VBBufFile::SelfDivide::");
            // 计算起始索引和结束索引
            int startIndex = MinNumber * Stride;
            int endIndex = MaxNumber * Stride;

            LOG.Info("Total Length: " + FinalVB0Bytes.Length.ToString());
            LOG.Info("StartIndex: " + startIndex.ToString());
            LOG.Info("EndIndex: " + endIndex.ToString());
            LOG.Info("Stride: " + Stride.ToString());

            // 计算需要复制的元素数量
            int lengthToCopy = endIndex - startIndex + Stride;
            LOG.Info("LengthToCopy: " + lengthToCopy.ToString());
            

            // 创建目标数组并复制数据
            byte[] resultArray = new byte[lengthToCopy];
            Array.Copy(FinalVB0Bytes, startIndex, resultArray, 0, lengthToCopy);

            this.FinalVB0Bytes = resultArray;
        }

        public Dictionary<string, byte[]> Get_ElementName_VBData_Map(D3D11GameType d3D11GameType)
        {
            LOG.Info("Get_ElementName_VBData_Map::Start");
            Dictionary<string, List<byte>> ElementName_VBData_Map = new Dictionary<string, List<byte>>();

            //提前初始化，避免在循环中初始化
            foreach (string ElementName in d3D11GameType.OrderedFullElementList)
            {
                D3D11Element d3D11Element = d3D11GameType.ElementNameD3D11ElementDict[ElementName];
                ElementName_VBData_Map[ElementName] = new List<byte>();
            }

            int stride = d3D11GameType.GetSelfStride();
            for (int i = 0; i < this.FinalVB0Bytes.Length; i = i + stride)
            {
                int offset = 0;

                foreach (string ElementName in d3D11GameType.OrderedFullElementList)
                {
                    D3D11Element d3D11Element = d3D11GameType.ElementNameD3D11ElementDict[ElementName];

                    byte[] CategoryData = DBMTBinaryUtils.GetRange_Byte(this.FinalVB0Bytes, i + offset, i + offset + d3D11Element.ByteWidth);
                 
                    ElementName_VBData_Map[ElementName].AddRange(CategoryData);
                    offset = offset + d3D11Element.ByteWidth;
                }

            }
            Dictionary<string, byte[]> ElementName_VBData_Map2 = new Dictionary<string, byte[]>();
            foreach (var item in ElementName_VBData_Map)
            {
                ElementName_VBData_Map2[item.Key] = item.Value.ToArray();
            }


            LOG.Info("Get_ElementName_VBData_Map::End");
            return ElementName_VBData_Map2;
        }



    }
}
