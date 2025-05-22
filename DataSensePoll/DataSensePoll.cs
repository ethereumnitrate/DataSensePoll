
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Net.Http;
using Newtonsoft.Json;
using CompositionRoot;
using IBusinessProcSvc;
using Microsoft.Practices.Unity;

namespace DataSensePoll
{
    public class DataSensePoll
    {
        private static string runMode;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        static void Main(string[] args)
        {
            try
            {
                DataSensePoll dp = new DataSensePoll();
                dp.SetupUnity();
                runMode = args[0];
                Console.WriteLine("Indexing job started ===========================");
                log.Info("Indexing job started ===========================");
               
                if (runMode == "-p")
                {
                    Console.WriteLine("Running in Pattern Mode ===========================");
                    log.Info("Running in Pattern Mode===========================");
                    var patternRun = (IPatternRunSvc)CompositionRoot.Bootstrap.container.Resolve(typeof(IPatternRunSvc), null, null);
                    patternRun.ScanFiles(Bootstrap.container);
                   
                }
                else
                {
                    Console.WriteLine("Invalid Option: -p (pattern) or -i (index)");
                    log.Error("Invalid Option: -p (pattern) or -i (index)");
                }
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + e.InnerException);
                log.Error("Exception Ocurred:" + e.Message);
                log.Error("Exception More Information: " + e.InnerException);
            }
            
        }

        public void SetupUnity()
        {
            CompositionRoot.Bootstrap.Start();
        }
      
    }
}
