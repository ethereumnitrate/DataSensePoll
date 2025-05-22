using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IBusinessProcSvc;
using DataTransfer.Request;
using DataTransfer.Response;
using DataTransfer.Common;
using System.Net.Http;
using Newtonsoft.Json;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;
using DataTransfer;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;


namespace BusinessProcSvc.DocumentProcessing
{
    public class ProcessDataBaseSvc : IProcessDBSvc
    {
        private List<ResponseDataSetIndexReg> regEx;
        private IRunInfoSvc IRunSvc;
        private IPatternWriterSvc IPatternWriterSvc;
        private Mutex mutex = new Mutex();
        private IGetPollingInfoSvc IPollAPISvc;
        private List<ResponseQuarantine> quarantine;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public ProcessDataBaseSvc(IRunInfoSvc _IRunSvc, IPatternWriterSvc _IPatternWriterSvc, IGetPollingInfoSvc _IPollAPISvc)
        {
            IRunSvc = _IRunSvc;
            IPatternWriterSvc = _IPatternWriterSvc;
            IPollAPISvc = _IPollAPISvc;
        }
        public void LoadRegExp(int credID)
        {
            if (regEx == null || regEx.Count == 0)
            {

                regEx = new List<ResponseDataSetIndexReg>();
                var tokentask = IPollAPISvc.getToken();
                tokentask.Wait();
                string token = tokentask.Result;
                HttpGetObject httpgetobject = new HttpGetObject();
                httpgetobject.accessToken = token;
                int credIdCopy = credID;
                var regExTask = IPollAPISvc.getRegExps(httpgetobject, credIdCopy);
                regExTask.Wait();
                regEx = regExTask.Result;
            }
        }

