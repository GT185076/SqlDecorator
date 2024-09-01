using SQLDecorator;

namespace DBTables.Sqlite
{
    public class Product : DBTable
    {
        [ColumnName("ProductID")]
        public IntegerColumn ProductId;

        [ColumnName("ProductName")]
        public StringColumn ProductName;

        [ColumnName("SupplierID")]
        public IntegerColumn SupplierID;

        [ColumnName("CategoryID")]
        public IntegerColumn CategoryID;

        [ColumnName("Unit")]
        public StringColumn Unit;

        [ColumnName("Price")]
        public NumberColumn Price;

        public Product() : base("Products")
        {}

        public override TableColumn[] GetPrimaryKey()
        {
            return new TableColumn[] { ProductId };
        }
    }
}
