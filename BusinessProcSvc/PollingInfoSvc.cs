using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IBusinessProcSvc;
using DataTransfer.Request;
using DataTransfer.Response;
using DataTransfer.Common;
using System.Net.Http;
using System.Threading;
using Newtonsoft.Json;

namespace BusinessProcSvc
{
    public class PollingInfoSvc : IBusinessProcSvc.IGetPollingInfoSvc
    {
        private Mutex mutexplease = new Mutex();
        private string RequestToken = "";
        //private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly string tokenendpoint = Configuration.APIPath() + "/login-token";
        private static readonly string dsendpoint = Configuration.APIPath() + "/dataset";
        private static readonly string dsindexendpoint = Configuration.APIPath() + "/dataset/index/{id}";
        private static readonly string regExendpoint = Configuration.APIPath() + "/expression/machine/credentials/{id}";
        private static readonly string regPreDefEndPoint = Configuration.APIPath() + "/expression/machine/credentials/{id}/predefined/all";

        private static readonly string patternendpoint = Configuration.APIPath() + "/patternrecord/machine/credentials/{id}";

        private static readonly string processStartendpoint = Configuration.APIPath() + "/run/start/datasetindex/{id}";
        private static readonly string processLogendpoint = Configuration.APIPath() + "/run/log/{id}";
        private static readonly string processEndendpoint = Configuration.APIPath() + "/run/end/{id}";
        private static readonly string processErrorendpoint = Configuration.APIPath() + "/run/error/{runid}";

        private static readonly string processCheckRunendpoint = Configuration.APIPath() + "/process/allowtorun";

        private static readonly string processSendCompleteendpoint = Configuration.APIPath() + "/machine/credentials/{id}/notifications/sendcomplete";

        private static readonly string processExcludeDirectoryendpoint = Configuration.APIPath() + "/exclude/directories/cred/{id}";
        private static readonly string quarantineEndPoint = Configuration.APIPath() + "/quarantine/machine/{id}/getrecords";
        async Task<string> IGetPollingInfoSvc.getToken()
        {
            //mutexplease.WaitOne();
            if (RequestToken != "")
                return RequestToken;
            string strToken = "";
            APIKey apikeyobj = new APIKey();
            HttpGetObject httpobj = new HttpGetObject();
            httpobj.endPoint = tokenendpoint;
            APIClient apiclient = new APIClient();
            HttpResponseMessage tokenresp = await apiclient.postAsync(httpobj, apiclient.convertToContent(apikeyobj));
            if (!tokenresp.IsSuccessStatusCode)
            {
                Console.WriteLine("Indexing Service is down...");
                Console.WriteLine("Exiting process...");
                return "";
            }
            string val = await tokenresp.Content.ReadAsStringAsync();
            ResponseToken resptoken = JsonConvert.DeserializeObject<ResponseToken>(val);
            strToken = resptoken.token;
            RequestToken = strToken;
            //mutexplease.ReleaseMutex();
            return strToken;
        }

        async  Task<List<ResponseDataSet>> IGetPollingInfoSvc.getDataSets(HttpGetObject httpobj)
        {
            List<ResponseDataSet> respds = new List<ResponseDataSet>();
            try
            {
                httpobj.endPoint = dsendpoint;
                APIClient apiclient = new APIClient();
                HttpResponseMessage respDataSets = await apiclient.getAsync(httpobj);
                if (!respDataSets.IsSuccessStatusCode)
                {
                    Console.WriteLine("Indexing Service is down...Unable to get datasets");
                    Console.WriteLine("Exiting process...");
                    return respds;
                }
                respds = JsonConvert.DeserializeObject<List<ResponseDataSet>>(await respDataSets.Content.ReadAsStringAsync());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + e.InnerException);
                //log.Error("Exception Ocurred:" + e.Message);
                //log.Error("Exception More Information: " + e.InnerException);
            }

            return respds;
        }

        async  Task<List<ResponseDataSetIndex>> IGetPollingInfoSvc.getIndexes(HttpGetObject httpobj, int DataSetID)
        {
            List<ResponseDataSetIndex> respindex = new List<ResponseDataSetIndex>();
            httpobj.endPoint = dsindexendpoint;
            httpobj.id = Convert.ToString(DataSetID);
            APIClient apiclient = new APIClient();
            HttpResponseMessage respDataSetIndexes = await apiclient.getAsync(httpobj);
            if (!respDataSetIndexes.IsSuccessStatusCode)
            {
                Console.WriteLine("Indexing Service is down...Unable to get Indexes");
                Console.WriteLine("Exiting process...");
                return respindex;
            }
            respindex = JsonConvert.DeserializeObject<List<ResponseDataSetIndex>>(await respDataSetIndexes.Content.ReadAsStringAsync());

            return respindex;
        }

