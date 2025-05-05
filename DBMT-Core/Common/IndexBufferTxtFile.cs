using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBMT_Core
{
    public class IndexBufferTxtFile
    {
        public string FileName = "";
        public string Index = "";
        public string Hash = "";
        public string FirstIndex = "0";
        public string IndexCount = "";
        public string Topology = "";
        public string Format = "";
        public string ByteOffset = "";

        public UInt64 MaxNumber = 0;
        public UInt64 MinNumber = 999999999;
        public UInt64 UniqueVertexCount = 0;
        public UInt64 IndexNumberCount = 0;

        List<UInt64> NumberList = [];

        public IndexBufferTxtFile(string IBTxtFilePath,bool ReadTxtNumberData) {
            this.FileName = Path.GetFileName(IBTxtFilePath);
            this.Index = this.FileName.Substring(0, 6);
            this.Hash = this.FileName.Substring(11, 8);

            string[] FileLineArray = File.ReadAllLines(IBTxtFilePath, Encoding.UTF8);

            string TopologyStr = "topology:";
            string FirstIndexStr = "first index:";
            string IndexCoutnStr = "index count:";
            string FormatStr = "format:";
            string ByteOffsetStr = "byte offset:";

            UInt64 Count = 0;
            HashSet<UInt64> UniqueVertexNumberSet = new HashSet<UInt64>();
            foreach (string Line in FileLineArray)
            {
                if (Line.StartsWith(TopologyStr))
                {
                    this.Topology = Line.Substring(TopologyStr.Length).Trim();
                }
                else if (Line.StartsWith(FirstIndexStr))
                {
                    this.FirstIndex = Line.Substring(FirstIndexStr.Length).Trim();
                }
                else if (Line.StartsWith(IndexCoutnStr))
                {
                    this.IndexCount = Line.Substring(IndexCoutnStr.Length).Trim();
                }
                else if (Line.StartsWith(FormatStr))
                {
                    this.Format = Line.Substring(FormatStr.Length).Trim();
                }
                else if (Line.StartsWith(ByteOffsetStr))
                {
                    this.ByteOffset = Line.Substring(ByteOffsetStr.Length).Trim();
                }
                else
                {
                    if (!ReadTxtNumberData)
                    {
                        if (Count > 8)
                        {
                            break;
                        }
                    }

                    string TrimLine = Line.Trim();
                    if (TrimLine == "")
                    {
                        continue;
                    }

                    string[] LineSplits = TrimLine.Split(" ");
                    if (LineSplits.Length == 3)
                    {
                        foreach (string LineSplit in LineSplits)
                        {
                            UInt64 Number = UInt64.Parse(LineSplit);
                            UniqueVertexNumberSet.Add(Number);

                            this.NumberList.Add(Number);

                            if (Number > this.MaxNumber)
                            {
                                this.MaxNumber = Number;
                            }else if (Number < this.MinNumber)
                            {
                                this.MinNumber = Number;
                            }

                        }
                    }
                }

                Count++;
            }

            this.IndexNumberCount = (UInt64)this.NumberList.Count;
            this.UniqueVertexCount = (UInt64)UniqueVertexNumberSet.Count;

        }

    }
}
