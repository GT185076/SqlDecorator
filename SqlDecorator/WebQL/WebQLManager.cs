using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

namespace SQLDecorator.WebQL
{
    public abstract class WebQLManager
    {
        public static Dictionary<string,WebQLManager> WebQLDirectory { get; set; } = new Dictionary<string,WebQLManager>();
        protected string InstanceName { get; set; }
        protected string DBConnectionName { get; set; }
        protected WebQLManager(string InstanceName,string DbCOnnectionName) 
        {
           this.DBConnectionName = DbCOnnectionName;
           this.InstanceName= InstanceName;
           WebQLManager.WebQLDirectory.Add(InstanceName, this);
        }
        
        public abstract string GetMeny(string[] ColumnsNames);
        public abstract string Get(string Id, string[] ColumnsNames);

    }
}