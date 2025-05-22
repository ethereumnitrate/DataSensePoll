using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTransfer.Response
{
    public class ResponseDataSet
    {
        public int id { get; set; }

        public string dataSetName { get; set; }

        public DateTime createdOn { get; set; }

        public string dataSetLookUpTable { get; set; }

        public bool active { get; set; }

        public Nullable<DateTime> modifiedOn { get; set; }

        public int? createdBy { get; set; }

        public int? modifiedBy { get; set; }
    }


}