        private void LoadQuarantine(int credID)
        {
            if (quarantine == null || quarantine.Count == 0)
            {
                quarantine = new List<ResponseQuarantine>();
                var tokentask = IPollAPISvc.getToken();
                tokentask.Wait();
                string token = tokentask.Result;
                HttpGetObject httpgetobject = new HttpGetObject();
                httpgetobject.accessToken = token;
                int credIdCopy = credID;
                quarantine = IPollAPISvc.getQuarantineFiles(httpgetobject, credIdCopy).Result;
            }
        }
        bool IProcessDBSvc.Process( ResponseDataSetIndexCred cred)
        {
            int credId = cred.id;
            LoadRegExp(credId);
            string connectionString = "";
            bool isProcess = true;
            if (cred.passWord == "" || cred.passWord == null)
                connectionString = "Data Source=" + cred.computerName + ";Initial Catalog=" + cred.domainName + ";Integrated Security=True";
            else
                connectionString = "Data Source=" + cred.computerName + ";Initial Catalog=" + cred.domainName + ";User ID=" + cred.userName + ";Password=" + cred.passWord + ";Persist Security Info=True;";

            DataTable TableNameDs = GetTableName(connectionString);
            if (TableNameDs.Rows.Count > 0)
            {
                for (int i = 0; i < TableNameDs.Rows.Count; i++)
                {
                    Console.WriteLine("Process Starting with Table : " + TableNameDs.Rows[i]["table_name"].ToString());
                    log.Info("Process Starting with Table : " + TableNameDs.Rows[i]["table_name"].ToString());
                    IRunSvc.LogStatus("Processing table: " + TableNameDs.Rows[i]["table_name"].ToString());
                    DataTable ColumnNameds = GetColumnName(connectionString, TableNameDs.Rows[i]["table_name"].ToString());
                    string ColumNameStr = string.Empty;
                    if (ColumnNameds.Rows.Count > 0)
                    {
                        for (int k = 0; k < ColumnNameds.Rows.Count; k++)
                        {
                            if (ColumNameStr == string.Empty)
                            {
                                ColumNameStr = "[" + ColumnNameds.Rows[k]["COLUMN_NAME"].ToString() + "]";
                            }
                            else
                            {
                                ColumNameStr = ColumNameStr + ",[" + ColumnNameds.Rows[k]["COLUMN_NAME"].ToString() + "]";
                            }
                        }
                    }

                    Console.WriteLine("Table scanning :" + TableNameDs.Rows[i]["table_name"].ToString() + " with ColumnName : " + ColumNameStr);
                    log.Info("Table scanning: " + TableNameDs.Rows[i]["table_name"].ToString() + " with ColumnName: " + ColumNameStr);
                    IRunSvc.LogStatus("Scanning Table: " + TableNameDs.Rows[i]["table_name"].ToString() + " with ColumnName: " + ColumNameStr);
                    GetTableData(connectionString, TableNameDs.Rows[i]["table_name"].ToString(), ColumNameStr);
                    IRunSvc.LogStatus("Finished Scanning Table: " + TableNameDs.Rows[i]["table_name"].ToString() + " with ColumnName: " + ColumNameStr);
                }
            }
            else
                isProcess = false;

            HttpGetObject httpobj = new HttpGetObject();
            httpobj.accessToken = IPollAPISvc.getToken().Result;
            int credIdCopy = cred.id;
            IPollAPISvc.NotifyComplete(httpobj, credIdCopy);
            return isProcess;
        }
        private void GetTableData(string ConnectionString, string TableName, string ColumNameStr)
        {

            SqlConnection sqlConn = new SqlConnection(ConnectionString);
            SqlCommand cmd = new SqlCommand("select " + ColumNameStr + " from " + TableName + " (NoLock)", sqlConn);

            DbDataReader rs;
            string[] ColArra = ColumNameStr.Split(',');
            //Regex SelectRegex = new Regex(searchdata);

            try
            {
                sqlConn.Open();
                cmd.CommandType = CommandType.Text;
                rs = cmd.ExecuteReader();

                while (rs.Read())
                {
                    for (int i = 0; i < ColArra.Length; i++)
                    {
                        foreach (ResponseDataSetIndexReg regExpression in regEx)
                        {
                            string regExSocialDashes = regExpression.regularExpression;
                            string StrValue = rs.GetValue(i).ToString();
                            var matchlist = Regex.Matches(StrValue, regExSocialDashes);
                            if (matchlist.Count > 0)
                            {
                                //Console.WriteLine("Found " + regExpression.Key + "  in: " + fileName);
                                PatternPost patternpost = new PatternPost();
                                patternpost.runId = IRunSvc.GetCurrentRunID();
                                patternpost.dataSetIndexExpId = regExpression.id;
                                patternpost.fileName = "Found in table: " +  TableName + " - Column : " + ColArra[i].ToString();
                                foreach (Match match in matchlist)
                                {
                                    int midx = match.Index;
                                    int endIndex = midx + 50;
                                    int lengthof = StrValue.Length;
                                    if (endIndex > StrValue.Length)
                                        endIndex = StrValue.Length;
                                    patternpost.previewText += StrValue.Substring(midx, endIndex - midx);
                                    break;
                                }
                                int credIdCopy = regExpression.dataSetIndexCredId;
                                //int tak =  IPatternWriterSvc.AddData(patternpost, credIdCopy).Result;
                                Task<int> tak = IPatternWriterSvc.AddData(patternpost, credIdCopy);
                                tak.Wait();
                            }
                        }
                        //if (SelectRegex.IsMatch(rs.GetValue(i).ToString()))
                        //{
                        //    string StrValue = Left(rs.GetValue(i).ToString(), 100);

                        //LogData(StrValue, FilePath, TableName, ColArra[i].ToString());
                        //}
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
                log.Error("Error trying to read data: " + ex.Message.ToString());
                log.Error("Table: " + TableName);
                IRunSvc.LogEnd(true);
            }
            finally
            {
               
                sqlConn.Close();
                cmd.Dispose();
                sqlConn.Dispose();
            }

        }

        private DataTable GetTableName(string ConnectionString)
        {
            DataTable dt = new DataTable();
            SqlConnection sqlConn = new SqlConnection(ConnectionString);
            string excludeViews = " and Table_Name not in ( " +
                                  " Select table_name " +
                                  " From INFORMATION_SCHEMA.Tables " +
                                  " where Table_type = 'VIEW')";
            if (Configuration.ExcludeViews() == false)
            {
                excludeViews = "";
            }
            string SQLStr = " select Distinct table_name from INFORMATION_SCHEMA.COLUMNS " + 
                            " where DATA_TYPE in ('varchar', 'nvarchar', 'nchar', 'char', 'text', 'ntext') " + 
                            " and table_name <> 'Patterntbl' " + excludeViews +  "   order by table_name";



            SqlCommand cmd = new SqlCommand(SQLStr, sqlConn);

            //SqlDataAdapter adp = new SqlDataAdapter();
            DbDataReader rs;
            try
            {
                sqlConn.Open();
                cmd.CommandType = CommandType.Text;
                rs = cmd.ExecuteReader();
                dt.Load(rs);


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
                log.Error("Error happened trying to read table schema information");
                IRunSvc.LogEnd(true);
            }
            finally
            {
                sqlConn.Close();
                cmd.Dispose();
                sqlConn.Dispose();
            }

            return dt;
        }

        private DataTable GetColumnName(string ConnectionString, string TableName)
        {
            DataTable dt = new DataTable();

            SqlConnection sqlConn = new SqlConnection(ConnectionString);
            SqlCommand cmd = new SqlCommand("select COLUMN_NAME from INFORMATION_SCHEMA.COLUMNS where DATA_TYPE in ('varchar', 'nvarchar', 'char', 'text') and TABLE_NAME='" + TableName + "' order by COLUMN_NAME", sqlConn);

            SqlDataAdapter adp = new SqlDataAdapter();
            DbDataReader rs;
            try
            {
                sqlConn.Open();
                cmd.CommandType = CommandType.Text;
                rs = cmd.ExecuteReader();
                dt.Load(rs);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
                log.Error("Error happened trying to read table column information");
                IRunSvc.LogEnd(true);
                //ErrorLog(ex.Message.ToString());
            }
            finally
            {
                sqlConn.Close();
                cmd.Dispose();
                sqlConn.Dispose();
            }

            return dt;
        }

      
    }

}
