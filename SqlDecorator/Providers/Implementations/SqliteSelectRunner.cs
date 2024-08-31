using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;

namespace SQLDecorator.Providers
{
    public class SqliteSelectRunner : DbProviderRunner
    {
        public DbConnection CreateDbConnection(string ConnectionString)
        {
            var DbConnection = new SqliteConnection(ConnectionString);
            DbConnection.Open();
            return DbConnection;
        }
        public IEnumerable<ResultRecord> Run(Select statment, DbConnection Dbconnection, List<DbParameter> parameters)
        {
            using (SqliteCommand command = new SqliteCommand(statment.ToString(), Dbconnection as SqliteConnection))
            {
                if (parameters != null && parameters.Count>0) 
                    foreach( var parameter in parameters) 
                    {
                        command.Parameters.Add(parameter as SqliteParameter);
                    }

                using (var reader = command.ExecuteReader())
                    while (reader.Read())
                    {
                        var record = statment.FeatchNextRecord(reader);
                        yield return record;
                    }
            }
        }
        public async Task<IEnumerable<ResultRecord>> RunAsync(Select statment, DbConnection DbConnection, List<DbParameter> parameters)
        {           
            await using (var cmd = new SqliteCommand(statment.ToString(), DbConnection as SqliteConnection))
            {
                if (parameters != null && parameters.Count > 0)
                    foreach (var parameter in parameters)
                    {
                        cmd.Parameters.Add(parameter as SqliteParameter);
                    }

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
        public string RunDMLSql(string statment, DbConnection DbConnection ,bool IsLog=false)
        {
            StringBuilder sf = new StringBuilder();

            if (IsLog)
            {
                Console.WriteLine("Run Sql:");
                Console.WriteLine(statment);
            }

            var command = DbConnection.CreateCommand();
            command.CommandText = statment;

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    sf.Append(reader.GetString(0));
                }
            }

            if (IsLog)
            {
                Console.WriteLine("");
                Console.WriteLine(sf.ToString());
            }

            return sf.ToString();
        }
        public DbParameter CreateParameter(string name, object value)
        {
            return new SqliteParameter(name, value);
        }
    }
}
