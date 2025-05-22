using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBusinessProcSvc
{
    public interface IFileDetectorSvc
    {
        string FileType(string fileName);

        Encoding FileEncoding(string fileName);
    }
}
