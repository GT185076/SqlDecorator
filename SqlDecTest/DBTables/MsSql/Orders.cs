using SQLDecorator;

namespace DBTables.MsSql
{
    public class Orders : DBTable
    {
        [ColumnName("OrderID")]
        public IntegerColumn OrderID;

        [ColumnName("CustomerID")]
        public IntegerColumn CustomerID;

        [ColumnName("OrderDate")]
        public DateTimeColumn OrderDate;

        [ColumnName("ShipName")]
        public StringColumn ShipName;
        
        public IntegerColumn numbersOflines = new IntegerColumn("NumberOfLines", 
                                             "(select count (*) from \"Order Details\" where \"Order Details\".\"OrderId\" = \"Orders_0\".\"OrderId\" )");

        public Orders() : base("Orders", "dbo")
        {
            SetPrimaryKey(OrderID);
        }
    }
}
