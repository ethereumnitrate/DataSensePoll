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
    public class ProcessPDFSvc : IProcessSvc
    {
        private readonly IPatternWriterSvc IPatternWriterSvc;
        private readonly IRunInfoSvc IRunSvc;

        public ProcessPDFSvc(IPatternWriterSvc _IPatternWriterSvc,  IRunInfoSvc _IRunSvc)
        {
            IPatternWriterSvc = _IPatternWriterSvc;
           
            IRunSvc = _IRunSvc;
        }
        bool IProcessSvc.Process(IList<ResponseDataSetIndexReg> regexes, string fileName)
        {
            bool isProcessable = true;
            try
            {
                
                var pdf = new Document(fileName);
                foreach (Page page in pdf.Pages)
                {
                    var textAbsorber = new TextAbsorber
                    {
                        ExtractionOptions = {
                        FormattingMode = TextExtractionOptions.TextFormattingMode.Pure
                    }
                    };

                    page.Accept(textAbsorber);
                    var ext = textAbsorber.Text;

                    //string regExSocialDashes = @"(?!219-09-9999|078-05-1120)(?!666|000|9\d{2})\d{3}-(?!00)\d{2}-(?!0{4})\d{4}";
                    foreach(ResponseDataSetIndexReg regExpression in regexes)
                    {
                        string regExSocialDashes = regExpression.regularExpression;
                        var matchlist = Regex.Matches(ext, regExSocialDashes);
                        if (matchlist.Count > 0)
                        {
                            //Console.WriteLine("Found " + regExpression.Key +  "  in: " + fileName);
                            PatternPost patternpost = new PatternPost();
                            patternpost.runId = IRunSvc.GetCurrentRunID();
                            patternpost.dataSetIndexExpId = regExpression.id;
                            patternpost.fileName = fileName;
                            
                            foreach(Match match in matchlist)
                            {
                                int midx = match.Index;
                                int endIndex = midx + 50;
                                patternpost.previewText += ext.Substring(midx, endIndex - midx);
                                break;
                            }
                            int credIdCopy = regExpression.dataSetIndexCredId;
                            Task<int> tak = IPatternWriterSvc.AddData(patternpost, credIdCopy);
                            tak.Wait();
                          
                            
                        }
                    }
                    
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("PDF Processor: An error happened while trying to process document");
                Console.WriteLine("PDF Processor: Error has been logged.");
                isProcessable = false;
                //Don't do anything else so we can continue
            }
            return isProcessable;
            
        }
    }
}
