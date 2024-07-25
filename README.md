# Sql Decorator
---------------
By Gadi Toledano, 2024

Sql Decorator is a simple project aimed to practice some of the common design patterns.
Sql decorator is also a simple Object Relationship Management mapper to SQL tables.  
Here an example how SQL Mapping can be easy :
(Based On **NorthWind** Sample data base 
from : https://github.com/microsoft/sql-server-samples/blob/master/samples/databases/northwind-pubs/instnwnd.sql)


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
    }

Wrting a SQL tables quaery (Select) can be a simple a this :

                    var product = new Product();
                    var price   = new Price();
                    var vPrice  = new IntegerColumn("Cents", "Max(Prices.Price)");

                    var select = new Select(connection)
                                   .TableAdd(product, "Products")
                                   .TableAdd(price, "Prices")
                                   .ColumnAdd(product.Product_Id)
                                   .ColumnAdd(vPrice)
                                   .Where(price.Product_Id.Equal(product.Product_Id))
                                   .And(price.EffectiveDate
                                   .GreaterThan(DateTime.Now - new TimeSpan(365, 0, 0, 0, 0)))
                                   .GroupByAdd(product.Product_Id)
                                   .OrderByAdd(OrderBy.Asc, vPrice);


And reading the answer   

        foreach (var record in select.Run())
                    {
                        foreach (var f in record.Columns) Console.Write($"{f}\t\t");
                        Console.WriteLine();
                    }