        async Task<List<ResponseDataSetIndexReg>> IGetPollingInfoSvc.getRegExps(HttpGetObject httpobj, int DataSetIndexCredID)
        {
            List<ResponseDataSetIndexReg> respdsidxregexp = new List<ResponseDataSetIndexReg>();
            //mutexplease.WaitOne();
            try
            {
                httpobj.endPoint = regExendpoint;
                httpobj.id = Convert.ToString(DataSetIndexCredID);
                APIClient apiclient = new APIClient();
                HttpResponseMessage respRegExps = await apiclient.getAsync(httpobj);
                if (!respRegExps.IsSuccessStatusCode)
                {
                    Console.WriteLine("Web Service is down - Trying to get RegExpressions");
                    Console.WriteLine("Exiting process...");
                    return respdsidxregexp;
                }
                respdsidxregexp = JsonConvert.DeserializeObject<List<ResponseDataSetIndexReg>>(await respRegExps.Content.ReadAsStringAsync());
                httpobj.endPoint = regPreDefEndPoint;
                httpobj.id = Convert.ToString(DataSetIndexCredID);                
                respRegExps = await apiclient.getAsync(httpobj);
                if (!respRegExps.IsSuccessStatusCode)
                {
                    Console.WriteLine("Web Service is down - Trying to get RegExpressions");
                    Console.WriteLine("Exiting process...");
                    return respdsidxregexp;
                }
                List<ResponseDataSetIndexReg> regExPreDefined = JsonConvert.DeserializeObject<List<ResponseDataSetIndexReg>>(await respRegExps.Content.ReadAsStringAsync());
                foreach(ResponseDataSetIndexReg regex in regExPreDefined)
                {
                    if (regex.active == true)
                    {
                        regex.dataSetIndexCredId = DataSetIndexCredID;
                        var reg = respdsidxregexp.Where(x => x.id == regex.id).FirstOrDefault();
                        if (reg == null)
                            respdsidxregexp.Add(regex);
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + e.InnerException);
                //log.Error("Exception Ocurred:" + e.Message);
                //log.Error("Exception More Information: " + e.InnerException);
            }
            finally
            {
                //mutexplease.ReleaseMutex();
            }
            return respdsidxregexp;
        }

        async Task<int> IGetPollingInfoSvc.BulkPostInsert(HttpGetObject httpobj, int credID, IList<PatternPost> patternlist)
        {

            httpobj.endPoint = patternendpoint;
            httpobj.id = Convert.ToString(credID);

            APIClient apiclient = new APIClient();
            var json = JsonConvert.SerializeObject((List<PatternPost>)patternlist);
            //var content = new StringContent(patternlist);
            System.Net.Http.StringContent stringContent = new StringContent(json, UnicodeEncoding.UTF8,"application/json");
            HttpResponseMessage dsResp = await apiclient.postAsync(httpobj, stringContent);
            if (!dsResp.IsSuccessStatusCode)
            {
                return -1;
            }
            ResponseUpdatedItems respUpdate = JsonConvert.DeserializeObject<ResponseUpdatedItems>(await dsResp.Content.ReadAsStringAsync());
            return respUpdate.noOfItemsUpdated;
        }

        async Task<int> IGetPollingInfoSvc.StartProcess(HttpGetObject httpobj, int dataSetIndexId)
        {
            httpobj.endPoint = processStartendpoint;
            httpobj.id = Convert.ToString(dataSetIndexId);

            APIClient apiclient = new APIClient();            
            HttpResponseMessage dsResp = await apiclient.postAsync(httpobj, null);
            if (!dsResp.IsSuccessStatusCode)
            {
                return -1;
            }
            ResponseGenericId runId = JsonConvert.DeserializeObject<ResponseGenericId>(await dsResp.Content.ReadAsStringAsync());
            return runId.id;
        }

        async Task<int> IGetPollingInfoSvc.LogStatus(HttpGetObject httpobj, DataSetLogPost logData, int runId)
        {
            httpobj.endPoint = processLogendpoint;
            httpobj.id = Convert.ToString(runId);

            APIClient apiclient = new APIClient();
            HttpResponseMessage dsResp = await apiclient.postAsync(httpobj, apiclient.convertToContent( logData));
            if (!dsResp.IsSuccessStatusCode)
            {
                return -1;
            }
           
            return 1;
        }

        async Task<int> IGetPollingInfoSvc.EndProcess(HttpGetObject httpobj, int runId, bool isError)
        {
            if (isError == false)
                httpobj.endPoint = processEndendpoint;
            else
                httpobj.endPoint = processErrorendpoint;

            httpobj.id = Convert.ToString(runId);

            APIClient apiclient = new APIClient();
            HttpResponseMessage dsResp = await apiclient.postAsync(httpobj, null);
            if (!dsResp.IsSuccessStatusCode)
            {
                return -1;
            }

            return 1;
        }

        async Task<int> IGetPollingInfoSvc.NotifyComplete(HttpGetObject httpobj, int credId)
        {
            httpobj.endPoint = processSendCompleteendpoint;
            httpobj.id = Convert.ToString(credId);

            APIClient apiclient = new APIClient();
            HttpResponseMessage dsResp = await apiclient.postAsync(httpobj, null);
            if (!dsResp.IsSuccessStatusCode)
            {
                return -1;
            }

            return 1;
        }

        async Task<bool> IGetPollingInfoSvc.CheckAllowedToRun(HttpGetObject httpobj, int dataSetIndexID)
        {
            bool allowedToRun = true;
            httpobj.endPoint = processCheckRunendpoint;
            RequestDataSetIndex dsidx = new RequestDataSetIndex();
            dsidx.dataSetIndexId = dataSetIndexID;

            APIClient apiclient = new APIClient();
            HttpResponseMessage dsResp = await apiclient.postAsync(httpobj, apiclient.convertToContent(dsidx));
            if (dsResp.StatusCode == System.Net.HttpStatusCode.Created)
            {
                allowedToRun = true;
            }
            else
                allowedToRun = false;

            return allowedToRun;
        }

        async Task<List<ResponseExcludeDirectory>> IGetPollingInfoSvc.getExcludeDirectories(HttpGetObject httpobj, int credId)
        {
            List<ResponseExcludeDirectory> excludeDirsList = new List<ResponseExcludeDirectory>();
            httpobj.endPoint = processExcludeDirectoryendpoint;
            httpobj.id = Convert.ToString(credId);
            APIClient apiclient = new APIClient();
            HttpResponseMessage respExcludeDirs = await apiclient.getAsync(httpobj);
            if (!respExcludeDirs.IsSuccessStatusCode)
            {
                Console.WriteLine("Indexing Service is down...Unable to get Exclude Directories");
                Console.WriteLine("Will Ignore for now...");
                return excludeDirsList;
            }
            excludeDirsList = JsonConvert.DeserializeObject<List<ResponseExcludeDirectory>>(await respExcludeDirs.Content.ReadAsStringAsync());

            return excludeDirsList;
        }

        async Task<List<ResponseQuarantine>> IGetPollingInfoSvc.getQuarantineFiles(HttpGetObject httpobj, int credId)
        {
            bool errorOccurred = false;
            bool nextPage = true;
            int pageNo = 1;
            int noOfRecords = 1000;
            PageRequest pageInfo = new PageRequest();
            pageInfo.pageNo = pageNo;
            pageInfo.noOfRecords = noOfRecords;
            List<ResponseQuarantine> quarantineList = new List<ResponseQuarantine>();
            httpobj.endPoint = quarantineEndPoint;
            httpobj.id = Convert.ToString(credId);
            APIClient apiclient = new APIClient();
            
            while(nextPage == true)
            {
                HttpResponseMessage dsResp = await apiclient.postAsync(httpobj, apiclient.convertToContent(pageInfo));
                if (!dsResp.IsSuccessStatusCode)
                {
                    errorOccurred = true;
                }

                string val = await dsResp.Content.ReadAsStringAsync();
                IList<ResponseQuarantine> reqlistquarantine = JsonConvert.DeserializeObject<List<ResponseQuarantine>>(val);
                if (reqlistquarantine.Count > 0)
                {
                    foreach(ResponseQuarantine respquar in reqlistquarantine)
                    {
                        quarantineList.Add(respquar);
                    }
                }
                else
                    nextPage = false;

                pageInfo.pageNo++;
            }
            return quarantineList;
        }
    }
}
