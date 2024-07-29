# Sql Decorator
---------------
By Gadi Toledano, 2024

Sql Decorator is a simple project aimed to practice some of the common design patterns.
Sql decorator is also a simple Object Relationship Management mapper to SQL tables.  

**Example 1:**
Here an example how SQL Mapping can be easy :
     
        using SQLDecorator;

        namespace DBTables
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

                public Orders() : base("Orders",  "dbo")
                {            
                    SetPrimaryKey(OrderID);
                }        
            }
        }

Writing a SQL tables query (Select) can be a simple a this :

                var product     = new Product();
                var order       = new Orders();
                var orderDetail = new OrderDetails();
                var totalAmount = new IntegerColumn("Total Amount", "Sum(Products.UnitPrice * OrderLines.Quantity)");

                var select = new Select(connection)
                         .Top(10)
                         .TableAdd(orderDetail, "OrderLines")
                         .ColumnAdd(orderDetail.ProductId)
                         .ColumnAdd(product.ProductName)
                         .ColumnAdd(totalAmount)
                         .TableJoin(order, "Orders", order.OrderID.Equal(orderDetail.OrderID))
                         .TableLeftJoin(product, "Products", product.ProductId.Equal(orderDetail.ProductId))
                         .Where(order.OrderDate.GreaterThan(DateTime.Now - new TimeSpan(365 * 32, 0, 0, 0)))
                         .GroupByAdd(orderDetail.ProductId, product.ProductName)
                         .OrderByAdd(totalAmount, OrderBy.Desc);

And reading the answer :

        foreach (var record in select.Run())
                    {
                        foreach (var f in record.Columns) Console.Write($"{f}\t\t");
                        Console.WriteLine();
                    }

The FInal result will be :

        SELECT    TOP(10) 
                  "OrderLines"."ProductID" "ProductID" ,
                  "Products"."ProductName" "ProductName" ,
                  Sum(Products.UnitPrice * OrderLines.Quantity) "Total Amount" 
        FROM      "dbo"."Order Details" "OrderLines"  
        JOIN      "dbo"."Orders" "Orders"  ON ("Orders"."OrderID"="OrderLines"."OrderID") 
        LEFT JOIN "dbo"."Products" "Products"  ON ("Products"."ProductID"="OrderLines"."ProductID")
        WHERE     ("Orders"."OrderDate">'1992-08-05T19:19:37') 
        GROUP BY  "OrderLines"."ProductID","Products"."ProductName" ORDER BY Sum(Products.UnitPrice * OrderLines.Quantity) Desc

        ProductID       ProductName                     Total Amount
        ---------       -----------                     ------------
        38              'Côte de Blaye'                 164160.5000
        29              'Thüringer Rostbratwurst'       92347.3400
        59              'Raclette Courdavault'          82280.0000
        60              'Camembert Pierrot'             53618.0000
        62              'Tarte au sucre'                53391.9000
        56              'Gnocchi di nonna Alice'        47994.0000
        51              'Manjimup Dried Apples'         46958.0000
        17              'Alice Mutton'                  38142.0000
        18              'Carnarvon Tigers'              33687.5000
        28              'Rössle Sauerkraut'             29184.0000

        10 Rows Selected.

**Example 2:**
All Columns selection and Auto columns mapping back:

               var selectAll = new Select(connection)
                         .TableAdd(orderDetail, "OrderLines", ColumnsSelection.All)
                         .TableJoin(order, "Orders", order.OrderID.Equal(orderDetail.OrderID))
                         .Where(order.OrderDate.GreaterThan(DateTime.Now - new TimeSpan(365 * 32, 0, 0, 0)));

                foreach (var olr in selectAll.Run().Export<OrderDetails>())
                {
                    Console.Write($"{olr.OrderID}\t{olr.ProductId}\t {olr.Quantity}\t{olr.UnitPrice}\t{olr.Discount}");
                    Console.WriteLine();
                }

**Remark** :Based On **NorthWind** Sample data base 
from : https://github.com/microsoft/sql-server-samples/blob/master/samples/databases/northwind-pubs/instnwnd.sql)
