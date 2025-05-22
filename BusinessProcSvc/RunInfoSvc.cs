using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IBusinessProcSvc;
using DataTransfer.Common;
using DataTransfer.Request;

namespace BusinessProcSvc
{
    public class RunInfoSvc : IRunInfoSvc
    {
        private int runId;
        private readonly IGetPollingInfoSvc IPollInfoSvc;
        public RunInfoSvc(IGetPollingInfoSvc _IPollInfoSvc)
        {
            IPollInfoSvc = _IPollInfoSvc;
        }
        int IRunInfoSvc.GenerateRunID(int dataSetIndexId)
        {
            HttpGetObject httpGetObj = new HttpGetObject();
            httpGetObj.accessToken = IPollInfoSvc.getToken().Result;
            runId = IPollInfoSvc.StartProcess(httpGetObj, dataSetIndexId).Result;
            return runId;
        }

        int IRunInfoSvc.GetCurrentRunID()
        {
            return runId;
        }

        int IRunInfoSvc.LogEnd(bool isError)
        {
            HttpGetObject httpGetObj = new HttpGetObject();
            httpGetObj.accessToken = IPollInfoSvc.getToken().Result;
            int endresult = IPollInfoSvc.EndProcess(httpGetObj, runId, isError).Result;
            return endresult;
        }

        int IRunInfoSvc.LogStatus(string message)
        {
            HttpGetObject httpGetObj = new HttpGetObject();
            httpGetObj.accessToken = IPollInfoSvc.getToken().Result;
            DataSetLogPost dslogpost = new DataSetLogPost();
            dslogpost.logData = message;
            int logresult = IPollInfoSvc.LogStatus(httpGetObj, dslogpost, runId).Result;
            return logresult;
        }
    }
}
