using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBMT_Core
{
    public class IASetVertexBuffers
    {
        public string CallIndex = "";
        public ulong StartSlot = 0;
        public ulong NumBuffers = 3;
        public string ppVertexBuffers = "";
        public string pStrides = "";
        public string pOffsets = "";

        public IASetVertexBuffers(string logLine)
        {
            var logCommand = new LogCommand(logLine);
            this.CallIndex = logCommand.CallIndex;

            if (logCommand.ArgumentKVMap.ContainsKey("StartSlot"))
            {
                this.StartSlot = ulong.Parse(logCommand.ArgumentKVMap["StartSlot"]);
            }
            if (logCommand.ArgumentKVMap.ContainsKey("NumBuffers"))
            {
                this.NumBuffers = ulong.Parse(logCommand.ArgumentKVMap["NumBuffers"]);
            }
            if (logCommand.ArgumentKVMap.ContainsKey("ppVertexBuffers"))
            {
                this.ppVertexBuffers = logCommand.ArgumentKVMap["ppVertexBuffers"];
            }
            if (logCommand.ArgumentKVMap.ContainsKey("pStrides"))
            {
                this.pStrides = logCommand.ArgumentKVMap["pStrides"];
            }
            if (logCommand.ArgumentKVMap.ContainsKey("pOffsets"))
            {
                this.pOffsets = logCommand.ArgumentKVMap["pOffsets"];
            }
        }
    }
}
