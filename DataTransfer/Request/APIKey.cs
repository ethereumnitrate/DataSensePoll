using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataTransfer.Common;

namespace DataTransfer.Request
{

    public class APIKey
    {
        private string CurrentLicenseKey = Configuration.CurrentAPIKey();

        public string apiKey { get { return CurrentLicenseKey; } set { value = CurrentLicenseKey; }  }
    }
}
