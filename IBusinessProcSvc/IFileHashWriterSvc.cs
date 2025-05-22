using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using DataTransfer.Response;

namespace IBusinessProcSvc
{
    public interface IFileHashWriterSvc
    {
        bool LogFile(FileSystemInfo fileprocessed, ResponseDataSetIndexCred creds);
    }
}
