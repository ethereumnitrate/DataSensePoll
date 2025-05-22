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
using Aspose.Words;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;
using DataTransfer;

namespace BusinessProcSvc.DocumentProcessing
{
    public class ProcessWordSvc : IProcessSvc
    {
        private readonly IPatternWriterSvc IPatternWriterSvc;
        private readonly IRunInfoSvc IRunSvc;
        
        public ProcessWordSvc(IPatternWriterSvc _IPatternWriterSvc, IRunInfoSvc _IRunSvc)
        {
            IPatternWriterSvc = _IPatternWriterSvc;
           
            IRunSvc = _IRunSvc;
        }
        bool IProcessSvc.Process(IList<ResponseDataSetIndexReg> regexes, string fileName)
        {
            bool isProcessable = true;
            try
            {
              
                    var worddoc = new Document(fileName);
                    traverseAllNodes(worddoc, regexes, fileName);              

            }
            catch (Exception e)
            {
                Console.WriteLine("Word Processor: An error happened while trying to process document");
                Console.WriteLine("Word Processor: Error has been logged.");
                isProcessable = false;
                //Don't do anything else so we can continue
            }
            return isProcessable;
            
        }
        public void traverseAllNodes(CompositeNode parentNode, IList<ResponseDataSetIndexReg> regexes, string fileName)
        {

            // This is the most efficient way to loop through immediate children of a node.
            for (Node childNode = parentNode.FirstChild; childNode != null; childNode = childNode.NextSibling)
            {
                //Console.WriteLine(Node.NodeTypeToString(childNode.NodeType));
                //Console.WriteLine(childNode.GetText());
                foreach (ResponseDataSetIndexReg regExpression in regexes)
                {
                    string regExSocialDashes = regExpression.regularExpression;
                    var matchlist = Regex.Matches(childNode.GetText(), regExSocialDashes);
                    if (matchlist.Count > 0)
                    {
                        PatternPost patternpost = new PatternPost();
                        patternpost.runId = IRunSvc.GetCurrentRunID();
                        patternpost.dataSetIndexExpId = regExpression.id;
                        patternpost.fileName = fileName;
                        foreach (Match match in matchlist)
                        {
                            int midx = match.Index;
                            int endIndex = midx + 50;
                            patternpost.previewText += childNode.GetText().Substring(midx, endIndex - midx);
                            break;
                        }
                        int credIdCopy = regExpression.dataSetIndexCredId;
                        Task<int> tak = IPatternWriterSvc.AddData(patternpost, credIdCopy);
                        tak.Wait();
                        
                    }
                }
                if (childNode.IsComposite)
                    traverseAllNodes((CompositeNode)childNode, regexes, fileName);
            }
        }
    }
}
