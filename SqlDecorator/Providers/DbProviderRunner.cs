using SQLDecorator.Providers.Implementations;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

namespace SQLDecorator.Providers
{
    public interface DbProviderRunner
    {
        public IProviderSyntax Syntax { get; set; }
        public DbParameter CreateParameter(string name,object value);
        public DbConnection CreateDbConnection(string ConnectionString);
        public string RunDMLSql(string sql, DbConnection DbConnection,bool IsLog= false);
        public IEnumerable<ResultRecord> Run(Select statment, DbConnection Dbconnection, List<DbParameter> parameters);
        public Task<IEnumerable<ResultRecord>> RunAsync(Select statment, DbConnection Dbconnection, List<DbParameter> parameters);
    }
}