using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IBusinessProcSvc;
using DataTransfer.Common;
using DataTransfer.Response;
using DataTransfer.Request;
using Unity;
using System.Threading;
using System.IO;

namespace BusinessProcSvc
{
    public class PatternWriterSvc : IPatternWriterSvc
    {
        private readonly IGetPollingInfoSvc IPostDataSvc;
        private IList<PatternPost> patternDataList;
        private IZipSourceSvc IZipSourceSvc;
        private Mutex mutex = new Mutex(true);
        private Task<int> tasks;
        public PatternWriterSvc(IGetPollingInfoSvc _IPostDataSvc, IZipSourceSvc _IZipSourceSvc)
        {
            IPostDataSvc = _IPostDataSvc;
            patternDataList = new List<PatternPost>();
            IZipSourceSvc = _IZipSourceSvc;
        }

        async Task<int> IPatternWriterSvc.AddData(PatternPost patternData, int CredID)
        {
            int resultvalue = 0;
            try
            {
                

                List<PatternPost> patterns =  new List<PatternPost>();
                Dictionary<string, string> sourcedirs = IZipSourceSvc.GetZipDirectories();
                foreach(KeyValuePair<string, string> kv in sourcedirs)
                {
                    if (patternData.fileName.Contains(kv.Value))
                    {
                        patternData.fileName = kv.Key + "|" + Path.GetFileName(patternData.fileName);
                    }
                }

                patterns.Add(patternData);
                HttpGetObject httpGetObj = new HttpGetObject();
                var tokentask = IPostDataSvc.getToken();
                tokentask.Wait();
                httpGetObj.accessToken = tokentask.Result;
                int credIDcopy = CredID;
                int result = await IPostDataSvc.BulkPostInsert(httpGetObj, credIDcopy, patterns);
                resultvalue = result;
                patternDataList.Clear();
            }
            catch (Exception e)
            {

            }
            finally
            {
                //mutex.ReleaseMutex();
            }
            return resultvalue;
        }

        Task<int> IPatternWriterSvc.PostPatternData(IList<PatternPost> patternsFound)
        {
            throw new NotImplementedException();
        }

        async Task<int> IPatternWriterSvc.ReleaseLeftOverData(int CredID)
        {
            int resultvalue = 0;
            if (patternDataList.Count > 0)
            {
                HttpGetObject httpGetObj = new HttpGetObject();
                var tokentask = IPostDataSvc.getToken();
                tokentask.Wait();
                httpGetObj.accessToken = tokentask.Result;
                int result = await IPostDataSvc.BulkPostInsert(httpGetObj, CredID, patternDataList);
                resultvalue = result;
                patternDataList.Clear();

            }
            return resultvalue;
        }
    }
}
