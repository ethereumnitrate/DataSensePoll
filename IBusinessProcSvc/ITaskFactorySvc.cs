using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBusinessProcSvc
{
    public interface ITaskFactorySvc
    {
        void AddTask(Task<int> task);

        HashSet<Task<int>> GetTasks(); 
    }
}
