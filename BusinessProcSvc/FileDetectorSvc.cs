using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aspose.Words;
using IBusinessProcSvc;
namespace BusinessProcSvc
{
    public class FileDetectorSvc : IFileDetectorSvc
    {
        public FileDetectorSvc()
        {
            Aspose.Words.License license = new Aspose.Words.License();
            license.SetLicense("Aspose.Total.lic");
        }

        Encoding IFileDetectorSvc.FileEncoding(string fileName)
        {
            FileFormatInfo fileformatinfo = FileFormatUtil.DetectFileFormat(fileName);
            Encoding encoding = fileformatinfo.Encoding;            
            return encoding;
        }

        string IFileDetectorSvc.FileType(string fileName)
        {
            FileFormatInfo fileformatinfo = FileFormatUtil.DetectFileFormat(fileName);
            
            string fileDetected = fileformatinfo.LoadFormat.ToString();
            if (fileformatinfo.LoadFormat == LoadFormat.Html || fileformatinfo.LoadFormat == LoadFormat.Mhtml)
                fileDetected = "Text";
            
            return fileDetected;
        }
        
    }
}
