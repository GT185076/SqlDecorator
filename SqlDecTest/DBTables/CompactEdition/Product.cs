using SQLDecorator;

namespace DBTables.CompactEdition
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

        [ColumnName("QuantityPerUnit")]
        public StringColumn QuantityPerUnit;

        [ColumnName("UnitPrice")]
        public NumberColumn UnitPrice;

        [ColumnName("UnitsInStock")]
        public IntegerColumn UnitsInStock;

        [ColumnName("UnitsOnOrder")]
        public IntegerColumn UnitsOnOrder;

        [ColumnName("ReorderLevel")]
        public IntegerColumn ReorderLevel;

        [ColumnName("Discontinued")]
        public LogicalColumn Discontinued;

        public Product() : base("Products", "dbo")
        {
            SetPrimaryKey(ProductId);
        }
    }
}
