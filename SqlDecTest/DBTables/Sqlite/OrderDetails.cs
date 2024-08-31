using SQLDecorator;

namespace DBTables.Sqlite
{
    public class OrderDetails : DBTable
    {
        [ColumnName("OrderID")]
        public IntegerColumn OrderID;

        [ColumnName("ProductID")]
        public IntegerColumn ProductId;

        [ColumnName("UnitPrice")]
        public NumberColumn UnitPrice;

        [ColumnName("Quantity")]
        public IntegerColumn Quantity;

        [ColumnName("Discount")]
        public NumberColumn Discount;

        public OrderDetails() : base("OrderDetails")
        { }

        public override TableColumn[] GetPrimaryKey()
        {
            return new TableColumn[] { OrderID, ProductId };
        }
    }
}
