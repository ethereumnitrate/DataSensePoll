using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTransfer.Request
{
    public class PageRequest
    {
        public int pageNo { get; set; }

        public int noOfRecords { get; set; }
    }
}
