using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;
using System.ComponentModel;

namespace CommonInfra.Serialization
{
    public class keyValRecord
    {
        public string Key = "";
        public string Tag = "";
        public string Value = "";
    }

    public class dataTable : Xml2Class<dataTable>
    {
        public List<keyValRecord> Records = new List<keyValRecord>();        
    }

    public abstract class XmlDb<RecordType> : IDisposable
    {
        protected static string voidTag = "Void";
        protected bool valueCanged = false;
        protected dataTable Data;
        public XmlDb(string fileName)
        {
            Data = dataTable.Create(fileName);
        }
        protected abstract string GetPrimaryKey();

        ~XmlDb()
        {
            Save();
        }

        public RecordType getValue(string key)
        {
            string answer = "";
            foreach (var r in Data.Records)
            {
                if (r.Key == key)
                {
                    answer = r.Value;
                    break;
                }
            }

            if (answer == "")
                return default(RecordType);
            else
                return fromJson(answer);
        }

        public RecordType getLast(string tag = "*")
        {
            var R = Data.Records.LastOrDefault((keyValRecord) => tag == "*" || keyValRecord.Tag == tag);
            if (R==null)  
                return default(RecordType);
            else
                return fromJson(R.Value);
        }

        public List<string> GetAllKeys(string tag="*")
        {
            List<string> keys = new List<string>();
            foreach (var r in Data.Records)
                if (tag == "*" || r.Tag==tag)
                keys.Add(r.Key);
            return keys;
        }

        public List<RecordType> GetAllValues(string tag = "*")
        {
            List<RecordType> values = new List<RecordType>();
            foreach (var r in Data.Records)
                if (tag == "*" || r.Tag == tag)
                    values.Add(fromJson(r.Value));

            return values;
        }
                
        public RecordType setValue(string key,RecordType value, string tag =null)
        {
            bool   done=false;            
            foreach (var r in Data.Records)
            {
                if (r.Key == key)
                {
                    string v= ToJson(value);
                    if (v != r.Value)
                    {
                        r.Value = ToJson(value);
                        valueCanged = true;
                    }
                    if (tag != null && (r.Tag == null || r.Tag != tag))
                    {
                        r.Tag = tag;
                        valueCanged = true;
                    }
                    done = true;
                    break;
                }
            }

            if (!done)
            {
                if (!string.IsNullOrWhiteSpace(key))
                   Data.Records.Add(new keyValRecord() { Key = key, Value = ToJson(value) });
                valueCanged = true;
            }

            return value;
        }

        public bool setTag(string key,string tag )
        {
            bool done = false;
            foreach (var r in Data.Records)
            {
                if (r.Key == key)
                {                    
                    if (tag != null) r.Tag = tag;
                    done = true;
                    valueCanged = true;
                    break;
                }
            }

            return done;
        }

        public string getTag(string key)
        {
            string tag = "N/A";
            foreach (var r in Data.Records)
            {
                if (r.Key == key)
                {
                    tag = r.Tag;
                    break;
                }
            }

            return tag;
        }

        public bool RemoveValue(string key)
        {
            bool done = false;
            foreach (var r in Data.Records)
            {
                if (r.Key == key)
                {
                    Data.Records.Remove(r);
                    valueCanged = true;
                    done = true;
                    break;
                }
            }

           return done;
        }

        public bool RemoveAllValues()
        {
            Data.Roll();
            Data.Records = new List<keyValRecord>();
            valueCanged = true;
            Save();            
            return true;
        }

        public BindingList<RecordType> GetAllRecords()
        {
            BindingList<RecordType> values = new BindingList<RecordType>();
            foreach (var r in Data.Records)
                if (r.Tag != voidTag)
                {
                    var rt = fromJson(r.Value);
                    if (rt != null && 
                        !string.IsNullOrWhiteSpace(r.Key) )
                        values.Add(rt);
                }
            values.ListChanged += updateListBack;
            return values;
        }
        public BindingList<RecordType> GetAllRecords(Predicate<RecordType> match = null)
        {
            BindingList<RecordType> values = new BindingList<RecordType>();
            foreach (var r in Data.Records)
                if (r.Tag != voidTag)
                {
                    var rt = fromJson(r.Value);
                    if (rt != null &&
                        !string.IsNullOrWhiteSpace(r.Key) &&
                        (match == null || match(rt)))
                        values.Add(rt);
                }
            values.ListChanged += updateListBack;
            return values;
        }

        private void updateListBack(object sender, ListChangedEventArgs e)
        {
            UpSert((BindingList<RecordType>)sender);
        }

        public bool UpSert(IList<RecordType> Records)
        {
            try
            {
                var to = typeof(RecordType);
                var pp = to.GetProperty(GetPrimaryKey());
                foreach (var r in Records)
                        {
                            var rid = pp.GetValue(r, null);
                            setValue(rid.ToString(), r);
                        }
                Save();
                return true;
            }
            catch
            { return false; }
        }

        public bool UpSert(RecordType Record)
        {
            try
            {
                setValue(GetPrimaryKeyValue(Record), Record, null);
                return true;
            }
            catch
            { return false; }
        }

        public string GetPrimaryKeyValue(RecordType Record)
        {
            try
            {
                var to = typeof(RecordType);
                var pp = to.GetProperty(GetPrimaryKey());
                var rid = pp.GetValue(Record, null);
                return rid.ToString();
            }
            catch
            { return string.Empty; }
        }

        public bool Delete(RecordType Record)
        {
            try
            {
                setValue(GetPrimaryKeyValue(Record),Record, voidTag);
                return true;
            }
            catch
            { return false; }
        }

        public void Save()
        {
            if (valueCanged)
            {
                valueCanged = false;
                Data.Save();
            }           
        }

        public void RollAndSave()
        {
            if (valueCanged)
            {
                valueCanged = false;
                Data.Roll();
                Data.Save();
            }
        }

        public void Dispose()
        {
            Save();
        }

        public void View()
        {
            System.Diagnostics.Process.Start("notepad.exe",Data.XmlFileName).WaitForExit();
        }

        static public string ToJson(RecordType o)
        {
            string json = new JavaScriptSerializer().Serialize(o);
            return json;
        }

        static public RecordType fromJson(string Json)
        {
            RecordType Rt = default(RecordType);

            if (Json != null && Json.Trim() != "")
            {
                var ds = new JavaScriptSerializer();

                try
                {
                    Rt = (RecordType)ds.Deserialize(Json, typeof(RecordType));
                }
                catch
                {

                }
            }
            return Rt;
        }    
       
    }

}