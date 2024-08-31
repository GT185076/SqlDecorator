using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Data.Common;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace SQLDecorator.Providers
{
    public interface DbProviderRunner
    {
        public DbConnection CreateDbConnection(string ConnectionString);
        public string RunDMLSql(string sql, DbConnection DbConnection,bool IsLog= false);
        public IEnumerable<ResultRecord> Run(Select statment, DbConnection Dbconnection, List<SqlParameter> parameters);
        public Task<IEnumerable<ResultRecord>> RunAsync(Select statment, DbConnection Dbconnection, List<SqlParameter> parameters);
    }
}