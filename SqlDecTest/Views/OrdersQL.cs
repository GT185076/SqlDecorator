using SQLDecorator;
using System.Collections.Generic;
using SQLDecorator.Providers;
using SQLDecorator.WebQL;


namespace NorthWind.Views
{
  
    public class OrdersQL : WebQLManager
    {
        
        public OrdersQL(string InstanceName,string DBConnectionName) : base(InstanceName,DBConnectionName)
        {
        }

        public override string Get(string[] ColumnsNames)
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
