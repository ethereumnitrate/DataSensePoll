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


using Aspose.Cells;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;
using DataTransfer;

namespace BusinessProcSvc.DocumentProcessing
{
    public class ProcessExcelSvc : IProcessSvc
    {
        private readonly IPatternWriterSvc IPatternWriterSvc;
        private readonly IRunInfoSvc IRunSvc;
        
        public ProcessExcelSvc(IPatternWriterSvc _IPatternWriterSvc, IRunInfoSvc _IRunSvc)
        {
            IPatternWriterSvc = _IPatternWriterSvc;
           
            IRunSvc = _IRunSvc;
        }
        bool IProcessSvc.Process(IList<ResponseDataSetIndexReg> regexes, string fileName)
        {
            bool isProcessable = true;
            try
            {
                Aspose.Cells.Workbook workbook = new Workbook(fileName);
                for (int i = 0; i < workbook.Worksheets.Count; i++)
                {
                    Cells cells = workbook.Worksheets[i].Cells;
                    var enumcells = cells.GetEnumerator();
                    //Parallel.ForEach(enumcells, cell => cell.StringValue);
                    while (enumcells.MoveNext())
                    {
                        Cell cell = (Cell)enumcells.Current;
                        if (cell.Type == CellValueType.IsString)
                        {
                            string text = cell.StringValue;
                            //Console.WriteLine("Cell Values: {0}", text);
                            foreach (ResponseDataSetIndexReg regExpression in regexes)
                            {
                                string regExSocialDashes = regExpression.regularExpression;
                                var matchlist = Regex.Matches(text, regExSocialDashes);
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
                                        patternpost.previewText += text.Substring(midx, endIndex - midx);
                                        break;
                                    }

                                    //patternpost.previewText = text;
                                    int credIdCopy = regExpression.dataSetIndexCredId;
                                    Task<int> tak = IPatternWriterSvc.AddData(patternpost, credIdCopy);
                                    tak.Wait();
                                   
                                    
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Excel Processor: An error happened while trying to process document");
                Console.WriteLine("Excel Processor: Error has been logged.");
                isProcessable = false;
                //Don't do anything else so we can continue
            }
            return isProcessable;
            
        }
    }
}
