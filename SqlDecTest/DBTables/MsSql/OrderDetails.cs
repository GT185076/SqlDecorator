using SQLDecorator;

namespace DBTables.MsSql
{
    public class OrderDetails : DBTable
    {
        [ColumnDBName("OrderID")]
        public IntegerColumn OrderID;

        [ColumnDBName("ProductID")]
        public IntegerColumn ProductId;

        [ColumnDBName("UnitPrice")]
        public NumberColumn UnitPrice;

        [ColumnDBName("Quantity")]
        public IntegerColumn Quantity;

        [ColumnDBName("Discount")]
        public NumberColumn Discount;

        public OrderDetails() : base("Order Details", "dbo")
        { }

        public override TableColumn[] GetPrimaryKey()
        {
            return new TableColumn[] { OrderID, ProductId };
        }
    }
}
