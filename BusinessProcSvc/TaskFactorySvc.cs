using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IBusinessProcSvc;
using System.Threading;
namespace BusinessProcSvc
{
    public class TaskFactorySvc : ITaskFactorySvc
    {
        private HashSet<Task<int>> TaskFactory = new HashSet<Task<int>>();
        private Mutex mutext = new Mutex();
        void ITaskFactorySvc.AddTask(Task<int> task)
        {
            mutext.WaitOne();
            TaskFactory.Add(task);
            mutext.ReleaseMutex();
        }

        HashSet<Task<int>> ITaskFactorySvc.GetTasks()
        {
            return TaskFactory;
        }
    }
}
