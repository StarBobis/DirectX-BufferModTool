using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBMT_Core.WWMI
{
    public class ShapeKeys
    {
        public string offsets_hash { get; set; } = "";
        public string scale_hash { get; set; } = "";
        public int vertex_count { get; set; } = 0;  //											29161
        public int dispatch_y { get; set; } = 0;    // vertex_count / 32 + 1 = dispatch_y		912
        public int checksum { get; set; } = 0;      //											1104

        public ShapeKeys()
        {

        }




    }

}
