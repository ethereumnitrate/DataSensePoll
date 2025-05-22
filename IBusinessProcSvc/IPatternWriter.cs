using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataTransfer.Request;
namespace IBusinessProcSvc
{
    public interface IPatternWriterSvc
    {
        Task<int> PostPatternData(IList<PatternPost> patternsFound);

        Task<int> AddData(PatternPost patternData, int CredID);

        Task<int> ReleaseLeftOverData(int CredID);
    }
}
