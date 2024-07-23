using System;
using System.Collections.Generic;
using System.Text;
using SQLDecorator;

namespace DBTables
{ 
    public class CAT_Price : DBTable
    {
        [ColumnName("Price_Id")]
        public StringColumn Price_Id;

        [ColumnName("EffectiveDate")]
        public DateTimeColumn EffectiveDate;

        [ColumnName("Product_Id")]
        public StringColumn Product_Id;

        [ColumnName("Price")]
        public NumberColumn Price;

        [ColumnName("Quantity")]
        public NumberColumn Quantity;

        [ColumnName("UnitOfMeasure")]
        public StringColumn UnitOfMeasure;

        [ColumnName("ISOCurrencySymbol")]
        public LogicalColumn ISOCurrencySymbol;

        [ColumnName("LastUpdated")]
        public DateTimeColumn LastUpdated;

        [ColumnName("ExpirationDate")]
        public DateTimeColumn ExpirationDate;

        [ColumnName("PriceType")]
        public IntegerColumn PriceType;

        [ColumnName("Status")]
        public IntegerColumn Status;

        public CAT_Price() : base("CAT_Price",  "dbo")
        {            
            SetPrimaryKey(Product_Id);
        }        
    }
}
