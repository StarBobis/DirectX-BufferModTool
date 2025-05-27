using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBMT_Core
{
    public class DBMTBinaryUtils
    {
        public static List<int> GetRange_INT32(List<int> list, long startIndex, long endIndex)
        {
            // 检查起始索引是否超出范围或大于结束索引，返回空列表
            if (startIndex >= list.Count || startIndex > endIndex)
            {
                return new List<int>();
            }

            // 计算新列表的大小
            long size = endIndex - startIndex;
            if (size < 0)
            {
                throw new ArgumentException("The calculated size for the new list is negative.");
            }

            // 使用LINQ来获取指定范围内的元素
            var rangeList = list.GetRange((int)startIndex, (int)size).ToList();

            return rangeList;
        }

        public static Dictionary<int, byte[]> SplitBytesByStride(byte[] fileBytes,int stride)
        {
            var result = new Dictionary<int, byte[]>();
            int totalChunks = (int)Math.Ceiling(fileBytes.Length / (double)stride);

            for (int i = 0; i < totalChunks; i++)
            {
                int startIndex = i * stride;
                // 计算当前块的实际长度，注意最后一个块可能比chunkSize小
                int currentChunkSize = Math.Min(stride, fileBytes.Length - startIndex);
                byte[] chunk = new byte[currentChunkSize];

                // 使用Buffer.BlockCopy高效复制数组片段
                Buffer.BlockCopy(fileBytes, startIndex, chunk, 0, currentChunkSize);

                // 将分割好的块添加到结果字典中
                result.Add(i, chunk);
            }

            return result;
        }

        public static Dictionary<int, byte[]> ReadBinaryFileByStride(string filePath, int stride)
        {
            // 读取文件的全部二进制内容
            byte[] fileBytes = File.ReadAllBytes(filePath);

            var result = new Dictionary<int, byte[]>();
            int totalChunks = (int)Math.Ceiling(fileBytes.Length / (double)stride);

            for (int i = 0; i < totalChunks; i++)
            {
                int startIndex = i * stride;
                // 计算当前块的实际长度，注意最后一个块可能比chunkSize小
                int currentChunkSize = Math.Min(stride, fileBytes.Length - startIndex);
                byte[] chunk = new byte[currentChunkSize];

                // 使用Buffer.BlockCopy高效复制数组片段
                Buffer.BlockCopy(fileBytes, startIndex, chunk, 0, currentChunkSize);

                // 将分割好的块添加到结果字典中
                result.Add(i, chunk);
            }

            return result;
        }



        public static byte[] AppendByteArray(byte[] first, byte[] second)
        {
            byte[] result = new byte[first.Length + second.Length];
            // 复制第一个数组到结果数组
            Array.Copy(first, result, first.Length);
            // 复制第二个数组到结果数组
            Array.Copy(second, 0, result, first.Length, second.Length);
            return result;
        }


        public static byte[] GetRange_Byte(byte[] arr, long startIndex, long endIndex)
        {
            // 检查起始索引是否超出范围或大于结束索引，返回空数组
            if (startIndex >= arr.Length)
            {
                Console.Error.WriteLine("StartIndex Can't >= array.Length");
                return new byte[0];
            }

            if (startIndex > endIndex)
            {
                return new byte[0];
            }

            // 计算新数组的大小并检查是否有效（非负）
            long size = endIndex - startIndex;
            if (size < 0)
            {
                throw new ArgumentException("The calculated size for the new array is negative.");
            }

            // 创建一个新的数组来存储指定范围内的元素
            byte[] rangeArr = new byte[size];
            // 使用Array.Copy方法复制范围内的元素到新数组中
            Array.Copy(arr, startIndex, rangeArr, 0, size);

            return rangeArr;
        }

        public static Dictionary<int, byte[]> MergeByteDicts(List<Dictionary<int, byte[]>> byteDicts)
        {
            var mergedDict = new Dictionary<int, byte[]>();

            if (byteDicts == null || byteDicts.Count == 0) return mergedDict;

            // 假定所有字典都有相同的键集，使用第一个字典的键集进行遍历
            foreach (var key in byteDicts[0].Keys)
            {
                int totalLength = 0;

                // 计算合并后的数组长度
                foreach (var dict in byteDicts)
                {
                    if (!dict.ContainsKey(key))
                    {
                        throw new ArgumentException($"Key {key} not found in all dictionaries.");
                    }
                    totalLength += dict[key].Length;
                }

                byte[] combinedArray = new byte[totalLength];
                int offset = 0;

                // 合并所有字典中对应键的byte[]
                foreach (var dict in byteDicts)
                {
                    Buffer.BlockCopy(dict[key], 0, combinedArray, offset, dict[key].Length);
                    offset += dict[key].Length;
                }

                // 将合并后的数组添加到新的字典中
                mergedDict[key] = combinedArray;
            }

            return mergedDict;
        }


        public static byte[] MergeDictionaryValues(Dictionary<int, byte[]> MergedVB0Dict)
        {
            // 使用List<byte>来存储合并后的结果
            List<byte> mergedBytes = new List<byte>();

            // 遍历字典，并将每个byte[]的元素添加到mergedBytes列表中
            foreach (var pair in MergedVB0Dict)
            {
                mergedBytes.AddRange(pair.Value);
            }

            // 将List<byte>转换为byte[]并返回
            return mergedBytes.ToArray();
        }


        public static List<int> ReadAsR32_UINT(string filePath)
        {
            var values = new List<int>();
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            using (var reader = new BinaryReader(fs))
            {
                while (reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    // 读取4个字节并转换为无符号整数
                    uint value = reader.ReadUInt32();
                    // 如果需要将其存储为有符号整数
                    values.Add((int)value);
                }
            }
            return values;
        }

        public static List<int> ReadAsR16_UINT(string filePath)
        {
            var values = new List<int>();
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            using (var reader = new BinaryReader(fs))
            {
                while (reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    // 读取2个字节并转换为无符号短整数
                    ushort value = reader.ReadUInt16();
                    // 如果需要将其存储为有符号整数
                    values.Add(value);
                }
            }
            return values;
        }

        public static void WriteAsR32_UINT(List<int> data, string outputPath)
        {
            using (var fs = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
            using (var writer = new BinaryWriter(fs))
            {
                foreach (int value in data)
                {
                    // 确保值在UInt32范围内
                    //if (value < 0 || value > uint.MaxValue)
                    //{
                    //    throw new ArgumentOutOfRangeException(nameof(value), "Value is out of UInt32 range.");
                    //}
                    writer.Write((uint)value);
                }
            }
        }

        public static void WriteAsR16_UINT(List<int> data, string outputPath)
        {
            using (var fs = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
            using (var writer = new BinaryWriter(fs))
            {
                foreach (int value in data)
                {
                    // 确保值在UInt16范围内
                    //if (value < 0 || value > ushort.MaxValue)
                    //{
                    //    throw new ArgumentOutOfRangeException(nameof(value), "Value is out of UInt16 range.");
                    //}
                    writer.Write((ushort)value);
                }
            }
        }

    }
}
