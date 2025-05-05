using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBMT_Core
{
    public class D3D11Element
    {
        [JsonProperty("SemanticName")]
        public string SemanticName { get; set; } = "";

        [JsonProperty("Format")]
        public string Format { get; set; } = "";

        [JsonProperty("ExtractSlot")]
        public string ExtractSlot { get; set; } = "";

        [JsonProperty("ExtractTechnique")]
        public string ExtractTechnique { get; set; } = "";

        [JsonProperty("Category")]
        public string Category { get; set; } = "";

        [JsonProperty("DrawCategory")]
        public string DrawCategory { get; set; } = "";

        //public bool GPUPreSkinning { get; set; } = false;

        /// <summary>
        /// 根据在列表中出现的次数和顺序来赋值。
        /// </summary>
        [JsonIgnore]
        public int SemanticIndex { get; set; } = 0;

        /// <summary>
        /// 根据Format来动态ByteWidth计算长度
        /// </summary>
        [JsonIgnore]
        public int ByteWidth { get; set; } = 0;

        [JsonIgnore]
        public string ElementName { get; set; } = "";

    }
}
