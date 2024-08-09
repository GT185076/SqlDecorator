using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace SQLDecorator.Composer
{
    public class MsssqlSelectRunner : DbProviderRunner
    {
        public IEnumerable<ResultRecord> Run(Select statment, DbConnection Dbconnection)
        {
            using (SqlCommand command = new SqlCommand(statment.ToString(), Dbconnection as SqlConnection))
            {
                using (SqlDataReader reader = command.ExecuteReader())
                    while (reader.Read())
                    {
                        var record = statment.FeatchNextRecord(reader);
                        yield return record;
                    }
            }
        }
        public async Task<IEnumerable<ResultRecord>> RunAsync(Select statment, DbConnection DbConnection)
        {
            await using (var cmd = new SqlCommand(statment.ToString(), DbConnection as SqlConnection))
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var record = statment.FeatchNextRecord(reader);
                }
            }
            return statment.Result;
        }
    }
}
