using DBTables.Sqlite;
using Microsoft.Data.Sqlite;
using Microsoft.Identity.Client;
using Microsoft.SqlServer.Server;
using SQLDecorator;
using System;
using System.Collections.Generic;
using SQLDecorator.Providers;


namespace NorthWind.Views
{
   
    public class Orders
    {              
        public IEnumerable<View1> GetOrders()
        {
            var View1 = new DBTables.Sqlite.View1();
            var selectOrder = new Select(DbConnectionManager.Connections["NorthWindOnMemeory"]).TableAdd(View1, null, ColumnsSelection.All);
            var viewRecord = selectOrder.Run().Export<DBTables.Sqlite.View1>();
            return viewRecord;
        }
    }
}
