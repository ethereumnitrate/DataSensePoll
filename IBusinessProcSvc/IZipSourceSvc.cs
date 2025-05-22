using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBusinessProcSvc
{
    public interface IZipSourceSvc
    {
        void Add(string originalsrc, string dest);

        void Remove(string val);

        Dictionary<string, string> GetZipDirectories();
    }
}
