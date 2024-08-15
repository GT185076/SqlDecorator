using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Text;
using SQLDecorator.Composer;

namespace DBTables.MsSql
{
    public class NorthWind : MsSqlConnectionManager
    {
        public NorthWind(string ConnectionString, bool IsLog = false) : base(ConnectionString, IsLog)
        {
            migrationActions.Add(Ver1);
            RunMigrationList();
        }
        protected override void Ver1()
        {
            var check = "select count(*) from sysobjects where id = object_id('dbo.orders')";
            var exists = RunSql(check).Trim();
            if (exists == "1") return;

            var NorthWindSeedFile = "DBTables\\Mssql\\NorthWind.sql";
            var lines = File.ReadAllLines(NorthWindSeedFile);
            var statement = new StringBuilder();

            foreach (var line in lines)
            {
                if (line.Trim().ToUpper() == "GO")
                {
                    if (statement.Length > 0) RunSql(statement.ToString());
                    statement = new StringBuilder();
                    continue;
                }
                statement.Append(line).Append("\n");
            }
        }
    }
}
