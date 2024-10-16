using SQLDecorator;
using SQLDecorator.WebQL;
using System;
using DBTables.Sqlite;

namespace NorthWind.Views
{
  
    public class OrdersQL : WebQLManager
    {
        
        public OrdersQL(string InstanceName,string DBConnectionName) : base(InstanceName,DBConnectionName)
        {
           
        }

        public override string Get(string Id,string[] ColumnsNames)
        {
            Select selectOrder = new Select(ConnectionManager);

            var OrderView1 = new OrderView();

            if (ColumnsNames != null && ColumnsNames.Length > 0)
                selectOrder.TableAdd(OrderView1, null, ColumnsNames);
            else
                selectOrder.TableAdd(OrderView1, null, ColumnsSelection.All);

            int orderid;
            if (int.TryParse(Id, out orderid))
            {
                selectOrder.Where(OrderView1.OrderID.Equal(orderid));
                selectOrder.One();
                var viewRecord = selectOrder.Run();
                return viewRecord.ToJson(true);
            }
            else
                throw new Exception("Item Id Must be a number");
        }
        public override string GetMeny(string[] ColumnsNames, WebQLCondition[] conditions)
        {
            var select = SelectBuild<OrderView>(ColumnsNames,conditions);
            var viewRecord = select.Run();
            return viewRecord.ToJson();
        }
    }
}
