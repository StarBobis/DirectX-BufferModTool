using DBMT_Core.Common;
using DBMT_Core.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBMT_Core.WWMI
{
    public class ShapeKey
    {
        public int CheckSum { get; set; } = 0;
        public int ShapeKeyOffsetNumberLast { get; set; } = 0;
        public int ShapeKeyVertexOffsetCount { get; set; } = 0;
        public List<int> ShapeKeyVertexIdList { get; set; } = [];

        public Dictionary<int, List<int>> ShapeKeyId_VertexIdList_Map = new Dictionary<int, List<int>>();

        public Dictionary<int, Dictionary<int, byte[]>> ShapeKeyId_VertexId_VertexOffset_Map = new Dictionary<int, Dictionary<int, byte[]>>();

        public ShapeKey()
        {

        }


        public ShapeKey(string InShapeKeyOffsetPath,string InShapeKeyVertexIdPath,string InShapeKeyVertexOffsetPath)
        {
            if (InShapeKeyOffsetPath == "")
            {
                LOG.Error("InShapeKeyOffsetPath是空的");
            }
            if (InShapeKeyVertexIdPath == "")
            {
                LOG.Error("InShapeKeyVertexIdPath是空的");
            }
            if (InShapeKeyVertexOffsetPath == "")
            {
                LOG.Error("InShapeKeyVertexOffsetPath是空的");
            }

            //1.读取cs-cb0的前512个字节数据为ShapeKeyOffset.buf
            byte[] CSCB0BufData = File.ReadAllBytes(InShapeKeyOffsetPath);
            byte[] ShapeKeyOffsetBuf = DBMTBinaryUtils.GetRange_Byte(CSCB0BufData,0,512);

            List<int> ShapeKeyOffsets = DBMTBinaryUtils.ReadAsR32_UINT(InShapeKeyOffsetPath);
            this.CheckSum = ShapeKeyOffsets[0] + ShapeKeyOffsets[1] + ShapeKeyOffsets[2] + ShapeKeyOffsets[3];


            //获取ShapeKeyOffset的最大值，这个值也是VertexId和VertexOffset的总数量
            byte[] ShapeKeyOffsetNumberData = DBMTBinaryUtils.GetRange_Byte(ShapeKeyOffsetBuf,ShapeKeyOffsetBuf.Length - 4, ShapeKeyOffsetBuf.Length);
            uint ShapeKeyOffsetNumberLast = BitConverter.ToUInt32(ShapeKeyOffsetNumberData, 0);
            this.ShapeKeyOffsetNumberLast = (int)ShapeKeyOffsetNumberLast;

            //2.读取cs-t0前ShapeKeyOffsetNumber * 4 个数据，作为ShapeKeyVertexID
            byte[] CST0BufData = File.ReadAllBytes(InShapeKeyVertexIdPath);
            byte[] ShapeKeyVertexIdBuf = DBMTBinaryUtils.GetRange_Byte(CST0BufData, 0, ShapeKeyOffsetNumberLast * 4);

            //3.读取cs-t1前ShapeKeyOffsetNumber * 12个数据，作为ShapeKeyVertexOffset
            byte[] CST1BufData = File.ReadAllBytes(InShapeKeyVertexOffsetPath);
            //这里必须是12才能读取完整的数据 因为每个数据都是12个字节
            byte[] ShapeKeyVertexOffsetBuf = DBMTBinaryUtils.GetRange_Byte(CST1BufData, 0, ShapeKeyOffsetNumberLast * 12);

            this.ShapeKeyVertexOffsetCount = CST1BufData.Length / 12;

            LOG.Info("Get_VertexID_VertexOffsetData_Map::");
            List<byte[]> ShapeKeyVertexOffsetList = [];
            for (int i = 0; i < ShapeKeyVertexOffsetBuf.Length; i = i + 12)
            {
                //只有前六个数据是有用的，后面6个是固定的 0000 0000 0000
                byte[] ShapeKeyOffsetData = DBMTBinaryUtils.GetRange_Byte(ShapeKeyVertexOffsetBuf, i, i + 6);
                ShapeKeyVertexOffsetList.Add(ShapeKeyOffsetData);
            }


            LOG.Info("Size: " + ShapeKeyVertexOffsetList.Count.ToString());
            LOG.NewLine();

            LOG.Info("Get_VertexIDList::");
            //ShapeKeyVertexID也需要转换成VertexID_List;
            List<int> ShapeKeyVertexIDList = [];
            HashSet<int> TmpVertexIDSet = new HashSet<int>();
            for (int i = 0; i < ShapeKeyVertexIdBuf.Length; i = i + 4)
            {
                byte[] ShapeKeyVertexIdData = DBMTBinaryUtils.GetRange_Byte(ShapeKeyVertexIdBuf, i, i + 4);
                uint VertexID = BitConverter.ToUInt32(ShapeKeyVertexIdData,0);
                ShapeKeyVertexIDList.Add((int)VertexID);
                TmpVertexIDSet.Add((int)VertexID);
            }
            this.ShapeKeyVertexIdList = ShapeKeyVertexIDList;
            LOG.Info("TmpVertexIDList: " + ShapeKeyVertexIDList.Count.ToString() + " TmpVertexIDSet: " + TmpVertexIDSet.Count.ToString());
            LOG.NewLine();


            //先解析ShapeKeyOffset
            LOG.Info("Parse ShapeKeyOffset::");
            //得到独一无二的ShapeKeyOffset，允许有最多俩相同值重复，因为只有最后一个数据会重复加上一次
            //不过无妨，因为后面重复的数据会被过滤掉。
            List<int> ShapeKeyOffsetList = [];
            Dictionary<int,int> SeenMap = new Dictionary<int, int>();
            for (int i = 0; i < ShapeKeyOffsetBuf.Length; i = i + 4)
            {
                byte[] NumberByteList1 = DBMTBinaryUtils.GetRange_Byte(ShapeKeyOffsetBuf, i, i + 4);
                uint Offset_Start = BitConverter.ToUInt32(NumberByteList1);
                if (!SeenMap.ContainsKey((int)Offset_Start))
                {
                    SeenMap[(int)Offset_Start] = 1;
                    ShapeKeyOffsetList.Add((int)Offset_Start);
                }
                else if (SeenMap[(int)Offset_Start] < 2)
                {
                    SeenMap[(int)Offset_Start] = SeenMap[(int)Offset_Start] + 1;
                    ShapeKeyOffsetList.Add((int)Offset_Start);
                }
            }

            LOG.Info("Find ShapeKeyId_VertexIdList_Map::");
            Dictionary<int, List<int>> ShapeKeyId_VertexIdList_Map = new Dictionary<int, List<int>>();
            Dictionary<int, List<byte[]>> ShapeKeyId_VertexOffsetList_Map = new Dictionary<int, List<byte[]>>();
            int ShapeKeyId = 0;
            for (int i = 0; i < ShapeKeyOffsetList.Count; i++)
            {
                int StartOffset = ShapeKeyOffsetList[i];
                int EndOffset = 0;
                if (ShapeKeyOffsetList.Count > i + 1)
                {
                    EndOffset = ShapeKeyOffsetList[i + 1];
                }
                else
                {
                    EndOffset = ShapeKeyVertexIdBuf.Length / 4;
                }
                //起始和结束相同的是无效的ShapeKey，也是对上一步重复数据的过滤。
                if (StartOffset == EndOffset)
                {
                    continue;
                }
                List<int> ShapeKeyVertexIdList = DBMTBinaryUtils.GetRange_INT32(ShapeKeyVertexIDList, StartOffset, EndOffset);
                List<byte[]> SubVertexOffsetList = [];

                for (int j = 0; j < ShapeKeyVertexOffsetList.Count; j++)
                {
                    if (j < StartOffset || j > EndOffset)
                    {
                        continue;
                    }
                    SubVertexOffsetList.Add(ShapeKeyVertexOffsetList[j]);
                }
                ShapeKeyId_VertexOffsetList_Map[ShapeKeyId] = SubVertexOffsetList;
                ShapeKeyId_VertexIdList_Map[ShapeKeyId] = ShapeKeyVertexIdList;
                ShapeKeyId++;
            }

            this.ShapeKeyId_VertexIdList_Map = ShapeKeyId_VertexIdList_Map;

            //读取ShapeKeyId_VertexId_VertexOffset_Map
            Dictionary<int, Dictionary<int, byte[]>> ShapeKeyId_VertexId_VertexOffset_Map = new Dictionary<int, Dictionary<int, byte[]>>();
            foreach (var item in ShapeKeyId_VertexIdList_Map) {
                int ShapeKeyIdIn = item.Key;
                List<int> ShapeKeyVertexIdList = item.Value;
                List<byte[]> VertexOffsetList = ShapeKeyId_VertexOffsetList_Map[ShapeKeyIdIn];
                Dictionary<int, byte[]> VertexId_VertexOffset_Map = new Dictionary<int, byte[]>();
                for (int i = 0; i < ShapeKeyVertexIdList.Count; i++)
                {
                    int VertexId = ShapeKeyVertexIdList[i];
                    byte[] VertexOffset = VertexOffsetList[i];
                    VertexId_VertexOffset_Map[VertexId] = VertexOffset;
                }
                ShapeKeyId_VertexId_VertexOffset_Map[ShapeKeyIdIn] = VertexId_VertexOffset_Map;
            }

            this.ShapeKeyId_VertexId_VertexOffset_Map = ShapeKeyId_VertexId_VertexOffset_Map;
            LOG.NewLine();

        }

        public List<int> GetShapeKeyIdList(IndexBufferBufFile DivideIBBufFile)
        {
            List<int> ShapeKeyIdList = [];
            //TODO 这里的算法有问题，当IB文件中顶点数过多的时候，运算时间指数倍增长。 30秒才能处理20MB的顶点数


            foreach (var item in this.ShapeKeyId_VertexIdList_Map) {
                int ShapeKeyId = item.Key;
                List<int> ShapeKeyVertexIdList = item.Value;
                //判断当前VertexIdList中，是否含有当前IB文件的值

                bool find = false;
                foreach (int ShapeKeyVertexId in ShapeKeyVertexIdList)
                {
                    //优化1：如果ShapeKeyVertexId小于IB文件最小值或者大于IB文件最大值，则默认找不到进行跳过。
                    if (ShapeKeyVertexId < DivideIBBufFile.MinNumber || ShapeKeyVertexId > DivideIBBufFile.MaxNumber)
                    {
                        continue;
                    }

                    foreach (int number in DivideIBBufFile.NumberList)
                    {
                        if (number == ShapeKeyVertexId)
                        {
                            find = true;
                            break;
                        }
                    }

                    if (find)
                    {
                        break;
                    }
                }

                if (find)
                {
                    LOG.Info("ShapeKeyId: " + ShapeKeyId.ToString() + " VertexIdListSize: " + ShapeKeyVertexIdList.ToString());
                    ShapeKeyIdList.Add(ShapeKeyId);
                }
            }
            LOG.NewLine();

            return ShapeKeyIdList;
        }

        public byte[] AppendShapeKeyToFinalVB0Buf(byte[] FinalVB0Buf, List<int> ShapeKeyIdList,int TotalStride, int Offset)
        {
            //这个方法花费了两秒的执行时间，无法忍受
            LOG.Info("追加ShapeKey数据到vb0 AppendShapeKeyToFinalVB0Buf::Start");
            //这一步遍历VB0的每一行，因为行数就是VertexID，但是要注意这个行数是要加上当前IB的MinNumber，随后追加ShapeKey数据。

            byte[] DefaultOffsetData = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            LOG.Info("初始化VertexIndexNumber_VB0Data_Map::Start");
            SortedDictionary<int, byte[]> VertexIndexNumber_VB0Data_Map = new SortedDictionary<int, byte[]>();
            for (int i = 0; i < FinalVB0Buf.Length; i = i + TotalStride)
            {
                VertexIndexNumber_VB0Data_Map[(int)(i / TotalStride)] = DBMTBinaryUtils.GetRange_Byte(FinalVB0Buf, i, i + TotalStride);
            }
            LOG.Info("初始化VertexIndexNumber_VB0Data_Map::End");


            LOG.Info("初始化Append_VertexIndexNumber_VB0Data_Map::Start");
            SortedDictionary<int, byte[]> Append_VertexIndexNumber_VB0Data_Map = new SortedDictionary<int, byte[]>();

            foreach (var item in VertexIndexNumber_VB0Data_Map) {
                byte[] LineVB0Data = item.Value;
                int GlobalVertexID = item.Key + Offset;

                //根据GlobalVertexID获取ShapeKey
                foreach (int ShapeKeyId in ShapeKeyIdList)
                {
                    byte[] VertexOffsetData = DefaultOffsetData;
                    if (this.ShapeKeyId_VertexId_VertexOffset_Map[ShapeKeyId].ContainsKey(GlobalVertexID))
                    {
                        VertexOffsetData = this.ShapeKeyId_VertexId_VertexOffset_Map[ShapeKeyId][GlobalVertexID];
                    }

                    LineVB0Data = DBMTBinaryUtils.AppendByteArray(LineVB0Data, VertexOffsetData);
                }

                Append_VertexIndexNumber_VB0Data_Map[item.Key] = LineVB0Data;
            }
            LOG.Info("初始化Append_VertexIndexNumber_VB0Data_Map::End");

            //Append_VertexID_VB0Data_Map 转换为FinalVB0，再重新创建VertexBufferBufFile。
            LOG.Info("转换为AppendFinalVB0Data::Start");
            //byte[] AppendFinalVB0Data = new byte[] { };

            List<byte> AppendFinalVB0DataList = [];
            foreach (var item in Append_VertexIndexNumber_VB0Data_Map) {

                AppendFinalVB0DataList.AddRange(item.Value);
                //AppendFinalVB0Data = DBMTBinaryUtils.AppendByteArray(AppendFinalVB0Data, item.Value);
            }
            byte[] AppendFinalVB0Data = AppendFinalVB0DataList.ToArray();
            LOG.Info("转换为AppendFinalVB0Data::End");


            LOG.NewLine("追加ShapeKey数据到vb0 AppendShapeKeyToFinalVB0Buf::End");
            return AppendFinalVB0Data;
        }

        public List<D3D11Element> GetD3D11ElementListWithShapeKey(D3D11GameType d3D11GameType,List<int> ShapeKeyIdList)
        {
            List<D3D11Element> d3D11ElementList = [];

            LOG.Info("GetD3d11ElementListWithShapeKey::");
            foreach (string elementname in d3D11GameType.OrderedFullElementList)
            {
                D3D11Element d3d11element = d3D11GameType.ElementNameD3D11ElementDict[elementname];
                d3D11ElementList.Add(d3d11element);
            }

            if (ShapeKeyIdList.Count != 0)
            {
                foreach (int ShapeKeyId in ShapeKeyIdList)
                {
                    //有几个有效ShapeKey就添加几个ShapeKey的D3D11Element
                    D3D11Element d3d11Element = new D3D11Element();
                    d3d11Element.SemanticName = "SHAPEKEY";
                    d3d11Element.SemanticIndex = ShapeKeyId;
                    d3d11Element.Format = "R16G16B16_FLOAT";
                    d3d11Element.ByteWidth = 6;
                    d3D11ElementList.Add(d3d11Element);
                }
            }
            LOG.NewLine();
            return d3D11ElementList;
        }

    }
}
