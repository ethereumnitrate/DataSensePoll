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


using Aspose.Pdf;
using Aspose.Pdf.Text;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;
using DataTransfer;

namespace BusinessProcSvc.DocumentProcessing
{
    public class ProcessTextSvc : IProcessSvc
    {
        private readonly IFileDetectorSvc FileDetectorSvc;
        private readonly IPatternWriterSvc IPatternWriterSvc;
        private readonly IRunInfoSvc IRunSvc;
        
        public ProcessTextSvc(IFileDetectorSvc _FileDetectorSvc, IPatternWriterSvc _IPatternWriterSvc,
                            IRunInfoSvc _IRunSvc
                                )
        {
            IPatternWriterSvc = _IPatternWriterSvc;
            Aspose.Pdf.License license = new Aspose.Pdf.License();
            license.SetLicense("Aspose.Total.lic");
            FileDetectorSvc = _FileDetectorSvc;
            IRunSvc = _IRunSvc;
        }
        bool IProcessSvc.Process(IList<ResponseDataSetIndexReg> regexes, string fileName)
        {
            bool isProcessable = true;
            try
            {
                const int chunkSize = 1024; // read the file by chunks of 1KB
                using (var file = File.OpenRead(fileName))
                {
                    int bytesRead;
                    var buffer = new byte[chunkSize];
                    while ((bytesRead = file.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        string strTextToSearch = "";
                        Encoding ecode = Encoding.UTF8;
                        strTextToSearch = ecode.GetString(buffer);
                        foreach (ResponseDataSetIndexReg regExpression in regexes)
                        {
                            string regExSocialDashes = regExpression.regularExpression;
                            var matchlist = Regex.Matches(strTextToSearch, regExSocialDashes);
                            if (matchlist.Count > 0)
                            {
                                //Console.WriteLine("Found " + regExpression.Key + "  in: " + fileName);
                                PatternPost patternpost = new PatternPost();
                                patternpost.runId = IRunSvc.GetCurrentRunID();
                                patternpost.dataSetIndexExpId = regExpression.id;
                                patternpost.fileName = fileName;
                                foreach (Match match in matchlist)
                                {
                                    int midx = match.Index;
                                    int endIndex = midx + 50;
                                    patternpost.previewText += strTextToSearch.Substring(midx, endIndex - midx);
                                    break;
                                }
                                int credIdCopy = regExpression.dataSetIndexCredId;
                                Task<int> tak = IPatternWriterSvc.AddData(patternpost, credIdCopy);
                                tak.Wait();                                
                            }
                        }
                    
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Text Processor: An error happened while trying to process document");
                Console.WriteLine("Text Processor: Error has been logged.");
                isProcessable = false;
                //Don't do anything else so we can continue
            }
            return isProcessable;
            
        }
    }
}
