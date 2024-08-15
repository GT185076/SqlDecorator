using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using Microsoft.Data.SqlClient;
using Npgsql;

namespace SQLDecorator.Composer
{
    public abstract class DbConnectionManager
    {
        public    DbConnection DbConnection { get; protected set; }
        protected DbDataReader reader;
        protected bool isLog;
        protected List<Action> migrationActions = new List<Action>();
        internal  DbProviderRunner runner { get; set;}

        static    DbConnectionManager() 
        {
        Installer.Init();
        }
        protected DbConnectionManager(string ConnectionString , bool IsLog = false)
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

    public abstract class MsSqlConnectionManager  : DbConnectionManager
    {
        public MsSqlConnectionManager(string ConnectionString , bool IsLog = false) :base(ConnectionString, IsLog) 
        {
            string ImpKey = DbConnection.GetType().Name;
            runner = Resolver<DbProviderRunner>.Resolve(ImpKey);
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
        protected abstract void Ver1();
    }

    public abstract class PostGressConnectionManager : DbConnectionManager
    {
        public PostGressConnectionManager(string ConnectionString, bool IsLog = false) : base(ConnectionString, IsLog)
        {
            string ImpKey = DbConnection.GetType().Name;
            runner = Resolver<DbProviderRunner>.Resolve(ImpKey);
        }
        protected override void CreateDbConnection(string ConnectionString)
        {
            DbConnection = new NpgsqlConnection(ConnectionString);
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

            using (NpgsqlCommand command = new NpgsqlCommand(statment, DbConnection as NpgsqlConnection))
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
        protected abstract void Ver1();
    }
}
