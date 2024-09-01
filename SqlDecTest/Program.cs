using Microsoft.Data.SqlClient;
using System;
using SQLDecorator;
using DBTables.Sqlite;
using Microsoft.Data.Sqlite;


namespace SqlDecTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Data Base Object Mapper");
            Console.WriteLine("-----------------------");
            // RunMssql();
            RunSqlite();
            Console.ReadKey();
        }
        static void RunMssql()
        {
           

            Console.WriteLine("\nQuery data example:");
            Console.WriteLine("=========================================\n");

            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
            builder.DataSource = Environment.GetEnvironmentVariable("COMPUTERNAME");
            builder.Authentication = SqlAuthenticationMethod.ActiveDirectoryIntegrated;
            builder.InitialCatalog = "NorthWind";
            builder.TrustServerCertificate = true;

            var northWind = new DBTables.MsSql.NorthWind(builder.ConnectionString);                     

            var product     = new Product();
            var order       = new Orders();
            var orderDetail = new OrderDetails();
            var totalAmount = new IntegerColumn("Total Amount", "Products.UnitPrice * OrderLines.Quantity");
           
            var select = new Select(northWind)
                     .Top(10)
                     .TableAdd(orderDetail, "OrderLines")
                     .ColumnAdd(orderDetail.ProductId)
                     .ColumnAdd(product.ProductName)
                     .ColumnAdd(totalAmount.Sum())
                     .TableJoin(order, "Orders", order.OrderID.Equal(orderDetail.OrderID))
                     .TableLeftJoin(product, "Products", product.ProductId.Equal(orderDetail.ProductId))
                     .Where(order.OrderDate.GreaterThan(DateTime.Now - new TimeSpan(365 * 32, 0, 0, 0)))
                     .WhereAnd(orderDetail.ProductId.In("12,13,14"))
                     .GroupByAdd(orderDetail.ProductId, product.ProductName)
                     .OrderByAdd(totalAmount.Sum(), OrderBy.Desc)
                     .Having(product.ProductId.Count().GreaterThan(10));

            printCaptions(select);

            foreach (var record in select.Run())
            {
                foreach (var f in record.Columns)
                    Console.Write($"{f}\t");
                Console.WriteLine();
            }

            Console.WriteLine($"\n{select.Result.Count} Rows Selected.\n");

            Console.ReadKey();
            var selectAll = new Select(northWind)
                     .TableAdd(orderDetail, "OrderLines", ColumnsSelection.All)
                     .TableJoin(order, "Orders", order.OrderID.Equal(orderDetail.OrderID))
                     .Where(order.OrderDate.GreaterThan(DateTime.Now - new TimeSpan(365 * 32, 0, 0, 0)));

            foreach (var olr in selectAll.Run().Export<OrderDetails>())
                Console.Write(
                    $"{olr.OrderID}\t" +
                    $"{olr.ProductId}\t" +
                    $"{olr.Quantity}\t");
           


            Console.ReadKey();
            var order2 = new Orders();
            var selectOrder = new Select(northWind).TableAdd(order2, null, ColumnsSelection.All);
            printCaptions(selectOrder);

            foreach (var or in selectOrder.Run().Export<Orders>())
                    Console.WriteLine(or.ToString());

            Console.ReadKey();
        }

        static void RunSqlite()
        {
            Console.WriteLine("\nQuery data example:");
            Console.WriteLine("=========================================\n");

            SqliteConnectionStringBuilder builder = new SqliteConnectionStringBuilder();
            builder.DataSource = "Nortwind_Sqlight.db";
            builder.DataSource = ":memory:";
            var northWind2 = new NorthWind2(builder.ConnectionString);

            var product = new Product();
            var order = new Orders();
            var orderDetail = new OrderDetails();
            var totalAmount = new IntegerColumn("Total Amount", "Products.Price * OrderLines.Quantity");

            var select = new Select(northWind2)                     
                     .TableAdd(orderDetail, "OrderLines")
                     .ColumnAdd(orderDetail.ProductId)
                     .ColumnAdd(product.ProductName)
                     .ColumnAdd(totalAmount.Sum())
                     .TableJoin(order, "Orders", order.OrderID.Equal(orderDetail.OrderID))
                     .TableLeftJoin(product, "Products", product.ProductId.Equal(orderDetail.ProductId))
                     .Where(order.OrderDate.GreaterThan(DateTime.Now - new TimeSpan(365 * 42, 0, 0, 0)))
                     .WhereAnd(orderDetail.ProductId.In("12,13,14"))
                     .GroupByAdd(orderDetail.ProductId, product.ProductName)
                     .OrderByAdd(totalAmount.Sum(), OrderBy.Desc)
                     .Having(product.ProductId.Count().GreaterThan(5));

            printCaptions(select);
            
            foreach (var record in select.Run())
            {
                foreach (var f in record.Columns)
                    Console.Write($"{f}\t");
                Console.WriteLine();
            }

            Console.WriteLine($"\n{select.Result.Count} Rows Selected.\n");

            Console.ReadKey();
            var selectAll = new Select(northWind2)
                     .TableAdd(orderDetail, "OrderLines", ColumnsSelection.All)
                     .TableJoin(order, "Orders", order.OrderID.Equal(orderDetail.OrderID))
                     .Where(order.OrderDate.GreaterThan(DateTime.Now - new TimeSpan(365 * 32, 0, 0, 0)));

            foreach (var olr in selectAll.Run().Export<OrderDetails>())
                Console.Write(
                    $"{olr.OrderID}\t" +
                    $"{olr.ProductId}\t" +
                    $"{olr.Quantity}\t\n");
                    

            Console.ReadKey();
            var View1 = new View1();
            var selectOrder = new Select(northWind2).TableAdd(View1, null, ColumnsSelection.All);
            printCaptions(selectOrder);

            foreach (var or in selectOrder.Run().Export<View1>())
                Console.WriteLine(or.ToString());

            Console.ReadKey();
        }
        
        private static void printCaptions(Select selectCmd)
            {
            Console.WriteLine(selectCmd.ParametersToString());
            Console.WriteLine();
            Console.WriteLine(selectCmd.ToString());
            Console.WriteLine();              
            Console.WriteLine(selectCmd.CaptionsToString());            
            Console.WriteLine();                                 
            }
    }
    }


