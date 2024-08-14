using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Text;
using Microsoft.Data.SqlClient;

namespace DBTables.MsSql
{
    public abstract class ConnectionManager
    {
        public DbConnection DbConnection { get; protected set; }
        protected DbDataReader reader;
        protected bool isLog;
        protected List<Action> migrationActions = new List<Action>();
        protected ConnectionManager(string ConnectionString , bool IsLog = false)
        {
            if (ConnectionString == null) throw new ArgumentNullException();    
            else
            {
                isLog = IsLog;
                CreateDbConnection(ConnectionString);              
            }
        }
        protected abstract string RunSql(string sql);
        protected abstract void CreateDbConnection(string ConnectionString);
        protected void RunMigrationList()
        {
            foreach (var action in migrationActions) action.Invoke();
        }
    }

    public class MsSqlConnectionManager  : ConnectionManager
    {
        public MsSqlConnectionManager(string ConnectionString , bool IsLog = false) :base(ConnectionString, IsLog) 
        {         
        }
        protected override void CreateDbConnection(string ConnectionString)
        {
            DbConnection = new SqlConnection(ConnectionString);
            DbConnection.Open();
        }     
        protected override string RunSql(string statment)
        {
            if (isLog)
            {
                Console.WriteLine("Run Sql:");
                Console.WriteLine(statment);
            }

            var sf = new StringBuilder();

            using (SqlCommand command = new SqlCommand(statment, DbConnection as SqlConnection))
            {
                reader = command.ExecuteReader();
                while (reader.Read())
                    if (reader.FieldCount > 0)
                        sf.Append(reader[0].ToString());
            }

            reader.Close();

            if (isLog)
            {
                Console.WriteLine("");
                Console.WriteLine(sf.ToString());
            }

            return sf.ToString();

        }

    }

    public class NorthWind : MsSqlConnectionManager
    {
        public NorthWind( string connectionString, bool IsLog=false) : base(connectionString, IsLog)
        {
            migrationActions.Add(Ver1);
            RunMigrationList();
        }
        private void Ver1()
        {
            var check = "select count(*) from sysobjects where id = object_id('dbo.orders')";
            var exists = RunSql(check).Trim();
            if (exists == "1") return;

            var NorthWindSeedFile = "MsssqlDBTables\\NorthWind.sql";
            var lines = File.ReadAllLines(NorthWindSeedFile);
            var statement = new StringBuilder();

            foreach (var line in lines)
            {
                if (line.Trim().ToUpper() == "GO")
                {
                    if (statement.Length > 0) RunSql(statement.ToString());
                    continue;
                }
                statement.Append(line);
            }
        }
    }
}
