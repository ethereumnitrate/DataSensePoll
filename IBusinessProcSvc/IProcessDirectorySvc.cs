using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataTransfer.Request;
using DataTransfer.Response;
using Unity;
namespace IBusinessProcSvc
{
    public interface IProcessDirectorySvc
    {
        void RunSearch(ResponseDataSetIndexCred creds, UnityContainer container);
    }
}
