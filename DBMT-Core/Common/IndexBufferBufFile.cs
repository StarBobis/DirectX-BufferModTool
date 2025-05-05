using DBMT_Core;
using DBMT_Core.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBMT_Core.Common
{
    public class IndexBufferBufFile
    {
        public string Index { get; set; } = "";
        public string MatchFirstIndex { get; set; } = "";

        public int ReadDrawNumber { get; set; } = 0; //作用未知，但保留，好像是逆向的遗留产物？

        //最小的顶点索引值
        public int MinNumber { get; set; }= 0;
        //最大的顶点索引值
        public int MaxNumber { get; set; } = 0;
        //总共有几个索引值
        public int NumberCount { get; set; } = 0;
        //完全不同的顶点索引值数量，也代表实际用到的顶点数
        public int UniqueVertexCount { get; set; } = 0;

        //顶点索引值列表，多个IB文件组合在一起时，要用这个组合而不是二进制组合，避免格式不一致的问题。
        public List<int> NumberList { get; set; } = [];

        public void SaveToFile_UInt32(string OutputIBBufFilePath,int Offset)
        {
            List<int> WriteNumberList = [];
            foreach (int number in this.NumberList)
            {
                WriteNumberList.Add(number + Offset);
            }
            DBMTBinaryUtils.WriteAsR32_UINT(WriteNumberList, OutputIBBufFilePath);
        }

        public void SaveToFile_UInt16(string OutputIBBufFilePath, int Offset)
        {
            List<int> WriteNumberList = [];
            foreach (int number in this.NumberList)
            {
                WriteNumberList.Add(number + Offset);
            }
            DBMTBinaryUtils.WriteAsR16_UINT(WriteNumberList, OutputIBBufFilePath);
        }


        public void SelfDivide(int FirstIndex, int IndexCount)
        {
            LOG.Info("IBBufFile::SelfDivide::Start");
            LOG.Info("FirstIndex: " + FirstIndex.ToString());
            LOG.Info("IndexCount: " + IndexCount.ToString());
            LOG.Info("当前IB文件总顶点数: " + this.NumberList.Count);
            List<int> NewNumberList = [];
            HashSet<int> NumberSet = new HashSet<int>();

            int SubCount = 0;

            int TmpMaxNumber = 0;
            int TmpMinNumber = 9999999;

            for (int i = 0; i < this.NumberList.Count; i++)
            {
                if (i < FirstIndex)
                {
                    continue;
                }

                int TmpNumber = this.NumberList[i];
                NewNumberList.Add(TmpNumber);
                NumberSet.Add(TmpNumber);
                //LOG.Info("TmpNumber: " + TmpNumber.ToString());

                SubCount++;


                if (TmpNumber > TmpMaxNumber)
                {
                    TmpMaxNumber = TmpNumber;
                }

                if (TmpNumber < TmpMinNumber)
                {
                    TmpMinNumber = TmpNumber;
                }


                if (SubCount == IndexCount)
                {
                    break;
                }
            }

            this.MinNumber = TmpMinNumber;
            this.MaxNumber = TmpMaxNumber;
            this.NumberCount = NewNumberList.Count;
            this.UniqueVertexCount = NumberSet.Count;
            this.NumberList = NewNumberList;
            LOG.NewLine("IBBufFile::SelfDivide::End");

        }


        public IndexBufferBufFile()
        {
            //啥也不干但是得有，方便凭空构造
        }


        public IndexBufferBufFile(string IndexBufferFilePath,string Format)
        {
            string FileName = Path.GetFileName(IndexBufferFilePath);
            this.Index = FileName.Substring(0,6);
            

            string FormatLower = Format.ToLower();

            List<int> TmpNumberList = [];

            if (FormatLower == "dxgi_format_r32_uint")
            {
                TmpNumberList = DBMTBinaryUtils.ReadAsR32_UINT(IndexBufferFilePath);
            }
            else if (FormatLower == "dxgi_format_r16_uint") 
            {
                TmpNumberList = DBMTBinaryUtils.ReadAsR16_UINT(IndexBufferFilePath);
            }

            this.NumberList = TmpNumberList;
            //分别求出最大值最小值，读取数量，独特顶点数

            int MaxNumber = 0;
            int MinNumber = 9999999;
            foreach (int TmpNumber in TmpNumberList)
            {
                if (MaxNumber < TmpNumber)
                {
                    MaxNumber = TmpNumber;
                }

                if (MinNumber > TmpNumber)
                {
                    MinNumber = TmpNumber;
                }
            }

            this.NumberCount = TmpNumberList.Count;

            HashSet<int> uniqueNumbers = new HashSet<int>(TmpNumberList);

            this.UniqueVertexCount = uniqueNumbers.Count;
        }

    }
}
