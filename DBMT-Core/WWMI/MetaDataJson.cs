using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBMT_Core.WWMI
{
    public class MetaDataJson
    {
        public string vb0_hash { get; set; } = "";
        public string cb4_hash { get; set; } = "";
        public int vertex_count { get; set; } = 0;
        public int index_count { get; set; } = 0;

        public List<Component> ComponentList { get; set; } = [];
        public ShapeKeys shapekeys { get; set; } = new ShapeKeys();

        public MetaDataJson()
        {

        }

        public void SaveToFile(string SaveFolderPath)
        {
            JObject metadataJsonJObject = DBMTJsonUtils.CreateJObject();
            metadataJsonJObject["vb0_hash"] = this.vb0_hash;
            metadataJsonJObject["cb4_hash"] = this.cb4_hash;
            metadataJsonJObject["vertex_count"] = this.vertex_count;
            metadataJsonJObject["index_count"] = this.index_count;

            JObject shapekeysJObject = DBMTJsonUtils.CreateJObject();
            shapekeysJObject["offsets_hash"] = this.shapekeys.offsets_hash;
            shapekeysJObject["scale_hash"] = this.shapekeys.scale_hash;
            shapekeysJObject["vertex_count"] = this.shapekeys.vertex_count;
            shapekeysJObject["dispatch_y"] = this.shapekeys.dispatch_y;
            shapekeysJObject["checksum"] = this.shapekeys.checksum;
            metadataJsonJObject["shapekeys"] = shapekeysJObject;

            //每个Component

            List<JObject> ComponentJObjectList = [];
            foreach (Component component in this.ComponentList)
            {
                JObject component_json = DBMTJsonUtils.CreateJObject();
                component_json["vertex_offset"] = component.vertex_offset;
                component_json["vertex_count"] = component.vertex_count;
                component_json["index_offset"] = component.index_offset;
                component_json["index_count"] = component.index_count;
                component_json["vg_offset"] = component.vg_offset;
                component_json["vg_count"] = component.vg_count;
                component_json["vg_map"] = JToken.FromObject(component.vg_map);

                ComponentJObjectList.Add(component_json);
            }

            metadataJsonJObject["components"] = JToken.FromObject(ComponentJObjectList);

            string SavePath = Path.Combine(SaveFolderPath,"Metadata.json");

            DBMTJsonUtils.SaveJObjectToFile(metadataJsonJObject,SavePath);
        }

    }
}
