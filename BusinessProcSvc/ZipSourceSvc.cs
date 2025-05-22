using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IBusinessProcSvc;
using DataTransfer.Common;
using DataTransfer.Request;

namespace BusinessProcSvc
{
    public class ZipSourceSvc : IZipSourceSvc
    {
        public Dictionary<string, string> ZipSourceDir = new Dictionary<string, string>();
        void IZipSourceSvc.Add(string originalsrc, string dest)
        {
            lock (ZipSourceDir)
            {
                ZipSourceDir.Add(originalsrc, dest);
            }
        }

        Dictionary<string, string> IZipSourceSvc.GetZipDirectories()
        {
            return ZipSourceDir;
        }

        void IZipSourceSvc.Remove(string val)
        {
            lock (ZipSourceDir)
            {
                ZipSourceDir.Remove(val);
            }
        }
    }
}
