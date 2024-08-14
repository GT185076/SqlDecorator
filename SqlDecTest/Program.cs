using Npgsql;
using Microsoft.Data.SqlClient;
using System;
using System.Threading.Tasks;
using SQLDecorator;

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
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
            builder.DataSource = Environment.GetEnvironmentVariable("COMPUTERNAME");
            builder.Authentication = SqlAuthenticationMethod.ActiveDirectoryIntegrated;
            builder.InitialCatalog = "NorthWind";
            builder.TrustServerCertificate = true;

            var CM = DBTables.MsSql.MsSqlConnectionManager.Create(builder.ConnectionString);

            using (var connection= CM.SqlConnection)
            {

                Console.WriteLine("\nQuery data example:");
                Console.WriteLine("=========================================\n");

                var product = new DBTables.MsSql.Product();
                var order = new DBTables.MsSql.Orders();
                var orderDetail = new DBTables.MsSql.OrderDetails();
                var totalAmount = new IntegerColumn("Total Amount", "Products.UnitPrice * OrderLines.Quantity");

                var select = new Select(connection)
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

                var selectAll = new Select(connection)
                         .TableAdd(orderDetail, "OrderLines", ColumnsSelection.All)
                         .TableJoin(order, "Orders", order.OrderID.Equal(orderDetail.OrderID))
                         .Where(order.OrderDate.GreaterThan(DateTime.Now - new TimeSpan(365 * 32, 0, 0, 0)));

                foreach (var olr in selectAll.Run().Export<DBTables.MsSql.OrderDetails>())
                    Console.Write(
                        $"{olr.OrderID}\t" +
                        $"{olr.ProductId}\t" +
                        $"{olr.Quantity}\t" +
                        $"{olr.UnitPrice}\t" +
                        $"{olr.Discount} \n");
            }
        }

            private static void printCaptions(Select selectCmd)
            {

                Console.WriteLine(selectCmd.ToString());
                Console.WriteLine();
                foreach( var p in selectCmd.Parameters)
                        Console.WriteLine($"{p.ToString()}\t = {p.Value.ToString()}");
                Console.WriteLine();

                foreach (var c in selectCmd.SelectedFields)
                    Console.Write(c.ColumnCaption + "\t\t");
                Console.WriteLine();
                foreach (var c in selectCmd.SelectedFields)
                    Console.Write(string.Empty.PadLeft(c.ColumnCaption.Length, '-') + "\t\t");
                Console.WriteLine();
            }

        }
    }


