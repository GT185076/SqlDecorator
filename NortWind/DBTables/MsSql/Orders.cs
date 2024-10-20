using SQLDecorator;

namespace DBTables.MsSql
{
    public class Orders : DBTable
    {
        [ColumnDBName("OrderID")]
        public IntegerColumn OrderID;

        [ColumnDBName("CustomerID")]
        public IntegerColumn CustomerID;

        [ColumnDBName("OrderDate")]
        public DateTimeColumn OrderDate;

        [ColumnDBName("ShipName")]
        public StringColumn ShipName;
        
        public IntegerColumn numbersOflines = new IntegerColumn("NumberOfLines", 
                                             "(select count (*) from" +
                                             " \"Order Details\" where " +
                                             " \"Order Details\".\"OrderId\" =" +
                                             " \"Orders_0\".\"OrderId\" )");

        public Orders() : base("Orders", "dbo")
        {          
        }

        public override TableColumn[] GetPrimaryKey()
        {
            return  new TableColumn[] { OrderID };
        }
    }
}
