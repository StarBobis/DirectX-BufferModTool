using DBMT_Core.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBMT_Core.Common
{
    public class VertexBufferCombFile
    {
        public int FirstVertex { get; set; } = 0;
        public int ByteOffset { get; set; } = 0;
        public int Stride { get; set; } = 0;
        public int VertexCount { get; set; } = 0;
        public int ByteLength { get; set; } = 0;


        public byte[] CategoryBufferBytes { get; set; } = [];

        public string TxtFileName { get; set; } = "";
        public string BufFileName { get; set; } = "";
        


        /// <summary>
        /// 用于提取模型时拼接为完整vb0用的
        /// </summary>
        public Dictionary<int, byte[]> BufDict = new Dictionary<int, byte[]>();


        public VertexBufferCombFile(string FrameAnalysisFolderPath, string TrianglelistIndex, string CategorySlot) {
            LOG.Info("VertexBufferCombFile::");
            LOG.Info("TrianglelistIndex: " + TrianglelistIndex);
            LOG.Info("CategorySlot: " + CategorySlot);

            string VB0TxtFileName = FrameAnalysisDataUtils.FilterFirstFile(FrameAnalysisFolderPath, TrianglelistIndex + "-"+ CategorySlot + "=", ".txt");
            string VB0TxtFilePath = Path.Combine(FrameAnalysisFolderPath, VB0TxtFileName);
            this.TxtFileName = VB0TxtFileName;
            LOG.Info("VB0TxtFileName: " + VB0TxtFileName);

            string VB0BufFileName = FrameAnalysisDataUtils.FilterFirstFile(FrameAnalysisFolderPath, TrianglelistIndex + "-" + CategorySlot + "=", ".buf");
            string VB0BufFilePath = Path.Combine(FrameAnalysisFolderPath, VB0BufFileName);
            this.BufFileName = VB0BufFileName;
            LOG.Info("VB0BufFileName: " + VB0BufFileName);

            string ByteOffset_Str = DBMTFileUtils.FindMigotoIniAttributeInFile(VB0TxtFilePath, "byte offset");
            if (ByteOffset_Str != "")
            {
                LOG.Info("ByteOffset: " + ByteOffset_Str);
                this.ByteOffset = int.Parse(ByteOffset_Str);
            }

            string Stride_Str = DBMTFileUtils.FindMigotoIniAttributeInFile(VB0TxtFilePath, "stride");
            if (Stride_Str != "")
            {
                LOG.Info("Stride: " + Stride_Str);
                this.Stride = int.Parse(Stride_Str);
            }

            string VertexCount_Str = DBMTFileUtils.FindMigotoIniAttributeInFile(VB0TxtFilePath, "vertex count");
            if (VertexCount_Str != "")
            {
                LOG.Info("VertexCount: " + VertexCount_Str);
                this.VertexCount = int.Parse(VertexCount_Str);
            }

            string FirstVertex_Str = DBMTFileUtils.FindMigotoIniAttributeInFile(VB0TxtFilePath, "first vertex");
            if (FirstVertex_Str != "")
            {
                LOG.Info("FirstVertex: " + FirstVertex_Str);
                this.FirstVertex = int.Parse(FirstVertex_Str);
            }


            byte[] VB0BufBytes = File.ReadAllBytes(VB0BufFilePath);

            this.ByteLength = Stride * VertexCount;
            this.CategoryBufferBytes = new byte[this.ByteLength];

            Array.Copy(VB0BufBytes, ByteOffset, this.CategoryBufferBytes, this.FirstVertex * this.Stride, this.ByteLength);

            this.BufDict = DBMTBinaryUtils.SplitBytesByStride(this.CategoryBufferBytes,this.Stride);
        }

    }
}
