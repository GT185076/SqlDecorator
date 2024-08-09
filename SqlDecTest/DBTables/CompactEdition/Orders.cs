using SQLDecorator;

namespace DBTables.CompactEdition
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

        public Orders() : base("Orders", "dbo")
        {
            SetPrimaryKey(OrderID);
        }
    }
}
