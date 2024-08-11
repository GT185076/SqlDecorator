using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data.Common;
using Npgsql;
using Microsoft.Data.SqlClient;

namespace SQLDecorator.Composer
{
    public class PostGressSelectRunner : DbProviderRunner
    {
        public IEnumerable<ResultRecord> Run(Select statment, DbConnection Dbconnection, List<SqlParameter> parameters)
        {
            using (var cmd = new NpgsqlCommand(statment.ToString(), Dbconnection as NpgsqlConnection))
            {
                if (parameters != null && parameters.Count > 0)
                    cmd.Parameters.AddRange(parameters.ToArray());
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var record = statment.FeatchNextRecord(reader);
                        yield return record;
                    }
                }
            }
        }

        public async Task<IEnumerable<ResultRecord>> RunAsync(Select statment, DbConnection Dbconnection, List<SqlParameter> parameters )
        {
            await using (var cmd = new NpgsqlCommand(statment.ToString(), Dbconnection as NpgsqlConnection))
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
