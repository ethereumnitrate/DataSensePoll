using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBusinessProcSvc
{
    public interface IProcessCompressedSvc
    {
        bool isCompresed(string filename);

        string ExtractFiles(string compressedfilename);
    }
}
