using System.Data.Common;
using Microsoft.Data.SqlClient;
using SQLDecorator.Composer;

namespace SQLDecorator.Providers
{
  
    public abstract class MsSqlConnectionManager  : DbConnectionManager
    {
        public MsSqlConnectionManager(string ConnectionString , bool IsLog = false) :base(ConnectionString, IsLog) 
        {         
            runner = Resolver<DbProviderRunner>.Resolve(DBProvider.MsSql.ToString());
            DbConnection= runner.CreateDbConnection(ConnectionString);
        }              
        protected abstract void Ver1();
    }

  
}
