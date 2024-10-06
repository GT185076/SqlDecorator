using SQLDecorator;

namespace DBTables.MsSql
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

        [ColumnDBName("QuantityPerUnit")]
        public StringColumn QuantityPerUnit;

        [ColumnDBName("UnitPrice")]
        public NumberColumn UnitPrice;

        [ColumnDBName("UnitsInStock")]
        public IntegerColumn UnitsInStock;

        [ColumnDBName("UnitsOnOrder")]
        public IntegerColumn UnitsOnOrder;

        [ColumnDBName("ReorderLevel")]
        public IntegerColumn ReorderLevel;

        [ColumnDBName("Discontinued")]
        public LogicalColumn Discontinued;

        public Product() : base("Products", "dbo")
        {}

        public override TableColumn[] GetPrimaryKey()
        {
            return new TableColumn[] { ProductId };
        }
    }
}
