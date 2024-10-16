using SQLDecorator.Providers;
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
        protected DbConnectionManager ConnectionManager { get; set; }      
        protected string DBConnectionString { get; set; }
        protected WebQLManager(string InstanceName,string DbCOnnectionName) 
        {          
           this.InstanceName= InstanceName;
           WebQLManager.WebQLDirectory.Add(InstanceName, this);
           this.DBConnectionName = DbCOnnectionName;
           ConnectionManager = DbConnectionManager.Connections[DBConnectionName];
        }
        protected Select SelectBuild<T>(string[] ColumnsNames, WebQLCondition[] conditions) where T : DBTable, new()
        {
            Select select = new Select(ConnectionManager);

            var tableInstance = new T();

            if (ColumnsNames != null && ColumnsNames.Length > 0)
                select.TableAdd(tableInstance, null, ColumnsNames);
            else
                select.TableAdd(tableInstance, null, ColumnsSelection.All);

            if (conditions != null && conditions.Length > 0)
                select.Where(conditions);

            return select;
        }
        public abstract string GetMeny(string[] ColumnsNames, WebQLCondition[] conditions);        
        public abstract string Get(string Id, string[] ColumnsNames);

    }
}