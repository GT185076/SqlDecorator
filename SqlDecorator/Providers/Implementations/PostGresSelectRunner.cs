using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data.Common;
using Npgsql;
using System.Text;
using System;
using SQLDecorator.Providers.Implementations;

namespace SQLDecorator.Providers
{
    public class PostGresSelectRunner : DbProviderRunner
    {
        public IProviderSyntax Syntax { get; set; } = new PostGresSyntax();

        public DbConnection CreateDbConnection(string ConnectionString)
        {
            var DbConnection = new NpgsqlConnection(ConnectionString);
            DbConnection.Open();
            return DbConnection;
        }
        public IEnumerable<ResultRecord> Run(Select statment, DbConnection Dbconnection, List<DbParameter> parameters)
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
                        if (statment.IsOne)
                            yield break;
                    }
                }
            }
        }
        public async Task<IEnumerable<ResultRecord>> RunAsync(Select statment, DbConnection Dbconnection, List<DbParameter> parameters)
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
                        if (statment.IsOne) break;
                    }
                }
            }
            return statment.Result;
        }
        public string RunDMLSql(string statment, DbConnection DbConnection, bool IsLog = false)
        {
            if (IsLog)
            {
                Console.WriteLine("Run Sql:");
                Console.WriteLine(statment);
            }

            var sf = new StringBuilder();

            using (NpgsqlCommand command = new NpgsqlCommand(statment, DbConnection as NpgsqlConnection))
            {
                using (NpgsqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                        if (reader.FieldCount > 0)
                            sf.Append(reader[0].ToString());
                }
            }

            if (IsLog)
            {
                Console.WriteLine("");
                Console.WriteLine(sf.ToString());
            }

            return sf.ToString();
        }

        DbConnection DbProviderRunner.CreateDbConnection(string ConnectionString)
        {
            throw new NotImplementedException();
        }

        DbParameter DbProviderRunner.CreateParameter(string name, object value)
        {
            return new NpgsqlParameter(name, value);
        }

        IEnumerable<ResultRecord> DbProviderRunner.Run(Select statment, DbConnection Dbconnection, List<DbParameter> parameters)
        {
            throw new NotImplementedException();
        }

        Task<IEnumerable<ResultRecord>> DbProviderRunner.RunAsync(Select statment, DbConnection Dbconnection, List<DbParameter> parameters)
        {
            throw new NotImplementedException();
        }

        string DbProviderRunner.RunDMLSql(string sql, DbConnection DbConnection, bool IsLog)
        {
            throw new NotImplementedException();
        }
    }
}
