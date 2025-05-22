using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IBusinessProcSvc;
using System.IO;
using System.IO.Compression;
using ICSharpCode.SharpZipLib.Tar;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.BZip2;


namespace BusinessProcSvc.DocumentProcessing
{
    public class ProcessCompressedSvc :  IProcessCompressedSvc
    {
        private IRunInfoSvc IRunSvc;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public ProcessCompressedSvc(IRunInfoSvc _IRunSvc)
        {
            IRunSvc = _IRunSvc;
        }

        bool IProcessCompressedSvc.isCompresed(string filename)
        {
            string ext = Path.GetExtension(filename);
            bool isCompressed = false;
            if (ext == ".tar")
            {
                isCompressed = true;
            }
            else if (ext == ".zip")
            {
                isCompressed = true;
            }           
            else if (ext == ".gz" || ext == ".tgz")
            {
                isCompressed = true;
            }
            else if (ext == ".bz2")
            {
                isCompressed = true;
            }

            return isCompressed;
        }

        string IProcessCompressedSvc.ExtractFiles(string compressedfilename)
        {
            IRunSvc.LogStatus("Uncompressing File: " + compressedfilename);
            log.Info("Uncompressing File: " + compressedfilename);
            string ext = Path.GetExtension(compressedfilename);
            string randomlyGeneratedFolderNamePart = Path.GetFileNameWithoutExtension(Path.GetRandomFileName());
            Directory.CreateDirectory(randomlyGeneratedFolderNamePart);
            if (ext == ".tar")
            {
                ExtractFile_Tar(compressedfilename, randomlyGeneratedFolderNamePart);
                
            }
            else if (ext == ".zip")
            {
                ExtractFile_Zip(compressedfilename, randomlyGeneratedFolderNamePart);
                
            }
            else if (ext == ".gz" || ext == ".tgz")
            {
                ExtractFile_GZip(compressedfilename, randomlyGeneratedFolderNamePart);
                
            }
            else if (ext == ".bz2")
            {
                ExtractFile_BZip2(compressedfilename, randomlyGeneratedFolderNamePart);
                
            }
            IRunSvc.LogStatus("Finished Uncompressing: " + compressedfilename);
            IRunSvc.LogStatus("Directory Extracted to: " + randomlyGeneratedFolderNamePart);
            log.Info("Finished Uncompressing: " + compressedfilename);
            log.Info("Directory Extracted to: " + randomlyGeneratedFolderNamePart);
            return randomlyGeneratedFolderNamePart;
        }
        void ExtractFile_Tar(string SourceFile, string DestinationPath)
        {
            try
            {
                Console.WriteLine(SourceFile);

                Stream inStream = File.OpenRead(SourceFile);

                TarArchive tarArchive = TarArchive.CreateInputTarArchive(inStream);
                tarArchive.ExtractContents(DestinationPath);
                tarArchive.CloseArchive();

                inStream.Close();
            }
            catch (System.Exception Ex)
            {
                log.Error(SourceFile + "-------- - " + Ex.ToString());
                IRunSvc.LogStatus("Error happened trying to extract: " + SourceFile);
                
            }
        }
        void ExtractFile_Zip(string SourceFile, string DestinationPath)
        {
            try
            {
                
                Console.WriteLine(SourceFile);
                ZipFile.ExtractToDirectory(SourceFile, DestinationPath);
            }
            catch (System.Exception Ex)
            {
                log.Error(SourceFile + "-------- - " + Ex.ToString());
                IRunSvc.LogStatus("Error happened trying to extract: " + SourceFile);
            }
        }
        void ExtractFile_GZip(string SourceFile, string DestinationPath)
        {
            try
            {
                Console.WriteLine(SourceFile);
                Stream inStream = File.OpenRead(SourceFile);
                Stream gzipStream = new GZipInputStream(inStream);

                TarArchive tarArchive = TarArchive.CreateInputTarArchive(gzipStream);
                tarArchive.ExtractContents(DestinationPath);
                tarArchive.CloseArchive();

                gzipStream.Close();
                inStream.Close();
            }
            catch (System.Exception Ex)
            {
                log.Error(SourceFile + "-------- - " + Ex.ToString());
                IRunSvc.LogStatus("Error happened trying to extract: " + SourceFile);
            }
        }
        void ExtractFile_BZip2(string SourceFile, string DestinationPath)
        {
            try
            {
                Console.WriteLine(SourceFile);
                Stream inStream = File.OpenRead(SourceFile);
                Stream bzipStream = new BZip2InputStream(inStream);

                TarArchive tarArchive = TarArchive.CreateInputTarArchive(bzipStream);
                tarArchive.ExtractContents(DestinationPath);
                tarArchive.CloseArchive();

                bzipStream.Close();
                inStream.Close();

            }
            catch (System.Exception Ex)
            {
                log.Error(SourceFile + "-------- - " + Ex.ToString());
                IRunSvc.LogStatus("Error happened trying to extract: " + SourceFile);
            }
        }


    }
}
