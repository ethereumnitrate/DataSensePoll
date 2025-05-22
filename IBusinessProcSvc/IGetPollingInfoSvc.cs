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
    public interface IGetPollingInfoSvc
    {
        Task<string> getToken();

        Task<List<ResponseDataSet>> getDataSets(HttpGetObject httpobj);

        Task<List<ResponseDataSetIndex>> getIndexes(HttpGetObject httpobj, int DataSetID);

        Task<List<ResponseDataSetIndexReg>> getRegExps(HttpGetObject httpobj, int DataSetIndexCredID);

        Task<int> BulkPostInsert(HttpGetObject httpobj,int CredID, IList<PatternPost> patternlist);

        Task<int> StartProcess(HttpGetObject httpobj, int dataSetIndexId);

        Task<int> LogStatus(HttpGetObject httpobj, DataSetLogPost logData, int runId);

        Task<int> EndProcess(HttpGetObject httpobj, int runId, bool isError);

        Task<int> NotifyComplete(HttpGetObject httpobj, int credId);

        Task<bool> CheckAllowedToRun(HttpGetObject httpobj, int dataSetIndexID);

        Task<List<ResponseExcludeDirectory>> getExcludeDirectories(HttpGetObject httpobj, int credId);

        Task<List<ResponseQuarantine>> getQuarantineFiles(HttpGetObject httpobj, int credId);
    }
}
