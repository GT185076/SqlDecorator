using SQLDecorator;

namespace DBTables.Sqlite
{
    public class OrderDetails : DBTable
    {
        [ColumnName("OrderDetailID")]
        public IntegerColumn OrderDetailID;

        [ColumnName("OrderID")]
        public IntegerColumn OrderID;

        [ColumnName("ProductID")]
        public IntegerColumn ProductId;

        [ColumnName("Quantity")]
        public IntegerColumn Quantity;

        public OrderDetails() : base("OrderDetails")
        { }

        public override TableColumn[] GetPrimaryKey()
        {
            return new TableColumn[] { OrderDetailID };
        }
    }

    public class View1 : DBTable
    {
        [ColumnName("OrderDetailID")]
        public IntegerColumn OrderDetailID;

        [ColumnName("OrderID")]
        public IntegerColumn OrderID;

        [ColumnName("ProductID")]
        public IntegerColumn ProductId;

        [ColumnName("Quantity")]
        public IntegerColumn Quantity;

        public View1() : base("View1")
        { }

        public override TableColumn[] GetPrimaryKey()
        {
            return new TableColumn[] { OrderDetailID };
        }
    }
}
