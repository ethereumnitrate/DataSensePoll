using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DataTransfer.Common
{
    public static class Configuration
    {
        public static string APIPath()
        {
            string retVal = "";     //make an empty string default
            try
            {
                retVal = Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["apiPath"]);
            }
            catch { }
            return retVal;
        }

        public static string APIBaseURLPath()
        {
            string retVal = "";     //make an empty string default
            try
            {
                retVal = Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["apiBaseURLPath"]);
            }
            catch { }
            return retVal;
        }
        public static string CurrentAPIKey()
        {
            string retVal = "";     //make an empty string default
            try
            {
                retVal = Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["LicenseKey"]);
            }
            catch { }
            return retVal;
        }
        public static string MinBatchFilterProp()
        {
            string retVal = "";     //make an empty string default
            try
            {
                retVal = Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["minFilterProbBatch"]);
            }
            catch { }
            return retVal;
        }
        public static bool  ExcludeViews()
        {
            bool retVal = true;     //make an empty string default
            try
            {
                retVal = Convert.ToBoolean(System.Configuration.ConfigurationManager.AppSettings["excludeViews"]);
            }
            catch { }
            return retVal;
        }

        public static string TwoNodeLicense()
        {
            return "NjgyMzQ2NDctMDQ2OS00YjIxLWJhMjQtOTJhOWY5NTY0NDUx";
        }
        public static string FiveNodeLicense()
        {
            return "MTczOWJkZGItMjViMC00MWU5LWJhOTMtMjNmYWVlZTJmZDhh";
        }
        public static string UnlimitedNodeLicense()
        {
            return "M2E0OWQ1YTEtZGNhMi00NThhLTljNDQtYzU3ZjkyZWVhNzIx";
        }

    }
}
