using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBMT_Core
{
    public class ConstantBufferFile
    {



        public ConstantBufferFile(string ConstantBufferFilePath)
        {
            //byte[] CSCB0BufData = File.ReadAllBytes(ConstantBufferFilePath);

            List<int> CSCB0List = DBMTBinaryUtils.ReadAsR32_UINT(ConstantBufferFilePath);

        }

    }
}
