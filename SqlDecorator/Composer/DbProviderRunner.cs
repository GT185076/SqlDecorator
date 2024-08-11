using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

namespace SQLDecorator.Composer
{
    public interface DbProviderRunner
    {
        public IEnumerable<ResultRecord> Run(Select statment, DbConnection Dbconnection, List<SqlParameter> parameters);
        public Task<IEnumerable<ResultRecord>> RunAsync(Select statment, DbConnection Dbconnection, List<SqlParameter> parameters);
    }
}