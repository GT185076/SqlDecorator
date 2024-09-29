using Microsoft.Data.Sqlite;
using SQLDecorator.Composer;
using SQLDecorator.Providers;
using SQLDecorator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NorthWind
{
    public static class ConnectionsInstaller
    {
        public static bool IsStart { get; internal set; }
        static ConnectionsInstaller()
        {
            Init();
            IsStart = true;
        }
        public static void Init()
        {
            SetNw2DbConnection();
        }
        public static void SetNw2DbConnection()
        {
            SqliteConnectionStringBuilder builder = new SqliteConnectionStringBuilder();
            builder.DataSource = "Nortwind_Sqlight.db";
            builder.DataSource = ":memory:";
            new DBTables.Sqlite.NorthWind2("NorthWindOnMemeory",builder.ConnectionString);
        }
    }
    
}
