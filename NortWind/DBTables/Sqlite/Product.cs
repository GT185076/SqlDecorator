using SQLDecorator;

namespace DBTables.Sqlite
{
    public class Product : DBTable
    {
        [ColumnDBName("ProductID")]
        public IntegerColumn ProductId;

        [ColumnDBName("ProductName")]
        public StringColumn ProductName;

        [ColumnDBName("SupplierID")]
        public IntegerColumn SupplierID;

        [ColumnDBName("CategoryID")]
        public IntegerColumn CategoryID;

        [ColumnDBName("Unit")]
        public StringColumn Unit;

        [ColumnDBName("Price")]
        public NumberColumn Price;

        public Product() : base("Products")
        {}

        public override TableColumn[] GetPrimaryKey()
        {
            return new TableColumn[] { ProductId };
        }
    }
}
