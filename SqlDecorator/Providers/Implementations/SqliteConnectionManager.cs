using System.Data.Common;
using Microsoft.Data.SqlClient;
using SQLDecorator.Composer;

namespace SQLDecorator.Providers
{
  
    public abstract class SqliteConnectionManager  : DbConnectionManager
    {
        public SqliteConnectionManager(string Alias,string ConnectionString , bool IsLog = false) :base(Alias,ConnectionString, IsLog) 
        {         
            runner = Resolver<DbProviderRunner>.Resolve(DBProvider.SqlLite.ToString());
            DbConnection= runner.CreateDbConnection(ConnectionString);
        }              
        protected abstract void Ver1();
    }

  
}
