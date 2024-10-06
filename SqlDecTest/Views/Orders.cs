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
        public IEnumerable<OrderView> GetOrders(string[] ColumnsNames)
        {
            var View1 = new DBTables.Sqlite.OrderView();
            var selectOrder = new Select(DbConnectionManager.Connections["NorthWindOnMemeory"]).TableAdd(View1, null,ColumnsNames);
            var viewRecord = selectOrder.Run().Export<DBTables.Sqlite.OrderView>();
            return viewRecord;
        }
    }
}
