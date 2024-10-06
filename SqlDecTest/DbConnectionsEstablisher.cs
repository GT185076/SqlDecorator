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
    public static class DbConnectionsEstablisher
    {
        static DbConnectionsEstablisher()
        {        
            SetNw2DbConnection();
        }
        static void SetNw2DbConnection()
        {
            SqliteConnectionStringBuilder builder = new SqliteConnectionStringBuilder();
            builder.DataSource = "Nortwind_Sqlight.db";
            builder.DataSource = ":memory:";
            new DBTables.Sqlite.NorthWind2("NorthWindOnMemeory",builder.ConnectionString);
        }

        static public DbConnectionManager GetNw2()
        {
            return DbConnectionManager.Connections["NorthWindOnMemeory"];
        }
    }
    
}
