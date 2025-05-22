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
namespace BusinessProcSvc
{
    public class PatternRunSvc : IPatternRunSvc
    {
        private readonly IGetPollingInfoSvc IPollInfoSvc;
        private readonly IDataSetIndexCredSvc IDataSetIdxCredSvc;
        //private readonly IProcessDirectorySvc IProcessDir;
        private readonly IRunInfoSvc IRunSvc;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public PatternRunSvc(IGetPollingInfoSvc _IPollInfoSvc, 
                            IDataSetIndexCredSvc _IDataSetIdxCredSvc, 
                            IRunInfoSvc _IRunSvc)
        {
            IPollInfoSvc = _IPollInfoSvc;
            IDataSetIdxCredSvc = _IDataSetIdxCredSvc;
            //IProcessDir = _IProcessDir;
            IRunSvc = _IRunSvc;

        }
        private List<ResponseDataSetIndexReg> LoadRegExp(int credID)
        {

            List<ResponseDataSetIndexReg> regexes = new List<ResponseDataSetIndexReg>();
            string token = IPollInfoSvc.getToken().Result;
            HttpGetObject httpgetobject = new HttpGetObject();
            httpgetobject.accessToken = token;
            regexes = IPollInfoSvc.getRegExps(httpgetobject, credID).Result;

            return regexes;
        }
        void IPatternRunSvc.ScanFiles(UnityContainer container)
        {
            HttpGetObject httpGetObj = new HttpGetObject();
            //httpGetObj.accessToken = IPollInfoSvc.getToken().Result;
            var tokentask = IPollInfoSvc.getToken();
            tokentask.Wait();
            httpGetObj.accessToken = tokentask.Result;
            List<ResponseDataSet> respdataset = IPollInfoSvc.getDataSets(httpGetObj).Result;

            foreach (ResponseDataSet rds in respdataset)
            {
                if (rds.active == true)
                {
                    List<ResponseDataSetIndex> dsindexes = IPollInfoSvc.getIndexes(httpGetObj, rds.id).Result;
                    foreach (ResponseDataSetIndex dsidx in dsindexes)
                    {
                        bool allowedtorun =  IPollInfoSvc.CheckAllowedToRun(httpGetObj, dsidx.id).Result;
                        if (dsidx.active == true && allowedtorun == true )
                        {
                            IRunSvc.GenerateRunID(dsidx.id);

                            httpGetObj.id = Convert.ToString(dsidx.id);
                            List<Task> tasks = new List<Task>();
                            List<ResponseDataSetIndexCred> dscreds = IDataSetIdxCredSvc.getMachineCreds(httpGetObj).Result;

                            foreach (ResponseDataSetIndexCred rsidxcred in dscreds)
                            {
                                if (rsidxcred.active == true)
                                {
                                    if (rsidxcred.isDataBase == true)
                                    {
                                        var IProc = (IProcessDBSvc)container.Resolve(typeof(IProcessDBSvc), "MSSQL");
                                        ResponseDataSetIndexCred rscredcopy = new ResponseDataSetIndexCred();
                                        rscredcopy.id = rsidxcred.id;
                                        rscredcopy.computerName = rsidxcred.computerName;
                                        rscredcopy.domainName = rsidxcred.domainName;
                                        rscredcopy.active = rsidxcred.active;
                                        rscredcopy.isDataBase = rsidxcred.isDataBase;
                                        rscredcopy.isWmi = rsidxcred.isWmi;
                                        rscredcopy.userName = rsidxcred.userName;
                                        rscredcopy.passWord = rsidxcred.passWord;
                                        Task t = Task.Run(() => IProc.Process( rscredcopy));
                                        tasks.Add(t);
                                        //Task.WaitAll(tasks.ToArray());
                                    }
                                    else
                                    {
                                        var IProcFile = (IProcessDirectorySvc)container.Resolve(typeof(IProcessDirectorySvc), "PROCFILE");
                                        
                                        ResponseDataSetIndexCred rscredcopy = new ResponseDataSetIndexCred();
                                        rscredcopy.id = rsidxcred.id;
                                        rscredcopy.computerName = rsidxcred.computerName;
                                        rscredcopy.domainName = rsidxcred.domainName;
                                        rscredcopy.active = rsidxcred.active;
                                        rscredcopy.isDataBase = rsidxcred.isDataBase;
                                        rscredcopy.isWmi = rsidxcred.isWmi;
                                        rscredcopy.userName = rsidxcred.userName;
                                        rscredcopy.passWord = rsidxcred.passWord;
                                        Task t = Task.Run(() => IProcFile.RunSearch(rscredcopy, container));

                                        tasks.Add(t);
                                        //Task.WaitAll(tasks.ToArray());
                                    }

                                }
                            }
                            try
                            {
                                Task.WaitAll(tasks.ToArray());

                            }
                            catch (AggregateException ae)
                            {
                                foreach (var innerException in ae.Flatten().InnerExceptions)
                                {
                                    log.Error("Exception has happened while waiting for task to complete.");
                                    log.Error("Exception Detail: " + ae.Message);
                                }
                            }
                            IRunSvc.LogEnd(false);

                        }
                    }
                }
                
            }
        }
    }
}
