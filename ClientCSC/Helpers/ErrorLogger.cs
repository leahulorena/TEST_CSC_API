using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientCSC
{
    public interface IErrorLogger
    {
        void LogError(Exception ex, string infoMessage);
    }

    public class ErrorLogger : IErrorLogger
    {
        public void LogError(Exception ex, string infoMessage)
        {
            string absolutePath = @"Logs/";
            string logFileName = "CSC_APP_" + DateTime.Now.Day + "." + DateTime.Now.Month + "." + DateTime.Now.Year + ".txt";

            string path = absolutePath + logFileName;

            if (!File.Exists(path))
            {
                string message = (DateTime.Now + " " + (ex.Message ==null?"":ex.Message) + " " + (ex.InnerException==null?"":ex.InnerException.Message) + " " + infoMessage);
                using (var streamWriter = File.Create(path))
                {
                    byte[] info = new UTF8Encoding(true).GetBytes(message);
                    streamWriter.Write(info, 0, info.Length);
                }
                
                //using (var streamWriter = new StreamWriter(path))
                //{
                //    streamWriter.WriteLine(DateTime.Now + " " + ex.Message + " " + ex.InnerException.Message + " " + infoMessage);
                //}
            }
            else if (File.Exists(path))
            {

                using (var streamWriter = new StreamWriter(path, true))
                {
                    streamWriter.WriteLine(DateTime.Now + " " + ex.Message + " " + infoMessage);
                }
            }
        }


    }

}
