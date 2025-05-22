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
using Unity;
using System.Threading;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace BusinessProcSvc
{
    public class ProcessDirectorySvc : IBusinessProcSvc.IProcessDirectorySvc
    {
        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
        [ResourceExposure(ResourceScope.None)]
        [return: MarshalAsAttribute(UnmanagedType.Bool)]
        internal static extern bool PathIsUNC([MarshalAsAttribute(UnmanagedType.LPWStr), In] string pszPath);

        private readonly IRunInfoSvc IRunSvc;
        private Object thisLock = new Object();
        private Mutex mut = new Mutex();
        public ProcessDirectorySvc( IRunInfoSvc _IRunSvc)
        {
            IRunSvc = _IRunSvc;
           

        }
        void IProcessDirectorySvc.RunSearch(ResponseDataSetIndexCred creds, UnityContainer container)
        {
            if (creds.isWmi == true)
            {
                IRunSvc.LogStatus("Running Search using WMI");
                SecureString securepassword = new SecureString();
                foreach (char c in creds.passWord)
                {
                    securepassword.AppendChar(c);
                }
                CimCredential Credentials = new CimCredential(PasswordAuthenticationMechanism.Default,
                                                              creds.domainName,
                                                              creds.userName,
                                                              securepassword);

                // create SessionOptions using Credentials
                WSManSessionOptions SessionOptions = new WSManSessionOptions();
                SessionOptions.AddDestinationCredentials(Credentials);
                LoopDrives(SessionOptions, creds, container);
            }
            else
            {
                IRunSvc.LogStatus("Running Normal Search");
                string drivedir = "";
                if (Directory.Exists(creds.computerName))
                {
                    drivedir = creds.computerName;
                }
                else
                    drivedir = @"\\" + creds.computerName;
                IRunSvc.LogStatus("Scanning: " + drivedir);
                //Uri foo = new Uri("file:" + drivedir);
                bool isUnc = PathIsUNC(drivedir);
               
                if (Directory.Exists(drivedir) || isUnc)
                {
                   
                    IFileParserSvc IFileParserSvc = (IFileParserSvc)container.Resolve(typeof(IFileParserSvc), "TRAVERSEFILES");
                    
                    IFileParserSvc.ReadAndParse(drivedir, creds, container);

                   
                    
                }
                else
                {
                    IRunSvc.LogStatus("Directory Doesn't Exist: " + drivedir);
                }
                IRunSvc.LogStatus("Finished Scanning: " + drivedir);
                
            }
            
        }
        private void LoopDrives(WSManSessionOptions SessionOptions, ResponseDataSetIndexCred creds, UnityContainer container)
        {
            CimSession Session = CimSession.Create(creds.computerName, SessionOptions);
            var allVolumes = Session.QueryInstances(@"root\cimv2", "WQL", "SELECT * FROM Win32_Volume");
            var allPDisks = Session.QueryInstances(@"root\cimv2", "WQL", "SELECT * FROM Win32_DiskDrive");
            foreach (CimInstance oneVolume in allVolumes)
            {
                if (oneVolume.CimInstanceProperties["DriveLetter"].ToString()[0] > ' ')
                {
                    string drivedir = (string)oneVolume.CimInstanceProperties["DriveLetter"].Value;
                    if (drivedir != null)
                    {
                        drivedir = drivedir.Replace(":", @"$\");
                        drivedir = @"\\" + creds.computerName + @"\" + drivedir;
                        if (Directory.Exists(drivedir))
                        {
                            IFileParserSvc IFileParserSvc = (IFileParserSvc)container.Resolve(typeof(IFileParserSvc), "TRAVERSEFILES");

                            IFileParserSvc.ReadAndParse(drivedir, creds, container);

                            //IFileParserSvc.ReadAndParse(drivedir, creds, container);
                        }

                    }
                }

            }
        }

    }
}
