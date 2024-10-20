using SQLDecorator;

namespace DBTables.Sqlite
{
    public class OrderDetails : DBTable
    {
        [ColumnDBName("OrderDetailID")]
        public IntegerColumn OrderDetailID;

        [ColumnDBName("OrderID")]
        public IntegerColumn OrderID;

        [ColumnDBName("ProductID")]
        public IntegerColumn ProductId;

        [ColumnDBName("Quantity")]
        public IntegerColumn Quantity;

        public OrderDetails() : base("OrderDetails")
        { }

        public override TableColumn[] GetPrimaryKey()
        {
            return new TableColumn[] { OrderDetailID };
        }
    }
  
}
