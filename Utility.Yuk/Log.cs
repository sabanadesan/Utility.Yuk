using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using Serilog;
using Serilog.Core;

namespace Utility.Yuk
{
    public class Log
    {
        private Logger log1;
        private Logger log2;

        private String path = "logs";

        private static Log _instance;

        private Log(String fileName = "log.txt")
        {

            var filePath = Path.Combine(path, fileName);

            log1 = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();

            log2 = new LoggerConfiguration()
                .WriteTo.File(filePath)
                .CreateLogger();
        }

        public void WriteToConsole(String msg)
        {
            log1.Information(msg);
        }

        public void WriteToFile(String msg)
        {
            log2.Information(msg);
        }


        public static Log Instance
        {
            get
            {
                if (_instance == null)
                {
                    String name = String.Format("{0:yyyy_MM_dd}_{1}", DateTime.Now, "log.txt");
                    _instance = new Log(name);
                }
                return _instance;
            }
        }
    }
}
