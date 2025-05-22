using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataTransfer.Request;
using DataTransfer.Response;
using Microsoft.Practices.Unity;
using Unity;

namespace IBusinessProcSvc
{
    public interface IFileParserSvc
    {
        void ReadAndParse(string strDrive, ResponseDataSetIndexCred creds, UnityContainer container);
    }
}
