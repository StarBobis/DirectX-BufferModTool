using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBMT_Core.WWMI
{
    public class Component
    {
        public int vertex_offset { get; set; } = 0;
        public int vertex_count { get; set; } = 0;
        public int index_offset { get; set; } = 0;
        public int index_count { get; set; } = 0;
        public int vg_offset { get; set; } = 0;
        public int vg_count { get; set; } = 0;

        public Dictionary<string, int> vg_map { get; set; } = new Dictionary<string, int>();

        public Component()
        {

        }
    }
}
