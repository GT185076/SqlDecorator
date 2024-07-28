using SQLDecorator;

namespace DBTables
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

        public OrderDetails() : base("Order Details", "dbo")
        {
            SetPrimaryKey(OrderID, ProductId);
        }
    }
}
