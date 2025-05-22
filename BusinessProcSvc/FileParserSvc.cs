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
using Newtonsoft.Json;
using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Options;
using System.Security;
using System.Management;
using System.Management.Instrumentation;
using System.IO;
using System.Threading;
using Microsoft.Practices.Unity;
using Unity;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace BusinessProcSvc
{
    public class FileParserSvc : IFileParserSvc
    {
        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
        [ResourceExposure(ResourceScope.None)]
        [return: MarshalAsAttribute(UnmanagedType.Bool)]
        internal static extern bool PathIsUNC([MarshalAsAttribute(UnmanagedType.LPWStr), In] string pszPath);

        private readonly IFileHashWriterSvc IFileHashWriterSvc;
        private readonly IGetPollingInfoSvc IPollAPISvc;
        private readonly IFileDetectorSvc IFileDetectorSvc;
        private readonly IPatternWriterSvc IPatternWriterSvc;
        private readonly IRunInfoSvc IRunSvc;
        private readonly IProcessCompressedSvc IFileCompSvc;
        private readonly IZipSourceSvc IZipFileSrcSvc;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private List<ResponseDataSetIndexReg> regexes;
        private List<ResponseExcludeDirectory> exclusions;
        private List<ResponseQuarantine> quarantine;
        private Mutex mutex = new Mutex();
        public FileParserSvc(IFileHashWriterSvc _IFileHashWriterSvc,
                                    IGetPollingInfoSvc _IPollAPISvc,
                                    IFileDetectorSvc _IFileDetectorSvc,
                                    IPatternWriterSvc _IPatternWriterSvc,
                                    IRunInfoSvc _IRunSvc,
                                    IProcessCompressedSvc _IFileCompSvc,
                                    IZipSourceSvc _IZipFileSrcSvc
                                    )
        {
            IFileHashWriterSvc = _IFileHashWriterSvc;
            IPollAPISvc = _IPollAPISvc;
            IFileDetectorSvc = _IFileDetectorSvc;
            IPatternWriterSvc = _IPatternWriterSvc;
            IRunSvc = _IRunSvc;
            IFileCompSvc = _IFileCompSvc;
            IZipFileSrcSvc = _IZipFileSrcSvc;
        }
       
        void IFileParserSvc.ReadAndParse(string strDrive, ResponseDataSetIndexCred creds, UnityContainer container)
        {
            bool directoryexists = Directory.Exists(strDrive);
            if (PathIsUNC(strDrive) == true && directoryexists == false)
            {
                List<string> shareDrives = new List<string>();
                using (ManagementClass shares = new ManagementClass(strDrive + @"\root\cimv2", "Win32_Share", new ObjectGetOptions()))
                {
                    foreach (ManagementObject share in shares.GetInstances())
                    {
                        string shareName = "";
                        shareName = strDrive + @"\" + share["Name"];
                        //ReadAndParseDirectory(strDrive, creds, container);
                        Console.WriteLine("Share Drive: " + shareName);
                        shareDrives.Add(shareName);
                    }
                }
                foreach(string strd in shareDrives)
                {
                    ReadAndParseDirectory(strd, creds, container);
                }
            }
            else
            {
                ReadAndParseDirectory(strDrive, creds, container);
            }

            HttpGetObject httpobj = new HttpGetObject();
            var tokentask = IPollAPISvc.getToken();
            tokentask.Wait();
            httpobj.accessToken = tokentask.Result;
            IPollAPISvc.NotifyComplete(httpobj, creds.id);
        }
        void ProcessFile(FileSystemInfo fileof, ResponseDataSetIndexCred creds, UnityContainer container)
        {
            bool needProcess = true;
            
            try
            {
                
                FileAttributes attr = File.GetAttributes(fileof.FullName);
                FileInfo fiinfo = new FileInfo(fileof.FullName);
                foreach(var direx in exclusions)
                {

                    if (direx.directoryExclude == fiinfo.DirectoryName)
                    {
                        Console.WriteLine("Excluding File: " + fileof.FullName);
                        log.Info("File Excluded: " + fileof.FullName + " Appears in " + direx.directoryExclude);
                        return;
                    }
                }

               
                if (! ((attr & FileAttributes.Directory) == FileAttributes.Directory) && fileof.Extension != ".exe" && fileof.Extension != ".dll")
                {                    
                    
                    Console.WriteLine(fileof.FullName);
                    log.Info("Processing File: " + fileof.FullName);

                    var quarList = (from qf in quarantine
                                    where qf.fileName.Equals(fileof.FullName)
                                    select qf);

                    if (quarList != null)
                    {
                        if (quarList.Count() > 0)
                        {
                            log.Info("File is in the ignore list..will not process");
                            needProcess = false;
                        }
                            
                    }
                    
                    if (needProcess == true)
                        needProcess = IFileHashWriterSvc.LogFile(fileof, creds);

                    if (needProcess == true)
                    {
                        log.Info("File Needs Processing..Still need to check if in ignore list..") ;
                        bool pdf = false, word = false, excel = false, ppt = false;
                        bool processzip = true;
                        if (IFileCompSvc.isCompresed(fileof.FullName))
                        {
                            var quarListComp = (from qf in quarantine                                            
                                            select qf).ToList();

                            if (quarListComp != null)
                            {
                                foreach(ResponseQuarantine quar in quarListComp)
                                {
                                    if (quar.fileName.Contains(fileof.FullName))
                                    {
                                        log.Info("File is in the ignore list..will not process");
                                        processzip = false;
                                    }
                                        
                                }
                            }

                            if (processzip == true)
                            {
                                string compzipName = IFileCompSvc.ExtractFiles(fileof.FullName);
                                IZipFileSrcSvc.Add(fileof.FullName, compzipName);
                                ReadAndParseDirectory(compzipName, creds, container);
                                Directory.Delete(compzipName, true);

                            }

                        }
                        else
                        {
                            string filetype = IFileDetectorSvc.FileType(fileof.FullName);
                            Console.WriteLine("File Type: " + filetype);
                            if (filetype != "Text")
                            {
                                if (fileof.Extension == ".pdf")
                                    pdf = SelectandProcess("PDF", container, fileof);
                                else if (fileof.Extension == ".xls" || fileof.Extension == ".xlsx" || fileof.Extension == ".xlsb" ||
                                            fileof.Extension == ".xltx" || fileof.Extension == ".xltm" || fileof.Extension == ".xlsm" ||
                                            fileof.Extension == ".ods")
                                {
                                    excel = SelectandProcess("EXCEL", container, fileof);
                                }
                                else if (fileof.Extension == ".ppt" || fileof.Extension == ".pptx" || fileof.Extension == ".pps" ||
                                            fileof.Extension == ".pot" || fileof.Extension == ".ppsx" || fileof.Extension == ".pptm" ||
                                            fileof.Extension == ".ppsm" || fileof.Extension == ".potx" || fileof.Extension == ".potm")
                                {
                                    ppt = SelectandProcess("PPT", container, fileof);

                                }

                                if (filetype != "Unknown")
                                {
                                    word = SelectandProcess("WORD", container, fileof);
                                    if (word == false)
                                    {
                                        log.Error("Could not process this file, tried using the WORD processor but failed.");
                                    }
                                }
                                else
                                {
                                    if (pdf == false && ppt == false && word == false && excel == false)
                                        log.Info("File Type Unsupported - could not find process for file: " + fileof.FullName);
                                }


                            }
                            else
                            {
                                if (filetype == "Text")
                                {
                                    SelectandProcess("TEXT", container, fileof);
                                }
                            }

                        }



                    }
                    else
                    {
                        log.Info("File Does not need processing...");
                    }
                }
            }
            catch(Exception e)
            {
                Console.WriteLine("An error happened while trying to process file.");
                Console.WriteLine("Exception: " + e.Message);
                IRunSvc.LogStatus("Problem happened trying to process: " + fileof.FullName);
                log.Error("Error occurred Exception: " + e.Message);
                log.Error("Error occurred Exception: " + e.InnerException);
            }
            finally
            {
                
            }
           
        }
        private void  ReadAndParseDirectory(string strDrive, ResponseDataSetIndexCred creds, UnityContainer container)
        {
            try
            {
                long filecount = 0;

              

                var root = new DirectoryInfo(strDrive);
                var searchPattern = @"*";
                var searchOption = SearchOption.AllDirectories;
                var enumerable = new FileSystemEnumerable(root, searchPattern, searchOption);
                IRunSvc.LogStatus("Loading Search Criteria for: " + strDrive);
                LoadRegExp(creds.id);
                LoadExclusions(creds.id);
                LoadQuarantine(creds.id);
                IRunSvc.LogStatus("Done Loading Criteria for: " + strDrive);
                IRunSvc.LogStatus("Starting to fetch files... ");
                using (IEnumerator<FileSystemInfo> empEnumerator = enumerable.GetEnumerator())
                {
                    while (empEnumerator.MoveNext())
                    {
                        FileSystemInfo fileinfo = empEnumerator.Current;
                        
                        ProcessFile(fileinfo, creds, container);
                        filecount++;
                        if (filecount == 5000)
                        {
                            IRunSvc.LogStatus(String.Format("Processed {0} files", filecount));
                            filecount = 0;
                        }
                    }
                }
                if (filecount > 0)
                {
                    IRunSvc.LogStatus(String.Format("Processed Remaining {0} files", filecount));
                }
                IPatternWriterSvc.ReleaseLeftOverData(creds.id);
            }
            catch (UnauthorizedAccessException acc)
            {
                IRunSvc.LogStatus("Exception ocurred while running Read and Parse");
                IRunSvc.LogStatus("Unauthorized when trying to access directory");
                IRunSvc.LogEnd(true);
            }
            catch (Exception ex)
            {
                IRunSvc.LogStatus("Critical error happened while iterating through directories");
                IRunSvc.LogEnd(true);
            }
        }
        private bool SelectandProcess(string filetype, UnityContainer container, FileSystemInfo fileof)
        {
            bool processable = false;
            var IProc = (IProcessSvc)container.Resolve(typeof(IProcessSvc), filetype);
            string fileName = fileof.FullName;
            List<ResponseDataSetIndexReg> regExListCopy = regexes;
            processable = IProc.Process(regExListCopy, fileName);
            return processable;
        }
        public void LoadRegExp(int credID)
        {
            if (regexes == null || regexes.Count ==0)
            {
              
                regexes = new List<ResponseDataSetIndexReg>() ;
                var tokentask = IPollAPISvc.getToken();
                tokentask.Wait();
                string token = tokentask.Result;
                HttpGetObject httpgetobject = new HttpGetObject();
                httpgetobject.accessToken = token;
                int credIdCopy = credID;
                regexes = IPollAPISvc.getRegExps(httpgetobject, credIdCopy).Result;
               
            }
        }
        private void LoadExclusions(int credID)
        {
            if (exclusions == null || exclusions.Count == 0)
            {
                exclusions = new List<ResponseExcludeDirectory>();
                var tokentask = IPollAPISvc.getToken();
                tokentask.Wait();
                string token = tokentask.Result;
                HttpGetObject httpgetobject = new HttpGetObject();
                httpgetobject.accessToken = token;
                int credIdCopy = credID;
                exclusions = IPollAPISvc.getExcludeDirectories(httpgetobject, credIdCopy).Result;
                foreach(var excldir in exclusions)
                {
                    if (!excldir.directoryExclude.StartsWith(@"\\") && !excldir.directoryExclude.Contains(@":\"))
                    {
                        excldir.directoryExclude = @"\\" + excldir.directoryExclude;
                    }
                }
            }
        }
        private void LoadQuarantine(int credID)
        {
            if (quarantine == null || quarantine.Count == 0)
            {
                quarantine = new List<ResponseQuarantine>();
                var tokentask = IPollAPISvc.getToken();
                tokentask.Wait();
                string token = tokentask.Result;
                HttpGetObject httpgetobject = new HttpGetObject();
                httpgetobject.accessToken = token;
                int credIdCopy = credID;
                quarantine = IPollAPISvc.getQuarantineFiles(httpgetobject, credIdCopy).Result;
            }
        }
    }
}
