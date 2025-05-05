using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBMT_Core.WWMI
{
    public class BoneMatrixBufFile
    {

        //骨骼姿态变换矩阵buf：
        // UnityCS中表现为 cs-t2槽位
        // UnityVS中表现为 vs-t0槽位
        // UnrealVS中表现为vs-cb4槽位
        // 每组数据长度为48，每组数据拥有16 * 3 ，每个16都是xyzw，所以基础类型为R32_FLOAT 基础字节为4

        public byte[] BoneMatrixBuf;

        public BoneMatrixBufFile()
        {

        }

        public BoneMatrixBufFile(string BoneMatrixFilePath)
        {
            this.BoneMatrixBuf = File.ReadAllBytes(BoneMatrixFilePath);
        }

        public byte[] GetBufDataByBlendIndex(int BlendIndex)
        {
            byte[] bufdata = new byte[] { };
            int StartIndex = BlendIndex * 48;
            int EndIndex = StartIndex + 48;
            bufdata = DBMTBinaryUtils.GetRange_Byte(this.BoneMatrixBuf, StartIndex, EndIndex);
            return bufdata;
        }
    }
}
