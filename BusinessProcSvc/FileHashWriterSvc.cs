using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IBusinessProcSvc;
using System.Security.Cryptography;
using System.Threading;
using DataTransfer.Response;
using System.Text.RegularExpressions;
namespace BusinessProcSvc
{
    public class FileHashWriterSvc : IFileHashWriterSvc
    {
        
        private HashSet<string> filesProcessed;
        private Mutex mutex = new Mutex();
        bool IFileHashWriterSvc.LogFile(FileSystemInfo fileprocessed, ResponseDataSetIndexCred creds)
        {
            mutex.WaitOne();
            bool needsProcess = true;
            try
            {
                byte[] hashValue;
                SHA256 hashfile = SHA256Managed.Create();
                creds.computerName = Regex.Replace(creds.computerName, "[^a-zA-Z0-9]", "");
                string filehashname = creds.computerName + "-" + creds.id + ".bin";
                FileStream fs;
                if (!File.Exists(filehashname))
                {
                    fs = File.Create(filehashname);
                    fs.Close();
                }
                
                LoadHashBytes(filehashname);                

                string filerecord = fileprocessed.FullName + "-" + fileprocessed.LastWriteTimeUtc;
                byte[] byteFileRecord = Encoding.ASCII.GetBytes(filerecord);
                hashValue = hashfile.ComputeHash(byteFileRecord);

                needsProcess = filesProcessed.Add(Convert.ToBase64String(hashValue));

                if (needsProcess == true)
                {                    
                    AppendByteData(filehashname, hashValue);
                }
            }
            catch(Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
                Console.WriteLine("More Information: " + e.InnerException);
            }
            finally
            {
                mutex.ReleaseMutex();
            }
            return needsProcess;

        }


        private static void AppendIntData(string filename, int intData)
        {
            using (var fileStream = new FileStream(filename, FileMode.Append, FileAccess.Write, FileShare.None))
            using (var bw = new BinaryWriter(fileStream))
            {
                bw.Write(intData);                                
            }

        }
        private static void AppendByteData(string filename, byte[] lotsOfData)
        {
            using (var fileStream = new FileStream(filename, FileMode.Append, FileAccess.Write, FileShare.None))
            using (var bw = new BinaryWriter(fileStream))
            {
                bw.Write(lotsOfData);
            }
        }
        private void LoadHashBytes(string filehashnames)
        {
            if (filesProcessed == null)
            {
                filesProcessed = new HashSet<string>();
                using (BinaryReader reader = new BinaryReader(File.Open(filehashnames, FileMode.Open)))
                {
                    int length = (int)reader.BaseStream.Length;
                    while (reader.BaseStream.Position != length)
                    {
                        int bytesToRead = 32;
                        byte[] v = reader.ReadBytes(bytesToRead);
                        filesProcessed.Add(Convert.ToBase64String(v));                        
                    }
                }
            }

        }
    }
}
