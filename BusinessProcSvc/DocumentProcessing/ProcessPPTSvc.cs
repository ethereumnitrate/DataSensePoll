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


using Aspose.Slides;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;
using DataTransfer;

namespace BusinessProcSvc.DocumentProcessing
{
    public class ProcessPPTSvc : IProcessSvc
    {
        private readonly IPatternWriterSvc IPatternWriterSvc;
        private readonly IRunInfoSvc IRunSvc;
        
        public ProcessPPTSvc(IPatternWriterSvc _IPatternWriterSvc, IRunInfoSvc _IRunSvc)
        {
            IPatternWriterSvc = _IPatternWriterSvc;
           
            IRunSvc = _IRunSvc;
        }
        bool IProcessSvc.Process(IList<ResponseDataSetIndexReg> regexes, string fileName)
        {
            bool isProcessable = true;
            try
            {
                using (Presentation prestg = new Presentation(fileName))
                {
                    //Get an Array of ITextFrame objects from all slides in the PPTX
                    ITextFrame[] textFramesPPTX = Aspose.Slides.Util.SlideUtil.GetAllTextFrames(prestg, true);

                    for (int i = 0; i < textFramesPPTX.Length; i++)
                        foreach (IParagraph para in textFramesPPTX[i].Paragraphs)
                            foreach (IPortion port in para.Portions)
                            {
                                //Display text in the current portion
                                //Console.WriteLine(port.Text);
                                foreach (ResponseDataSetIndexReg regExpression in regexes)
                                {
                                    string regExSocialDashes = regExpression.regularExpression;
                                    var matchlist = Regex.Matches(port.Text, regExSocialDashes);
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
                                            patternpost.previewText += port.Text.Substring(midx, endIndex - midx);
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
                Console.WriteLine("Powerpoint Processor: An error happened while trying to process document");
                Console.WriteLine("Powerpoint Processor: Error has been logged.");
                isProcessable = false;
                //Don't do anything else so we can continue
            }
            return isProcessable;
            
        }
    }
}
