using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataTransfer.Request;
using DataTransfer.Response;
using DataTransfer.Common;
namespace IBusinessProcSvc
{
    public interface IDataSetIndexCredSvc
    {
        Task<List<ResponseDataSetIndexCred>> getMachineCreds(HttpGetObject httpobj);
    }
}
