using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBusinessProcSvc
{
    public interface IRunInfoSvc
    {
        int GenerateRunID(int dataSetIndexId);

        int GetCurrentRunID();

        int LogStatus(string message);

        int LogEnd(bool isError);
    }
}
