using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace DuckRowNet.Helpers
{
    public class Logger
    {

        public static void LogWarning(string title, string command, string paramaters, string warning, Exception ex = null)
        {
            var stackTrace = "";
            if(ex != null)
            {
                stackTrace = ex.StackTrace;
            }

            List<String> paramList = new List<string>();
            paramList.Add(title);
            paramList.Add(command);
            paramList.Add(paramaters);
            paramList.Add(warning + " -- " + stackTrace);
            paramList.Add(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            DAL db = new DAL();
            db.logWarning(paramList);
            Email.sendError("<b>Title:</b> <br/>" + title + "<br/><br/><b>Command:</b> <br/>" + command + "<br/><br/><b>Parameters:</b><br/> " + paramaters + "<br/><br/><b>Warning:</b><br/> " + warning + "<br/><br/><b>Stack:</b><br/>" + stackTrace);

        }

        public static void LogTime(string title)
        {
            string lines = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " - " + title;

            // Write the string to a file.
            using (StreamWriter file = File.AppendText(HttpContext.Current.Server.MapPath("~/time.txt")))
            {
                file.WriteLine(lines);
            }

            //System.IO.StreamWriter file = new System.IO.StreamWriter(Server.MapPath("~/time.txt"));
            //file..WriteLine(lines);

            //file.Close();   
        }

    }
}