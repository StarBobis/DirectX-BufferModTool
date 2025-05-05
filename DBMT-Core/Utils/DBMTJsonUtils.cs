using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBMT_Core
{
    public class DBMTJsonUtils
    {

        /// <summary>
        /// 创建一个新的 JObject 实例。
        /// </summary>
        /// <returns>一个新的 JObject 实例。</returns>
        public static JObject CreateJObject()
        {
            // 创建一个新的 JObject 实例
            return new JObject();
        }

        /// <summary>
        /// 将提供的 JObject 保存到指定文件路径。
        /// </summary>
        /// <param name="jsonObject">要保存的 JObject 实例。</param>
        /// <param name="filePath">保存 JObject 的文件路径。</param>
        public static void SaveJObjectToFile<T>(T jsonObject, string filePath) where T : JToken
        {
            // 确保目录存在
            string directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // 将 JObject 序列化为格式化的 JSON 字符串，并写入文件
            File.WriteAllText(filePath, jsonObject.ToString(Formatting.Indented));
        }

        /// <summary>
        /// 从指定文件路径读取 JSON 内容并解析为 JObject。
        /// </summary>
        /// <param name="filePath">包含 JSON 内容的文件路径。</param>
        /// <returns>解析后的 JObject 实例。</returns>
        /// <exception cref="FileNotFoundException">当文件不存在时抛出。</exception>
        /// <exception cref="JsonReaderException">当文件内容不是有效的 JSON 时抛出。</exception>
        public static JObject ReadJObjectFromFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("JSON 文件未找到", filePath);
            }

            try
            {
                string jsonContent = File.ReadAllText(filePath);
                return JObject.Parse(jsonContent);
            }
            catch (JsonReaderException e)
            {
                throw new JsonReaderException($"文件 {filePath} 中的内容不是有效的 JSON 格式。详细信息: {e.Message}", e);
            }
        }



    }
}
