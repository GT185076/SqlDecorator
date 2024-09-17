using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Logging;
using Npgsql;

namespace SQLDecorator.Providers

{
    public class MsssqlSelectRunner : DbProviderRunner
    {
        public DbConnection CreateDbConnection(string ConnectionString)
        {
            var DbConnection = new SqlConnection(ConnectionString);
            DbConnection.Open();
            return DbConnection;
        }
        public IEnumerable<ResultRecord> Run(Select statment, DbConnection Dbconnection, List<DbParameter> parameters)
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
                        if (statment.IsOne) 
                            yield break;
                    }
            }
        }
        public async Task<IEnumerable<ResultRecord>> RunAsync(Select statment, DbConnection DbConnection, List<DbParameter> parameters)
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
                        if (statment.IsOne) break;
                    }
                }
            }
            return statment.Result;
        }       
        public string RunDMLSql(string statment, DbConnection DbConnection ,bool IsLog=false)
        {
            StringBuilder sf = new StringBuilder();

            if (IsLog)
            {
                Console.WriteLine("Run Sql:");
                Console.WriteLine(statment);
            }

            using (SqlCommand command = new SqlCommand(statment, DbConnection as SqlConnection))
            {
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader.FieldCount > 0)
                            sf.Append(reader[0].ToString());                       
                    }
                }
            }
                
            
            if (IsLog)
            {
                Console.WriteLine("");
                Console.WriteLine(sf.ToString());
            }

            return sf.ToString();
        }
        DbParameter DbProviderRunner.CreateParameter(string name, object value)
        {
           return new SqlParameter(name, value);
        }
    }
}
