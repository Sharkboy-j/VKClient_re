using Microsoft.Phone.Info;
using System;
using System.IO;
using System.IO.IsolatedStorage;
using VKClient.Common.Backend.DataObjects;

namespace VKClient.Common.Utils
{
    public class Logger
    {
        private object lockObj = new object();
        private string LOGNAME = "VKLog.txt";
        private static bool IsLoggingToIsolatedStorageEnabled = false;
        private static Logger _logger;

        public static Logger Instance
        {
            get
            {
                if (Logger._logger == null)
                    Logger._logger = new Logger();
                return Logger._logger;
            }
        }

        public void LogMemoryUsage()
        {
            this.Info("Memory usage: {0}", (object)(long)DeviceExtendedProperties.GetValue("ApplicationCurrentMemoryUsage"));
        }

        public void Assert(bool assertion, string commentOnFailure)
        {
            if (assertion)
                return;
            this.Info("ASSERTION FAILED, {0}", (object)commentOnFailure);
        }

        public void Info(string info, params object[] formatParameters)
        {
            string logMsg = info;

            if (formatParameters != null && formatParameters.Length != 0)
                logMsg = string.Format(info, formatParameters);

            //
            Console.WriteLine(logMsg);
            //

            string str = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + ": " + logMsg;
            if (!Logger.IsLoggingToIsolatedStorageEnabled)
                return;
            this.WriteLogToStorage(logMsg);
        }

        public void Error(string error, ResultCode code)
        {
            //if (!Logger.IsLoggingToIsolatedStorageEnabled)
            //  return;
            this.WriteLogToStorage("ERROR:" + error + " ErrorCode: " + code.ToString());
        }

        public void Error(string error)
        {
            //if (!Logger.IsLoggingToIsolatedStorageEnabled)
            //  return;
            this.WriteLogToStorage("ERROR:" + error);
        }

        public void Error(string error, Exception e)
        {
            string exceptionData = this.GetExceptionData(e);
            //if (!Logger.IsLoggingToIsolatedStorageEnabled)
            //  return;
            this.WriteLogToStorage("ERROR: " + error + Environment.NewLine + exceptionData);
        }

        public void ErrorAndSaveToIso(string error, Exception e)
        {
            string exceptionData = this.GetExceptionData(e);
            this.WriteLogToStorage("ERROR: " + error + Environment.NewLine + exceptionData);
        }

        private string GetExceptionData(Exception e)
        {
            string str = "e.Message = " + e.Message + Environment.NewLine + "e.Stack = " + e.StackTrace;
            if (e.InnerException != null)
                return str + Environment.NewLine + this.GetExceptionData(e.InnerException);
            return str;
        }

        private void WriteLogToStorage(string logMsg)
        {
            try
            {
                logMsg = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ": " + logMsg;
                lock (this.lockObj)
                {
                    using (IsolatedStorageFile resource_1 = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        Stream local_4_1;
                        if (!resource_1.FileExists(this.LOGNAME))
                        {
                            local_4_1 = (Stream)resource_1.CreateFile(this.LOGNAME);
                        }
                        else
                        {
                            local_4_1 = (Stream)resource_1.OpenFile(this.LOGNAME, FileMode.OpenOrCreate);
                            local_4_1.Seek(0L, SeekOrigin.End);
                        }
                        using (StreamWriter resource_0 = new StreamWriter(local_4_1))
                        {
                            resource_0.WriteLine(logMsg);
                            resource_0.Flush();
                            resource_0.Close();
                        }
                    }
                }
                System.Diagnostics.Debug.WriteLine(logMsg);
            }
            catch
            {
            }
        }

        private string SanitizeLog(string msg)
        {
            string str = msg.ToLowerInvariant();
            if (str.Contains("body"))
                str = str.Substring(0, str.IndexOf("body"));
            if (str.Contains("password"))
                str = str.Substring(0, str.IndexOf("password"));
            return str;
        }

        public void DeleteLogFromIsolatedStorage()
        {
            using (IsolatedStorageFile storeForApplication = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!storeForApplication.FileExists(this.LOGNAME))
                    return;
                storeForApplication.DeleteFile(this.LOGNAME);
            }
        }
    }
}
