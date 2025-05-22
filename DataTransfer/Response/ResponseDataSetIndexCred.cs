using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTransfer.Response
{
    public class ResponseDataSetIndexCred
    {
        public int id { get; set; }

        public int dataSetIndexId { get; set; }
        public string computerName { get; set; }
        public string domainName { get; set; }
        public string userName { get; set; }
        public string passWord { get; set; }
        public bool isWmi { get; set; }

        public bool isDataBase { get; set;  }

        public bool active { get; set; }
    }
}
