using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMWTest
{
    public class Logger
    {
        public static void Log(string message)
        {
            string filePath = "bmwTestLogFile.txt";//this should really be handled by a framework like log4net
            using (StreamWriter sw = new StreamWriter(filePath,true))
            {
                sw.WriteLine(string.Format("{0}\t\t -- {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), message));
            }
        }
    }
}
