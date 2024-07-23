using System;
using System.Collections.Generic;
using System.Text;
using SQLDecorator;

namespace DBTables
{ 
    public class Product : DBTable
    {
        [ColumnName("Product_Id")]
        public StringColumn Product_Id ;

        [ColumnName("Weight")]
        public NumberColumn Weight ;

        [ColumnName("UnitOfMeasure")]
        public StringColumn UnitOfMeasure ;

        [ColumnName("IsManualPercentageEnable")]
        public LogicalColumn IsManualPercentageEnable ;

        [ColumnName("IsNonMerchandise")]
        public LogicalColumn IsNonMerchandise ;

        [ColumnName("MerchandiseCategoryFk")]
        public IntegerColumn MerchandiseCategoryFk ;

        [ColumnName("ItemTypeCode")]
        public IntegerColumn ItemTypeCode;

        [ColumnName("NacsCode")]
        public StringColumn NacsCode;
        
        [ColumnName("LastUpdated")]
        public DateTimeColumn LastUpdated;

        [ColumnName("Status")]
        public IntegerColumn Status;

        public Product() : base("CAT_Product", "dbo")
        {
            SetPrimaryKey(Product_Id);
        }
    }
}
