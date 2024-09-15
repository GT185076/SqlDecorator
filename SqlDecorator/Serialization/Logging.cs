using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CommonInfra.Serialization
{
    public enum LogLevel 
    {
      FatalError,
      Error,
      Warning,
      HighInfo,
      MedInfo,
      LowInfo
    }
    public class Logging : IDisposable
    {
        static protected readonly string Loglock = "Loglock";

        public string fullFileName;
        protected FileStream fs;
        protected StreamWriter writer;

        public Logging(string filename)
        {
            var Directory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            if (filename.IsFullFileName())
                fullFileName = filename;
            else
                fullFileName = Path.Combine(Directory, filename);

            var fileinfo = new System.IO.FileInfo(fullFileName);
            if (fileinfo.Exists && fileinfo.Length > 10000000)
            {
                try
                {
                    if (File.Exists(fullFileName + ".9")) File.Delete(fullFileName + ".9");
                    if (File.Exists(fullFileName + ".8")) File.Move(fullFileName   + ".8", fullFileName + ".9");
                    if (File.Exists(fullFileName + ".7")) File.Move(fullFileName   + ".7", fullFileName + ".8");
                    if (File.Exists(fullFileName + ".6")) File.Move(fullFileName   + ".6", fullFileName + ".7");
                    if (File.Exists(fullFileName + ".5")) File.Move(fullFileName   + ".5", fullFileName + ".6");
                    if (File.Exists(fullFileName + ".4")) File.Move(fullFileName   + ".4", fullFileName + ".5");
                    if (File.Exists(fullFileName + ".3")) File.Move(fullFileName   + ".3", fullFileName + ".4");
                    if (File.Exists(fullFileName + ".2")) File.Move(fullFileName   + ".2", fullFileName + ".3");
                    if (File.Exists(fullFileName + ".1")) File.Move(fullFileName   + ".1", fullFileName + ".2");
                    if (File.Exists(fullFileName))            File.Move(fullFileName, fullFileName + ".1");
                }
                catch
                { }
            }

                  
            Writeline("Start Session");
                
        }

        public Logging(string path,string filename)
        {
            var Directory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            if (filename.IsFullFileName())
                fullFileName = filename;
            else
            {
                if (string.IsNullOrWhiteSpace(path))  path = Directory;
                fullFileName = Path.Combine(path, filename);
            }

            var fileinfo = new System.IO.FileInfo(fullFileName);
            if (fileinfo.Exists && fileinfo.Length > 10000000)
            {
                try
                {
                    if (File.Exists(fullFileName + ".9")) File.Delete(fullFileName + ".9");
                    if (File.Exists(fullFileName + ".8")) File.Move(fullFileName + ".8", fullFileName + ".9");
                    if (File.Exists(fullFileName + ".7")) File.Move(fullFileName + ".7", fullFileName + ".8");
                    if (File.Exists(fullFileName + ".6")) File.Move(fullFileName + ".6", fullFileName + ".7");
                    if (File.Exists(fullFileName + ".5")) File.Move(fullFileName + ".5", fullFileName + ".6");
                    if (File.Exists(fullFileName + ".4")) File.Move(fullFileName + ".4", fullFileName + ".5");
                    if (File.Exists(fullFileName + ".3")) File.Move(fullFileName + ".3", fullFileName + ".4");
                    if (File.Exists(fullFileName + ".2")) File.Move(fullFileName + ".2", fullFileName + ".3");
                    if (File.Exists(fullFileName + ".1")) File.Move(fullFileName + ".1", fullFileName + ".2");
                    if (File.Exists(fullFileName)) File.Move(fullFileName, fullFileName + ".1");

                }
                catch
                { }
            }

            Writeline("Start Session");
        }

        public void Writeline(string message)
        {
            if (_open())
            {
                string timestamp = DateTime.Now.ToString();
                writer.WriteLine(timestamp + ":" + message);
                _close();
            }
        }

        public void Info(string m1,string m2)
        {
            Writeline("Info:"+ string.Format(m1,m2));
        }

        public void Info(string m1)
        {
            Writeline("Info:" + m1 );
        }

        public void Error(Exception ex,string message)
        {
            Writeline("Error:"+message + " " + ex.ToString());
        }

        public void Error(string message)
        {
            Writeline("Error" + message);
        }

        public void Writelines(string Header, string JsonMessage)
        {
            if (_open())
            {
                string timestamp = DateTime.Now.ToString();
                writer.WriteLine(timestamp + ":" + Header);
                var lines = JsonMessage.Split(',');
                foreach (var line in lines)
                {
                    var keyVal = line.Split(':');
                    if (keyVal.Length > 1 && keyVal[1] != "null" && keyVal[1] != "\"\"")
                        writer.WriteLine(" ".PadLeft(11, ' ') + line);
                }
                writer.WriteLine();
                _close();         
            }
        }

        public void Writelines(string Header, string[] Lines)
        {           
            if (_open())
            {
                string timestamp = DateTime.Now.ToString();
                writer.WriteLine(timestamp + ":" + Header);
                foreach (var line in Lines)
                {
                    writer.WriteLine("".PadRight(20) + line);
                }
                writer.WriteLine();
                _close();
            }
           
        }

        public void View()
        {
            Writeline("View File");
            System.Diagnostics.Process.Start("notepad.exe", fullFileName).WaitForExit();
        }

        public void Dispose()
        {
            Close();
        }

        void Close()
        {
            Writeline("End Session");            
        }

        bool _open()
        {
            
            bool ready = false;
            lock(Loglock)
            try
            {
                fs = new FileStream(fullFileName, FileMode.Append, FileAccess.Write);
                writer = new StreamWriter(fs);
                ready = true;
            }
            catch 
            {
                
            }

            return ready;

        }

        void _close()
        {
            try
            {
                if (writer != null)
                {
                    writer.Flush();
                    writer.Close();
                }
                if (fs != null)
                    fs.Close();
            }
            catch
            { }
            writer = null;
            fs = null;            
        }

    }
}

