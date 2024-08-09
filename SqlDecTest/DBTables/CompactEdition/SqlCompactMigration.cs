using System;
using System.IO;
using System.Text;
using System.Data.SqlServerCe;
using static System.Net.WebRequestMethods;

namespace DBTables.CompactEdition
{
    internal class SqlCompactMigration
    {
        private static SqlCompactMigration _SqlCeMigration;
        private string connectionString;
        public SqlCeConnection sqlConnection { get; private set; }
        private SqlCeDataReader reader;
        private bool isLog;
        private string dbFile;

        private SqlCompactMigration(string dbFile, bool isLog)
        {
            this.dbFile = dbFile;
            this.isLog = isLog;
            connectionString = checkAndCreateDb(dbFile);
            sqlConnection = new SqlCeConnection(connectionString);
        }

        public static SqlCompactMigration Create(string DbFile, bool IsLog = false)
        {
            if (string.IsNullOrWhiteSpace(DbFile))
                DbFile = "netcore-sqlce.sdf";

            if (_SqlCeMigration == null)
            {
                _SqlCeMigration = new SqlCompactMigration(DbFile, IsLog);
                _SqlCeMigration.Ver1();
            }
            return _SqlCeMigration;
        }

        string checkAndCreateDb(string dbFile)
        {
            string connectionString = $"Data Source={dbFile}";

            if (System.IO.File.Exists(dbFile))
                return connectionString;
            else
            {
                using (SqlCeEngine engine = new SqlCeEngine())
                {
                    engine.CreateDatabase();
                }
                return connectionString;
            }
        }

        private void Ver1()
        {
            var check = "select count(*) from sysobjects where id = object_id('dbo.orders')";
            var exists = RunSql(check, false).Trim();
            if (exists == "1") return;

            var NorthWindSeedFile = "MsssqlDBTables\\NorthWind.sql";
            var lines = System.IO.File.ReadAllLines(NorthWindSeedFile);
            var statement = new StringBuilder();

            foreach (var line in lines)
            {
                if (line.Trim().ToUpper() == "GO")
                {
                    if (statement.Length > 0) RunSql(statement.ToString(), isLog);
                    continue;
                }
                statement.Append(line);
            }
        }

        private string RunSql(string statment, bool IsLog)
        {
            if (IsLog)
            {
                Console.WriteLine("Run Sql:");
                Console.WriteLine(statment);
            }

            var sf = new StringBuilder();

            using (SqlCeCommand command = new SqlCeCommand(statment, sqlConnection))
            {
                reader = command.ExecuteReader();
                while (reader.Read())
                    if (reader.FieldCount > 0)
                        sf.Append(reader[0].ToString());
            }

            reader.Close();

            if (IsLog)
            {
                Console.WriteLine("");
                Console.WriteLine(sf.ToString());
            }

            return sf.ToString();

        }
    }
}
