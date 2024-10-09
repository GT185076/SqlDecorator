using SQLDecorator;
using System.Collections.Generic;
using SQLDecorator.Providers;
using SQLDecorator.WebQL;
using System;

namespace NorthWind.Views
{
  
    public class OrdersQL : WebQLManager
    {
        
        public OrdersQL(string InstanceName,string DBConnectionName) : base(InstanceName,DBConnectionName)
        {
        }

        public override string Get(string Id,string[] ColumnsNames)
        {
            Select selectOrder;
            var DBConnection = DbConnectionManager.Connections[DBConnectionName];
            var OrderView1 = new DBTables.Sqlite.OrderView();

            if (ColumnsNames != null && ColumnsNames.Length > 0)
                selectOrder = new Select(DBConnection).TableAdd(OrderView1, null, ColumnsNames);
            else
                selectOrder = new Select(DBConnection).TableAdd(OrderView1, null, ColumnsSelection.All);

            int orderid;
            if (int.TryParse(Id, out orderid))
            {
                selectOrder.Where(OrderView1.OrderID.Equal(orderid));
                selectOrder.One();
                var viewRecord = selectOrder.Run().Export<DBTables.Sqlite.OrderView>();
                return viewRecord.ToJson(true);
            }
            else
                throw new Exception("Item Id Must be a number");
        }

        public override string GetMeny(string[] ColumnsNames)
        {
            Select selectOrder;
            var DBConnection = DbConnectionManager.Connections[DBConnectionName];
            var OrderView1 = new DBTables.Sqlite.OrderView();

            if (ColumnsNames != null && ColumnsNames.Length > 0)
                selectOrder = new Select(DBConnection).TableAdd(OrderView1, null, ColumnsNames);
            else
                selectOrder = new Select(DBConnection).TableAdd(OrderView1, null, ColumnsSelection.All);

            var viewRecord = selectOrder.Run().Export<DBTables.Sqlite.OrderView>();

            return viewRecord.ToJson();
        }
    }
}
