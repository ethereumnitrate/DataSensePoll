using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTransfer.Response
{
    public class ResponseDataSetIndexReg
    {
        public int id { get; set; }

        public int dataSetIndexCredId { get; set; }

        public string regularExpression { get; set; }

        public string dataTypeDesc { get; set; }

        public bool active { get; set; }
    }
}
