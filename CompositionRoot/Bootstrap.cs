using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using Unity;
using IBusinessProcSvc;
using BusinessProcSvc.DocumentProcessing;
using BusinessProcSvc;
using Unity.Lifetime;

namespace CompositionRoot
{
    public class Bootstrap
    {
        public static UnityContainer container;

        public static void Start()
        {
            container = new UnityContainer();

            Aspose.Pdf.License licensepdf = new Aspose.Pdf.License();
            licensepdf.SetLicense("Aspose.Total.lic");
            Aspose.Cells.License licensecells = new Aspose.Cells.License();
            licensecells.SetLicense("Aspose.Total.lic");
            Aspose.Slides.License licenseslides = new Aspose.Slides.License();
            licenseslides.SetLicense("Aspose.Total.lic");
            Aspose.Words.License licensewords = new Aspose.Words.License();
            licensewords.SetLicense("Aspose.Total.lic");

            container.RegisterType<IGetPollingInfoSvc, PollingInfoSvc>(new ContainerControlledLifetimeManager ());
            container.RegisterType<IPatternRunSvc, PatternRunSvc>(new ContainerControlledLifetimeManager());
            container.RegisterType<IDataSetIndexCredSvc, DataSetIndexCredsSvc>(new ContainerControlledLifetimeManager());
            //container.RegisterType<IFileParserSvc, FileParserSvc>(new PerThreadLifetimeManager());

            //container.RegisterType<IProcessDirectorySvc, ProcessDirectorySvc>(new PerThreadLifetimeManager());

            container.RegisterType<IFileHashWriterSvc, FileHashWriterSvc>(new ContainerControlledLifetimeManager());
            container.RegisterType<IFileDetectorSvc, FileDetectorSvc>(new ContainerControlledLifetimeManager());
            container.RegisterType<IPatternWriterSvc, PatternWriterSvc>(new ContainerControlledLifetimeManager());
            container.RegisterType<ITaskFactorySvc, TaskFactorySvc>(new ContainerControlledLifetimeManager());
            container.RegisterType<IRunInfoSvc, RunInfoSvc>(new ContainerControlledLifetimeManager());
            container.RegisterType<IProcessCompressedSvc, ProcessCompressedSvc>(new TransientLifetimeManager());
            container.RegisterType<IZipSourceSvc, ZipSourceSvc>(new ContainerControlledLifetimeManager());
            //container.RegisterType<IProcessSvc, ProcessPDFSvc>(new ContainerControlledLifetimeManager());
            container.RegisterType(typeof(IProcessSvc), typeof(ProcessPDFSvc), "PDF");
            container.RegisterType(typeof(IProcessSvc), typeof(ProcessWordSvc), "WORD");
            container.RegisterType(typeof(IProcessSvc), typeof(ProcessExcelSvc), "EXCEL");
            container.RegisterType(typeof(IProcessSvc), typeof(ProcessPPTSvc), "PPT");
            container.RegisterType(typeof(IProcessSvc), typeof(ProcessTextSvc), "TEXT");
            container.RegisterType(typeof(IProcessDBSvc), typeof(ProcessDataBaseSvc), "MSSQL");
            //container.RegisterInstance<IProcessSvc>("PDF", pdfsvc , new ContainerControlledLifetimeManager());
            container.RegisterType(typeof(IProcessDirectorySvc), typeof(ProcessDirectorySvc), "PROCFILE");
            
            container.RegisterType(typeof(IFileParserSvc), typeof(FileParserSvc), "TRAVERSEFILES");
            //container.Resolve<IProcessSvc>("PDF");


            container.Resolve<IGetPollingInfoSvc>();
            
            container.Resolve<IDataSetIndexCredSvc>();
            container.Resolve<IPatternRunSvc>();
            container.Resolve<IZipSourceSvc>();
            container.Resolve<IFileHashWriterSvc>();
            container.Resolve<IPatternWriterSvc>();
            container.Resolve<ITaskFactorySvc>();
            container.Resolve<IRunInfoSvc>();
            
        }

    }
}
