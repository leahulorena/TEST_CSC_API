using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TEST_CSC_API
{
    public interface IErrorLogger
    {
        void LogError(Exception ex, string infoMessage);
    }

    public class ErrorLogger : IErrorLogger
    {
        public void LogError(Exception ex, string infoMessage)
        {
            string absolutePath = @"..\..\Logs\";
            string logFileName = "CSC_API_" + DateTime.Now.Day + "." + DateTime.Now.Month + "." + DateTime.Now.Year + ".txt";

            string path = absolutePath + logFileName;

            if (!File.Exists(path))
            {
                File.Create(path);
                using (var streamWriter = new StreamWriter(path, true))
                {
                    streamWriter.WriteLine(DateTime.Now + " " + ex.Message + " " + ex.InnerException.Message + " " + infoMessage);
                }
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
