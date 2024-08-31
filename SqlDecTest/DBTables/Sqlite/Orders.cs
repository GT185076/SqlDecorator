using SQLDecorator;

namespace DBTables.Sqlite
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
                                             "(select count (*) from \"OrderDetails\" where \"OrderDetails\".\"OrderId\" = \"Orders_0\".\"OrderId\" )");

        public Orders() : base("Orders")
        {          
        }

        public override TableColumn[] GetPrimaryKey()
        {
            return  new TableColumn[] { OrderID };
        }
    }
}
