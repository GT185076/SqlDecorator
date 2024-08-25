using Npgsql;
using Microsoft.Data.SqlClient;
using System;
using System.Threading.Tasks;
using SQLDecorator;
using DBTables.MsSql;
using System.Text;

namespace SqlDecTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Data Base Object Mapper");
            Console.WriteLine("-----------------------");
            RunMssql();
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

            var northWind = new NorthWind(builder.ConnectionString);                     

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
                     .GroupByAdd(orderDetail.ProductId, product.ProductName)
                     .OrderByAdd(totalAmount.Sum(), OrderBy.Desc)
                     .Having(product.ProductId.Count().GreaterThan(10));

            printCaptions(select);

            foreach (var record in select.Run())
            {
                foreach (var f in record.Columns)
                    Console.Write($"{f}\t\t");
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
                    $"{olr.Quantity}\t" +
                    $"{olr.UnitPrice}\t" +
                    $"{olr.Discount} \n");


            Console.ReadKey();
            var order2 = new Orders();
            var selectOrder = new Select(northWind).TableAdd(order2, null, ColumnsSelection.All);
            printCaptions(selectOrder);

            foreach (var or in selectOrder.Run().Export<Orders>())
                    Console.WriteLine(or.ToString());

            Console.ReadKey();
        }

            private static void printCaptions(Select selectCmd)
            {
                Console.WriteLine(selectCmd.ToString());
                Console.WriteLine();
                Console.WriteLine(selectCmd.CaptionsToString());
                Console.WriteLine();                
            }

    }
    }


