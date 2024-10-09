using Microsoft.Data.Sqlite;
using SQLDecorator.Composer;
using SQLDecorator.Providers;
using SQLDecorator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLDecorator.WebQL;
using NorthWind.Views;

namespace NorthWind
{
    public static class DbConnectionsEstablisher
    {
        static string DbconnectionName;
        static public bool Connect()
        {
            if (DbconnectionName == null)
            {
                DbconnectionName = SetNw2DbConnection();
                new OrdersQL("Orders", DbconnectionName);
            }

            return DbConnectionManager.Connections.Count > 0 &&
                   WebQLManager.WebQLDirectory.Count >0;
        }

        static string SetNw2DbConnection()
        {
            string DBConnectionName = "NorthWindOnMemeory";
            SqliteConnectionStringBuilder builder = new SqliteConnectionStringBuilder();
            builder.DataSource = "Nortwind_Sqlight.db";
            builder.DataSource = ":memory:";
            new DBTables.Sqlite.NorthWind2(DBConnectionName, builder.ConnectionString);
            return DBConnectionName;
        }

    }

}
