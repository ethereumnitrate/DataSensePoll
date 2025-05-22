using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTransfer.Response
{
    public class ResponseDataSetIndex
    {
        public int id { get; set; }
        public int dataSetId { get; set; }

        public string nodeUrl { get; set;  }

        public bool active { get; set; }
    }


}
