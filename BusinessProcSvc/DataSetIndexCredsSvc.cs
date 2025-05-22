using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataTransfer.Common;
using DataTransfer.Response;
using IBusinessProcSvc;
using DataTransfer.Request;
using System.Net.Http;
using Newtonsoft.Json;
namespace BusinessProcSvc
{
    public class DataSetIndexCredsSvc : IDataSetIndexCredSvc
    {
        private string credmachineEndPoint = Configuration.APIPath() + "/machine/credentials/datasetindex/{id}";
        async Task<List<ResponseDataSetIndexCred>> IDataSetIndexCredSvc.getMachineCreds(HttpGetObject httpobj)
        {
            List<ResponseDataSetIndexCred> respdsidxcreds = new List<ResponseDataSetIndexCred>();
            try
            {
                httpobj.endPoint = credmachineEndPoint;
                credmachineEndPoint = credmachineEndPoint.Replace("{id}", httpobj.id);
                APIClient apiclient = new APIClient();
                HttpResponseMessage respDataSetIdxCreds = await apiclient.getAsync(httpobj);
                if (!respDataSetIdxCreds.IsSuccessStatusCode)
                {
                    Console.WriteLine("Cred Service is down..unable to get machines..");
                    Console.WriteLine("Exiting process...");
                    return respdsidxcreds;
                }
                respdsidxcreds = JsonConvert.DeserializeObject<List<ResponseDataSetIndexCred>>(await respDataSetIdxCreds.Content.ReadAsStringAsync());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + e.InnerException);
                //log.Error("Exception Ocurred:" + e.Message);
                //log.Error("Exception More Information: " + e.InnerException);
            }

            return respdsidxcreds;
        }
    }
}
