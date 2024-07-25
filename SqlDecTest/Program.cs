using Npgsql;
using Microsoft.Data.SqlClient;
using System;
using System.Threading.Tasks;
using SQLDecorator;
using DBTables;
using SqlDecTest.MsssqlDBTables;

namespace SqlDecTest
{
        class Program
        {
            static async Task Main(string[] args)
            {               
                Console.WriteLine("Data Base Object Mapper");
                Console.WriteLine("-----------------------");               
                RunMssql();
                Console.ReadKey();
            }
          
            static async Task RunPostGress()
            {
                var connString = "Host=localhost;Username=postgres;Password=admin1234;Database=postgres";

                await using var conn = new NpgsqlConnection(connString);
                await conn.OpenAsync();

                var cr = new cacheReady();
                var cr2 = new cacheReady();

                var select = new Select(conn).TableAdd(cr, "redis")
                                      .ColumnAdd(cr.PKey)
                                      .ColumnAdd(cr.PValue, "Value")
                                      .ColumnAdd(cr.IsActive)
                                      .ColumnAdd(cr.Height, "Height")
                                      .ColumnAdd(cr.createTS, "Create TS")
                                      .AndNot(cr.PKey.Equal("gadi")
                                                       .And(cr.IsActive)
                                                       .AndNot(cr.PValue.Equal("LEON")))
                                      .And(cr.PValue.Equal("toledano"))
                                      .Or(cr.PValue.Equal(cr.PKey))
                                      .LeftJoinTableAdd(cr2, cr2.PKey.Equal("5"), "selfi");

                var selectString = select.ToString();
                Console.WriteLine(selectString);

                Console.ReadKey();
                Console.WriteLine();

                foreach (var c in select.SelectedFields)
                    Console.Write(c.ColumnCaption + "\t\t");
                Console.WriteLine();

                foreach (var c in select.SelectedFields)
                    Console.Write(string.Empty.PadLeft(c.ColumnCaption.Length, '-') + "\t\t");
                Console.WriteLine();

                var runner = new PostGressSelectRunner();

                foreach (var record in runner.Run(select, conn))
                {
                    foreach (var f in record.Columns)
                        Console.Write($"{f}\t\t");
                    Console.WriteLine();
                }

                Console.WriteLine();
                Console.Write($"{select.Result.Count} Rows Selected.");
            }
        static void RunMssql()
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();

            builder.DataSource = "WILGT185076-R4B";
            builder.Authentication = SqlAuthenticationMethod.ActiveDirectoryIntegrated;
            builder.InitialCatalog = "NorthWind";
            builder.TrustServerCertificate = true;

            using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
            {
                Console.WriteLine("\nQuery data example:");
                Console.WriteLine("=========================================\n");

                connection.Open();

                var product = new Product();
                var order   = new Orders();
                var vPrice  = new IntegerColumn("Cents", "Max(Products.UnitPrice)");

                var select = new Select(connection)
                         .TableAdd(product, "Products")
                         .ColumnAdd(product.ProductName)
                         .ColumnAdd(vPrice)
                         .GroupByAdd(product.ProductName);


                Console.WriteLine(select.ToString());

                printCaptions(select);

                foreach (var record in select.Run())
                {
                    foreach (var f in record.Columns) Console.Write($"{f}\t\t");
                    Console.WriteLine();
                }
                Console.WriteLine($"\n{select.Result.Count} Rows Selected.\n");
                Console.ReadKey();

            }
        }
            private static void printCaptions(Select select)
            {
                Console.WriteLine();
                foreach (var c in select.SelectedFields)
                    Console.Write(c.ColumnCaption + "\t\t");
                Console.WriteLine();
                foreach (var c in select.SelectedFields)
                    Console.Write(string.Empty.PadLeft(c.ColumnCaption.Length, '-') + "\t\t");
                Console.WriteLine();
            }
        }
    }

