using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Npgsql;

namespace SQLDecorator
{
    public class MsssqlSelectRunner : DbProviderRunner
    {
        public IEnumerable<ResultRecord> Run(Select statment, DbConnection Dbconnection)
        {          
            using (SqlCommand command = new SqlCommand(statment.ToString(),Dbconnection as SqlConnection))
            {
                using (SqlDataReader reader = command.ExecuteReader())                
                while (reader.Read())
                {
                    var record = statment.FeatchNextRecord(reader);
                    yield return record;
                }             
            }            
        }
        public async Task<IEnumerable<ResultRecord>> RunAsync(Select statment, NpgsqlConnection connectionString)
        {
            await using (var cmd = new NpgsqlCommand(statment.ToString(), connectionString))
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
