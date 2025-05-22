using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTransfer.Response
{
    public class ResponseQuarantine
    {
        public int quarantineId { get; set; }

        public int credId { get; set; }

        public string fileName { get; set; }

        public bool isQuarantine { get; set; }
    }
}
