using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace SQLDecorator.Composer
{
    public class MsssqlSelectRunner : DbProviderRunner
    {
        public IEnumerable<ResultRecord> Run(Select statment, DbConnection Dbconnection, List<SqlParameter> parameters)
        {
            using (SqlCommand command = new SqlCommand(statment.ToString(), Dbconnection as SqlConnection))
            {
                if (parameters != null && parameters.Count>0) 
                command.Parameters.AddRange(parameters.ToArray());

                using (SqlDataReader reader = command.ExecuteReader())
                    while (reader.Read())
                    {
                        var record = statment.FeatchNextRecord(reader);
                        yield return record;
                    }
            }
        }
        public async Task<IEnumerable<ResultRecord>> RunAsync(Select statment, DbConnection DbConnection, List<SqlParameter> parameters)
        {
            await using (var cmd = new SqlCommand(statment.ToString(), DbConnection as SqlConnection))
            {
                if (parameters != null && parameters.Count > 0)
                    cmd.Parameters.AddRange(parameters.ToArray());
                await using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var record = statment.FeatchNextRecord(reader);
                    }
                }
            }
            return statment.Result;
        }
    }
}
