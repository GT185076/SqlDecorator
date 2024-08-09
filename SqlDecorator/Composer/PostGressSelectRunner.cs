using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data.Common;
using Npgsql;

namespace SQLDecorator.Composer
{
    public class PostGressSelectRunner : DbProviderRunner
    {
        public IEnumerable<ResultRecord> Run(Select statment, DbConnection Dbconnection)
        {
            using (var cmd = new NpgsqlCommand(statment.ToString(), Dbconnection as NpgsqlConnection))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var record = statment.FeatchNextRecord(reader);
                    yield return record;
                }
            }
        }

        public async Task<IEnumerable<ResultRecord>> RunAsync(Select statment, DbConnection Dbconnection)
        {
            await using (var cmd = new NpgsqlCommand(statment.ToString(), Dbconnection as NpgsqlConnection))
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
