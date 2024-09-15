using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Globalization;

namespace CommonInfra.Serialization
{
    public interface iXml2Class
    {
        string XmlFileName { get; set; }
        bool   FileExist { get; set; }
    }

    public abstract class Xml2Class<T> : iXml2Class where T : iXml2Class, new()
    {
        [XmlIgnore]
        public string XmlFileName { get; set; }
        [XmlIgnore]
        public bool FileExist { get; set; }
        public static T Create(string FileFullName)
        {

            T myObject = new T();

            if (!FileFullName.Contains('\\'))
            {
                var Directory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                FileFullName = Path.Combine(Directory, FileFullName);
            }

            myObject.XmlFileName = FileFullName;

            if (File.Exists(FileFullName))
            {
                myObject.FileExist = true;
                try
                {
                    XmlSerializer mySerializer = new XmlSerializer(typeof(T));
                    using (FileStream myFileStream = new FileStream(FileFullName, FileMode.Open))
                    {
                        myObject = (T)mySerializer.Deserialize(myFileStream);
                        myObject.XmlFileName = FileFullName;
                        myObject.FileExist = true;
                        myFileStream.Close();
                    }
                }
                catch (Exception e)
                {
                    e.ToString();
                }
            }
            return myObject;
        }
        public bool Save()
        {
            Type myType = this.GetType();
            try
            {
                XmlSerializer mySerializer = new XmlSerializer(myType);
                using (FileStream myFileStream = new FileStream(XmlFileName, FileMode.Create))
                {
                    mySerializer.Serialize(myFileStream, this);
                    myFileStream.Close();
                }
                return true;
            }
            catch  
            {
                return false;
            }

        }
        public static T CreateFromXml(string Xml)
        {
            T tr = default(T);
            try
            {
                XmlSerializer mySerializer = new XmlSerializer(typeof(T));
                using (MemoryStream S = new MemoryStream(Encoding.UTF8.GetBytes(Xml)))
                {
                    tr = (T)mySerializer.Deserialize(S);
                }
            }
            catch
            {

            }

            return tr;
        }
        public bool Roll()
        {
            try
            {
                if (File.Exists(XmlFileName + ".9")) File.Delete(XmlFileName + ".9");
                if (File.Exists(XmlFileName + ".8")) File.Move(XmlFileName + ".8", XmlFileName + ".9");
                if (File.Exists(XmlFileName + ".7")) File.Move(XmlFileName + ".7", XmlFileName + ".8");
                if (File.Exists(XmlFileName + ".6")) File.Move(XmlFileName + ".6", XmlFileName + ".7");
                if (File.Exists(XmlFileName + ".5")) File.Move(XmlFileName + ".5", XmlFileName + ".6");
                if (File.Exists(XmlFileName + ".4")) File.Move(XmlFileName + ".4", XmlFileName + ".5");
                if (File.Exists(XmlFileName + ".3")) File.Move(XmlFileName + ".3", XmlFileName + ".4");
                if (File.Exists(XmlFileName + ".2")) File.Move(XmlFileName + ".2", XmlFileName + ".3");
                if (File.Exists(XmlFileName + ".1")) File.Move(XmlFileName + ".1", XmlFileName + ".2");
                if (File.Exists(XmlFileName))        File.Copy(XmlFileName, XmlFileName + ".1");
                return true;
            }
            catch
            {
                return false;
            }
        }

    }
}
