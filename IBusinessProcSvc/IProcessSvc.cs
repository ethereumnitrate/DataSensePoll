using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataTransfer.Request;
using DataTransfer.Response;

namespace IBusinessProcSvc
{
    public interface IProcessSvc
    {
        
        bool Process(IList<ResponseDataSetIndexReg> regExp, string fileName);
    }
}
