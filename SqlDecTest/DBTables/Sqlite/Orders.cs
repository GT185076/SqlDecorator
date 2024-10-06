using DBTables.MsSql;
using SQLDecorator;
using SQLDecorator.Providers;
using System.Linq;

namespace DBTables.Sqlite
{
    public class Orders : DBTable
    {
        [ColumnDBName("OrderID")]
        public IntegerColumn OrderID;

        [ColumnDBName("CustomerID")]
        public IntegerColumn CustomerID;

        [ColumnDBName("OrderDate")]
        public DateTimeColumn OrderDate;

        [ColumnDBName("ShipperId")]
        public IntegerColumn ShipperId;
        
        public IntegerColumn numbersOflines = new IntegerColumn("NumberOfLines", 
                                             "(select count (*) from \"OrderDetails\" where \"OrderDetails\".\"OrderId\" = \"Orders_0\".\"OrderId\" )");

        public Orders() : base("Orders")
        {          
        }

        public override TableColumn[] GetPrimaryKey()
        {
            return  new TableColumn[] { OrderID };
        }
        
        public static Orders GetById(DbConnectionManager DbconnectionManager, int OrderId)
        {
            return GetById<Orders>(DbconnectionManager, OrderId);
        }
    }
}
